using Flight.Airports.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flight.Data.Configurations;

public class AirportConfiguration: IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.ToTable(nameof(Airport));

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
    }
}
