using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public interface IPathPart
{
    string Key { get; }
    ImmutableList<IPathPart> Children { get; }
    ImmutableDictionary<EHttpMethod, Endpoint>? Endpoints { get; }
}