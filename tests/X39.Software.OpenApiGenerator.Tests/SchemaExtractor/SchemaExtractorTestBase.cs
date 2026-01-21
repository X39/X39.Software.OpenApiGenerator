using Microsoft.OpenApi.Reader;
using X39.Software.OpenApiGenerator.Tests;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public abstract class SchemaExtractorTestBase
{
    protected static readonly OpenApiReaderSettings Settings = new()
    {
        Readers    = new() { { "yaml", new Microsoft.OpenApi.YamlReader.OpenApiYamlReader() } },
        HttpClient = Shared.HttpClient,
    };
}
