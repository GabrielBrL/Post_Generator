using Microsoft.Extensions.Configuration;
using PostGenerator.Infra.Http;
using PostGenerator.Service.Utils;
using PostGenerator.Shared.IServices;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PostGenerator.Service.Services;

public class TopicAgentService(ManagedAgentsHttpClient http, IConfiguration config) : ITopicAgentService
{
    private readonly string _agentId = config.GetSection("Anthropic").GetSection("DEV").GetValue<string>("topic_agent")
    ?? throw new InvalidOperationException("Set topic_agent on appsettings.");

    private readonly string _envId = config.GetSection("Anthropic").GetSection("DEV").GetValue<string>("env_id")
        ?? throw new InvalidOperationException("Set env_id on appsettings.");

    public async IAsyncEnumerable<TopicResult> GenerateAsync(
        TopicRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var session = await http.PostAsync("/v1/sessions", new
        {
            agent = _agentId,
            environment_id = _envId,
            title = "Post topic generation"
        }, ct);

        var sessionId = session.GetProperty("id").GetString()!;

        // Send user message (non-blocking — stream must open first)
        _ = Task.Run(async () =>
        {
            await Task.Delay(200, ct);
            await http.PostAsync($"/v1/sessions/{sessionId}/events", new
            {
                events = new[]
                {
                    new
                    {
                        type    = "user.message",
                        content = new[] { new { type = "text", text = BuildPrompt(request) } }
                    }
                }
            }, ct);
        }, ct);

        // Collect agent text until idle, then parse and yield
        var buffer = new StringBuilder();

        await foreach (var evt in http.StreamAsync($"/v1/sessions/{sessionId}/events/stream", ct))
        {
            var type = evt.TryGetProperty("type", out var t) ? t.GetString() : null;

            if (type == "agent.message" && evt.TryGetProperty("content", out var content))
                foreach (var block in content.EnumerateArray())
                    if (block.TryGetProperty("text", out var text))
                        buffer.Append(text.GetString());

            if (type == "session.status_idle") break;
        }

        foreach (TopicResult topic in JsonTreatment.Parse<List<TopicResult>>(buffer.ToString()) ?? new())
            yield return topic;

        await http.DeleteAsync($"/v1/sessions/{sessionId}", ct);
    }

    private string BuildPrompt(TopicRequest r) =>
        $"""
        Generate exactly {r.Quantity} creative post topic ideas for: {string.Join(", ", r.Stacks)}.
 
        {(r.Platform == "mixed"
            ? "Vary platforms: LinkedIn, Technical Blog, Twitter/X, YouTube, Newsletter."
            : $"Platform: {r.Platform}.")}
        {(r.Level == "mixed"
            ? "Vary levels: beginner, intermediate, advanced."
            : $"Level: {r.Level}.")}
 
        Language: {r.Language}.
 
        JSON fields per item: title (max 80 chars), hook (2-3 sentences), platform, stacks, format, level, language.
        Respond ONLY with the JSON array.
        """;

    
}
