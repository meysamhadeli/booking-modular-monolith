namespace Identity.Data.Configurations;

using Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));

        builder.Property(r => r.FirstName).HasMaxLength(50);
        builder.Property(r => r.LastName).HasMaxLength(50);

        // Must stay in sync with the Passenger module's `passport_number` column
        // (`character varying(10)`). `UserCreated` is handled in-process in this modular
        // monolith, so a longer value here would overflow the Passenger column at runtime.
        builder.Property(r => r.PassPortNumber).HasMaxLength(10);

        // // ref: https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=fluent-api
        builder.Property(r => r.Version).IsConcurrencyToken();
    }
}
