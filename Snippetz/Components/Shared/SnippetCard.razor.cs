using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Snippetz.Components.Shared;

public partial class SnippetCard
{
    [Parameter, EditorRequired]
    public SnippetDto Snippet { get; set; } = default!;

    [Parameter]
    public EventCallback<SnippetDto> OnPin { get; set; }

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    private async Task PinAsync() => await OnPin.InvokeAsync(Snippet);

    private async Task CopyAsync()
    {
        await Js.InvokeVoidAsync("snippetz.copy", Snippet.Code);
        Toasts.Success("Code copied.");
    }
}