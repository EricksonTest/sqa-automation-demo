using System.Net;

namespace SeniorQaAutomation.Framework.Api;

public record ApiResponse(HttpStatusCode StatusCode, string RawBody, string Method, Uri RequestUri)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    public void EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw new ApiClientException(
                $"{Method} {RequestUri} returned {(int)StatusCode} ({StatusCode}). Response: {DisplayBody(RawBody)}",
                StatusCode,
                RawBody);
        }
    }

    internal static string DisplayBody(string body) =>
        string.IsNullOrWhiteSpace(body) ? "<empty>" : body;
}

public sealed record ApiResponse<T>(
    HttpStatusCode StatusCode,
    string RawBody,
    string Method,
    Uri RequestUri,
    T? Data)
    : ApiResponse(StatusCode, RawBody, Method, RequestUri)
{
    public new T EnsureSuccess()
    {
        base.EnsureSuccess();
        return Data ?? throw new ApiClientException(
            $"{Method} {RequestUri} returned success but its response body did not contain a valid {typeof(T).Name}. " +
            $"Response: {DisplayBody(RawBody)}",
            StatusCode,
            RawBody);
    }
}
