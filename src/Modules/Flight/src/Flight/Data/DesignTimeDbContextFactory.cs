using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flight.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FlightDbContext>
    {
        public FlightDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FlightDbContext>();

            builder.UseNpgsql("Server=localhost;Port=5432;Database=flight_db;User Id=postgres;Password=postgres;Include Error Detail=true");
            return new FlightDbContext(builder.Options, null);
        }
    }
}
