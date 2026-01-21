using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public sealed class DuplicateSchemaTests : SchemaExtractorTestBase
{
    [Fact]
    public async Task ExtractSchemas_WithMultipleReferencesToSameSchema_ShouldNotGenerateDuplicates()
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
              /test1:
                parameters:
                  - name: param1
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyObject'
              /test2:
                parameters:
                  - name: param2
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyObject'
            components:
              schemas:
                MyObject:
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
        Assert.Equal(2, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
        repository.AssertModel(
            new ObjectModel
            {
                Name = "MyObject",
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["id"] = new("id", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithReferencedAndUnreferencedSchema_ShouldNotGenerateDuplicates()
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
                  - name: param
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyObject'
            components:
              schemas:
                MyObject:
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
        Assert.Equal(2, repository.ModelCount);
        repository.AssertModel(new PrimitiveModel(EModelType.Integer));
        repository.AssertModel(
            new ObjectModel
            {
                Name = "MyObject",
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["id"] = new("id", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithNestedReferences_ShouldNotGenerateDuplicates()
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
                  - name: param
                    in: query
                    schema:
                      $ref: '#/components/schemas/Parent'
            components:
              schemas:
                Parent:
                  type: object
                  properties:
                    child1:
                      $ref: '#/components/schemas/Child'
                    child2:
                      $ref: '#/components/schemas/Child'
                Child:
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
            new ObjectModel
            {
                Name = "Child",
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["id"] = new("id", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
        repository.AssertModel(
            new ObjectModel
            {
                Name = "Parent",
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["child1"] = new("child1", ["Child"], EModelModifier.Optional),
                    ["child2"] = new("child2", ["Child"], EModelModifier.Optional),
                },
            }
        );
    }
}
