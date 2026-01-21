using Microsoft.OpenApi.Reader;

namespace X39.Software.OpenApiGenerator.Tests.EndpointExtractor;

public abstract class EndpointExtractorTestBase
{
    protected static readonly OpenApiReaderSettings Settings = new()
    {
        Readers    = new() { { "yaml", new Microsoft.OpenApi.YamlReader.OpenApiYamlReader() } },
        HttpClient = Shared.HttpClient,
    };
}
