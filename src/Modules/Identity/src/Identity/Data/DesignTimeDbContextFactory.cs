using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityContext>();

        builder.UseNpgsql("Server=localhost;Port=5432;Database=identity_db;User Id=postgres;Password=postgres;Include Error Detail=true");
        return new IdentityContext(builder.Options, null);
    }
}
