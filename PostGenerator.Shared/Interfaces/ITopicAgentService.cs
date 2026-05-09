using PostGenerator.Shared.Request;
using PostGenerator.Shared.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.IServices;

public interface ITopicAgentService
{
    IAsyncEnumerable<TopicResult> GenerateAsync(TopicRequest request, CancellationToken ct = default);
}
