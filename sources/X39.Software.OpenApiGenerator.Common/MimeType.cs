namespace X39.Software.OpenApiGenerator.Common;

public readonly record struct MimeType(string Name)
{
    public static implicit operator string(MimeType mimeType) => mimeType.Name;
    public static implicit operator MimeType(string key) => new (key);
    public override string ToString() => Name;
}
