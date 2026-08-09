using NUnit.Framework;
using SeniorQaAutomation.Framework.PageObjects;

namespace SeniorQaAutomation.Tests.Ui;

[TestFixture]
[Category("Ui")]
[Category("Smoke")]
public sealed class HomeSmokeTests : UiTestBase
{
    [Test]
    public void HomePage_LoadsBrandingAndAvailableRooms()
    {
        var homePage = new HomePage(Driver, Settings.BaseUrl, Settings.UiTimeout).Open();

        Assert.Multiple(() =>
        {
            Assert.That(() => homePage.BrandName, Is.Not.Empty, $"Expected the configured hotel brand to be displayed.");
            Assert.That(() => homePage.HasRooms, Is.True, $"Expected at least one bookable room to be displayed.");
        });
    }
}
