using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Snippetz.Components.Shared;

namespace Snippetz.Components.Pages;

public partial class SnippetDetail
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    private SnippetDto? snippet;
    private bool loading = true;
    private string docsHtml = string.Empty;
    private ConfirmDialog confirm = default!;

    protected override async Task OnParametersSetAsync()
    {
        loading = true;
        var result = await Snippets.GetAsync(Id);
        snippet = result.IsSuccess ? result.Value : null;
        docsHtml = snippet is null ? string.Empty : MarkdownRenderer.ToHtml(snippet.Docs);
        loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (snippet is not null)
        {
            await Js.InvokeVoidAsync("snippetz.highlight", "body");
        }
    }

    private async Task TogglePinAsync()
    {
        if (snippet is null)
        {
            return;
        }

        var updated = await Snippets.SetPinnedAsync(snippet.Id, !snippet.IsPinned);
        if (!updated.IsSuccess)
        {
            Toasts.Error(updated.GetDisplayMessage());
            return;
        }

        snippet = updated.Value;
    }

    private async Task CopyCodeAsync()
    {
        if (snippet is null)
        {
            return;
        }

        await Js.InvokeVoidAsync("snippetz.copy", snippet.Code);
        Toasts.Success("Code copied.");
    }

    private async Task CopyRawUrlAsync()
    {
        if (snippet is null)
        {
            return;
        }

        string url = Navigation.ToAbsoluteUri($"api/snippets/raw/{snippet.Id}").ToString();
        await Js.InvokeVoidAsync("snippetz.copy", url);
        Toasts.Success("Raw URL copied.");
    }

    private async Task DeleteAsync()
    {
        if (snippet is null)
        {
            return;
        }

        if (!await confirm.ShowAsync("Delete snippet", "Are you sure you want to delete this snippet?"))
        {
            return;
        }

        var deleted = await Snippets.DeleteAsync(snippet.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.Error(deleted.GetDisplayMessage());
            return;
        }

        Navigation.NavigateTo("/snippets");
    }
}
