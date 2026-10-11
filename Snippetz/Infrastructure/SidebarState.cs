namespace Snippetz.Infrastructure;

/// <summary>Tag counts the library page publishes for the sidebar to render.</summary>
public sealed class SidebarState
{
    public IReadOnlyList<TagCountDto> Tags { get; private set; } = [];

    public string? ActiveTag { get; private set; }

    public int SnippetCount { get; private set; }

    public event Action? Changed;

    public void SetLibrary(int snippetCount, IReadOnlyList<TagCountDto> tags, string? activeTag)
    {
        SnippetCount = snippetCount;
        Tags = tags;
        ActiveTag = activeTag;
        Changed?.Invoke();
    }

    public void SetActiveTag(string? activeTag)
    {
        ActiveTag = activeTag;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Tags = [];
        ActiveTag = null;
        SnippetCount = 0;
        Changed?.Invoke();
    }
}
