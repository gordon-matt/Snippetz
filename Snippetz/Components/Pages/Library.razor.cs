using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Pages;

public partial class Library : IDisposable
{
    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    [Inject]
    private SidebarState Sidebar { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "tag")]
    public string? Tag { get; set; }

    private bool loading = true;
    private string? loadError;
    private List<SnippetDto> snippets = [];
    private List<SnippetDto> visible = [];
    private string? activeTag;

    protected override async Task OnParametersSetAsync()
    {
        activeTag = string.IsNullOrWhiteSpace(Tag) ? null : Tag.Trim().ToLowerInvariant();

        if (snippets.Count == 0 && loadError is null)
        {
            loading = true;
            var listed = await Snippets.ListAsync();
            var counts = await Snippets.GetTagCountsAsync();

            if (!listed.IsSuccess)
            {
                loadError = listed.GetDisplayMessage();
                loading = false;
                return;
            }

            snippets = [.. listed.Value];
            var tags = counts.IsSuccess ? counts.Value : [];
            Sidebar.SetLibrary(snippets.Count, tags, activeTag);
        }
        else
        {
            Sidebar.SetActiveTag(activeTag);
        }

        visible = activeTag is null
            ? snippets
            : [.. snippets.Where(snippet => snippet.Tags.Contains(activeTag, StringComparer.OrdinalIgnoreCase))];

        loading = false;
    }

    private async Task TogglePinAsync(SnippetDto snippet)
    {
        var updated = await Snippets.SetPinnedAsync(snippet.Id, !snippet.IsPinned);
        if (!updated.IsSuccess)
        {
            Toasts.Error(updated.GetDisplayMessage());
            return;
        }

        int index = snippets.FindIndex(item => item.Id == updated.Value.Id);
        if (index >= 0)
        {
            snippets[index] = updated.Value;
        }

        visible = activeTag is null
            ? snippets
            : [.. snippets.Where(item => item.Tags.Contains(activeTag, StringComparer.OrdinalIgnoreCase))];
    }

    public void Dispose() => Sidebar.Clear();
}
