using PostGenerator.Shared.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Service.IServices;

public interface IIdeaAgentService
{
    Task<PostIdea> GenerateIdeaAsync(string topic, string? tone, string? audience);
}
