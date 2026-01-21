using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common.Models;

public sealed class EnumModel : IModel, IEquatable<EnumModel>, IEqualityComparer<EnumModel>
{
    public EModelType Type => EModelType.String;
    public required string Name { get; init; }
    public required ImmutableList<string> Values { get; init; }
    public required bool Nullable { get; init; }

    public bool Equals(EnumModel? other)
    {
        if (other is null)
            return false;
        return Name == other.Name && Values.SequenceEqual(other.Values) && Nullable == other.Nullable;
    }

    public bool Equals(EnumModel? x, EnumModel? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;
        return x.Equals(y);
    }

    public int GetHashCode(EnumModel obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Name);
        foreach (var value in Values)
        {
            hashCode.Add(value);
        }

        hashCode.Add(obj.Nullable);
        return hashCode.ToHashCode();
    }

    public override bool Equals(object? obj) => Equals(obj as EnumModel);

    public override int GetHashCode() => GetHashCode(this);
}
