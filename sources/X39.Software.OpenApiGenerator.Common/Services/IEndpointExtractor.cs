using Microsoft.OpenApi;

namespace X39.Software.OpenApiGenerator.Common.Services;

public interface IEndpointExtractor
{
    Task<bool> ExtractEndpointsFromDocumentAsync(OpenApiDocument resultDocument, CancellationToken cancellationToken);
}
