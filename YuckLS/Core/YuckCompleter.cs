namespace YuckLS.Core;
using YuckLS.Handlers;
using YuckLS.Core.Models;
internal sealed class YuckCompleter(string _text, ILogger<CompletionHandler> _logger)
{
    private readonly SExpression _sExpression = new(_text);
    public CompletionList GetCompletions()
    {
        var completeTrigger = _sExpression.TryGetCompletionTrigger();
        if (completeTrigger == YuckCompleterTypes.None) return new CompletionList();
        var items = new List<CompletionItem>();
        if (completeTrigger == Models.YuckCompleterTypes.TopLevel)
        {
            foreach (var yuckType in YuckTypesProvider.YuckTypes.Where(p => p.IsTopLevel == true))
            {
                items.Add(new()
                {
                    Label = yuckType.name,
                    Documentation = new StringOrMarkupContent(yuckType.description),
                    Kind = CompletionItemKind.Class,
                    InsertText = yuckType.name
                });
            }
        }
        else if (completeTrigger == Models.YuckCompleterTypes.Widget)
        {
            _logger.LogError("Trying to suggest widgets");
            foreach (var yuckType in YuckTypesProvider.YuckTypes.Where(p => p.IsGtkWidgetType == true))
            {
                items.Add(new()
                {
                    Label = yuckType.name,
                    Documentation = new StringOrMarkupContent(yuckType.description),
                    Kind = CompletionItemKind.Class,
                    InsertText = yuckType.name
                });
            }
        }
        else
        {
            return new CompletionList();
        }
        return new CompletionList(items);
    }
}