namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed class EndpointResponse()
{
    public EndpointResponse(int statusCode, params IEnumerable<ModelReference> schemas) : this()
    {
        StatusCode = statusCode;
        Schemas    = schemas.ToDictionary(s => s.Name);
    }

    public required int StatusCode { get; init; }
    public required Dictionary<string, ModelReference> Schemas { get; init; }
}