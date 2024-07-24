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

            x.UseDashboard();
            x.FailedRetryCount = 5;
            x.FailedThresholdCallback = failed =>
            {
                var logger = failed.ServiceProvider.GetService<ILogger>();
                logger?.LogError(
                    $@"A message of type {failed.MessageType} failed after executing {x.FailedRetryCount} several times,
                        requiring manual troubleshooting. Message name: {failed.Message.GetName()}");
            };
            x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        });

        services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(ICapSubscribe)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
              .AddCapInstrumentation()
              .AddJaegerExporter();
        });

        return services;
    }
}
