using Microsoft.Extensions.Configuration;
using PostGenerator.Infra.Http;
using PostGenerator.Shared.IServices;
using PostGenerator.Shared.Model;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PostGenerator.Service.Services;

public class WriterAgentService(ManagedAgentsHttpClient http, IConfiguration config) : IWriterAgentService
{
    private readonly string _agentId = config.GetSection("Anthropic").GetSection("DEV").GetValue<string>("writer_agent")
    ?? throw new InvalidOperationException("Set topic_agent on appsettings.");
    private readonly string _envId = config.GetSection("Anthropic").GetSection("DEV").GetValue<string>("env_id")
        ?? throw new InvalidOperationException("Set env_id on appsettings.");
    public async Task<string> WritePostAsync(PostIdeaResponse idea, CancellationToken ct)
    {
        var session = await http.PostAsync("/v1/sessions", new
        {
            agent = _agentId,
            environment_id = _envId,
            title = "Post topic generation"
        }, ct);

        var sessionId = session.GetProperty("id").GetString()!;

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
                        content = new[] { new { type = "text", text = JsonSerializer.Serialize(idea) } }
                    }
                }
            }, ct);
        }, ct);

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
        return buffer.ToString().Trim();
    }
}
