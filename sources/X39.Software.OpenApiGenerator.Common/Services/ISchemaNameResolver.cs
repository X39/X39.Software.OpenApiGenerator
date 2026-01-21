using Microsoft.OpenApi;

namespace X39.Software.OpenApiGenerator.Common.Services;

/// <summary>
/// Defines methods to resolve schema names based on various OpenAPI components and configurations.
/// </summary>
public interface ISchemaNameResolver
{
    /// <summary>
    /// Generates a schema name for a specific path and parameter.
    /// </summary>
    /// <param name="path">The path associated with the schema.</param>
    /// <param name="parameterName">The name of the parameter for which the schema name is being created.</param>
    /// <returns>The generated schema name for the given path and parameter.</returns>
    string GetPathSchemaName(string path, string parameterName);

    /// <summary>
    /// Resolves the schema name for an enumeration based on the provided path hint and nullable flag.
    /// </summary>
    /// <param name="pathHint">
    /// A string representing a hint related to the path or context of the schema.
    /// </param>
    /// <param name="isNullable">
    /// A boolean value indicating whether the enumeration schema should be treated as nullable.
    /// </param>
    /// <returns>
    /// A string representing the resolved schema name for the enumeration, potentially including a nullable indicator.
    /// </returns>
    string ResolveEnumSchemaName(string pathHint, bool isNullable);

    /// <summary>
    /// Resolves the reference name for a schema using its unique identifier.
    /// </summary>
    /// <param name="schemaId">The unique identifier of the schema.</param>
    /// <returns>A string representing the reference name of the schema.</returns>
    string GetSchemaReferenceName(string schemaId);

    /// <summary>
    /// Constructs and retrieves the schema name for an object property based on a given path hint and property name.
    /// </summary>
    /// <param name="pathHint">The path hint or prefix used to construct the schema name.</param>
    /// <param name="propertyName">The name of the object property for which the schema name is being resolved.</param>
    /// <returns>A string representing the fully-qualified schema name for the specified property.</returns>
    string GetObjectSchemaName(string pathHint, string propertyName);

    /// <summary>
    /// Generates a name for an "allOf" schema parameter based on the specified path hint and index.
    /// </summary>
    /// <param name="pathHint">A string representing the path hint used to derive the parameter name.</param>
    /// <param name="index">An integer representing the index of the "allOf" schema parameter.</param>
    /// <returns>A string representing the constructed name for the "allOf" schema parameter.</returns>
    string GetAllOfParameterName(string pathHint, int index);

    /// <summary>
    /// Generates a unique name for a parameter within a 'OneOf' schema based on the provided path hint and index.
    /// </summary>
    /// <param name="pathHint">A string that serves as a hint for identifying the schema's path or context.</param>
    /// <param name="index">The index of the parameter within the 'OneOf' schema.</param>
    /// <returns>A string representing the generated name for the specified parameter in the 'OneOf' schema.</returns>
    string GetOneOfParameterName(string pathHint, int index);

    /// <summary>
    /// Generates a unique name for an "AnyOf" schema parameter based on the provided path hint and index.
    /// </summary>
    /// <param name="pathHint">A string representing the base path or hint associated with the schema.</param>
    /// <param name="index">The index of the "AnyOf" parameter within the schema.</param>
    /// <returns>A string that uniquely identifies the "AnyOf" parameter within the schema.</returns>
    string GetAnyOfParameterName(string pathHint, int index);

    /// <summary>
    /// Generates a schema name for an array based on the provided path hint.
    /// </summary>
    /// <param name="pathHint">A string representing the hint for the array's path, typically describing its context or structure.</param>
    /// <return>Returns a string representing the schema name for the array, derived from the given path hint.</return>
    string GetArraySchemaName(string pathHint);

    /// <summary>
    /// Generates the schema name for a given request body based on the specified path, operation type, and content type.
    /// </summary>
    /// <param name="path">The API endpoint path associated with the request body.</param>
    /// <param name="operationType">The HTTP method or operation type (e.g., GET, POST, PUT) related to the request.</param>
    /// <param name="contentType">The MIME type of the request body content (e.g., application/json, multipart/form-data).</param>
    /// <returns>A string representing the generated schema name for the request body.</returns>
    string GetRequestBodySchemaName(string path, string operationType, string contentType);

    /// <summary>
    /// Generates a schema name for a response based on the provided path, operation type, response key, and content type.
    /// </summary>
    /// <param name="path">The API path for which the response schema name is generated.</param>
    /// <param name="operationType">The type of operation (e.g., GET, POST, PUT) for which the response schema name is being derived.</param>
    /// <param name="responseKey">The key identifying the response (e.g., status code like 200, 404).</param>
    /// <param name="contentType">The content type of the response, such as "application/json" or "text/plain".</param>
    /// <returns>A string representing the generated schema name for the response.</returns>
    string GetResponseSchemaName(string path, string operationType, string responseKey, string contentType);
}
