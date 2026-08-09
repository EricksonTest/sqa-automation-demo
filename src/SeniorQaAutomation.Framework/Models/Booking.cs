using System.Text.Json.Serialization;

namespace SeniorQaAutomation.Framework.Models;

public sealed record Booking
{
    [JsonPropertyName("bookingid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int BookingId { get; init; }

    [JsonPropertyName("roomid")]
    public int RoomId { get; init; }

    [JsonPropertyName("firstname")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("depositpaid")]
    public bool DepositPaid { get; init; }

    [JsonPropertyName("bookingdates")]
    public BookingDates BookingDates { get; init; } = new(default, default);

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    [JsonPropertyName("phone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; init; }
}
