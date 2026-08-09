using NUnit.Framework;
using SeniorQaAutomation.Framework.Api;
using SeniorQaAutomation.Framework.Configuration;
using SeniorQaAutomation.Framework.PageObjects;
using SeniorQaAutomation.Framework.TestData;
using SeniorQaAutomation.Framework.Ui;

namespace SeniorQaAutomation.Tests.E2E;

[TestFixture]
[Category("E2E")]
public sealed class ApiToUiBookingTests
{
    [Test]
    public async Task BookingCreatedThroughApi_IsVisibleForSameGuestInAdminUi()
    {
        var settings = TestSettings.FromEnvironment();
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var booking = BookingBuilder.CreateValid()
            .WithGuest($"E2E{suffix}", $"Guest{suffix}")
            .WithRoom(1)
            .WithDeposit(true)
            .Build();

        using var httpClient = new HttpClient();
        var api = new BookingApiClient(httpClient, settings);
        var createResponse = await api.CreateBookingAsync(booking);
        var created = createResponse.EnsureSuccess();

        using var driver = DriverFactory.Create(settings);

        try
        {
            new AdminLoginPage(driver, settings.BaseUrl, settings.UiTimeout)
                .Open()
                .LoginSuccessfully(settings.AdminUsername, settings.AdminPassword)
                .OpenRoom(booking.RoomId)
                .HasBooking(booking.FirstName, booking.LastName)
                .ShouldBeTrue("The API-created guest should appear against the same room in the admin UI.");
        }
        catch
        {
            var outputDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts", "e2e");
            foreach (var artifact in BrowserDiagnostics.Capture(
                         driver,
                         outputDirectory,
                         TestContext.CurrentContext.Test.Name))
            {
                TestContext.AddTestAttachment(artifact);
            }

            throw;
        }
        finally
        {
            var deleteResponse = await api.DeleteBookingAsync(created.BookingId);
            if (!deleteResponse.IsSuccess)
            {
                TestContext.Progress.WriteLine(
                    $"Cleanup failed for booking {created.BookingId}: HTTP {(int)deleteResponse.StatusCode}.");
            }
        }
    }
}

internal static class BooleanAssertionExtensions
{
    public static void ShouldBeTrue(this bool actual, string message) => Assert.That(() => actual, Is.True, $"{message}");
}
