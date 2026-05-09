using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace PostGenerator.Shared.Response;

public record class PostIdeaResponse
([property: JsonPropertyName("title")] string Title,
[property: JsonPropertyName("content_angle")] string Angle,
[property: JsonPropertyName("target_audience")] string TargetAudience,
[property: JsonPropertyName("content_goal")] string Objective,
[property: JsonPropertyName("outline")] string[] Outline,
[property: JsonPropertyName("key_points")] string[] KeyPoints,
[property: JsonPropertyName("cta")] string CallToAction,
[property: JsonPropertyName("suggested_visuals")] string[] SuggestVisual,
[property: JsonPropertyName("estimated_length")] string Length,
[property: JsonPropertyName("tone")] string Tone,
[property: JsonPropertyName("language")] string Language);