using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace Snippetz.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private SidebarState Sidebar { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthState { get; set; } = default!;

    [Inject]
    private IAuthProviderService Auth { get; set; } = default!;

    [Inject]
    private ISnippetService Snippets { get; set; } = default!;

    private bool sidebarOpen;
    private int sidebarRefresh;
    private string theme = "light";
    private string searchText = string.Empty;
    private string userName = string.Empty;
    private bool isAdmin;
    private bool showSignOut;

    private SidebarState sidebar => Sidebar;

    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }

    protected override void OnParametersSet()
    {
        searchText = Query ?? string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        Sidebar.Changed += OnSidebarChanged;
        Navigation.LocationChanged += OnLocationChanged;
        await RefreshSidebarAsync();

        var state = await AuthState.GetAuthenticationStateAsync();
        userName = state.User.Identity?.Name ?? string.Empty;
        isAdmin = state.User.IsInRole(Constants.Roles.Administrator);
        showSignOut = Auth.Provider != AuthProvider.None;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            theme = await Js.InvokeAsync<string>("snippetz.getTheme");
            StateHasChanged();
        }
    }

    private void OnSidebarChanged() => InvokeAsync(StateHasChanged);

    private void ToggleSidebar() => sidebarOpen = !sidebarOpen;

    private async Task ToggleThemeAsync()
    {
        theme = theme == "dark" ? "light" : "dark";
        await Js.InvokeVoidAsync("snippetz.setTheme", theme);
    }

    private void SearchAsync()
    {
        string query = searchText.Trim();
        Navigation.NavigateTo(query.Length == 0 ? "/" : $"/?q={Uri.EscapeDataString(query)}");
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) =>
        _ = InvokeAsync(RefreshSidebarAsync);

    private async Task RefreshSidebarAsync()
    {
        int version = ++sidebarRefresh;
        var count = await Snippets.CountAsync();
        var tags = await Snippets.GetTagCountsAsync();

        if (version != sidebarRefresh)
        {
            return;
        }

        var query = QueryHelpers.ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);
        string path = Navigation.ToBaseRelativePath(Navigation.Uri).Split('?', '#')[0].Trim('/');
        bool onList = path is "" or "snippets";
        string? tag = onList && query.TryGetValue("tag", out var tagValue) ? tagValue.ToString() : null;
        bool searching = onList && query.TryGetValue("q", out var q) && !string.IsNullOrWhiteSpace(q);

        Sidebar.Set(
            count.IsSuccess ? count.Value : 0,
            tags.IsSuccess ? tags.Value : [],
            string.IsNullOrWhiteSpace(tag) ? null : tag.Trim().ToLowerInvariant(),
            onList && !searching && string.IsNullOrWhiteSpace(tag));
    }

    public void Dispose()
    {
        Sidebar.Changed -= OnSidebarChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
