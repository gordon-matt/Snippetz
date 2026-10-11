using Markdig;

namespace Snippetz.Services.Snippets;

/// <summary>
/// Renders snippet documentation. Raw HTML is stripped so a pasted note cannot embed a script.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseAutoLinks()
        .Build();

    public static string ToHtml(string? markdown) =>
        string.IsNullOrWhiteSpace(markdown)
            ? string.Empty
            : Markdown.ToHtml(markdown, Pipeline);
}
