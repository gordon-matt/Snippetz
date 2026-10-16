namespace Snippetz.Components;

public static class SnippetFormat
{
    private static readonly string[] BadgeTones =
    [
        "nf-badge-accent", "nf-badge-success", "nf-badge-info", "nf-badge-warning", "nf-badge-danger", "nf-badge-muted"
    ];

    public static string Relative(DateTime utc)
    {
        var moment = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var span = DateTime.UtcNow - moment;

        return span < TimeSpan.Zero
            ? "just now"
            : span.TotalSeconds < 45
                ? "just now"
                : span.TotalMinutes < 60
                    ? $"{Math.Max(1, (int)span.TotalMinutes)} min ago"
                    : span.TotalHours < 24
                        ? $"{(int)span.TotalHours} hr ago"
                        : span.TotalDays < 30 ? $"{(int)span.TotalDays} days ago" : moment.ToLocalTime().ToString("d MMM yyyy");
    }

    /// <summary>
    /// A stable tone per language (the same letter always gets the same colour), picked from the
    /// <c>nf-badge-*</c> tones so the colours follow the theme.
    /// </summary>
    public static string BadgeClass(string? language) =>
        string.IsNullOrEmpty(language) ? BadgeTones[0] : BadgeTones[char.ToLowerInvariant(language[0]) % BadgeTones.Length];

    public static string HighlightClass(string? language) => string.IsNullOrWhiteSpace(language)
            ? "plaintext"
            : language.Trim().ToLowerInvariant()
                .Replace("c#", "csharp", StringComparison.Ordinal)
                .Replace("c++", "cpp", StringComparison.Ordinal)
                .Replace("f#", "fsharp", StringComparison.Ordinal);
}