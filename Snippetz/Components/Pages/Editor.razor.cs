using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Pages;

public partial class Editor
{
    [Parameter]
    public int? Id { get; set; }

    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    private readonly SnippetFormModel form = new();
    private bool loading;
    private bool missing;
    private bool saving;
    private bool isPinned;
    private string? error;

    private bool isEdit => Id is > 0;

    protected override async Task OnParametersSetAsync()
    {
        error = null;
        missing = false;

        if (Id is not int id)
        {
            return;
        }

        loading = true;
        var result = await Snippets.GetAsync(id);
        loading = false;

        if (!result.IsSuccess)
        {
            missing = true;
            return;
        }

        var snippet = result.Value;
        form.Title = snippet.Title;
        form.Description = snippet.Description;
        form.Language = snippet.Language;
        form.Code = snippet.Code;
        form.Docs = snippet.Docs;
        form.Tags = string.Join(", ", snippet.Tags.Where(tag =>
            !string.Equals(tag, snippet.Language, StringComparison.OrdinalIgnoreCase)));
        isPinned = snippet.IsPinned;
    }

    private async Task SaveAsync()
    {
        saving = true;
        error = null;

        var request = new SaveSnippetRequest(
            form.Title,
            form.Description,
            form.Language,
            form.Code,
            form.Docs,
            form.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            isPinned);

        var result = isEdit && Id is int id
            ? await Snippets.UpdateAsync(id, request)
            : await Snippets.CreateAsync(request);

        saving = false;

        if (!result.IsSuccess)
        {
            error = result.GetDisplayMessage();
            return;
        }

        Toasts.Success(isEdit ? "Snippet saved." : "Snippet created.");
        Navigation.NavigateTo($"/snippet/{result.Value.Id}");
    }

    private sealed class SnippetFormModel
    {
        [Required(ErrorMessage = "Give the snippet a title.")]
        [MaxLength(Constants.MaxLengths.Title)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say which language this is.")]
        [MaxLength(Constants.MaxLengths.Language)]
        public string Language { get; set; } = string.Empty;

        public string Tags { get; set; } = string.Empty;

        [Required(ErrorMessage = "The snippet needs some code.")]
        public string Code { get; set; } = string.Empty;

        public string Docs { get; set; } = string.Empty;
    }
}
