using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Common.Services;
using X39.Util;

namespace X39.Software.OpenApiGenerator.Services;

internal sealed class EndpointExtractor(
    ILogger<EndpointExtractor> logger,
    IEndpointRepository endpointRepository,
    ISchemaNameResolver schemaNameResolver
) : IEndpointExtractor
{
    public async Task<bool> ExtractEndpointsFromDocumentAsync(
        OpenApiDocument resultDocument,
        CancellationToken cancellationToken
    )
    {
        var result = true;
        foreach (var (key, pathItem) in resultDocument.Paths)
        {
            var keyElements = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
            IPathPart? currentPathPart = null;
            var dictionary = new Dictionary<string, ParameterPathPart>();
            var abort = false;
            foreach (var keyElement in keyElements)
            {
                if (keyElement.StartsWith('{') && keyElement.EndsWith('}'))
                {
                    currentPathPart = await endpointRepository.GetOrAddParameterPathPartAsync(
                        currentPathPart,
                        keyElement
                    );
                    if (!dictionary.TryAdd(keyElement, (ParameterPathPart) currentPathPart))
                    {
                        logger.LogError("Key element {KeyElement} is duplicated in path {Path}", keyElement, key);
                        result = false;
                        abort  = true;
                        break;
                    }
                }
                else
                {
                    currentPathPart = await endpointRepository.GetOrAddConstantPathPartAsync(
                        currentPathPart,
                        keyElement
                    );
                }
            }

            if (currentPathPart is null)
            {
                logger.LogError("Path {Path} did not contain any path parts", key);
                result = false;
                continue;
            }

            if (abort)
                continue;
            foreach (var (httpMethod, operation) in pathItem.Operations ?? [])
            {
                currentPathPart.Endpoints ??= new Dictionary<EHttpMethod, Endpoint>();
                var httpMethodEnum = httpMethod.ToEnum();
                if (currentPathPart.Endpoints.ContainsKey(httpMethodEnum))
                {
                    logger.LogError(
                        "Path {Path} contains multiple operations for method {HttpMethod}",
                        key,
                        httpMethod
                    );
                    result = false;
                    continue;
                }

                var requests = operation.RequestBody?.Content?.Select(e => (new MimeType(e.Key),
                                   new ModelReference(
                                       schemaNameResolver.GetRequestBodySchemaName(key, httpMethodEnum, e.Key)
                                   ))
                               )
                               ?? [];
                var responses = operation.Responses?.Select(e => new EndpointResponse(
                                        e.Key.ToInt32(),
                                        e.Value.Content?.Select(kvp => new KeyValuePair<MimeType, ModelReference>(
                                                new MimeType(kvp.Key),
                                                new ModelReference(
                                                    schemaNameResolver.GetResponseSchemaName(
                                                        key,
                                                        httpMethodEnum,
                                                        kvp.Key,
                                                        kvp.Key
                                                    )
                                                )
                                            )
                                        )
                                        ?? []
                                    )
                                )
                                ?? [];
                var endpointParameters = new List<EndpointParameter>(operation.Parameters?.Count ?? 0);
                foreach (var parameter in operation.Parameters ?? [])
                {
                    if (parameter.Name is null)
                    {
                        logger.LogError(
                            "Path parameter {PathParameter} in path {Path} has no name",
                            parameter.Name,
                            key
                        );
                        result = false;
                        continue;
                    }

                    var endpointParameter = new EndpointParameter
                    {
                        Name     = parameter.Name,
                        Location = parameter.In?.ToInternalEnum() ?? EEndpointParameterLocation.Query,
                        Modifier = parameter.Required ? EEndpointModifier.Required : EEndpointModifier.Optional,
                        Schema   = schemaNameResolver.GetPathParameterName(key, parameter.Name),
                    };
                    endpointParameters.Add(endpointParameter);

                    if (endpointParameter.Location is EEndpointParameterLocation.Path)
                    {
                        if (!dictionary.TryGetValue(endpointParameter.Name, out var part))
                        {
                            logger.LogError(
                                "Path parameter {PathParameter} in path {Path} is not defined in path",
                                endpointParameter.Name,
                                key
                            );
                            result = false;
                            continue;
                        }

                        if (part.ModelReference is null)
                        {
                            logger.LogError(
                                "Path parameter {PathParameter} in path {Path} has no schema",
                                endpointParameter.Name,
                                key
                            );
                            result = false;
                            continue;
                        }

                        part.ModelReference = endpointParameter.Schema;
                    }
                }

                currentPathPart.Endpoints[httpMethodEnum] = new Endpoint(
                    httpMethodEnum,
                    endpointParameters,
                    requests,
                    responses
                );
            }
        }

        return result;
    }
}
