using System.Text.RegularExpressions;

namespace Snippetz.Services.Snippets;

/// <summary>
/// Ports the snippet-box search box. <c>lang:</c> and <c>tags:</c> take comma-separated letters;
/// everything else is a free-text query against the title and description.
/// </summary>
public static partial class SearchQueryParser
{
    [GeneratedRegex(@"(tags|lang):[a-zA-Z]+(,[a-zA-Z]+)*", RegexOptions.CultureInvariant)]
    private static partial Regex FilterToken();

    public static SnippetSearchRequest Parse(string? rawQuery)
    {
        string raw = rawQuery ?? string.Empty;
        var tags = Extract(raw, "tags");
        var languages = Extract(raw, "lang");
        string query = FilterToken().Replace(raw, string.Empty).Trim();

        return new SnippetSearchRequest(query, tags, languages);
    }

    private static IReadOnlyList<string> Extract(string query, string filter)
    {
        var match = Regex.Match(query, filter + @":[a-zA-Z]+(,[a-zA-Z]+)*", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return [];
        }

        return match.Value.Split(':')[1].Split(',');
    }
}
