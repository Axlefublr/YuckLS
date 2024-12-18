
using YuckLS.Handlers;
using YuckLS.Services;
namespace YuckLS;
public class Program
{
    static ILanguageServer? server;
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
                        .AddSingleton<IEwwWorkspace,EwwWorkspace>()
                        )
                    );
        await server.WaitForExit;
    }
    private static void InitLogging()
    {
        //temporary log location for my computer 
        if(Path.Exists("/home/noble/Documents/YuckLS/YuckLS/")){
             string logfile = Path.Combine("/home/noble/Documents/YuckLS/YuckLS/", "yucklsp.log");
             Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logfile)
            .Enrich.FromLogContext()
            .MinimumLevel.Error()
            .CreateLogger();
        }
        else{
        string logfile = Path.Combine(Path.GetTempPath(), "yucklsp.log");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logfile)
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .CreateLogger();
        }
    }
    private static ILanguageServer? GetServer() => server;

 
}