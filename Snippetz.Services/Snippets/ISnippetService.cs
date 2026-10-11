namespace Snippetz.Services.Snippets;

public interface ISnippetService
{
    Task<Result<IReadOnlyList<SnippetDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<SnippetDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<string>> GetRawCodeAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TagCountDto>>> GetTagCountsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SnippetDto>>> SearchAsync(SnippetSearchRequest request, CancellationToken cancellationToken = default);

    Task<Result<SnippetDto>> CreateAsync(SaveSnippetRequest request, CancellationToken cancellationToken = default);

    Task<Result<SnippetDto>> UpdateAsync(int id, SaveSnippetRequest request, CancellationToken cancellationToken = default);

    Task<Result<SnippetDto>> SetPinnedAsync(int id, bool isPinned, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
