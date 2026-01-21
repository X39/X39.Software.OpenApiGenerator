namespace X39.Software.OpenApiGenerator.Common.Models;

public interface IModel
{
    string Name { get; }
    EModelType Type { get; }
}