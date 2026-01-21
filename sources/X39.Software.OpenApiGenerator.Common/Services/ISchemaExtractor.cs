using Microsoft.OpenApi;

namespace X39.Software.OpenApiGenerator.Common.Services;

public interface ISchemaExtractor
{
    Task<bool> ExtractSchemasFromDocumentAsync(OpenApiDocument documentDocument, CancellationToken cancellationToken);
}