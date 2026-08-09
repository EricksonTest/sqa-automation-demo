using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeniorQaAutomation.Framework.Ui;

public sealed class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public WaitHelper(IWebDriver driver, TimeSpan timeout)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _wait = new WebDriverWait(driver, timeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(200)
        };
        _wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    public void ForDocumentReady() => _wait.Until(_ =>
        string.Equals(
            ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState")?.ToString(),
            "complete",
            StringComparison.OrdinalIgnoreCase));

    public IWebElement UntilVisible(By locator) => _wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed ? element : null;
    })!;

    public IWebElement UntilClickable(By locator) => _wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed && element.Enabled ? element : null;
    })!;

    public bool UntilUrlContains(string value) => _wait.Until(driver =>
        driver.Url.Contains(value, StringComparison.OrdinalIgnoreCase));

    public bool Until(Func<IWebDriver, bool> condition) => _wait.Until(condition);
}
