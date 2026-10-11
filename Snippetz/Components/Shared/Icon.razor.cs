using Microsoft.AspNetCore.Components;

namespace Snippetz.Components.Shared;

public partial class Icon
{
    [Parameter]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public string? Class { get; set; }
}
