namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public enum EEndpointParameterLocation
{
    /// <summary>
    /// Parameters that are appended to the URL.
    /// </summary>
    Query,

    /// <summary>
    /// Custom headers that are expected as part of the request.
    /// </summary>
    Header,

    /// <summary>
    /// Used together with Path Templating,
    /// where the parameter value is actually part of the operation's URL
    /// </summary>
    Path,

    /// <summary>
    /// Used to pass a specific cookie value to the API.
    /// </summary>
    Cookie,

    /// <summary>
    /// Parameters that are appended to the URL query string (OpenAPI 3.2+ only).
    /// </summary>
    QueryString,
}
