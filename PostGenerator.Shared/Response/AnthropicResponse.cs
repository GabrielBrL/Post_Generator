using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Response;

public record AnthropicResponse(AnthropicContent[] Content);
public record AnthropicContent(string Text);
