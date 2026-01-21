using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using X39.Software.OpenApiGenerator.Common.Services;
using X39.Software.OpenApiGenerator.Services;

namespace X39.Software.OpenApiGenerator;

public sealed class Runner(
    ILogger<Runner> logger,
    ISchemaExtractor schemaExtractor,
    IEndpointExtractor endpointExtractor,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var rootCommand = new RootCommand("OpenApi Generator")
            {
                new Option<string>("--input")
                {
                    Required = false,
                    Arity = ArgumentArity.ZeroOrOne,
                },
                new Option<string>("--output")
                {
                    Required = false,
                    Arity = ArgumentArity.ZeroOrOne,
                },
                new Option<string>("--config")
                {
                    Required = false,
                    Arity = ArgumentArity.ZeroOrOne,
                },
            };
            var parseResult = rootCommand.Parse(Environment.GetCommandLineArgs());
            foreach (var parseError in parseResult.Errors)
            {
                logger.LogError("{ErrorMessage}", parseError.Message);
            }

            if (parseResult.Errors.Count > 0)
            {
                Program.ExitCode = -1;
                return;
            }

            var input = parseResult.GetValue<string>("--input");
            var output = parseResult.GetValue<string>("--output");
            var config = parseResult.GetValue<string>("--config");
            if (config is null && (input is null || output is null))
            {
                logger.LogError("--input and --output or --config must be specified");
                Program.ExitCode = -1;
                return;
            }

            var options = new Dictionary<string, string>();
            if (config is not null)
            {
                foreach (var line in await File.ReadAllLinesAsync(config))
                {
                    var index = line.IndexOf('=');
                    if (index == -1)
                    {
                        continue;
                    }

                    options.Add(line[..index].Trim(), line[(index + 1)..].Trim());
                }
            }

            if (input is null && !options.TryGetValue("input", out input))
            {
                logger.LogError("--input must be supplied or config must contain input");
                Program.ExitCode = -1;
                return;
            }

            if (output is null && !options.TryGetValue("output", out output))
            {
                logger.LogError("--output must be supplied or config must contain output");
                Program.ExitCode = -1;
                return;
            }


            var result = await RunAsync(
                input, output, options);
            Program.ExitCode = result ? 0 : -1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during execution");
            Program.ExitCode = 1;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task<bool> RunAsync(string input, string output,
        Dictionary<string, string> config, CancellationToken cancellationToken = default)
    {
        var fileContents = await ReadInputAsync(input);
        if (fileContents is null)
            return false;

        var result = await OpenApiDocument.LoadAsync(fileContents, settings: new OpenApiReaderSettings(),
            cancellationToken: cancellationToken);
        if (result.Diagnostic is not null)
        {
            if (result.Diagnostic.Warnings.Count > 0)
            {
                foreach (var documentWarning in result.Diagnostic.Warnings)
                {
                    logger.LogWarning("Warning during parsing: {Location} {Message}", documentWarning.Pointer,
                        documentWarning.Message);
                }
            }

            if (result.Diagnostic.Errors.Count > 0)
            {
                foreach (var diagnosticError in result.Diagnostic.Errors)
                {
                    logger.LogError("Error during parsing: {Location} {Message}", diagnosticError.Pointer,
                        diagnosticError.Message);
                }

                return false;
            }
        }

        if (result.Document is null)
        {
            logger.LogError("Failed to parse document");
            return false;
        }

        // ToDo: Process document
        if (!await schemaExtractor.ExtractSchemasFromDocumentAsync(result.Document, cancellationToken))
        {
            logger.LogError("Schema extraction did not succeed");
            return false;
        }

        if (!await endpointExtractor.ExtractEndpointsFromDocumentAsync(result.Document, cancellationToken))
        {
            logger.LogError("Endpoint extraction did not succeed");
            return false;
        }
        return true;
    }

    private async Task<MemoryStream?> ReadInputAsync(string input)
    {
        if (!Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out var uri))
        {
            logger.LogError("Failed to parse input '{Input}' as uri", input);
            return null;
        }

        if (uri.IsFile)
        {
            var absolute = Path.GetFullPath(uri.LocalPath);
            if (!File.Exists(absolute))
            {
                logger.LogError("The input file '{Input}' could not be found", absolute);
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(absolute);
            return new MemoryStream(bytes);
        }
        else
        {
            using var httpClient = new HttpClient();
            var result = await httpClient.GetAsync(uri);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes);
        }
    }
}
