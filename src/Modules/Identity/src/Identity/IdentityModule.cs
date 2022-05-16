using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Swagger;
using FluentValidation;
using Identity.Data;
using Identity.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModules(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        services.AddScoped<IDbContext>(provider => provider.GetService<IdentityContext>()!);

        services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsAssembly(typeof(IdentityRoot).Assembly.GetName().Name)));
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();
        services.AddTransient<IEventMapper, EventMapper>();
        
        services.AddCustomMediatR();
        services.AddCustomProblemDetails();
        services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
        services.AddCustomMapster(typeof(IdentityRoot).Assembly);
        services.AddCustomMassTransit(typeof(IdentityRoot).Assembly, env);
        services.AddCustomSwagger(configuration, typeof(IdentityRoot).Assembly);

        return services;
    }
    
    public static IApplicationBuilder UseIdentityModules(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env = null)
    {
        app.UseMigrations(env);
        return app;
    }
}