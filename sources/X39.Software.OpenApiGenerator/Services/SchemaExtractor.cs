using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Common.Services;
using X39.Util;

namespace X39.Software.OpenApiGenerator.Services;

internal sealed class SchemaExtractor(
    ILogger<SchemaExtractor> logger,
    ISchemaNameResolver schemaNameResolver,
    IModelRepository modelRepository
) : ISchemaExtractor
{
    public async Task<bool> ExtractSchemasFromDocumentAsync(
        OpenApiDocument documentDocument,
        CancellationToken cancellationToken
    )
    {
        var flag = true;
        foreach (var (key, pathItem) in documentDocument.Paths)
        {
            foreach (var (index, pathParameter) in pathItem.Parameters?.Index() ?? [])
            {
                if (pathParameter.Schema is null)
                    continue;
                if (pathParameter.Name is null)
                {
                    logger.LogError(
                        "Path parameter {PathParameterIndex} in path {PathKey} has no name",
                        index,
                        pathParameter.Name
                    );
                    flag = false;
                    continue;
                }

                var results = await ExtractSchemaAsync(
                    cancellationToken,
                    pathParameter.Schema,
                    schemaNameResolver.GetPathParameterName(key, pathParameter.Name)
                );
                if (results is null)
                    flag = false;
            }

            if (pathItem.Operations is not null)
            {
                foreach (var operationKeyValuePair in pathItem.Operations)
                {
                    var operationType = operationKeyValuePair.Key.ToEnum();
                    var operation = operationKeyValuePair.Value;
                    if (operation.RequestBody?.Content is not null)
                    {
                        foreach (var (contentType, mediaType) in operation.RequestBody.Content)
                        {
                            if (mediaType.Schema is null)
                                continue;
                            var results = await ExtractSchemaAsync(
                                cancellationToken,
                                mediaType.Schema,
                                schemaNameResolver.GetRequestBodySchemaName(key, operationType, contentType)
                            );
                            if (results is null)
                                flag = false;
                        }
                    }

                    foreach (var (responseKey, response) in operation.Responses ?? [])
                    {
                        if (response.Content is null)
                            continue;
                        foreach (var (contentType, mediaType) in response.Content)
                        {
                            if (mediaType.Schema is null)
                                continue;
                            var results = await ExtractSchemaAsync(
                                cancellationToken,
                                mediaType.Schema,
                                schemaNameResolver.GetResponseSchemaName(key, operationType, responseKey.ToInt32(), contentType)
                            );
                            if (results is null)
                                flag = false;
                        }
                    }
                }
            }
        }

        if (documentDocument.Components?.Schemas is not null)
        {
            foreach (var (key, schema) in documentDocument.Components.Schemas)
            {
                if (modelRepository.IsModelKnown(schemaNameResolver.GetSchemaReferenceName(key)))
                    continue;
                var results = await ExtractSchemaAsync(
                    cancellationToken,
                    schema,
                    schemaNameResolver.GetSchemaReferenceName(key)
                );
                if (results is null)
                    flag = false;
            }
        }

        return flag;
    }

    private async Task<ImmutableList<ModelReference>?> ExtractSchemaAsync(
        CancellationToken cancellationToken,
        IOpenApiSchema schema,
        string key
    )
    {
        switch (schema)
        {
            case OpenApiSchema openApiSchema when openApiSchema is { OneOf: { } oneOf }:
            {
                var outputSchemas = new List<ModelReference>(8);
                var error = false;
                foreach (var (index, oneOfSchema) in oneOf.Index())
                {
                    var schemasFound = await ExtractSchemaAsync(
                        cancellationToken,
                        oneOfSchema,
                        schemaNameResolver.GetOneOfParameterName(key, index)
                    );
                    if (schemasFound is null)
                        error = true;
                    else
                        outputSchemas.AddRange(schemasFound);
                }
                if (!modelRepository.IsModelKnown(key))
                    modelRepository.AddModel(new OneOfModel(key, outputSchemas.ToImmutableList()));
                return error ? null : [key];
            }
            case OpenApiSchema openApiSchema when openApiSchema is { AllOf: { } allOf }:
            {
                var outputSchemas = new List<ModelReference>(8);
                var error = false;
                foreach (var (index, allOfSchema) in allOf.Index())
                {
                    var schemasFound = await ExtractSchemaAsync(
                        cancellationToken,
                        allOfSchema,
                        schemaNameResolver.GetAllOfParameterName(key, index)
                    );
                    if (schemasFound is null)
                        error = true;
                    else
                        outputSchemas.AddRange(schemasFound);
                }

                if (!modelRepository.IsModelKnown(key))
                    modelRepository.AddModel(new AllOfModel(key, outputSchemas.ToImmutableList()));
                return error ? null : [key];
            }
            case OpenApiSchema openApiSchema when openApiSchema is { AnyOf: { } anyOf }:
            {
                var outputSchemas = new List<ModelReference>(8);
                var error = false;
                foreach (var (index, anyOfSchema) in anyOf.Index())
                {
                    var schemasFound = await ExtractSchemaAsync(
                        cancellationToken,
                        anyOfSchema,
                        schemaNameResolver.GetAnyOfParameterName(key, index)
                    );
                    if (schemasFound is null)
                        error = true;
                    else
                        outputSchemas.AddRange(schemasFound);
                }

                if (!modelRepository.IsModelKnown(key))
                    modelRepository.AddModel(new AnyOfModel(key, outputSchemas.ToImmutableList()));
                return error ? null : [key];
            }
            case OpenApiSchema openApiSchema:
                return await ExtractSchemaAsync(key, openApiSchema, cancellationToken);
            case OpenApiSchemaReference { RecursiveTarget: { } actualSchema, Reference: { Id: { } schemaId } }:
                return await ExtractSchemaAsync(
                    schemaNameResolver.GetSchemaReferenceName(schemaId),
                    actualSchema,
                    cancellationToken
                );
            default:
                throw new Exception(
                    $"Unhandled schema type {schema.GetType().FullName}. This indicates an application error."
                );
        }
    }

    private async Task<ImmutableList<ModelReference>?> ExtractSchemaAsync(
        string pathHint,
        OpenApiSchema schema,
        CancellationToken cancellationToken
    )
    {
        if (schema.Type is null)
        {
            logger.LogError("Schema {PathHint} has no type", pathHint);
            return null;
        }

        var results = new List<ModelReference>(8);
        var error = false;

        var schemaType = schema.Type.Value;
        if (schemaType.HasFlag(JsonSchemaType.String))
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.String))
                modelRepository.AddModel(new PrimitiveModel(EModelType.String));

            results.Add(Constants.KnownTypes.String);
        }


        if (schemaType.HasFlag(JsonSchemaType.Integer))
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.Integer))
                modelRepository.AddModel(new PrimitiveModel(EModelType.Integer));

            results.Add(Constants.KnownTypes.Integer);
        }

        if (schemaType.HasFlag(JsonSchemaType.Number))
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.Number))
                modelRepository.AddModel(new PrimitiveModel(EModelType.Number));

            results.Add(Constants.KnownTypes.Number);
        }

        if (schemaType.HasFlag(JsonSchemaType.Boolean))
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.Boolean))
                modelRepository.AddModel(new PrimitiveModel(EModelType.Boolean));

            results.Add(Constants.KnownTypes.Boolean);
        }

        if (schemaType.HasFlag(JsonSchemaType.Null))
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.Null))
                modelRepository.AddModel(new PrimitiveModel(EModelType.Null));

            results.Add(Constants.KnownTypes.Null);
        }
        else if (schema.AnyOf?.Any((s) => s.Type?.HasFlag(JsonSchemaType.Null) ?? false) ?? false)
        {
            if (!modelRepository.IsModelKnown(Constants.KnownTypes.Null))
                modelRepository.AddModel(new PrimitiveModel(EModelType.Null));

            results.Add(Constants.KnownTypes.Null);
        }

        if (schemaType.HasFlag(JsonSchemaType.String) && schema.Enum is { Count: > 0 })
        {
            var isEnumNullable = schema.Enum.Any(e => e.IsJsonNullSentinel());
            var enumPathHint = schemaNameResolver.ResolveEnumSchemaName(pathHint, isEnumNullable);
            if (!modelRepository.IsModelKnown(enumPathHint))
            {
                modelRepository.AddModel(
                    new EnumModel
                    {
                        Name     = enumPathHint,
                        Nullable = isEnumNullable,
                        Values = schema.Enum
                            .Where(e => e.GetValueKind() is JsonValueKind.String)
                            .Where(e => !e.IsJsonNullSentinel())
                            .Select(e => e.GetValue<string>())
                            .ToImmutableList(),
                    }
                );
            }

            results.Add(enumPathHint);
        }

        if (schemaType.HasFlag(JsonSchemaType.Object))
        {
            if (!modelRepository.IsModelKnown(pathHint))
            {
                var properties = new Dictionary<string, ModelProperty>();
                if (schema.Properties is not null)
                {
                    foreach (var (key, openApiSchema) in schema.Properties)
                    {
                        var propertyKey = schemaNameResolver.GetObjectSchemaName(pathHint, key);
                        var schemasFound = await ExtractSchemaAsync(cancellationToken, openApiSchema, propertyKey);
                        if (schemasFound is null)
                        {
                            error = true;
                            continue;
                        }

                        properties[key] = new ModelProperty(
                            key,
                            schemasFound,
                            (schema.Required?.Contains(key) ?? false)
                                ? EModelModifier.Required
                                : EModelModifier.Optional
                        );
                    }
                }

                modelRepository.AddModel(
                    new ObjectModel
                    {
                        Name       = pathHint,
                        Properties = properties,
                    }
                );
            }

            results.Add(pathHint);
        }

        if (schemaType.HasFlag(JsonSchemaType.Array))
        {
            if (!modelRepository.IsModelKnown(pathHint))
            {
                if (schema.Items is null)
                {
                    logger.LogWarning("Array schema {PathHint} has no items", pathHint);
                    error = true;
                }
                else
                {
                    var name = schemaNameResolver.GetArraySchemaName(pathHint);
                    var schemasFound = await ExtractSchemaAsync(cancellationToken, schema.Items, name);
                    if (schemasFound is null)
                    {
                        error = true;
                    }
                    else
                    {
                        modelRepository.AddModel(
                            new ArrayModel
                            {
                                Name                = pathHint,
                                ItemModelReferences = schemasFound,
                            }
                        );
                    }
                }
            }

            results.Add(pathHint);
        }

        if (error)
            return null;
        return results.ToImmutableList();
    }
}
