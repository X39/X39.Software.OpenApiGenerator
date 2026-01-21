using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed class ArrayModel : IModel, IEquatable<ArrayModel>, IEqualityComparer<ArrayModel>
{
    public required string Name { get; init; }
    public EModelType Type => EModelType.Array;

    public required ImmutableList<ModelReference> ItemModelReferences { get; init; }

    public bool Equals(ArrayModel? other)
    {
        if (other is null)
            return false;
        return Name == other.Name && ItemModelReferences == other.ItemModelReferences;
    }

    public bool Equals(ArrayModel? x, ArrayModel? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;
        return x.Equals(y);
    }

    public int GetHashCode(ArrayModel obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Name);
        hashCode.Add(ItemModelReferences);

        return hashCode.ToHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as ArrayModel);

    public override int GetHashCode() => GetHashCode(this);
}
