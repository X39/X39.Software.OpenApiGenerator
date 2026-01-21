namespace X39.Software.OpenApiGenerator.Common;

public readonly record struct HttpStatusCode(int Code)
{
    public static implicit operator int(HttpStatusCode httpStatusCode) => httpStatusCode.Code;
    public static implicit operator HttpStatusCode(int key) => new (key);
    public override string ToString() => Code.ToString();
}
