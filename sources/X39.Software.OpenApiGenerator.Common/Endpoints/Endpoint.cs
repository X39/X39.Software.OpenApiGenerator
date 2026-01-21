using X39.Software.OpenApiGenerator.Common.Models;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed record Endpoint()
{
    public Endpoint(EHttpMethod method, IEnumerable<EndpointParameter> parameters, IEnumerable<EndpointResponse> responses)
        : this()
    {
        Method     = method;
        Parameters = parameters.ToDictionary(p => p.Name);
        Responses  = responses.ToDictionary(r => r.StatusCode);
    }
    public required EHttpMethod Method { get; init; }
    public required Dictionary<string, EndpointParameter> Parameters { get; init; }
    public required Dictionary<int, EndpointResponse> Responses { get; init; }
}
