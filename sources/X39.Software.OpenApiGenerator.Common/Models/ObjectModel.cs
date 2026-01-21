namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed record ObjectModel : IModel
{
    public required string Name { get; init; }
    public EModelType Type => EModelType.Object;
    public required Dictionary<string, ModelProperty> Properties { get; init; }
}