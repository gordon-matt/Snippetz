using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Pages;

public partial class Library
{
    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "tag")]
    public string? Tag { get; set; }

    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }

    private bool loading = true;
    private bool searching;
    private string? loadError;
    private string? activeTag;
    private List<SnippetDto> snippets = [];
    private List<SnippetDto> visible = [];
    private List<SnippetDto> results = [];

    protected override async Task OnParametersSetAsync()
    {
        activeTag = string.IsNullOrWhiteSpace(Tag) ? null : Tag.Trim().ToLowerInvariant();
        searching = !string.IsNullOrWhiteSpace(Query);

        if (snippets.Count == 0 && loadError is null)
        {
            loading = true;
            var listed = await Snippets.ListAsync();

            if (!listed.IsSuccess)
            {
                loadError = listed.GetDisplayMessage();
                loading = false;
                return;
            }

            snippets = [.. listed.Value];
            loadError = null;
        }

        if (searching)
        {
            var found = await Snippets.SearchAsync(SearchQueryParser.Parse(Query));
            if (!found.IsSuccess)
            {
                Toasts.ShowFailure(found);
                results = [];
            }
            else
            {
                results = [.. found.Value];
            }
        }
        else
        {
            results = [];
            visible = activeTag is null
                ? snippets
                : [.. snippets.Where(snippet => snippet.Tags.Contains(activeTag, StringComparer.OrdinalIgnoreCase))];
        }

        loading = false;
    }

    private async Task TogglePinAsync(SnippetDto snippet)
    {
        var updated = await Snippets.SetPinnedAsync(snippet.Id, !snippet.IsPinned);
        if (!updated.IsSuccess)
        {
            Toasts.ShowFailure(updated);
            return;
        }

        Replace(snippets, updated.Value);
        Replace(results, updated.Value);
        visible = activeTag is null
            ? snippets
            : [.. snippets.Where(item => item.Tags.Contains(activeTag, StringComparer.OrdinalIgnoreCase))];
    }

    private static void Replace(List<SnippetDto> list, SnippetDto updated)
    {
        int index = list.FindIndex(item => item.Id == updated.Id);
        if (index >= 0)
        {
            list[index] = updated;
        }
    }
}