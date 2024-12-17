namespace YuckLS.Services;
using YuckLS.Core.Models;
using YuckLS.Core;
internal interface IEwwWorkspace
{
    public void LoadWorkspace();
}
internal sealed class EwwWorkspace(ILogger<EwwWorkspace> _logger, ILoggerFactory _loggerFactory, IServiceProvider _serviceProvider) : IEwwWorkspace
{
    //store include paths and whether they have been visited 
    private System.Collections.Concurrent.ConcurrentDictionary<string,bool> _includePaths = new();
    public YuckType[] UserDefinedTypes = new YuckType[] {};
    private string _ewwRoot;
    public void LoadWorkspace()
    {
        //current directory where the LSP client started the executable
        var current_path = Directory.GetCurrentDirectory();

        //recursively traverse upwards until we find an eww.yuck file.
        while (_ewwRoot == null)
        {
            if (File.Exists(Path.Combine(current_path, "eww.yuck")))
            {
                _ewwRoot = Path.Combine(current_path, "eww.yuck");
            }
            else
            {
                var parent_dir = Directory.GetParent(current_path);
                if (parent_dir is null) return; //no parent dir, break for now 
                current_path = parent_dir.FullName;
            }
        }
        LoadVariables();
    }
    internal protected void LoadVariables()
    {
        var ewwRootBuffer = File.ReadAllText(_ewwRoot);
        var _completionHandlerLogger = _loggerFactory.CreateLogger<YuckLS.Handlers.CompletionHandler>();
        var _ewwWorkspace = _serviceProvider.GetRequiredService<IEwwWorkspace>();
        var sExpression = new SExpression(ewwRootBuffer, _completionHandlerLogger, _ewwWorkspace);
        var customVariables = sExpression.GetVariables();
        UserDefinedTypes = customVariables.ToArray();
    }
}