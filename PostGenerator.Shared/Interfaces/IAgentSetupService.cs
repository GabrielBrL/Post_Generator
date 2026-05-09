using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Shared.IServices;

public interface IAgentSetupService
{
    Task<(string AgentId, string EnvironmentId)> SetupAsync(CancellationToken ct = default);
}
