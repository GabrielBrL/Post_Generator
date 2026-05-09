using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostGenerator.Infra.Http;
using PostGenerator.Infra.Services;
using PostGenerator.Shared.IServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
    {
        string apiKey = configuration.GetSection("Anthropic").GetValue<string>("ApiKey");

        services.AddHttpClient<ManagedAgentsHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            client.DefaultRequestHeaders.Add("anthropic-beta", "managed-agents-2026-04-01");
        });

        services.AddTransient<IAgentSetupService, AgentSetupService>();

        return services;
    }
}
