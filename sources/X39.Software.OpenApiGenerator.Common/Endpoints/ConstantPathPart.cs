using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed class ConstantPathPart() : IPathPart
{
    [SetsRequiredMembers]
    public ConstantPathPart(string name, IEnumerable<IPathPart> children, IEnumerable<Endpoint>? endpoints)
        : this()
    {
        Name       = name;
        Children  = children.ToDictionary(e => e.Name);
        Endpoints = endpoints?.ToDictionary(e => e.Method);
    }

    public required string Name { get; init; }
    public required Dictionary<string, IPathPart> Children { get; init; }
    public required Dictionary<EHttpMethod, Endpoint>? Endpoints { get; set; }
}
