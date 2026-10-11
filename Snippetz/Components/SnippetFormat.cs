namespace Snippetz.Components;

public static class SnippetFormat
{
    public static string Relative(DateTime utc)
    {
        var moment = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var span = DateTime.UtcNow - moment;

        if (span < TimeSpan.Zero)
        {
            return "just now";
        }

        if (span.TotalSeconds < 45)
        {
            return "just now";
        }

        if (span.TotalMinutes < 60)
        {
            return $"{Math.Max(1, (int)span.TotalMinutes)} min ago";
        }

        if (span.TotalHours < 24)
        {
            return $"{(int)span.TotalHours} hr ago";
        }

        if (span.TotalDays < 30)
        {
            return $"{(int)span.TotalDays} days ago";
        }

        return moment.ToLocalTime().ToString("d MMM yyyy");
    }

    public static string BadgeClass(string? language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return "sz-badge-0";
        }

        int code = char.ToLowerInvariant(language[0]);
        return $"sz-badge-{code % 6}";
    }

    public static string HighlightClass(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "plaintext";
        }

        return language.Trim().ToLowerInvariant()
            .Replace("c#", "csharp", StringComparison.Ordinal)
            .Replace("c++", "cpp", StringComparison.Ordinal)
            .Replace("f#", "fsharp", StringComparison.Ordinal);
    }
}
