using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed record AnyOfModel(string Name, ImmutableList<ModelReference> ModelReferences) : IModel
{
    public EModelType Type => EModelType.Unknown;
}