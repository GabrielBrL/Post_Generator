using PostGenerator.Infra.Http;
using PostGenerator.Shared.IServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Infra.Services;

public class AgentSetupService (ManagedAgentsHttpClient http) : IAgentSetupService
{
    private const string SystemPrompt = """
        You are an expert in technical content marketing for software developers.
        Your sole purpose is to generate creative, relevant, and strategic post topic
        ideas based on the technology stacks provided by the user.
 
        Rules:
        - Respond ONLY with a valid JSON array. No markdown fences, no extra text.
        - Avoid generic titles. Favor unexpected angles that address real developer pain points.
        - Vary formats: tutorial, thread, opinion piece, comparison, case study, quick tip, etc.
        - Write all titles and hooks in the language specified by the user.
        """;
    public async Task<(string AgentId, string EnvironmentId)> SetupAsync(CancellationToken ct = default)
    {
        var agent = await http.PostAsync("/v1/agents", new
        {
            name = "Post Topic Generator",
            description = "Generates creative post topic ideas for technical content based on technology stacks.",
            model = new { id = "claude-sonnet-4-6" },
            system = SystemPrompt,
            tools = new[] { new { type = "agent_toolset_20260401" } }
        }, ct);

        var env = await http.PostAsync("/v1/environments", new
        {
            name = "post-topic-env",
            config = new { type = "cloud", networking = new { type = "unrestricted" } }
        }, ct);

        return (
            agent.GetProperty("id").GetString()!,
            env.GetProperty("id").GetString()!
        );
    }
}
