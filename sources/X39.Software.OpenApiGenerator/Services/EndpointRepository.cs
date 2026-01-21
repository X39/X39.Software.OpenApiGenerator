using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Services;

internal sealed class EndpointRepository : IEndpointRepository
{
    private readonly ConstantPathPart _root = new("/", [], []);

    public ValueTask<ConstantPathPart> GetOrAddConstantPathPartAsync(IPathPart? currentPathPart, string keyElement)
    {
        var pathPart = currentPathPart ?? _root;
        if (pathPart.Children.TryGetValue(keyElement, out var child))
            if (child is ConstantPathPart constantPathPart)
                return ValueTask.FromResult(constantPathPart);
            else
                throw new InvalidOperationException($"Path part {keyElement} is not a constant path part");
        child = new ConstantPathPart(keyElement, [], []);
        pathPart.Children.Add(keyElement, child);
        return ValueTask.FromResult((ConstantPathPart) child);
    }

    public ValueTask<ParameterPathPart> GetOrAddParameterPathPartAsync(IPathPart? currentPathPart, string keyElement)
    {
        var pathPart = currentPathPart ?? _root;
        if (pathPart.Children.TryGetValue(keyElement, out var child))
            if (child is ParameterPathPart parameterPathPart)
                return ValueTask.FromResult(parameterPathPart);
            else
                throw new InvalidOperationException($"Path part");
        child = new ParameterPathPart(keyElement, [], null, []);
        pathPart.Children.Add(keyElement, child);
        return ValueTask.FromResult((ParameterPathPart) child);
    }
}
