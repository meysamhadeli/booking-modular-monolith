using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passenger.Data;
using Passenger.Extensions;

namespace Passenger;

public static class PassengerModule
{
    public static IServiceCollection AddPassengerModules(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        services.AddCustomDbContext<PassengerDbContext>(configuration);
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(2);

        services.AddCustomMediatR();
        services.AddCustomProblemDetails();
        services.AddValidatorsFromAssembly(typeof(PassengerRoot).Assembly);
        services.AddCustomMapster(typeof(PassengerRoot).Assembly);
        services.AddCustomMassTransit(typeof(PassengerRoot).Assembly, env);
        services.AddCustomSwagger(configuration, typeof(PassengerRoot).Assembly);

        return services;
    }
    
    public static IApplicationBuilder UsePassengerModules(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        app.UseMigrations(env);
        return app;
    }
}