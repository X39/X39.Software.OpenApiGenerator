using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public sealed class EnumSchemaTests : SchemaExtractorTestBase
{
    [Fact]
    public async Task ExtractSchemas_WithEnumType_ShouldSucceed()
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
                  - name: enumParam
                    in: query
                    schema:
                      type: string
                      enum: [value1, value2]
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
            new EnumModel
            {
                Name     = schemaNameResolver.GetPathSchemaName("/test", "enumParam"),
                Values   = ["value1", "value2"],
                Nullable = false,
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithEnumType_Nullable_ShouldSucceed()
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
                  - name: enumParam
                    in: query
                    schema:
                      type: [string, "null"]
                      enum: [value1, value2]
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
            new EnumModel
            {
                Name     = schemaNameResolver.GetPathSchemaName("/test", "enumParam"),
                Values   = ["value1", "value2"],
                Nullable = true,
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithMultipleEnums_SharedReference_ShouldSucceed()
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
                  - name: enumParam1
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyEnum'
                  - name: enumParam2
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyEnum'
            components:
              schemas:
                MyEnum:
                  type: string
                  enum: [value1, value2]
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
        repository.AssertModel(
            new EnumModel { Name = "MyEnum", Values = ["value1", "value2"], Nullable = false }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithMultipleEnums_SharedReference_Nullable_ShouldSucceed()
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
                  - name: enumParam1
                    in: query
                    schema:
                      $ref: '#/components/schemas/MyEnum'
                  - name: enumParam2
                    in: query
                    schema:
                      type: [string, "null"]
                      enum: [value1, value2]
            components:
              schemas:
                MyEnum:
                  type: string
                  enum: [value1, value2]
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
        repository.AssertModel(new PrimitiveModel(EModelType.String));
        repository.AssertModel(new PrimitiveModel(EModelType.Null));
        repository.AssertModel(
            new EnumModel { Name = "MyEnum", Values = ["value1", "value2"], Nullable = false }
        );
        repository.AssertModel(
            new EnumModel
            {
                Name     = schemaNameResolver.GetPathSchemaName("/test", "enumParam2"),
                Values   = ["value1", "value2"],
                Nullable = true,
            }
        );
    }
}
