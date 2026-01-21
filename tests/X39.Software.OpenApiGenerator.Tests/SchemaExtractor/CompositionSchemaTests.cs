using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public sealed class CompositionSchemaTests : SchemaExtractorTestBase
{
    [Fact]
    public async Task ExtractSchemas_WithAllOf_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.SchemaExtractor>>();
        var repository = new MockModelRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var schemaExtractor = new X39.Software.OpenApiGenerator.Services.SchemaExtractor(logger, schemaNameResolver, repository);
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                parameters:
                  - name: allOfParam
                    in: query
                    schema:
                      allOf:
                        - type: object
                          properties:
                            id:
                              type: integer
                        - type: object
                          properties:
                            name:
                              type: string
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await schemaExtractor.ExtractSchemasFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        Assert.Equal(4, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
        repository.AssertModel(new PrimitiveModel(EModelType.String));
        repository.AssertModel(
            new ObjectModel
            {
                Name = schemaNameResolver.GetAllOfParameterName(
                    schemaNameResolver.GetPathParameterName("/test", "allOfParam"),
                    0
                ),
                Properties = new()
                    { { "id", new ModelProperty("id", [Constants.KnownTypes.Integer], EModelModifier.Optional) } },
            }
        );
        repository.AssertModel(
            new ObjectModel
            {
                Name = schemaNameResolver.GetAllOfParameterName(
                    schemaNameResolver.GetPathParameterName("/test", "allOfParam"),
                    1
                ),
                Properties = new()
                {
                    { "name", new ModelProperty("name", [Constants.KnownTypes.String], EModelModifier.Optional) },
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithOneOf_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.SchemaExtractor>>();
        var repository = new MockModelRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var schemaExtractor = new X39.Software.OpenApiGenerator.Services.SchemaExtractor(logger, schemaNameResolver, repository);
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                parameters:
                  - name: oneOfParam
                    in: query
                    schema:
                      oneOf:
                        - type: string
                        - type: integer
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await schemaExtractor.ExtractSchemasFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        Assert.Equal(2, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.String));
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
    }

    [Fact]
    public async Task ExtractSchemas_WithAnyOf_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.SchemaExtractor>>();
        var repository = new MockModelRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var schemaExtractor = new X39.Software.OpenApiGenerator.Services.SchemaExtractor(logger, schemaNameResolver, repository);
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                parameters:
                  - name: anyOfParam
                    in: query
                    schema:
                      anyOf:
                        - type: string
                        - type: integer
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await schemaExtractor.ExtractSchemasFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        Assert.Equal(2, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.String));
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
    }
}
