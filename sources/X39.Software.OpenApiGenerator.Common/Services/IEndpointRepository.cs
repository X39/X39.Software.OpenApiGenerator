using X39.Software.OpenApiGenerator.Common.Endpoints;

namespace X39.Software.OpenApiGenerator.Common.Services;

public interface IEndpointRepository
{
    ValueTask<ConstantPathPart> GetOrAddConstantPathPartAsync(IPathPart? currentPathPart, string keyElement);
    ValueTask<ParameterPathPart> GetOrAddParameterPathPartAsync(IPathPart? currentPathPart, string keyElement);
}
