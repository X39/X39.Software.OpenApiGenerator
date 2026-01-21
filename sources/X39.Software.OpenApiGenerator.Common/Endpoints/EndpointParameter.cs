namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed class EndpointParameter
{
    public required string Name { get; init; }
    public required ModelReference Schema { get; init; }
    public required EEndpointParameterLocation Location { get; init; }
    public required EEndpointModifier Modifier { get; init; }
}
