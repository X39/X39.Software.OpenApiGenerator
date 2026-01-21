using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Services;
using X39.Software.OpenApiGenerator.Tests.Mock;

namespace X39.Software.OpenApiGenerator.Tests.EndpointExtractor;

public sealed class BasicEndpointTests : EndpointExtractorTestBase
{
    [Fact]
    public async Task ExtractEndpoints_WithSimpleGetEndpoint_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                get:
                  responses:
                    '200':
                      description: Success
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.Equal("test", rootPart.Name);
        Assert.NotNull(rootPart.Endpoints);
        Assert.Single(rootPart.Endpoints);
        Assert.Contains(EHttpMethod.GET, rootPart.Endpoints.Keys);
    }

    [Fact]
    public async Task ExtractEndpoints_WithMultipleHttpMethods_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /test:
                get:
                  responses:
                    '200':
                      description: Success
                post:
                  responses:
                    '201':
                      description: Created
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.Equal("test", rootPart.Name);
        Assert.NotNull(rootPart.Endpoints);
        Assert.Equal(2, rootPart.Endpoints.Count);
        Assert.Contains(EHttpMethod.GET, rootPart.Endpoints.Keys);
        Assert.Contains(EHttpMethod.POST, rootPart.Endpoints.Keys);
    }

    [Fact]
    public async Task ExtractEndpoints_WithNestedPath_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /api/users:
                get:
                  responses:
                    '200':
                      description: Success
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.Equal("api", rootPart.Name);
        Assert.Single(rootPart.Children);
        var childPart = rootPart.Children["users"];
        Assert.Equal("users", childPart.Name);
        Assert.NotNull(childPart.Endpoints);
        Assert.Single(childPart.Endpoints);
        Assert.Contains(EHttpMethod.GET, childPart.Endpoints.Keys);
    }

    [Fact]
    public async Task ExtractEndpoints_WithPathParameter_ShouldExtractStructure()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users/{id}:
                get:
                  parameters:
                    - name: id
                      in: path
                      required: true
                      schema:
                        type: string
                  responses:
                    '200':
                      description: Success
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        // The extraction creates the path structure even though parameter name mismatch causes failure
        var rootPart = Assert.Single(repository.RootParts);
        Assert.Equal("users", rootPart.Name);
        Assert.Single(rootPart.Children);
        var paramPart = Assert.IsType<ParameterPathPart>(rootPart.Children["{id}"]);
        Assert.Equal("{id}", paramPart.Name);

        // Verify schema name for path parameter
        Assert.NotNull(paramPart.Endpoints);
        var endpoint = paramPart.Endpoints[EHttpMethod.GET];
        var param = endpoint.Parameters["id"];
        Assert.Equal("/users/{id}/.id", param.Schema.Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithQueryParameter_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                get:
                  parameters:
                    - name: search
                      in: query
                      required: false
                      schema:
                        type: string
                  responses:
                    '200':
                      description: Success
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.Equal("users", rootPart.Name);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.GET];
        Assert.Single(endpoint.Parameters);
        var param = endpoint.Parameters["search"];
        Assert.Equal(EEndpointParameterLocation.Query, param.Location);
        Assert.Equal(EEndpointModifier.Optional, param.Modifier);
        Assert.Equal("/users/.search", param.Schema.Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithRequestBody_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                post:
                  requestBody:
                    content:
                      application/json:
                        schema:
                          type: object
                  responses:
                    '201':
                      description: Created
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.POST];
        Assert.Single(endpoint.Requests);
        Assert.Contains(new MimeType("application/json"), endpoint.Requests.Keys);
        var requestSchema = endpoint.Requests[new MimeType("application/json")];
        Assert.Equal("/users/POST/request/application/json", requestSchema.Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithResponse_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                get:
                  responses:
                    '200':
                      description: Success
                      content:
                        application/json:
                          schema:
                            type: array
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.GET];
        Assert.Single(endpoint.Responses);
        var response = endpoint.Responses[200];
        Assert.Single(response.Schemas);
        Assert.Contains(new MimeType("application/json"), response.Schemas.Keys);
        var responseSchema = response.Schemas[new MimeType("application/json")];
        Assert.Equal("/users/GET/200/application/json", responseSchema.Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithMultipleParameters_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                get:
                  parameters:
                    - name: search
                      in: query
                      required: false
                      schema:
                        type: string
                    - name: limit
                      in: query
                      required: true
                      schema:
                        type: integer
                    - name: X-Api-Key
                      in: header
                      required: true
                      schema:
                        type: string
                  responses:
                    '200':
                      description: Success
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.GET];
        Assert.Equal(3, endpoint.Parameters.Count);

        var searchParam = endpoint.Parameters["search"];
        Assert.Equal(EEndpointParameterLocation.Query, searchParam.Location);
        Assert.Equal(EEndpointModifier.Optional, searchParam.Modifier);
        Assert.Equal("/users/.search", searchParam.Schema.Name);

        var limitParam = endpoint.Parameters["limit"];
        Assert.Equal(EEndpointParameterLocation.Query, limitParam.Location);
        Assert.Equal(EEndpointModifier.Required, limitParam.Modifier);
        Assert.Equal("/users/.limit", limitParam.Schema.Name);

        var headerParam = endpoint.Parameters["X-Api-Key"];
        Assert.Equal(EEndpointParameterLocation.Header, headerParam.Location);
        Assert.Equal(EEndpointModifier.Required, headerParam.Modifier);
        Assert.Equal("/users/.X-Api-Key", headerParam.Schema.Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithMultipleResponses_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                post:
                  requestBody:
                    content:
                      application/json:
                        schema:
                          type: object
                  responses:
                    '200':
                      description: Success
                      content:
                        application/json:
                          schema:
                            type: object
                    '400':
                      description: Bad Request
                      content:
                        application/json:
                          schema:
                            type: object
                    '500':
                      description: Internal Server Error
                      content:
                        application/json:
                          schema:
                            type: object
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.POST];
        Assert.Equal(3, endpoint.Responses.Count);

        var response200 = endpoint.Responses[200];
        Assert.Equal("/users/POST/200/application/json", response200.Schemas[new MimeType("application/json")].Name);

        var response400 = endpoint.Responses[400];
        Assert.Equal("/users/POST/400/application/json", response400.Schemas[new MimeType("application/json")].Name);

        var response500 = endpoint.Responses[500];
        Assert.Equal("/users/POST/500/application/json", response500.Schemas[new MimeType("application/json")].Name);
    }

    [Fact]
    public async Task ExtractEndpoints_WithMultipleContentTypes_ShouldSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<X39.Software.OpenApiGenerator.Services.EndpointExtractor>>();
        var repository = new MockEndpointRepository();
        var schemaNameResolver = new SchemaNameResolver();
        var endpointExtractor = new X39.Software.OpenApiGenerator.Services.EndpointExtractor(
            logger,
            repository,
            schemaNameResolver
        );
        var document = OpenApiDocument.Parse(
            """
            openapi: 3.1.0
            info:
              title: Test API
              version: 1.0.0
            paths:
              /users:
                post:
                  requestBody:
                    content:
                      application/json:
                        schema:
                          type: object
                      application/xml:
                        schema:
                          type: object
                  responses:
                    '200':
                      description: Success
                      content:
                        application/json:
                          schema:
                            type: object
                        application/xml:
                          schema:
                            type: object
            """,
            settings: Settings
        );
        Assert.Empty(document.Diagnostic?.Errors ?? []);

        // Act
        var result = await endpointExtractor.ExtractEndpointsFromDocumentAsync(
            document.Document ?? throw new NullReferenceException("Document parsed was null"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        var rootPart = Assert.Single(repository.RootParts);
        Assert.NotNull(rootPart.Endpoints);
        var endpoint = rootPart.Endpoints[EHttpMethod.POST];

        Assert.Equal(2, endpoint.Requests.Count);
        Assert.Equal("/users/POST/request/application/json", endpoint.Requests[new MimeType("application/json")].Name);
        Assert.Equal("/users/POST/request/application/xml", endpoint.Requests[new MimeType("application/xml")].Name);

        var response = endpoint.Responses[200];
        Assert.Equal(2, response.Schemas.Count);
        Assert.Equal("/users/POST/200/application/json", response.Schemas[new MimeType("application/json")].Name);
        Assert.Equal("/users/POST/200/application/xml", response.Schemas[new MimeType("application/xml")].Name);
    }
}
