using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostGenerator.Service.IServices;
using PostGenerator.Service.Services;
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

        return services;
    }
}
