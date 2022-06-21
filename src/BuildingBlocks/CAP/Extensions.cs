using System.Text.Encodings.Web;
using System.Text.Unicode;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Savorboard.CAP.InMemoryMessageQueue;

namespace BuildingBlocks.CAP;

public static class Extensions
{
    public static IServiceCollection AddCustomCap(this IServiceCollection services)
    {

        services.AddCap(x =>
        {
            x.UseInMemoryStorage();
            x.UseInMemoryMessageQueue();
        });
        
        services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(ICapSubscribe)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddOpenTelemetryTracing(builder => builder
            .AddAspNetCoreInstrumentation()
            .AddCapInstrumentation()
            .AddJaegerExporter()
        );

        return services;
    }
}
