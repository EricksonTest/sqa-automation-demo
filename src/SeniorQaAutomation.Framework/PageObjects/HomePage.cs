using OpenQA.Selenium;

namespace SeniorQaAutomation.Framework.PageObjects;

public sealed class HomePage : PageObject
{
    private static readonly By Brand = By.CssSelector("nav .navbar-brand");
    private static readonly By RoomsHeading = By.XPath("//section[@id='rooms']//h2[normalize-space()='Our Rooms']");
    private static readonly By ContactName = By.CssSelector("[data-testid='ContactName']");
    private static readonly By ContactSubmit = By.CssSelector("section#contact button[type='button']");
    private static readonly By ContactErrors = By.CssSelector("section#contact .alert-danger p");

    public HomePage(IWebDriver driver, Uri baseUrl, TimeSpan timeout)
        : base(driver, baseUrl, timeout)
    {
    }

    public HomePage Open()
    {
        NavigateTo("/");
        Wait.UntilVisible(Brand);
        Wait.UntilVisible(RoomsHeading);
        Wait.Until(driver => driver.FindElements(By.CssSelector("section#rooms .room-card"))
            .Any(element => element.Displayed));
        return this;
    }

    public string BrandName => Wait.UntilVisible(Brand).Text.Trim();

    public bool HasRooms => Driver.FindElements(By.CssSelector("section#rooms .room-card")).Count > 0;

    public HomePage SubmitEmptyContactForm()
    {
        var name = Wait.UntilVisible(ContactName);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", name);
        Wait.UntilClickable(ContactSubmit).Click();
        var firstError = Wait.UntilVisible(ContactErrors);
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block: 'center'});",
            firstError);
        return this;
    }

    public IReadOnlyList<string> ContactValidationErrors => Driver.FindElements(ContactErrors)
        .Select(element => element.Text.Trim())
        .Where(text => text.Length > 0)
        .ToArray();
}
