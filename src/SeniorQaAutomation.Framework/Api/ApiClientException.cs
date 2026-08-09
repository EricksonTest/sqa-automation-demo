using System.Net;

namespace SeniorQaAutomation.Framework.Api;

public sealed class ApiClientException : Exception
{
    public ApiClientException(string message, HttpStatusCode? statusCode = null, string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode? StatusCode { get; }

    public string? ResponseBody { get; }
}
