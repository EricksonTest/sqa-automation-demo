using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using SeniorQaAutomation.Framework.Configuration;
using SeniorQaAutomation.Framework.Ui;

namespace SeniorQaAutomation.Tests.Ui;

public abstract class UiTestBase
{
    protected IWebDriver Driver { get; private set; } = null!;

    protected TestSettings Settings { get; private set; } = null!;

    protected void AttachEvidenceScreenshot(string label)
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("TEST_ARTIFACTS_DIR");
        var outputDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
            ? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts", "evidence")
            : Path.GetFullPath(configuredDirectory);
        var evidenceName = $"{TestContext.CurrentContext.Test.Name}-{label}";
        var screenshot = BrowserDiagnostics.CaptureScreenshot(Driver, outputDirectory, evidenceName);

        TestContext.AddTestAttachment(screenshot, $"Expected UI state: {label}");
        TestContext.Progress.WriteLine($"Evidence screenshot: {screenshot}");
    }

    [SetUp]
    public void StartBrowser()
    {
        Settings = TestSettings.FromEnvironment();
        Driver = DriverFactory.Create(Settings);
    }

    [TearDown]
    public void StopBrowser()
    {
        if (Driver is null)
        {
            return;
        }

        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed)
        {
            var configuredDirectory = Environment.GetEnvironmentVariable("TEST_ARTIFACTS_DIR");
            var outputDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
                ? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts", "ui")
                : Path.GetFullPath(configuredDirectory);

            foreach (var artifact in BrowserDiagnostics.Capture(
                         Driver,
                         outputDirectory,
                         TestContext.CurrentContext.Test.Name))
            {
                TestContext.AddTestAttachment(artifact);
            }
        }

        try
        {
            Driver.Quit();
        }
        finally
        {
            Driver.Dispose();
        }
    }
}
