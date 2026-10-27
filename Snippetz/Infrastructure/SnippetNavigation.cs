using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Snippetz.Infrastructure;

/// <summary>
/// Sidebar for the snippet library: a new-snippet link, one row per tag, and the log viewer.
/// Tag rows match the query string, so only the tag in the address is highlighted.
/// </summary>
public sealed class SnippetNavigation(ISnippetService snippets) : IShellNavigation
{
    private static readonly string[] ListPaths = ["/", "/snippets"];

    public string? SearchUrlFormat => "/?q={0}";

    public async Task<IReadOnlyList<ForgeNavSection>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        var count = await snippets.CountAsync(cancellationToken);
        var tags = await snippets.GetTagCountsAsync(cancellationToken);

        var filters = new List<ForgeNavItem>
        {
            new()
            {
                Title = "All snippets",
                Href = "/",
                Icon = "library",
                MatchAll = true,
                ActivePaths = ListPaths,
                ActiveQuery = new Dictionary<string, string?> { ["tag"] = null, ["q"] = null },
                Count = count.IsSuccess ? count.Value : null,
            },
        };

        if (tags.IsSuccess)
        {
            filters.AddRange(tags.Value.Select(tag => new ForgeNavItem
            {
                Title = tag.Name,
                Href = $"/?tag={Uri.EscapeDataString(tag.Name)}",
                TagIcon = true,
                MatchAll = true,
                ActivePaths = ListPaths,
                ActiveQuery = new Dictionary<string, string?> { ["tag"] = tag.Name },
                Count = tag.Count,
            }));
        }

        return
        [
            new ForgeNavSection
            {
                Items = [new ForgeNavItem { Title = "New snippet", Href = "/editor", Icon = "plus" }],
            },
            new ForgeNavSection
            {
                Heading = "Filter by tag",
                Items = filters,
            },
            new ForgeNavSection
            {
                Heading = "Administration",
                Roles = WellKnownRoles.Administrator,
                Items =
                [
                    new ForgeNavItem
                    {
                        Title = "Logs",
                        Href = "/logvue",
                        Icon = "logs",
                        External = true,
                        Tooltip = "Log viewer",
                    },
                ],
            },
        ];
    }
}