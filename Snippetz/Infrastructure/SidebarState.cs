namespace Snippetz.Infrastructure;

/// <summary>Tag counts shown in the sidebar on every page.</summary>
public sealed class SidebarState
{
    public IReadOnlyList<TagCountDto> Tags { get; private set; } = [];

    public string? ActiveTag { get; private set; }

    public int SnippetCount { get; private set; }

    /// <summary>True when the snippet list is showing everything, with no tag or search filter.</summary>
    public bool AllSelected { get; private set; }

    public bool Loaded { get; private set; }

    public event Action? Changed;

    public void Set(int snippetCount, IReadOnlyList<TagCountDto> tags, string? activeTag, bool allSelected)
    {
        SnippetCount = snippetCount;
        Tags = tags;
        ActiveTag = activeTag;
        AllSelected = allSelected;
        Loaded = true;
        Changed?.Invoke();
    }
}
