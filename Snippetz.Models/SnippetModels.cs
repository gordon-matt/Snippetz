namespace Snippetz.Models;

public sealed record SnippetDto(
    int Id,
    string Title,
    string Description,
    string Language,
    string Code,
    string Docs,
    bool IsPinned,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<string> Tags);

public sealed record SaveSnippetRequest(
    string? Title,
    string? Description,
    string? Language,
    string? Code,
    string? Docs,
    IReadOnlyList<string>? Tags,
    bool IsPinned);

public sealed record SnippetSearchRequest(
    string Query,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Languages);

public sealed record TagCountDto(string Name, int Count);
