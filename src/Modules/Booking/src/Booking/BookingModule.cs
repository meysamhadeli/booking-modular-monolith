using System.Reflection;
using Booking.Configuration;
using Booking.Data;
using BuildingBlocks.Caching;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.EventStoreDB;
using BuildingBlocks.Exception;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Mapster;
using BuildingBlocks.Mongo;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking;

public static class BookingModule
{
    public static IServiceCollection AddBookingModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<BookingReadDbContext>(configuration);
        services.AddTransient<IEventMapper, EventMapper>();
        SnowFlakIdGenerator.Configure(3);

        services.AddValidatorsFromAssembly(typeof(BookingRoot).Assembly);
        services.AddCustomMapster(typeof(BookingRoot).Assembly);

        // EventStoreDB Configuration
        services.AddEventStore(configuration, typeof(BookingRoot).Assembly)
            .AddEventStoreDBSubscriptionToAll();

        services.Configure<GrpcOptions>(options => configuration.GetSection("Grpc").Bind(options));

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        services.AddMagicOnion();

        services.AddCachingRequest(new List<Assembly> {typeof(BookingRoot).Assembly});

        return services;
    }

    public static IApplicationBuilder UseBookingModules(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMagicOnionService();
        });
        return app;
    }
}
