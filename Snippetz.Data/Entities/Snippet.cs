namespace Snippetz.Data.Entities;

/// <summary>
/// A saved piece of code, owned by one user. The language is also stored as a tag so the library
/// sidebar can filter on it the same way as any other label.
/// </summary>
public class Snippet : BaseEntity<int>
{
    public required string Title { get; set; }

    public string Description { get; set; } = string.Empty;

    public required string Language { get; set; }

    public required string Code { get; set; }

    public string Docs { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    /// <summary>Identity user id, Keycloak <c>sub</c>, or the synthetic id in <c>None</c> mode.</summary>
    public required string OwnerUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<SnippetTag> SnippetTags { get; set; } = [];
}

public class SnippetMap : IEntityTypeConfiguration<Snippet>
{
    public void Configure(EntityTypeBuilder<Snippet> builder)
    {
        builder.ToTable("Snippets", Constants.Schemas.App);
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(Constants.MaxLengths.Title).IsUnicode(true);
        builder.Property(m => m.Description).IsRequired().IsUnicode(true);
        builder.Property(m => m.Language).IsRequired().HasMaxLength(Constants.MaxLengths.Language).IsUnicode(false);
        builder.Property(m => m.Code).IsRequired().IsUnicode(true);
        builder.Property(m => m.Docs).IsRequired().IsUnicode(true);
        builder.Property(m => m.OwnerUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(false);

        builder.HasIndex(m => new { m.OwnerUserId, m.IsPinned, m.UpdatedAt });
        builder.HasIndex(m => new { m.OwnerUserId, m.Language });
    }
}