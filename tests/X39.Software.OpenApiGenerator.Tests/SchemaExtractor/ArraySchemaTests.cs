using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public sealed class ArraySchemaTests : SchemaExtractorTestBase
{

    [Fact]
    public async Task ExtractSchemas_WithArrayOfObjects_ShouldSucceed()
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
                  - name: arrayOfObjectsParam
                    in: query
                    schema:
                      type: array
                      items:
                        type: object
                        properties:
                          id:
                            type: integer
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
        Assert.Equal(3, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
        repository.AssertModel(
            new ArrayModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "arrayOfObjectsParam"),
                ItemModelReferences =
                [
                    schemaNameResolver.GetArraySchemaName(
                        schemaNameResolver.GetPathSchemaName("/test", "arrayOfObjectsParam")
                    )
                ],
            }
        );
        repository.AssertModel(
            new ObjectModel
            {
                Name = schemaNameResolver.GetArraySchemaName(
                    schemaNameResolver.GetPathSchemaName("/test", "arrayOfObjectsParam")
                ),
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["id"] = new("id", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }
    [Fact]
    public async Task ExtractSchemas_WithArrayType_ShouldSucceed()
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
                  - name: arrayParam
                    in: query
                    schema:
                      type: array
                      items:
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
        repository.AssertModel(
            new ArrayModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "arrayParam"),
                ItemModelReferences = [Constants.KnownTypes.String],
            }
        );
    }
}
