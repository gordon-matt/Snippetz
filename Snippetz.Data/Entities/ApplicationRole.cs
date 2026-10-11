using Microsoft.AspNetCore.Identity;

namespace Snippetz.Data.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}

public class ApplicationRoleMap : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder) =>
        builder.Property(m => m.Description).HasMaxLength(512).IsUnicode(true);
}
