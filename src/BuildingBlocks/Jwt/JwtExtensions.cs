using BuildingBlocks.Web;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Jwt;

public static class JwtExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        var jwtOptions = services.GetOptions<JwtBearerOptions>("Jwt");

        services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = jwtOptions.Authority;
                    options.TokenValidationParameters.ValidateAudience = false;
                });

        if (!string.IsNullOrEmpty(jwtOptions.Audience))
        {
            services.AddAuthorization(
                options =>
                {
                    // Set JWT as the default scheme for all [Authorize] attributes
                    options.DefaultPolicy =
                        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                            .RequireAuthenticatedUser()
                            .Build();

                    // Add your scope policy (optional)
                    if (!string.IsNullOrEmpty(jwtOptions.Audience))
                    {
                        options.AddPolicy(
                            nameof(ApiScope),
                            policy =>
                            {
                                policy.AuthenticationSchemes.Add(
                                    JwtBearerDefaults.AuthenticationScheme);

                                policy.RequireAuthenticatedUser();
                                policy.RequireClaim("scope", jwtOptions.Audience);
                            });
                    }
                });
        }

        return services;
    }
}
