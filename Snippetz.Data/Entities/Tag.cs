namespace Snippetz.Data.Entities;

/// <summary>
/// A label attached to snippets. Names are stored lowercase and are unique per owner.
/// </summary>
public class Tag : BaseEntity<int>
{
    public required string Name { get; set; }

    public required string OwnerUserId { get; set; }

    public ICollection<SnippetTag> SnippetTags { get; set; } = [];
}

public class TagMap : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags", Constants.Schemas.App);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.TagName).IsUnicode(false);
        builder.Property(m => m.OwnerUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(false);

        builder.HasIndex(m => new { m.OwnerUserId, m.Name }).IsUnique();
    }
}