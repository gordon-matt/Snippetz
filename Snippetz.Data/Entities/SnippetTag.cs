namespace Snippetz.Data.Entities;

/// <summary>Join between a snippet and a tag. Unused tags are left in place when a snippet is deleted.</summary>
public class SnippetTag : BaseEntity<int>
{
    public int SnippetId { get; set; }

    public int TagId { get; set; }

    public Snippet Snippet { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}

public class SnippetTagMap : IEntityTypeConfiguration<SnippetTag>
{
    public void Configure(EntityTypeBuilder<SnippetTag> builder)
    {
        builder.ToTable("SnippetTags", Constants.Schemas.App);
        builder.HasKey(m => m.Id);

        builder.HasIndex(m => new { m.SnippetId, m.TagId }).IsUnique();

        builder.HasOne(m => m.Snippet)
            .WithMany(m => m.SnippetTags)
            .HasForeignKey(m => m.SnippetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Tag)
            .WithMany(m => m.SnippetTags)
            .HasForeignKey(m => m.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
