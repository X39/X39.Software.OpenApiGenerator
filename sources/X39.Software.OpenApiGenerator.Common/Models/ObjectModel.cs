using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed record ObjectModel() : IModel
{
    [SetsRequiredMembers]
    public ObjectModel(string name, params IEnumerable<ModelProperty> properties)
        : this()
    {
        Name       = name;
        Properties = properties.ToDictionary(p => p.Name);
    }

    public required string Name { get; init; }
    public EModelType Type => EModelType.Object;
    public required Dictionary<string, ModelProperty> Properties { get; init; }
}
