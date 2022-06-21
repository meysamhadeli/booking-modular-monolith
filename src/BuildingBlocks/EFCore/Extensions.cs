using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EFCore;

public static class Extensions
{
    public static IServiceCollection AddCustomDbContext<TContext>(
        this IServiceCollection services,
        string connectionName,
        IConfiguration configuration)
        where TContext : DbContext, IDbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(connectionName),
                x => x.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name)));

        services.AddScoped<IDbContext>(provider => provider.GetService<TContext>());
        
        return services;
    }
    
    
    public static async Task<IApplicationBuilder> UseMigrationsAsync<TContext>(this IApplicationBuilder app)
        where TContext : DbContext, IDbContext
    {
        await MigrateDatabaseAsync<TContext>(app.ApplicationServices);
        await SeedDataAsync<TContext>(app.ApplicationServices);
        
        return app;
    }

    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext, IDbContext
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }

    private static async Task SeedDataAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext, IDbContext
    {
        using var scope = serviceProvider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
        foreach (var seeder in seeders)
        {
            await seeder.SeedAllAsync<TContext>();
        }
    }
}
