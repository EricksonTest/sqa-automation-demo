using OpenQA.Selenium;

namespace SeniorQaAutomation.Framework.PageObjects;

public sealed class AdminRoomsPage : PageObject
{
    private static readonly By RoomListings = By.CssSelector("[data-testid='roomlisting'][data-type='room']");
    private static readonly By LogoutButton = By.XPath("//button[normalize-space()='Logout']");

    public AdminRoomsPage(IWebDriver driver, Uri baseUrl, TimeSpan timeout)
        : base(driver, baseUrl, timeout)
    {
    }

    public AdminRoomsPage WaitUntilLoaded()
    {
        Wait.UntilVisible(LogoutButton);
        Wait.UntilVisible(RoomListings);
        return this;
    }

    public AdminRoomDetailsPage OpenRoom(int roomId)
    {
        NavigateTo($"/admin/room/{roomId}");
        return new AdminRoomDetailsPage(Driver, BaseUrl, Timeout, roomId).WaitUntilLoaded();
    }
}
