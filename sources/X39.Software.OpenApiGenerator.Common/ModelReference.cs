namespace X39.Software.OpenApiGenerator.Common;

public readonly record struct ModelReference(string Name)
{
    public static implicit operator string(ModelReference modelReference) => modelReference.Name;
    public static implicit operator ModelReference(string key) => new (key);
}
