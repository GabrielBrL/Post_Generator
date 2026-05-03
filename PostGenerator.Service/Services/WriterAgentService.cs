using Microsoft.Extensions.Configuration;
using PostGenerator.Service.IServices;
using PostGenerator.Shared.Model;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PostGenerator.Service.Services;

public class WriterAgentService(IHttpClientFactory factory, IConfiguration config) : IWriterAgentService
{
    public async Task<string> WritePostAsync(PostIdea idea, string? tone, string? audience)
    {
        var client = factory.CreateClient();
        var apiKey = config["Anthropic:ApiKey"];

        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var prompt = $"""
            You are an expert LinkedIn copywriter. Write a high-engagement LinkedIn post.
            
            Use this idea as your blueprint:
            - Angle: {idea.Angle}
            - Key Points: {idea.KeyPoints}
            - Hook (opening line): {idea.Hook}
            - Call to Action: {idea.CallToAction}
            
            Tone: {tone ?? "professional"}
            Target Audience: {audience ?? "general professionals"}
            
            Rules:
            - Start with the hook (no intro like "Here's a post:")
            - Use short paragraphs (1-2 lines max)
            - Add relevant emojis sparingly
            - Use line breaks for readability
            - End with the call to action
            - Max 1300 characters (LinkedIn limit)
            - Do NOT use hashtags (keep it clean)
            
            Return ONLY the post text, nothing else.
        """;

        var body = new
        {
            model = "claude-sonnet-4-6",
            max_tokens = 1000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>();
        return result!.Content[0].Text.Trim();
    }
}
