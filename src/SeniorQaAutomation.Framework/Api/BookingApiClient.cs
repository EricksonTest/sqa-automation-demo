using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SeniorQaAutomation.Framework.Configuration;
using SeniorQaAutomation.Framework.Models;

namespace SeniorQaAutomation.Framework.Api;

public sealed class BookingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly TestSettings _settings;
    private readonly Uri _baseUri;
    private readonly SemaphoreSlim _authenticationLock = new(1, 1);
    private string? _token;

    public BookingApiClient(HttpClient httpClient, TestSettings settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _baseUri = new Uri(settings.BaseUrl.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
    }

    public async Task<ApiResponse<CreatedBooking>> CreateBookingAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<CreatedBooking>(
            HttpMethod.Post, "api/booking", booking, null, cancellationToken);

        // The hosted API currently returns a flat Booking, while some published
        // versions return { bookingid, booking }. Normalize both into one model.
        if (response.IsSuccess && response.Data is { Booking: null })
        {
            var flatBooking = JsonSerializer.Deserialize<Booking>(response.RawBody, JsonOptions);
            if (flatBooking is not null)
            {
                response = response with
                {
                    Data = new CreatedBooking(flatBooking.BookingId, flatBooking)
                };
            }
        }

        return response;
    }

    public Task<ApiResponse<Booking>> GetBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default) =>
        SendAuthenticatedAsync<Booking>(HttpMethod.Get, $"api/booking/{bookingId}", null, cancellationToken);

    public Task<ApiResponse<CreatedBooking>> UpdateBookingAsync(
        int bookingId,
        Booking booking,
        CancellationToken cancellationToken = default) =>
        SendAuthenticatedAsync<CreatedBooking>(HttpMethod.Put, $"api/booking/{bookingId}", booking, cancellationToken);

    public Task<ApiResponse> DeleteBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default) =>
        SendAuthenticatedAsync(HttpMethod.Delete, $"api/booking/{bookingId}", null, cancellationToken);

    private async Task<ApiResponse<T>> SendAuthenticatedAsync<T>(
        HttpMethod method,
        string relativeUri,
        object? body,
        CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        var response = await SendAsync<T>(method, relativeUri, body, token, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Forbidden)
        {
            return response;
        }

        _token = null;
        token = await GetTokenAsync(cancellationToken);
        return await SendAsync<T>(method, relativeUri, body, token, cancellationToken);
    }

    private async Task<ApiResponse> SendAuthenticatedAsync(
        HttpMethod method,
        string relativeUri,
        object? body,
        CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        var response = await SendAsync(method, relativeUri, body, token, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Forbidden)
        {
            return response;
        }

        _token = null;
        token = await GetTokenAsync(cancellationToken);
        return await SendAsync(method, relativeUri, body, token, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_token))
        {
            return _token;
        }

        await _authenticationLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_token))
            {
                return _token;
            }

            var authentication = new AuthenticationRequest(_settings.AdminUsername, _settings.AdminPassword);
            var response = await SendAsync<AuthenticationResponse>(
                HttpMethod.Post,
                "api/auth/login",
                authentication,
                null,
                cancellationToken);

            var authenticated = response.EnsureSuccess();
            if (string.IsNullOrWhiteSpace(authenticated.Token))
            {
                throw new ApiClientException("Authentication succeeded but no token was returned.",
                    response.StatusCode, response.RawBody);
            }

            _token = authenticated.Token;
            return _token;
        }
        finally
        {
            _authenticationLock.Release();
        }
    }

    private async Task<ApiResponse<T>> SendAsync<T>(
        HttpMethod method,
        string relativeUri,
        object? body,
        string? token,
        CancellationToken cancellationToken)
    {
        var response = await SendRequestAsync(method, relativeUri, body, token, cancellationToken);
        T? data = default;

        if (!string.IsNullOrWhiteSpace(response.RawBody))
        {
            try
            {
                data = JsonSerializer.Deserialize<T>(response.RawBody, JsonOptions);
            }
            catch (JsonException) when (!response.IsSuccess)
            {
                // An error response may legitimately use a different schema.
            }
            catch (JsonException exception)
            {
                throw new ApiClientException(
                    $"{response.Method} {response.RequestUri} returned invalid JSON for {typeof(T).Name}. " +
                    $"Response: {ApiResponse.DisplayBody(response.RawBody)}",
                    response.StatusCode,
                    response.RawBody,
                    exception);
            }
        }

        return new ApiResponse<T>(response.StatusCode, response.RawBody, response.Method, response.RequestUri, data);
    }

    private async Task<ApiResponse> SendAsync(
        HttpMethod method,
        string relativeUri,
        object? body,
        string? token,
        CancellationToken cancellationToken) =>
        await SendRequestAsync(method, relativeUri, body, token, cancellationToken);

    private async Task<ApiResponse> SendRequestAsync(
        HttpMethod method,
        string relativeUri,
        object? body,
        string? token,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(_baseUri, relativeUri);
        using var request = new HttpRequestMessage(method, requestUri);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Add("Cookie", $"token={token}");
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ApiResponse(response.StatusCode, rawBody, method.Method, requestUri);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new ApiClientException(
                $"{method.Method} {requestUri} failed before a response was received: {exception.Message}",
                innerException: exception);
        }
    }
}
