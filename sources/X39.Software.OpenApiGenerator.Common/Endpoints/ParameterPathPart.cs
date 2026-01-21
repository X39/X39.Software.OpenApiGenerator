using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public sealed class ParameterPathPart() : IPathPart
{
    [SetsRequiredMembers]
    public ParameterPathPart(
        string name,
        IEnumerable<IPathPart> children,
        ModelReference? modelReference,
        IEnumerable<Endpoint>? endpoints
    )
        : this()
    {
        Name            = name;
        Children        = children.ToDictionary(e => e.Name);
        ModelReference = modelReference;
        Endpoints       = endpoints?.ToDictionary(e => e.Method);
    }

    public required string Name { get; init; }
    public required Dictionary<string, IPathPart> Children { get; init; }
    public required ModelReference? ModelReference { get; set; }
    public required Dictionary<EHttpMethod, Endpoint>? Endpoints { get; set; }
}
