using Microsoft.OpenApi;
using X39.Software.OpenApiGenerator.Common.Endpoints;
using X39.Software.OpenApiGenerator.Common.Models;
using X39.Software.OpenApiGenerator.Common.Services;

namespace X39.Software.OpenApiGenerator.Common;

public static class Extensions
{
    extension(IModelRepository modelRepository)
    {
        public T? GetModel<T>(string name)
            where T : IModel
        {
            var model = modelRepository.GetModel(name);
            if (model is null)
                return default;
            if (model is T typedModel)
                return typedModel;
            throw new InvalidCastException($"Model with name '{name}' is not of type {typeof(T)}");
        }

        public IModel GetModelOrThrow(string name)
        {
            if (!modelRepository.TryGetModel(name, out var model))
                throw new KeyNotFoundException($"Model with name '{name}' not found in repository");
            return model;
        }

        public T GetModelOrThrow<T>(string name)
            where T : IModel
        {
            var model = modelRepository.GetModelOrThrow(name);
            if (model is not T typedModel)
                throw new InvalidCastException($"Model with name '{name}' is not of type {typeof(T)}");
            return typedModel;
        }
    }

    extension(ModelReference modelReference)
    {
        public IModel? Resolve(IModelRepository modelRepository) => modelRepository.GetModel(modelReference.Name);

        public T? Resolve<T>(IModelRepository modelRepository)
            where T : IModel
            => modelRepository.GetModel<T>(modelReference.Name);

        public IModel ResolveOrThrow(IModelRepository modelRepository)
            => modelRepository.GetModelOrThrow(modelReference.Name);

        public T ResolveOrThrow<T>(IModelRepository modelRepository)
            where T : IModel
            => modelRepository.GetModelOrThrow<T>(modelReference.Name);
    }

    extension(HttpMethod httpMethod)
    {
        public EHttpMethod ToEnum()
            => httpMethod.Method.ToUpper() switch
            {
                "GET"     => EHttpMethod.GET,
                "PUT"     => EHttpMethod.PUT,
                "POST"    => EHttpMethod.POST,
                "DELETE"  => EHttpMethod.DELETE,
                "HEAD"    => EHttpMethod.HEAD,
                "OPTIONS" => EHttpMethod.OPTIONS,
                "TRACE"   => EHttpMethod.TRACE,
                "PATCH"   => EHttpMethod.PATCH,
                "QUERY"   => EHttpMethod.QUERY,
                "CONNECT" => EHttpMethod.CONNECT,
                _         => throw new ArgumentOutOfRangeException(),
            };
    }

    extension(ParameterLocation parameterLocation)
    {
        public EEndpointParameterLocation ToInternalEnum() => (EEndpointParameterLocation) parameterLocation;
    }
}
