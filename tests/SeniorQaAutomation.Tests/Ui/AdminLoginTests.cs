using NUnit.Framework;
using SeniorQaAutomation.Framework.PageObjects;

namespace SeniorQaAutomation.Tests.Ui;

[TestFixture]
[Category("Ui")]
public sealed class AdminLoginTests : UiTestBase
{
    [Test]
    [Category("Smoke")]
    public void Admin_CanLogInWithConfiguredCredentials()
    {
        var roomsPage = new AdminLoginPage(Driver, Settings.BaseUrl, Settings.UiTimeout)
            .Open()
            .LoginSuccessfully(Settings.AdminUsername, Settings.AdminPassword);

        Assert.That(() => Driver.Url, Does.Contain("/admin/rooms"));
        Assert.That(() => roomsPage, Is.Not.Null);
    }

    [Test]
    [Category("Evidence")]
    public void AdminLogin_ShowsFeedbackForInvalidCredentials()
    {
        var loginPage = new AdminLoginPage(Driver, Settings.BaseUrl, Settings.UiTimeout)
            .Open()
            .LoginExpectingFailure("not-a-real-user", "not-a-real-password");

        Assert.That(() => loginPage.ErrorMessage, Does.Contain("Invalid credentials"));
        AttachEvidenceScreenshot("invalid-credentials-rejected");
    }
}
