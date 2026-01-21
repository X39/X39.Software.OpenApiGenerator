using System.Diagnostics.CodeAnalysis;
using X39.Software.OpenApiGenerator.Common.Models;

namespace X39.Software.OpenApiGenerator.Common.Services;

public interface IModelRepository
{
    bool IsModelKnown(string name);
    void AddModel(IModel model);
    public IModel? GetModel(string name);
    public bool TryGetModel(string name, [NotNullWhen(true)] out IModel? model);
}
