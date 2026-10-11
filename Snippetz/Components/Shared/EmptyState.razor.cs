using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Shared;

public partial class EmptyState
{
    [Parameter]
    public string IconName { get; set; } = "code";

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
