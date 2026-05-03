using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Request;

public record class PostRequest(
    string Topic,
    string? Tone,
    string? Audience,
    string AccessToken);
