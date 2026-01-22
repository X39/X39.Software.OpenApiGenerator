using System.Collections.Immutable;

namespace X39.Software.OpenApiGenerator.Common;

public interface IGenerator
{
    ValueTask<bool> ConfigureAsync(ImmutableDictionary<string, string> config, CancellationToken cancellationToken);
    ValueTask<bool> ValidateAsync(CancellationToken cancellationToken);
    ValueTask<bool> GenerateAsync(string output, CancellationToken cancellationToken);
}
