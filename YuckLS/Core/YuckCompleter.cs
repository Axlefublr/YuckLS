namespace YuckLS.Core;
using YuckLS.Handlers;
using YuckLS.Core.Models;
internal sealed class YuckCompleter(string _text, ILogger<CompletionHandler> _logger)
{
    private readonly SExpression _sExpression = new(_text);
    public CompletionList GetCompletions()
    {
        var completeTrigger = _sExpression.TryGetCompletionTrigger();
        var items = new List<CompletionItem>();
        _logger.LogError($"Completion trigger was {completeTrigger}");
        if (completeTrigger == Models.YuckCompleterTypes.TopLevel)
        {
            foreach (var yuckType in YuckTypesProvider.YuckTopLevelTypes)
            {
                items.Add(new()
                {
                    Label = yuckType.name,
                    Documentation = new StringOrMarkupContent(yuckType.description),
                    Kind = CompletionItemKind.Class,
                    InsertText = yuckType.name
                });
            }
            return new CompletionList(items);
        }
        else
        {
            return new CompletionList();
        }
    }
}