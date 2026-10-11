using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Pages;

public partial class Home
{
    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }

    private bool loading = true;
    private string? loadError;
    private bool searching;
    private List<SnippetDto> snippets = [];
    private List<SnippetDto> results = [];
    private List<SnippetDto> pinned = [];

    protected override async Task OnParametersSetAsync()
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
        pinned = [.. snippets.Where(snippet => snippet.IsPinned)];
        loadError = null;

        if (string.IsNullOrWhiteSpace(Query))
        {
            searching = false;
            results = [];
        }
        else
        {
            searching = true;
            var found = await Snippets.SearchAsync(SearchQueryParser.Parse(Query));
            results = found.IsSuccess ? [.. found.Value] : [];
            if (!found.IsSuccess)
            {
                Toasts.Error(found.GetDisplayMessage());
            }
        }

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

        Replace(snippets, updated.Value);
        Replace(results, updated.Value);
        pinned = [.. snippets.Where(item => item.IsPinned)];
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
