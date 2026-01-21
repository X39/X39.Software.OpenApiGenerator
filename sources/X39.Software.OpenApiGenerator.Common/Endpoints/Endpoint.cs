using System.Diagnostics.CodeAnalysis;
using X39.Software.OpenApiGenerator.Common.Models;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed record Endpoint()
{
    [SetsRequiredMembers]
    public Endpoint(
        EHttpMethod method,
        IEnumerable<EndpointParameter> parameters,
        IEnumerable<KeyValuePair<MimeType, ModelReference>> requests,
        IEnumerable<EndpointResponse> responses
    )
        : this()
    {
        Method     = method;
        Parameters = parameters.ToDictionary(p => p.Name);
        Responses  = responses.ToDictionary(r => r.StatusCode);
        Requests   = requests.ToDictionary(r => r.Key, r => r.Value);
    }

    [SetsRequiredMembers]
    public Endpoint(
        EHttpMethod method,
        IEnumerable<EndpointParameter> parameters,
        IEnumerable<(MimeType mimeType, ModelReference modelReference)> requests,
        IEnumerable<EndpointResponse> responses
    )
        : this()
    {
        Method     = method;
        Parameters = parameters.ToDictionary(p => p.Name);
        Responses  = responses.ToDictionary(r => r.StatusCode);
        Requests   = requests.ToDictionary(r => r.mimeType, r => r.modelReference);
    }

    public required EHttpMethod Method { get; init; }
    public required Dictionary<string, EndpointParameter> Parameters { get; init; }
    public required Dictionary<MimeType, ModelReference> Requests { get; init; }
    public required Dictionary<int, EndpointResponse> Responses { get; init; }
}
