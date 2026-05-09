using PostGenerator.Shared.Model;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.IServices;

public interface IWriterAgentService
{
    Task<string> WritePostAsync(PostRequest idea, string? tone, string? audience);
}
