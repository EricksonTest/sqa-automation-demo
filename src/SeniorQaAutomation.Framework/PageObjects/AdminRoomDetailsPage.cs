using OpenQA.Selenium;

namespace SeniorQaAutomation.Framework.PageObjects;

public sealed class AdminRoomDetailsPage : PageObject
{
    private readonly int _roomId;

    public AdminRoomDetailsPage(IWebDriver driver, Uri baseUrl, TimeSpan timeout, int roomId)
        : base(driver, baseUrl, timeout)
    {
        _roomId = roomId;
    }

    public AdminRoomDetailsPage WaitUntilLoaded()
    {
        Wait.UntilVisible(By.XPath("//h2[starts-with(normalize-space(), 'Room:')]") );
        Wait.UntilVisible(By.XPath("//p[normalize-space()='First name']"));
        return this;
    }

    public bool HasBooking(string firstName, string lastName)
    {
        var row = By.XPath(
            $"//div[contains(concat(' ', normalize-space(@class), ' '), ' detail ') and contains(concat(' ', normalize-space(@class), ' '), ' booking-{_roomId} ')]" +
            $"[.//p[normalize-space()={XPathLiteral(firstName)}] and .//p[normalize-space()={XPathLiteral(lastName)}]]");

        return Wait.Until(driver => driver.FindElements(row).Any(element => element.Displayed));
    }

    private static string XPathLiteral(string value)
    {
        if (!value.Contains('\''))
        {
            return $"'{value}'";
        }

        if (!value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        return $"concat('{value.Replace("'", "', \"'\", '", StringComparison.Ordinal)}')";
    }
}
