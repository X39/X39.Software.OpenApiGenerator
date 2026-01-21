using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

public interface IPathPart
{
    string Name { get; }
    Dictionary<string, IPathPart> Children { get; }
    Dictionary<EHttpMethod, Endpoint>? Endpoints { get; set; }
}
