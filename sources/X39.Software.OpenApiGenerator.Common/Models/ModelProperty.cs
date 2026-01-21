using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed record ModelProperty(string Name, ImmutableList<ModelReference> ModelReferences, EModelModifier ModelModifier);
