using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Tests.Mock;

public sealed class MockEndpointRepository : IEndpointRepository
{
    private readonly Dictionary<string, IPathPart> _rootParts = new();

    public ValueTask<ConstantPathPart> GetOrAddConstantPathPartAsync(IPathPart? currentPathPart, string keyElement)
    {
        if (currentPathPart is null)
        {
            if (_rootParts.TryGetValue(keyElement, out var existingRoot))
            {
                return ValueTask.FromResult((ConstantPathPart)existingRoot);
            }

            var newRoot = new ConstantPathPart
            {
                Name = keyElement,
                Children = new Dictionary<string, IPathPart>(),
                Endpoints = null
            };
            _rootParts.Add(keyElement, newRoot);
            return ValueTask.FromResult(newRoot);
        }

        if (currentPathPart.Children.TryGetValue(keyElement, out var existing))
        {
            return ValueTask.FromResult((ConstantPathPart)existing);
        }

        var newPart = new ConstantPathPart
        {
            Name = keyElement,
            Children = new Dictionary<string, IPathPart>(),
            Endpoints = null
        };
        currentPathPart.Children.Add(keyElement, newPart);
        return ValueTask.FromResult(newPart);
    }

    public ValueTask<ParameterPathPart> GetOrAddParameterPathPartAsync(IPathPart? currentPathPart, string keyElement)
    {
        if (currentPathPart is null)
        {
            if (_rootParts.TryGetValue(keyElement, out var existingRoot))
            {
                return ValueTask.FromResult((ParameterPathPart)existingRoot);
            }

            var newRoot = new ParameterPathPart
            {
                Name = keyElement,
                Children = new Dictionary<string, IPathPart>(),
                ModelReference = null,
                Endpoints = null
            };
            _rootParts.Add(keyElement, newRoot);
            return ValueTask.FromResult(newRoot);
        }

        if (currentPathPart.Children.TryGetValue(keyElement, out var existing))
        {
            return ValueTask.FromResult((ParameterPathPart)existing);
        }

        var newPart = new ParameterPathPart
        {
            Name = keyElement,
            Children = new Dictionary<string, IPathPart>(),
            ModelReference = null,
            Endpoints = null
        };
        currentPathPart.Children.Add(keyElement, newPart);
        return ValueTask.FromResult(newPart);
    }

    public IEnumerable<IPathPart> RootParts => _rootParts.Values;
}
