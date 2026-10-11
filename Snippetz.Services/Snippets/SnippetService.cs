namespace Snippetz.Services.Snippets;

public sealed class SnippetService(
    ILogger<SnippetService> logger,
    IUserContextService userContext,
    IRepository<Snippet> snippets,
    IRepository<Tag> tags,
    IRepository<SnippetTag> snippetTags) : ISnippetService
{
    public async Task<Result<IReadOnlyList<SnippetDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<IReadOnlyList<SnippetDto>>.Forbidden();
        }

        var page = await snippets.FindAsync(new SearchOptions<Snippet>
        {
            Query = s => s.OwnerUserId == ownerUserId,
            OrderBy = q => q.OrderByDescending(s => s.UpdatedAt),
            CancellationToken = cancellationToken,
        });

        return Result<IReadOnlyList<SnippetDto>>.Success(await WithTagsAsync(page, cancellationToken));
    }

    public async Task<Result<SnippetDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var found = await FindOwnedAsync(id, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return found.To<SnippetDto>();
        }

        var dto = (await WithTagsAsync([found.Value], cancellationToken)).Single();
        return Result<SnippetDto>.Success(dto);
    }

    public async Task<Result<string>> GetRawCodeAsync(int id, CancellationToken cancellationToken = default)
    {
        var found = await FindOwnedAsync(id, cancellationToken);
        return found.IsSuccessStatus()
            ? Result<string>.Success(found.Value.Code)
            : found.To<string>();
    }

    public async Task<Result<IReadOnlyList<TagCountDto>>> GetTagCountsAsync(CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<IReadOnlyList<TagCountDto>>.Forbidden();
        }

        var ownedTags = await tags.FindAsync(new SearchOptions<Tag>
        {
            Query = t => t.OwnerUserId == ownerUserId,
            CancellationToken = cancellationToken,
        });

        if (ownedTags.Count == 0)
        {
            return Result<IReadOnlyList<TagCountDto>>.Success([]);
        }

        int[] tagIds = [.. ownedTags.Select(t => t.Id)];
        var links = await snippetTags.FindAsync(new SearchOptions<SnippetTag>
        {
            Query = link => tagIds.Contains(link.TagId),
            CancellationToken = cancellationToken,
        });

        var counts = links
            .GroupBy(link => link.TagId)
            .ToDictionary(group => group.Key, group => group.Count());

        var result = ownedTags
            .Select(tag => new TagCountDto(tag.Name, counts.GetValueOrDefault(tag.Id)))
            .Where(tag => tag.Count > 0)
            .OrderBy(tag => tag.Name, StringComparer.Ordinal)
            .ToList();

        return Result<IReadOnlyList<TagCountDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<SnippetDto>>> SearchAsync(
        SnippetSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<IReadOnlyList<SnippetDto>>.Forbidden();
        }

        string query = request.Query.Trim();
        string[] languages = [.. request.Languages.Select(language => language.ToLowerInvariant())];
        string[] wantedTags = [.. request.Tags.Select(tag => tag.ToLowerInvariant())];

        if (query.Length == 0 && languages.Length == 0 && wantedTags.Length == 0)
        {
            return Result<IReadOnlyList<SnippetDto>>.Success([]);
        }

        string needle = query.ToLowerInvariant();
        bool hasQuery = needle.Length > 0;
        bool hasLanguages = languages.Length > 0;

        var page = await snippets.FindAsync(new SearchOptions<Snippet>
        {
            Query = snippet => snippet.OwnerUserId == ownerUserId
                && (!hasQuery || snippet.Title.ToLower().Contains(needle) || snippet.Description.ToLower().Contains(needle))
                && (!hasLanguages || languages.Contains(snippet.Language.ToLower())),
            OrderBy = q => q.OrderByDescending(s => s.UpdatedAt),
            CancellationToken = cancellationToken,
        });

        var dtos = await WithTagsAsync(page, cancellationToken);

        if (wantedTags.Length > 0)
        {
            var wanted = new HashSet<string>(wantedTags, StringComparer.Ordinal);
            dtos = [.. dtos.Where(dto => dto.Tags.Any(tag => wanted.Contains(tag)))];
        }

        return Result<IReadOnlyList<SnippetDto>>.Success(dtos);
    }

    public async Task<Result<SnippetDto>> CreateAsync(
        SaveSnippetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<SnippetDto>.Forbidden();
        }

        var validated = Validate(request);
        if (!validated.IsSuccess)
        {
            return validated.To<SnippetDto>();
        }

        var (title, description, language, code, docs, parsedTags) = validated.Value;
        var now = DateTime.UtcNow;

        var saved = await snippets.InsertAsync(new Snippet
        {
            Title = title,
            Description = description,
            Language = language,
            Code = code,
            Docs = docs,
            IsPinned = request.IsPinned,
            OwnerUserId = ownerUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await ReplaceTagsAsync(saved.Id, ownerUserId, parsedTags, cancellationToken);

        logger.LogInformation("{User} created snippet {SnippetId} ({Language})", userContext.GetCurrentUserName(), saved.Id, language);

        return Result<SnippetDto>.Success(ToDto(saved, parsedTags));
    }

    public async Task<Result<SnippetDto>> UpdateAsync(
        int id,
        SaveSnippetRequest request,
        CancellationToken cancellationToken = default)
    {
        var found = await FindOwnedAsync(id, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return found.To<SnippetDto>();
        }

        var validated = Validate(request);
        if (!validated.IsSuccess)
        {
            return validated.To<SnippetDto>();
        }

        var snippet = found.Value;
        var (title, description, language, code, docs, parsedTags) = validated.Value;

        snippet.Title = title;
        snippet.Description = description;
        snippet.Language = language;
        snippet.Code = code;
        snippet.Docs = docs;
        snippet.IsPinned = request.IsPinned;
        snippet.UpdatedAt = DateTime.UtcNow;

        var saved = await snippets.UpdateAsync(snippet);
        await ReplaceTagsAsync(saved.Id, saved.OwnerUserId, parsedTags, cancellationToken);

        logger.LogInformation("{User} updated snippet {SnippetId}", userContext.GetCurrentUserName(), saved.Id);

        return Result<SnippetDto>.Success(ToDto(saved, parsedTags));
    }

    public async Task<Result<SnippetDto>> SetPinnedAsync(
        int id,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        var found = await FindOwnedAsync(id, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return found.To<SnippetDto>();
        }

        var snippet = found.Value;
        snippet.IsPinned = isPinned;
        var saved = await snippets.UpdateAsync(snippet);
        var dto = (await WithTagsAsync([saved], cancellationToken)).Single();

        return Result<SnippetDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var found = await FindOwnedAsync(id, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return ToVoid(found);
        }

        await snippets.DeleteAsync(found.Value);
        logger.LogInformation("{User} deleted snippet {SnippetId}", userContext.GetCurrentUserName(), id);

        return Result.Success();
    }

    private static Result ToVoid(Result<Snippet> result)
    {
        if (result.IsSuccessStatus())
        {
            return Result.Success();
        }

        var errors = result.Errors?.ToArray() ?? [];

        return result.Status switch
        {
            ResultStatus.NotFound => Result.NotFound(errors),
            ResultStatus.Forbidden => Result.Forbidden(errors),
            ResultStatus.Unauthorized => Result.Unauthorized(errors),
            _ => Result.Error(new ErrorList(errors)),
        };
    }

    private async Task<Result<Snippet>> FindOwnedAsync(int id, CancellationToken cancellationToken)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<Snippet>.Forbidden();
        }

        var snippet = await snippets.FindOneAsync(new SearchOptions<Snippet>
        {
            Query = s => s.Id == id && s.OwnerUserId == ownerUserId,
            CancellationToken = cancellationToken,
        });

        return snippet is null
            ? Result<Snippet>.NotFound($"Snippet {id} was not found.")
            : Result<Snippet>.Success(snippet);
    }

    private async Task ReplaceTagsAsync(
        int snippetId,
        string ownerUserId,
        IReadOnlyList<string> parsedTags,
        CancellationToken cancellationToken)
    {
        var existing = await tags.FindAsync(new SearchOptions<Tag>
        {
            Query = tag => tag.OwnerUserId == ownerUserId,
            CancellationToken = cancellationToken,
        });

        var byName = existing.ToDictionary(tag => tag.Name, StringComparer.Ordinal);
        var resolved = new List<Tag>(parsedTags.Count);

        foreach (string name in parsedTags)
        {
            if (name.Length > Constants.MaxLengths.TagName)
            {
                continue;
            }

            if (!byName.TryGetValue(name, out var tag))
            {
                tag = await tags.InsertAsync(new Tag
                {
                    Name = name,
                    OwnerUserId = ownerUserId,
                });
                byName[name] = tag;
            }

            resolved.Add(tag);
        }

        await snippetTags.DeleteAsync(link => link.SnippetId == snippetId);

        if (resolved.Count > 0)
        {
            await snippetTags.InsertAsync(resolved.Select(tag => new SnippetTag
            {
                SnippetId = snippetId,
                TagId = tag.Id,
            }));
        }
    }

    private async Task<IReadOnlyList<SnippetDto>> WithTagsAsync(
        IEnumerable<Snippet> source,
        CancellationToken cancellationToken)
    {
        var list = source.ToList();
        if (list.Count == 0)
        {
            return [];
        }

        int[] ids = [.. list.Select(snippet => snippet.Id)];
        var links = await snippetTags.FindAsync(new SearchOptions<SnippetTag>
        {
            Query = link => ids.Contains(link.SnippetId),
            CancellationToken = cancellationToken,
        });

        int[] tagIds = [.. links.Select(link => link.TagId).Distinct()];
        IEnumerable<Tag> tagRows = tagIds.Length == 0
            ? []
            : await tags.FindAsync(new SearchOptions<Tag>
            {
                Query = tag => tagIds.Contains(tag.Id),
                CancellationToken = cancellationToken,
            });

        var names = tagRows.ToDictionary(tag => tag.Id, tag => tag.Name);
        var grouped = links
            .GroupBy(link => link.SnippetId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(link => names.GetValueOrDefault(link.TagId))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Cast<string>()
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList());

        return list
            .Select(snippet => ToDto(snippet, grouped.GetValueOrDefault(snippet.Id) ?? []))
            .ToList();
    }

    private static Result<(string Title, string Description, string Language, string Code, string Docs, IReadOnlyList<string> Tags)> Validate(
        SaveSnippetRequest request)
    {
        var errors = new List<ValidationError>();

        string title = request.Title?.Trim() ?? string.Empty;
        string language = request.Language?.Trim() ?? string.Empty;
        string code = request.Code ?? string.Empty;
        string description = request.Description?.Trim() ?? string.Empty;
        string docs = request.Docs ?? string.Empty;

        if (title.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Title), "Give the snippet a title."));
        }
        else if (title.Length > Constants.MaxLengths.Title)
        {
            errors.Add(new ValidationError(nameof(request.Title), $"Titles can be at most {Constants.MaxLengths.Title} characters."));
        }

        if (language.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Language), "Say which language this is."));
        }
        else if (language.Length > Constants.MaxLengths.Language)
        {
            errors.Add(new ValidationError(nameof(request.Language), $"Language names can be at most {Constants.MaxLengths.Language} characters."));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add(new ValidationError(nameof(request.Code), "The snippet needs some code."));
        }

        if (errors.Count > 0)
        {
            return Result<(string, string, string, string, string, IReadOnlyList<string>)>.Invalid(errors);
        }

        var parsed = TagParser.Parse(request.Tags, language);
        return Result<(string, string, string, string, string, IReadOnlyList<string>)>.Success(
            (title, description, language, code.Replace("\r\n", "\n"), docs.Replace("\r\n", "\n"), parsed));
    }

    private static SnippetDto ToDto(Snippet snippet, IReadOnlyList<string> tagNames) =>
        new(
            snippet.Id,
            snippet.Title,
            snippet.Description,
            snippet.Language,
            snippet.Code,
            snippet.Docs,
            snippet.IsPinned,
            snippet.CreatedAt,
            snippet.UpdatedAt,
            tagNames);

    private string? ResolveOwner() => userContext.GetCurrentUserId();
}
