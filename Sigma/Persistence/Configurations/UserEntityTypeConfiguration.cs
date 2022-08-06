using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sigma.Persistence.Entities;

namespace Sigma.Persistence.Configurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));

        builder.HasData(
            new User(new Guid("ECDA3324-659D-4162-8A47-2B16852DCC81"), "Alice"),
            new User(new Guid("E0A1233D-0E68-4B13-918C-5579E65AA83E"), "Bob"));
    }
}