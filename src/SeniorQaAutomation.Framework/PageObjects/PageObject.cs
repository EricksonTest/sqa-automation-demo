using OpenQA.Selenium;
using SeniorQaAutomation.Framework.Ui;

namespace SeniorQaAutomation.Framework.PageObjects;

public abstract class PageObject
{
    protected PageObject(IWebDriver driver, Uri baseUrl, TimeSpan timeout)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Timeout = timeout;
        Wait = new WaitHelper(driver, timeout);
    }

    protected IWebDriver Driver { get; }

    protected Uri BaseUrl { get; }

    protected TimeSpan Timeout { get; }

    protected WaitHelper Wait { get; }

    protected void NavigateTo(string relativePath)
    {
        Driver.Navigate().GoToUrl(new Uri(BaseUrl, relativePath));
        Wait.ForDocumentReady();
    }
}
