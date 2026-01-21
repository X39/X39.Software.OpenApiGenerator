using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.SchemaExtractor;

public sealed class ObjectSchemaTests : SchemaExtractorTestBase
{
    [Fact]
    public async Task ExtractSchemas_WithObject_AllRequiredProperties_ShouldSucceed()
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
                  - name: objParam
                    in: query
                    required: true
                    schema:
                      type: object
                      required: [prop1, prop2]
                      properties:
                        prop1:
                          type: string
                        prop2:
                          type: object
                          properties:
                            inner:
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
        var objName = schemaNameResolver.GetPathSchemaName("/test", "objParam");
        repository.AssertModel(
            new ObjectModel
            {
                Name = objName,
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["prop1"] = new("prop1", [Constants.KnownTypes.String], EModelModifier.Required),
                    ["prop2"] = new(
                        "prop2",
                        [schemaNameResolver.GetObjectSchemaName(objName, "prop2")],
                        EModelModifier.Required
                    ),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithObject_MixedRequiredProperties_ShouldSucceed()
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
                  - name: objParam
                    in: query
                    required: true
                    schema:
                      type: object
                      required: [prop1]
                      properties:
                        prop1:
                          type: string
                        prop2:
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
        repository.AssertModel(
            new ObjectModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "objParam"),
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["prop1"] = new("prop1", [Constants.KnownTypes.String], EModelModifier.Required),
                    ["prop2"] = new("prop2", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithObject_NullableProperties_V30_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.SchemaExtractor>>();
        var repository = new MockModelRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var schemaExtractor = new X39.Software.OpenApiGenerator.Services.SchemaExtractor(logger, schemaNameResolver, repository);
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.0.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                parameters:
                  - name: objParam
                    in: query
                    required: true
                    schema:
                      type: object
                      properties:
                        prop1:
                          type: string
                          nullable: true
                        prop2:
                          type: integer
                          nullable: false
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
            new ObjectModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "objParam"),
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["prop1"] = new(
                        "prop1",
                        [Constants.KnownTypes.String, Constants.KnownTypes.Null],
                        EModelModifier.Optional
                    ),
                    ["prop2"] = new("prop2", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithObject_NullableProperties_V31_ShouldSucceed()
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
                  - name: objParam
                    in: query
                    required: true
                    schema:
                      type: object
                      properties:
                        prop1:
                          type: [string, "null"]
                        prop2:
                          type: [integer]
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
            new ObjectModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "objParam"),
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["prop1"] = new(
                        "prop1",
                        [Constants.KnownTypes.String, Constants.KnownTypes.Null],
                        EModelModifier.Optional
                    ),
                    ["prop2"] = new("prop2", [Constants.KnownTypes.Integer], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithObject_NullableObjectProperty_TypeArray_V31_ShouldSucceed()
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
                  - name: objParam
                    in: query
                    required: true
                    schema:
                      type: object
                      properties:
                        prop1:
                          type: [object, "null"]
                          properties:
                            inner:
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
        var objName = schemaNameResolver.GetPathSchemaName("/test", "objParam");
        repository.AssertModel(
            new ObjectModel
            {
                Name = objName,
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["prop1"] = new(
                        "prop1",
                        [schemaNameResolver.GetObjectSchemaName(objName, "prop1"), Constants.KnownTypes.Null],
                        EModelModifier.Optional
                    ),
                },
            }
        );
        repository.AssertModel(
            new ObjectModel
            {
                Name = schemaNameResolver.GetObjectSchemaName(objName, "prop1"),
                Properties = new Dictionary<string, ModelProperty>
                {
                    ["inner"] = new("inner", [Constants.KnownTypes.String], EModelModifier.Optional),
                },
            }
        );
    }

    [Fact]
    public async Task ExtractSchemas_WithObjectType_ShouldSucceed()
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
                  - name: objParam
                    in: query
                    schema:
                      type: object
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
            new ObjectModel
            {
                Name = schemaNameResolver.GetPathSchemaName("/test", "objParam"),
                Properties = new Dictionary<string, ModelProperty>(),
            }
        );
    }
}
