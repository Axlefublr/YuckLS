
namespace YuckLS.Services;
using YuckLS.Core.Models;
using YuckLS.Core;
public interface IEwwWorkspace
{
    public void LoadWorkspace();
}
internal sealed class EwwWorkspace(ILogger<EwwWorkspace> _logger, ILoggerFactory _loggerFactory, IServiceProvider _serviceProvider) : IEwwWorkspace
{
    //store include paths and whether they have been visited 
    private System.Collections.Concurrent.ConcurrentDictionary<string, bool> _includePaths = new();
    public YuckType[] UserDefinedTypes = new YuckType[] { };
    private string _ewwRoot;
    public void LoadWorkspace()
    {
        _logger.LogError("Loading workspace");
        //empty user types and include paths
        UserDefinedTypes = new YuckType[] {};
        _includePaths = new();
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
        //run initial load at eww.yuck 
        LoadVariables(_ewwRoot);

        //recursively load any included files by checking if any file path has not been visited 
        while (_includePaths.Where(p => p.Value == false).Count() > 0)
        {
            LoadVariables(_includePaths.Where(p => p.Value == false).First().Key);
        }
    }
    internal protected void LoadVariables(string filepath)
    {
        if (!File.Exists(filepath))
        {
            _includePaths.AddOrUpdate(filepath, true, (key, oldvalue) => true);
            return;
        };
        bool isLoaded = false;
        _includePaths.TryGetValue(filepath, out isLoaded);
        //prevent infinte loads 
        if (isLoaded == true) return;
        //mark this file as loaded
        _includePaths.AddOrUpdate(filepath, true, (key, oldvalue) => true);
        var ewwRootBuffer = File.ReadAllText(filepath);
        var _completionHandlerLogger = _loggerFactory.CreateLogger<YuckLS.Handlers.CompletionHandler>();
        var _ewwWorkspace = _serviceProvider.GetRequiredService<IEwwWorkspace>();
        var sExpression = new SExpression(ewwRootBuffer, _completionHandlerLogger, _ewwWorkspace);
        var customVariables = sExpression.GetVariables();
        UserDefinedTypes = UserDefinedTypes.Concat(customVariables.ToArray()).ToArray();
        List<string> includePaths = sExpression.GetIncludes();
        foreach (string include in includePaths)
        {
            string parentDir = Directory.GetParent(_ewwRoot).FullName;
            string includePath = Path.Combine(parentDir, include);
            _includePaths.TryAdd(includePath, false);
        }
    }
}