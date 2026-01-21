using System.Diagnostics;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed record PrimitiveModel : IModel
{
    public PrimitiveModel(EModelType modelType)
    {
        if (modelType is EModelType.Array or EModelType.Object)
            throw new ArgumentException("Primitive model cannot be an array or object.", nameof(modelType));
        Type = modelType;
    }

    public string Name
        => Type switch
        {
            EModelType.Unknown => throw new UnreachableException("This path should not be reachable."),
            EModelType.String  => Constants.KnownTypes.String,
            EModelType.Integer => Constants.KnownTypes.Integer,
            EModelType.Number  => Constants.KnownTypes.Number,
            EModelType.Boolean => Constants.KnownTypes.Boolean,
            EModelType.Null    => Constants.KnownTypes.Null,
            EModelType.Array   => throw new UnreachableException("This path should not be reachable."),
            EModelType.Object  => throw new UnreachableException("This path should not be reachable."),
            _                  => throw new ArgumentOutOfRangeException(),
        };

    public EModelType Type { get; }
}
