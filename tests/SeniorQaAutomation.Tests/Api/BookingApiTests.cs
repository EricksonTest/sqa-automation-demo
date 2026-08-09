using System.Net;
using System.Text.Json;
using NUnit.Framework;
using SeniorQaAutomation.Framework.Api;
using SeniorQaAutomation.Framework.Configuration;
using SeniorQaAutomation.Framework.Models;
using SeniorQaAutomation.Framework.TestData;

namespace SeniorQaAutomation.Tests.Api;

[TestFixture]
[NonParallelizable]
[Category("Api")]
public sealed class BookingApiTests
{
    private readonly List<int> _bookingsToDelete = [];
    private HttpClient _httpClient = null!;
    private BookingApiClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _client = new BookingApiClient(_httpClient, TestSettings.FromEnvironment());
    }

    [TearDown]
    public async Task TearDown()
    {
        foreach (var bookingId in _bookingsToDelete)
        {
            try
            {
                await _client.DeleteBookingAsync(bookingId);
            }
            catch (ApiClientException exception)
            {
                TestContext.Progress.WriteLine($"Cleanup failed for booking {bookingId}: {exception.Message}");
            }
        }

        _bookingsToDelete.Clear();
        _httpClient.Dispose();
    }

    [Test]
    [Category("Smoke")]
    public async Task CreateBooking_WithValidData_ReturnsCreatedBooking()
    {
        var requested = BookingBuilder.CreateValid().Build();

        var response = await _client.CreateBookingAsync(requested);
        var created = response.EnsureSuccess();
        TrackForCleanup(created.BookingId);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.BookingId, Is.GreaterThan(0));
            Assert.That(created.Booking.RoomId, Is.EqualTo(requested.RoomId));
            Assert.That(created.Booking.FirstName, Is.EqualTo(requested.FirstName));
            Assert.That(created.Booking.LastName, Is.EqualTo(requested.LastName));
            Assert.That(created.Booking.BookingDates, Is.EqualTo(requested.BookingDates));
        });
    }

    [Test]
    [Category("Smoke")]
    [Category("Regression")]
    public async Task GetBooking_AfterCreation_ReturnsPersistedBooking()
    {
        var requested = BookingBuilder.CreateValid().WithDeposit(false).Build();
        var created = await CreateAndTrackAsync(requested);

        var response = await _client.GetBookingAsync(created.BookingId);
        var retrieved = response.EnsureSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(retrieved.BookingId, Is.EqualTo(created.BookingId));
            Assert.That(retrieved.RoomId, Is.EqualTo(requested.RoomId));
            Assert.That(retrieved.FirstName, Is.EqualTo(requested.FirstName));
            Assert.That(retrieved.LastName, Is.EqualTo(requested.LastName));
            Assert.That(retrieved.DepositPaid, Is.False);
            Assert.That(retrieved.BookingDates, Is.EqualTo(requested.BookingDates));
        });
    }

    [Test]
    [Category("Regression")]
    public async Task UpdateBooking_WithValidChanges_PersistsChanges()
    {
        var requested = BookingBuilder.CreateValid().Build();
        var created = await CreateAndTrackAsync(requested);
        var updatedRequest = requested with
        {
            BookingId = created.BookingId,
            FirstName = "UpdatedGuest",
            DepositPaid = !requested.DepositPaid
        };

        var updateResponse = await _client.UpdateBookingAsync(created.BookingId, updatedRequest);
        var updated = updateResponse.EnsureSuccess();
        var retrieved = (await _client.GetBookingAsync(created.BookingId)).EnsureSuccess();

        Assert.Multiple(() =>
        {
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updated.BookingId, Is.EqualTo(created.BookingId));
            Assert.That(updated.Booking.FirstName, Is.EqualTo("UpdatedGuest"));
            Assert.That(retrieved.FirstName, Is.EqualTo("UpdatedGuest"));
            Assert.That(retrieved.DepositPaid, Is.EqualTo(updatedRequest.DepositPaid));
        });
    }

    [Test]
    [Category("Regression")]
    public async Task DeleteBooking_ForExistingBooking_ReturnsAcceptedAndRemovesBooking()
    {
        var created = await CreateAndTrackAsync(BookingBuilder.CreateValid().Build());

        var deleteResponse = await _client.DeleteBookingAsync(created.BookingId);
        _bookingsToDelete.Remove(created.BookingId);
        var getResponse = await _client.GetBookingAsync(created.BookingId);

        Assert.Multiple(() =>
        {
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    [Category("Contract")]
    public async Task CreateBooking_ResponseMatchesDocumentedContract()
    {
        var createdResponse = await _client.CreateBookingAsync(BookingBuilder.CreateValid().Build());
        var created = createdResponse.EnsureSuccess();
        TrackForCleanup(created.BookingId);

        using var json = JsonDocument.Parse(createdResponse.RawBody);
        var root = json.RootElement;
        var booking = root.TryGetProperty("booking", out var nestedBooking) ? nestedBooking : root;

        Assert.Multiple(() =>
        {
            Assert.That(root.TryGetProperty("bookingid", out var id), Is.True);
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.Number));
            Assert.That(booking.GetProperty("roomid").ValueKind, Is.EqualTo(JsonValueKind.Number));
            Assert.That(booking.GetProperty("firstname").ValueKind, Is.EqualTo(JsonValueKind.String));
            Assert.That(booking.GetProperty("lastname").ValueKind, Is.EqualTo(JsonValueKind.String));
            Assert.That(booking.GetProperty("depositpaid").ValueKind, Is.EqualTo(JsonValueKind.True).Or.EqualTo(JsonValueKind.False));
            Assert.That(booking.GetProperty("bookingdates").GetProperty("checkin").ValueKind,
                Is.EqualTo(JsonValueKind.String));
        });
    }

    [Test]
    [Category("Contract")]
    [Category("Regression")]
    public async Task CreateBooking_WithBlankFirstName_ReturnsValidationErrors()
    {
        var invalid = BookingBuilder.CreateValid().Build() with { FirstName = string.Empty };

        var response = await _client.CreateBookingAsync(invalid);
        var error = JsonSerializer.Deserialize<ApiError>(response.RawBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error, Is.Not.Null);
            Assert.That(error!.Errors.Concat(error.FieldErrors),
                Has.Some.Contains("Firstname should not be blank"));
        });
    }

    [Test]
    [Category("Regression")]
    public async Task CreateBooking_WithConflictingDates_ReturnsConflict()
    {
        var requested = BookingBuilder.CreateValid().Build();
        var first = await CreateAndTrackAsync(requested);

        var duplicateResponse = await _client.CreateBookingAsync(requested);

        Assert.Multiple(() =>
        {
            Assert.That(first.BookingId, Is.GreaterThan(0));
            Assert.That(duplicateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }

    [Test]
    [Category("Regression")]
    public async Task CreateBooking_WhenCheckoutPrecedesCheckin_ReturnsConflict()
    {
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(120);
        var invalid = BookingBuilder.CreateValid()
            .WithStay(checkIn, checkIn.AddDays(-1))
            .Build();

        var response = await _client.CreateBookingAsync(invalid);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    [Category("Contract")]
    public async Task GetBooking_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetBookingAsync(int.MaxValue);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private async Task<CreatedBooking> CreateAndTrackAsync(Booking booking)
    {
        var created = (await _client.CreateBookingAsync(booking)).EnsureSuccess();
        TrackForCleanup(created.BookingId);
        return created;
    }

    private void TrackForCleanup(int bookingId) => _bookingsToDelete.Add(bookingId);
}
