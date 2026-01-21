using System.CommandLine.Parsing;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using X39.Software.OpenApiGenerator.Common.Services;
using X39.Software.OpenApiGenerator.Services;

namespace X39.Software.OpenApiGenerator;

public static class Program
{
    public static int ExitCode { get; set; }

    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.AddSerilog();
        builder.Services.AddHostedService<Runner>();
        builder.Services.AddTransient<ISchemaExtractor, SchemaExtractor>();
        builder.Services.AddSingleton<IModelRepository, ModelRepository>();
        var app = builder.Build();
        await app.RunAsync();
        return ExitCode;
    }
}