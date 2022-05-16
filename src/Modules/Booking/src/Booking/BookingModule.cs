using Booking.Configuration;
using Booking.Data;
using Booking.Extensions;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.EventStoreDB;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking;

public static class BookingModule
{
    public static IServiceCollection AddBookingModules(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        services.AddCustomDbContext<BookingDbContext>(configuration);
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(3);
        
        services.AddCustomMediatR();
        services.AddCustomProblemDetails();
        services.AddValidatorsFromAssembly(typeof(BookingRoot).Assembly);
        services.AddCustomMapster(typeof(BookingRoot).Assembly);
        services.AddCustomMassTransit(typeof(BookingRoot).Assembly, env);
        services.AddCustomSwagger(configuration, typeof(BookingRoot).Assembly);
        
        // EventStoreDB Configuration
        services.AddEventStore(configuration, typeof(BookingRoot).Assembly)
            .AddEventStoreDBSubscriptionToAll();
        
        services.Configure<GrpcOptions>(options => configuration.GetSection("Grpc").Bind(options));

        return services;
    }
    
    public static IApplicationBuilder UseBookingModules(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        app.UseMigrations(env);
        
        return app;
    }
}