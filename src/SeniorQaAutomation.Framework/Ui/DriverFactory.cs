using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using SeniorQaAutomation.Framework.Configuration;

namespace SeniorQaAutomation.Framework.Ui;

public static class DriverFactory
{
    public static IWebDriver Create(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        IWebDriver driver = settings.Browser switch
        {
            "chrome" => CreateChrome(settings.Headless),
            "firefox" => CreateFirefox(settings.Headless),
            _ => throw new ArgumentException(
                $"Unsupported browser '{settings.Browser}'. Supported values are 'chrome' and 'firefox'.",
                nameof(settings))
        };

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        driver.Manage().Timeouts().PageLoad = settings.UiTimeout;
        driver.Manage().Timeouts().AsynchronousJavaScript = settings.UiTimeout;

        if (!settings.Headless)
        {
            driver.Manage().Window.Maximize();
        }

        return driver;
    }

    private static ChromeDriver CreateChrome(bool headless)
    {
        var options = new ChromeOptions
        {
            PageLoadStrategy = PageLoadStrategy.Normal
        };

        options.AddArgument("--window-size=1440,1200");
        options.AddArgument("--disable-dev-shm-usage");
        options.SetLoggingPreference(LogType.Browser, LogLevel.All);

        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        return new ChromeDriver(options);
    }

    private static FirefoxDriver CreateFirefox(bool headless)
    {
        var options = new FirefoxOptions
        {
            PageLoadStrategy = PageLoadStrategy.Normal
        };

        options.AddArgument("--width=1440");
        options.AddArgument("--height=1200");

        if (headless)
        {
            options.AddArgument("-headless");
        }

        return new FirefoxDriver(options);
    }
}
