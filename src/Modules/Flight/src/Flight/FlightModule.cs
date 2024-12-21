using System.Collections.Generic;
using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using Flight.Data;
using Flight.Data.Seed;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flight;

public static class FlightModule
{
    public static IServiceCollection AddFlightModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomDbContext<FlightDbContext>(nameof(Flight), configuration);
        services.AddScoped<IDataSeeder, FlightDataSeeder>();
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(1);

        services.AddValidatorsFromAssembly(typeof(FlightRoot).Assembly);
        services.AddCustomMapster(typeof(FlightRoot).Assembly);
        services.AddCachingRequest(new List<Assembly> {typeof(FlightRoot).Assembly});

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxFlightBehavior<,>));

        return services;
    }

    public static IApplicationBuilder UseFlightModules(this IApplicationBuilder app)
    {
        app.UseMigration<FlightDbContext>();
        return app;
    }
}
