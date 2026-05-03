using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PostGenerator.Service.IServices;
using PostGenerator.Shared.Model;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace PostGenerator.Service.Services;

public class IdeaAgentService(IHttpClientFactory factory, IConfiguration config) : IIdeaAgentService
{
    public async Task<PostIdea> GenerateIdeaAsync(string topic, string? tone, string? audience)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", config["Anthropic:ApiKey"]);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = "claude-sonnet-4-6",
            max_tokens = 1000,
            system = """
                You are a JSON API. You only output raw JSON objects.
                Never use markdown, headers, bullet points, backticks, or any explanation.
                Your entire response must be a single valid JSON object and nothing else.
                Never wrap the result in a parent key like "post_idea" or any other wrapper.
                Always use exactly the field names provided in the request.
            """,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = $@"
                        Generate a LinkedIn post idea for the following:
 
                        Topic: " + topic + @"
                        Tone:" + tone ?? "professional" + @"
                        Target Audience:" + audience ?? "general professionals" + """

                        You MUST return exactly this JSON structure.
                        Use exactly these field names: Angle, KeyPoints, Hook, CallToAction.
                        Do not add, rename, or wrap any fields.
 
                        Example of the only acceptable response format:
                        {{
                            "Angle": "why most developers underestimate clean code",
                            "KeyPoints": "readability saves time; code is read more than written; naming matters most; refactoring is not optional",
                            "Hook": "You will spend 10x more time reading code than writing it.",
                            "CallToAction": "What is the one rule your team never breaks when writing code?"
                        }}
                    """
                }
            }
        };

        var response = await client.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            JsonElement jsonElement = JsonElement.Parse(await response.Content.ReadAsStringAsync());
            string error = jsonElement.GetProperty("error").GetProperty("message").ToString();
            throw new Exception(error);
        }

        var result = await response.Content.ReadFromJsonAsync<AnthropicResponse>();
        var raw = result!.Content[0].Text;

        return ParseToPostIdea(raw);
    }

    private static PostIdea ParseToPostIdea(string raw)
    {
        var stripped = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*", "", RegexOptions.Multiline);
        stripped = Regex.Replace(stripped, @"```\s*$", "", RegexOptions.Multiline).Trim();

        var match = Regex.Match(stripped, @"\{[\s\S]*\}");
        if (!match.Success)
            throw new InvalidOperationException(
                $"Idea agent did not return a JSON object. Raw: {raw}");

        using var doc = JsonDocument.Parse(match.Value);
        var root = doc.RootElement;

        return new PostIdea(
            Angle: GetField(root, "Angle", "angle", "title", "concept", "perspective"),
            KeyPoints: GetField(root, "KeyPoints", "keyPoints", "key_points", "points", "body", "content"),
            Hook: GetField(root, "Hook", "hook", "opening", "opening_line", "intro"),
            CallToAction: GetField(root, "CallToAction", "callToAction", "call_to_action", "cta", "closing", "call_to_action_question")
        );
    }

    private static string GetField(JsonElement root, params string[] candidates)
    {
        foreach (var name in candidates)
        {
            if (root.TryGetProperty(name, out var prop) &&
                prop.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(prop.GetString()))
            {
                return prop.GetString()!;
            }
        }

        // Last resort: return the first string property that has a value
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(prop.Value.GetString()))
            {
                return prop.Value.GetString()!;
            }
        }

        return string.Empty;
    }
}