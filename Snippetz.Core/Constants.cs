namespace Snippetz;

public static class Constants
{
    /// <summary>Browser <c>localStorage</c> key for the light/dark theme.</summary>
    public const string ClientThemeLocalStorageKey = "snippetz.theme";

    public static class MaxLengths
    {
        public const int Title = 256;
        public const int Language = 64;
        public const int TagName = 64;
        public const int UserId = 450;
    }

    public static class Schemas
    {
        public const string App = "app";
    }

    public static class ServerSettingDefaults
    {
        public const string SiteName = "Snippetz";
    }

    /// <summary>
    /// Languages offered in the editor. Any other value can still be typed; this list is only a prompt.
    /// </summary>
    public static readonly IReadOnlyList<string> Languages =
    [
        "bash", "c", "cpp", "csharp", "css", "dart", "dockerfile", "fsharp", "go", "html",
        "java", "javascript", "json", "kotlin", "lua", "markdown", "php", "plaintext",
        "powershell", "python", "ruby", "rust", "sql", "swift", "toml", "typescript",
        "xml", "yaml"
    ];
}