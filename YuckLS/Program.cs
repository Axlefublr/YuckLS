global using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
global using OmniSharp.Extensions.LanguageServer.Protocol.Document;
global using OmniSharp.Extensions.LanguageServer.Protocol.Models;
global using OmniSharp.Extensions.LanguageServer.Protocol.Server;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;
global using OmniSharp.Extensions.LanguageServer.Server;
global using Serilog;
global using OmniSharp.Extensions.LanguageServer.Protocol;
global using MediatR;
global using System.Linq;
global using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using YuckLS.Handlers;
using YuckLS.Services;
namespace YuckLS;
public class Program
{
    static ILanguageServer server;
    public async static Task Main(string[] args)
    {
        InitLogging();
        server = await LanguageServer.From(options =>
                    options.WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(p =>
                        p.AddSerilog(Log.Logger)
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Trace)
                )
                    .WithHandler<CompletionHandler>()
                    .WithHandler<TextDocumentSyncHandler>()
                    .WithServices(s =>
                        s.AddSingleton(new ConfigurationItem { Section = "Yuck Server" })
                        .AddSingleton(GetServer)
                        .AddSingleton(new TextDocumentSelector(new TextDocumentFilter { Pattern = "**/*.yuck"}))
                        .AddSingleton<IBufferService,BufferService>()
                        )
                    );
        await server.WaitForExit;
    }
    private static void InitLogging()
    {
        //temporary log location
        string logfile = Path.Combine(Path.GetTempPath(), "yucklsp.log");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logfile)
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }
    private static ILanguageServer GetServer() => server;
    
}