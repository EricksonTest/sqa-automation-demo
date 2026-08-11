using System.Text;
using OpenQA.Selenium;

namespace SeniorQaAutomation.Framework.Ui;

public static class BrowserDiagnostics
{
    public static string CaptureScreenshot(
        IWebDriver driver,
        string outputDirectory,
        string evidenceName)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Directory.CreateDirectory(outputDirectory);

        var path = Path.ChangeExtension(CreatePrefix(outputDirectory, evidenceName), ".png");
        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
        return path;
    }

    public static IReadOnlyList<string> Capture(IWebDriver driver, string outputDirectory, string testName)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Directory.CreateDirectory(outputDirectory);

        var prefix = CreatePrefix(outputDirectory, testName);
        var artifacts = new List<string>();

        TryCapture(Path.ChangeExtension(prefix, ".png"), artifacts, path =>
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path));
        TryCapture(Path.ChangeExtension(prefix, ".html"), artifacts, path =>
            File.WriteAllText(path, driver.PageSource));
        TryCapture(Path.ChangeExtension(prefix, ".txt"), artifacts, path =>
        {
            var details = new StringBuilder()
                .AppendLine($"URL: {driver.Url}")
                .AppendLine($"Title: {driver.Title}")
                .AppendLine($"Window: {driver.Manage().Window.Size}");

            try
            {
                foreach (var entry in driver.Manage().Logs.GetLog(LogType.Browser))
                {
                    details.AppendLine($"[{entry.Timestamp:O}] {entry.Level}: {entry.Message}");
                }
            }
            catch (WebDriverException exception)
            {
                details.AppendLine($"Browser console unavailable: {exception.Message}");
            }

            File.WriteAllText(path, details.ToString());
        });

        return artifacts;
    }

    private static string CreatePrefix(string outputDirectory, string name)
    {
        var safeName = string.Concat(name.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        return Path.Combine(
            outputDirectory,
            $"{safeName}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}");
    }

    private static void TryCapture(string path, ICollection<string> artifacts, Action<string> capture)
    {
        try
        {
            capture(path);
            artifacts.Add(path);
        }
        catch (Exception)
        {
            // Diagnostics must not hide the original test failure.
        }
    }
}
