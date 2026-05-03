using PostGenerator.Shared.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.Response;

public record class PreviewResponse(object? ReturnObj, string Message, bool Success);
