using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public record ParameterPathPart() : IPathPart
{
    public ParameterPathPart(
        string Key,
        IEnumerable<IPathPart> Children,
        IEnumerable<ModelReference> ModelReferences,
        IEnumerable<Endpoint>? Endpoints
    ) : this()
    {
        this.Key             = Key;
        this.Children        = Children.ToImmutableList();
        this.ModelReferences = ModelReferences.ToImmutableList();
        this.Endpoints       = Endpoints?.ToImmutableDictionary(e => e.Method);
    }

    public required string Key { get; init; }
    public required ImmutableList<IPathPart> Children { get; init; }
    public required ImmutableList<ModelReference> ModelReferences { get; init; }
    public required ImmutableDictionary<EHttpMethod, Endpoint>? Endpoints { get; init; }
}
