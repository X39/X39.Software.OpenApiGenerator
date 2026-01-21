using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using X39.Software.OpenApiGenerator.Common;
using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Services;

internal sealed class SchemaNameResolver : ISchemaNameResolver
{
    public string GetPathParameterName(string path, string parameterName)
    {
        return $"{path}/.{parameterName}";
    }

    public string ResolveEnumSchemaName(string pathHint, bool isNullable)
    {
        return isNullable ? $"{pathHint}?" : pathHint;
    }

    public string GetSchemaReferenceName(string schemaId)
    {
        return schemaId;
    }

    public string GetObjectSchemaName(string pathHint, string propertyName)
    {
        return $"{pathHint}.{propertyName}";
    }

    public string GetAllOfParameterName(string pathHint, int index)
    {
        return $"{pathHint}#{index}";
    }
    public string GetOneOfParameterName(string pathHint, int index)
    {
        return $"{pathHint}#{index}";
    }
    public string GetAnyOfParameterName(string pathHint, int index)
    {
        return $"{pathHint}#{index}";
    }

    public string GetArraySchemaName(string pathHint)
    {
        return $"{pathHint}[]";
    }

    public string GetRequestBodySchemaName(string path, EHttpMethod operationType, MimeType contentType)
    {
        return $"{path}/{Enum.GetName(operationType)}/request/{contentType}";
    }

    public string GetResponseSchemaName(string path, EHttpMethod operationType, HttpStatusCode responseKey, MimeType contentType)
    {
        return $"{path}/{Enum.GetName(operationType)}/{responseKey}/{contentType}";
    }
}
