using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Swagger;
using Flight.Data;
using Flight.Data.Seed;
using Flight.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flight;

public static class FlightModule
{
    public static IServiceCollection AddFlightModules(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        services.AddCustomDbContext<FlightDbContext>(configuration);
        services.AddScoped<IDataSeeder, FlightDataSeeder>();
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(1);
        
        services.AddCustomMediatR();
        services.AddCustomProblemDetails();
        services.AddValidatorsFromAssembly(typeof(FlightRoot).Assembly);
        services.AddCustomMapster(typeof(FlightRoot).Assembly);
        services.AddCustomMassTransit(typeof(FlightRoot).Assembly, env);
        services.AddCustomSwagger(configuration, typeof(FlightRoot).Assembly);

        return services;
    }
    
    public static IApplicationBuilder UseFlightModules(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        app.UseMigrations(env);
        return app;
    }
}