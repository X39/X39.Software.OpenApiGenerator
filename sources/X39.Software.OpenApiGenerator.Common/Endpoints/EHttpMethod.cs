using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Enum names are stylized")]
public enum EHttpMethod
{
    GET,
    PUT,
    POST,
    DELETE,
    HEAD,
    OPTIONS,
    TRACE,
    PATCH,
    QUERY,
    CONNECT,
}
