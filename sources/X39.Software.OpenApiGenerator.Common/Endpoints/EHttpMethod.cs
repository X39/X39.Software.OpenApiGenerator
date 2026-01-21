using System.Diagnostics.CodeAnalysis;

namespace X39.Software.OpenApiGenerator.Common.Endpoints;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Enum names are stylized")]
public enum EHttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    OPTIONS,
    HEAD,
    PATCH,
    TRACE,
}