using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public record ConstantPathPart() : IPathPart
{
    public ConstantPathPart(string Key, IEnumerable<IPathPart> Children, IEnumerable<Endpoint>? Endpoints)
        : this()
    {
        this.Key       = Key;
        this.Children  = Children.ToImmutableList();
        this.Endpoints = Endpoints?.ToImmutableDictionary(e => e.Method);
    }

    public required string Key { get; init; }
    public required ImmutableList<IPathPart> Children { get; init; }
    public required ImmutableDictionary<EHttpMethod, Endpoint>? Endpoints { get; init; }
}
