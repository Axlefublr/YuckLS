namespace YuckLS.Core;
using YuckLS.Handlers;
using YuckLS.Core.Models;
internal sealed class YuckCompleter(string _text, ILogger<CompletionHandler> _logger, YuckLS.Services.IEwwWorkspace _workspace )
{
    private readonly SExpression _sExpression = new(_text, _logger , _workspace);
    public CompletionList GetCompletions()
    {
        var completionContext = _sExpression.TryGetCompletionContext();
        if (completionContext == default) return new CompletionList();
        return completionContext.Completions();
    }
}