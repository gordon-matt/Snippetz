using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Snippetz.Controllers;

/// <summary>
/// Plain-text copy of a snippet, so a raw URL can be pasted into a script or a curl command.
/// </summary>
[Authorize]
[Route("api/snippets")]
public sealed class SnippetsController(ISnippetService snippets) : Controller
{
    [HttpGet("raw/{id:int}")]
    public async Task<IActionResult> Raw(int id, CancellationToken cancellationToken)
    {
        var result = await snippets.GetRawCodeAsync(id, cancellationToken);

        return !result.IsSuccess
            ? result.Status == ResultStatus.NotFound
                ? NotFound()
                : StatusCode(StatusCodes.Status403Forbidden)
            : Content(result.Value, "text/plain");
    }
}