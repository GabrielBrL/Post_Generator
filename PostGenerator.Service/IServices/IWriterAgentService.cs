using PostGenerator.Shared.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Service.IServices;

public interface IWriterAgentService
{
    Task<string> WritePostAsync(PostIdea idea, string? tone, string? audience);
}
