using OpenQA.Selenium;

namespace SeniorQaAutomation.Framework.PageObjects;

public sealed class AdminLoginPage : PageObject
{
    private static readonly By Username = By.Id("username");
    private static readonly By Password = By.Id("password");
    private static readonly By LoginButton = By.Id("doLogin");
    private static readonly By LoginError = By.CssSelector("[role='alert']");

    public AdminLoginPage(IWebDriver driver, Uri baseUrl, TimeSpan timeout)
        : base(driver, baseUrl, timeout)
    {
    }

    public AdminLoginPage Open()
    {
        NavigateTo("/admin");
        Wait.UntilVisible(Username);
        return this;
    }

    public AdminRoomsPage LoginSuccessfully(string username, string password)
    {
        Submit(username, password);
        Wait.UntilUrlContains("/admin/rooms");
        return new AdminRoomsPage(Driver, BaseUrl, Timeout).WaitUntilLoaded();
    }

    public AdminLoginPage LoginExpectingFailure(string username, string password)
    {
        Submit(username, password);
        Wait.UntilVisible(LoginError);
        return this;
    }

    public string ErrorMessage => Wait.UntilVisible(LoginError).Text.Trim();

    private void Submit(string username, string password)
    {
        var usernameInput = Wait.UntilVisible(Username);
        usernameInput.Clear();
        usernameInput.SendKeys(username);

        var passwordInput = Wait.UntilVisible(Password);
        passwordInput.Clear();
        passwordInput.SendKeys(password);
        Wait.UntilClickable(LoginButton).Click();
    }
}
