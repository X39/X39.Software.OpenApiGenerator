using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using X39.Software.OpenApiGenerator.Common;

namespace X39.Software.OpenApiGenerator.Generators.CSharp;

public sealed class CSharpGenerator(ILogger<CSharpGenerator> logger) : IGenerator
{
    private string _baseNamespace = string.Empty;

    public async ValueTask<bool> ConfigureAsync(
        ImmutableDictionary<string, string> config,
        CancellationToken cancellationToken
    )
    {
        // We can ignore nullable violations with field assignments here,
        // as program flow prevents calling the remaining methods.

        var result = true;
        if (config.TryGetValue("Namespace", out var baseNamespace))
            _baseNamespace = baseNamespace;
        else
        {
            result = false;
            logger.LogError("'Namespace' not found in configuration");
        }

        return result;

    }

    public ValueTask<bool> ValidateAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(true);
    }

    public async ValueTask<bool> GenerateAsync(string output, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        return true;
    }
}
