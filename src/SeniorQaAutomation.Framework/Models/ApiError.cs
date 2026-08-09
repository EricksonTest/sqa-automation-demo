using System.Text.Json.Serialization;

namespace SeniorQaAutomation.Framework.Models;

public sealed record ApiError
{
    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("fieldErrors")]
    public IReadOnlyList<string> FieldErrors { get; init; } = [];

    [JsonPropertyName("errors")]
    public IReadOnlyList<string> Errors { get; init; } = [];
}
