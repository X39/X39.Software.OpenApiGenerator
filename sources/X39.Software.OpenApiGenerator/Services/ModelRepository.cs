using System.Diagnostics.CodeAnalysis;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Services;

internal class ModelRepository : IModelRepository
{
    private readonly Dictionary<string, IModel> _models = new();

    public bool IsModelKnown(string name)
    {
        return _models.ContainsKey(name);
    }

    public void AddModel(IModel model)
    {
        try
        {
            _models.Add(model.Name, model);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Model {model.Name} already exists", ex);
        }
    }

    public IModel? GetModel(string name)
    {
        return _models.GetValueOrDefault(name);
    }

    public bool TryGetModel(string name, [NotNullWhen(true)] out IModel? model)
    {
        return _models.TryGetValue(name, out model);
    }
}
