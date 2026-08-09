using System.Text.Json.Serialization;

namespace SeniorQaAutomation.Framework.Models;

public sealed record CreatedBooking(
    [property: JsonPropertyName("bookingid")] int BookingId,
    [property: JsonPropertyName("booking")] Booking Booking);
