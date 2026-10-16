namespace Snippetz.Services.Snippets;

/// <summary>
/// Trims, lowercases and de-duplicates tags, and always includes the language as a tag.
/// </summary>
public static class TagParser
{
    public static IReadOnlyList<string> Parse(IEnumerable<string>? tags, string? language)
    {
        var parsed = new HashSet<string>(StringComparer.Ordinal);

        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                foreach (string part in tag.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    string normalized = part.Trim().ToLowerInvariant();
                    if (normalized.Length > 0)
                    {
                        parsed.Add(normalized);
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            parsed.Add(language.Trim().ToLowerInvariant());
        }

        return [.. parsed];
    }
}