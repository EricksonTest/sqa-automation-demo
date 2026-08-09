namespace SeniorQaAutomation.Framework.Configuration;

public sealed record TestSettings(
    Uri BaseUrl,
    bool Headless,
    TimeSpan UiTimeout,
    string Browser,
    string AdminUsername,
    string AdminPassword)
{
    public static TestSettings FromEnvironment() => new(
        BaseUrl: ParseUri("TEST_BASE_URL", "https://automationintesting.online"),
        Headless: ParseBool("HEADLESS", true),
        UiTimeout: TimeSpan.FromSeconds(ParsePositiveInt("UI_TIMEOUT_SECONDS", 15)),
        Browser: Environment.GetEnvironmentVariable("BROWSER")?.Trim().ToLowerInvariant() ?? "chrome",
        AdminUsername: Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin",
        AdminPassword: Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "password");

    private static Uri ParseUri(string key, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(key) ?? fallback;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException($"{key} must be an absolute URL.");
    }

    private static bool ParseBool(string key, bool fallback) =>
        bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : fallback;

    private static int ParsePositiveInt(string key, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(key), out var value) && value > 0 ? value : fallback;
}
