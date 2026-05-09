using PostGenerator.Shared.Model;
using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.IServices;

public interface IIdeaAgentService
{
    Task<PostIdeaResponse?> GenerateIdeaAsync(PostRequest postRequest, CancellationToken ct);
}
