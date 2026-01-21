using System.Diagnostics.CodeAnalysis;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Common.Services;
using X39.Software.OpenApiGenerator.Services;
using Xunit.Sdk;

namespace X39.Software.OpenApiGenerator.Tests.Mock;

public sealed class MockModelRepository : IModelRepository
{
    private readonly Dictionary<string, IModel> _models = new();

    public bool IsModelKnown(string name)
    {
        return _models.ContainsKey(name);
    }

    public void AddModel(IModel model)
    {
        _models.Add(model.Name, model);
    }

    public IModel? GetModel(string name)
    {
        return _models.GetValueOrDefault(name);
    }

    public bool TryGetModel(string name, [NotNullWhen(true)] out IModel? model)
    {
        return _models.TryGetValue(name, out model);
    }

    public int ModelCount => _models.Count;

    public void AssertModel(IModel model)
    {
        var existing = _models.GetValueOrDefault(model.Name);
        Assert.NotNull(existing);
        Assert.Equal(model.Type, existing?.Type);
        Assert.Equal(model.Name, existing?.Name);
        Assert.IsType(model.GetType(), existing);
        switch (model)
        {
            case PrimitiveModel:
                // Empty
                break;
            case EnumModel enumModel:
                Assert.Equal(enumModel.Values.Count, ((EnumModel) existing).Values.Count);
                foreach (var (expected, actual) in enumModel.Values.Zip(
                             ((EnumModel) existing).Values,
                             (a, b) => (a, b)
                         ))
                {
                    Assert.Equal(expected, actual);
                }

                break;
            case ObjectModel objectModel:
                var existingObjectModel = (ObjectModel) existing;
                Assert.Equal(objectModel.Properties?.Count, existingObjectModel.Properties?.Count);
                if (objectModel.Properties is not null)
                {
                    Assert.NotNull(existingObjectModel.Properties);
                    foreach (var (key, value) in objectModel.Properties)
                    {
                        try
                        {
                            Assert.Contains(key, existingObjectModel.Properties.Keys);
                            var property = existingObjectModel.Properties[key];
                            Assert.Equal(value.Name, property.Name);
                            Assert.Equal(value.ModelModifier, property.ModelModifier);
                            Assert.Equal(value.ModelReferences.Count, property.ModelReferences.Count);
                            foreach (var valueModelKey in value.ModelReferences)
                            {
                                Assert.Contains(valueModelKey, property.ModelReferences);
                            }
                        }
                        catch (XunitException ex)
                        {
                            throw new XunitException($"Exception while validating property '{key}' of object model '{objectModel.Name}': {ex.Message}", ex);
                        }
                    }
                }
                break;
            case ArrayModel arrayModel:
                Assert.Equal(arrayModel.ItemModelReferences.Count, ((ArrayModel) existing).ItemModelReferences.Count);
                foreach (var value in arrayModel.ItemModelReferences)
                {
                    Assert.Contains(value, ((ArrayModel) existing).ItemModelReferences);
                }
                break;
            default:
                Assert.Fail($"Unhandled model type {model.GetType().FullName}");
                break;
        }
    }
}
