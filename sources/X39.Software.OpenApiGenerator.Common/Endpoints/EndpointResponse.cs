using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed class EndpointResponse()
{
    [SetsRequiredMembers]
    public EndpointResponse(int statusCode, params IEnumerable<KeyValuePair<MimeType, ModelReference>> schemas)
        : this()
    {
        StatusCode = statusCode;
        Schemas    = schemas.ToDictionary(s => s.Key, s => s.Value);
    }

    public required int StatusCode { get; init; }
    public required Dictionary<MimeType, ModelReference> Schemas { get; init; }
}
