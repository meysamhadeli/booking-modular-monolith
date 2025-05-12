using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Identity.Identity.Constants;
using IdentityModel;

namespace BookingMonolith.Identity.Configurations;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };


    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new(Constants.StandardScopes.Booking),
            new(JwtClaimTypes.Role, new List<string> {"role"})
        };


    public static IList<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new(Constants.StandardScopes.Booking)
            {
                Scopes = { Constants.StandardScopes.Booking }
            },
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    JwtClaimTypes.Role, // Include roles scope
                    Constants.StandardScopes.Booking,
                },
                AccessTokenLifetime = 3600,  // authorize the client to access protected resources
                IdentityTokenLifetime = 3600, // authenticate the user,
                AlwaysIncludeUserClaimsInIdToken = true // Include claims in ID token
            }
        };
}
