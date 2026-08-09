using System.Text.Json.Serialization;

namespace SeniorQaAutomation.Framework.Models;

internal sealed record AuthenticationRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);

internal sealed record AuthenticationResponse(
    [property: JsonPropertyName("token")] string Token);
