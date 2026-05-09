using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostGenerator.Service.Services;
using PostGenerator.Shared.IServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGenerator.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        //Add services
        services.AddScoped<IIdeaAgentService, IdeaAgentService>();
        services.AddScoped<IWriterAgentService, WriterAgentService>();
        services.AddTransient<ITopicAgentService, TopicAgentService>();

        return services;
    }
}
