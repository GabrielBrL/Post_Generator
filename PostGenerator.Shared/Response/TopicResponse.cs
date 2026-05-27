using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PostGenerator.Shared.Response;

public record TopicResponse(
    [property: JsonPropertyName("topic")] List<TopicResult> Topics
    );

public record TopicResult(
[property: JsonPropertyName("title")] string Title,
[property: JsonPropertyName("hook")] string Hook,
[property: JsonPropertyName("platform")] string Platform,
[property: JsonPropertyName("stacks")] string[] Stacks,
[property: JsonPropertyName("format")] string Format,
[property: JsonPropertyName("level")] string Level,
[property: JsonPropertyName("language")] string Language
);
