using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PostGenerator.Shared.Request;

public class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    [JsonPropertyName("messages")]
    public List<Message> Messages = new List<Message>();
    [JsonPropertyName("stream")]
    public bool Stream = true;
}

public class Message
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public class OllamaStreamChunk
{
    // The 'content' will hold the actual text chunk from the model
    public string Content { get; set; }
}