using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passenger.Data;

namespace Passenger;

public static class PassengerModule
{
    public static IServiceCollection AddPassengerModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomDbContext<PassengerDbContext>(nameof(Passenger), configuration);
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(2);

        services.AddValidatorsFromAssembly(typeof(PassengerRoot).Assembly);
        services.AddCustomMapster(typeof(PassengerRoot).Assembly);
        services.AddCachingRequest(new List<Assembly> {typeof(PassengerRoot).Assembly});

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxPassengerBehavior<,>));

        return services;
    }

    public static IApplicationBuilder UsePassengerModules(this IApplicationBuilder app)
    {
        app.UseMigration<PassengerDbContext>();
        return app;
    }
}
