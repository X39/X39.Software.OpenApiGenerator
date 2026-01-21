using Microsoft.OpenApi;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Services;

internal sealed class EndpointExtractor : IEndpointExtractor
{
    public async Task<bool> ExtractEndpointsFromDocumentAsync(OpenApiDocument resultDocument, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
