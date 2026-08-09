using System.Text.Json.Serialization;

namespace SeniorQaAutomation.Framework.Models;

public sealed record BookingDates(
    [property: JsonPropertyName("checkin")] DateOnly CheckIn,
    [property: JsonPropertyName("checkout")] DateOnly CheckOut);
