namespace YuckLS.Core;
using YuckLS.Handlers;
internal sealed class YuckCompleter(string _text, ILogger<CompletionHandler> _logger)
{
    public CompletionList GetCompletions()
    {
        var completeTrigger = SExpression.TryGetCompletionTrigger(_text);
        _logger.LogError($"Completion trigger was {completeTrigger}");
        if (completeTrigger == "(")
        {
            return new CompletionList(new[] {
                    new CompletionItem{
                     Label = "defwindow",
                     Documentation = new StringOrMarkupContent("Create a window"),
                     Kind = CompletionItemKind.Class,
                     InsertText = "defwindow"
                    },
                    new CompletionItem{
                     Label = "defwidget",
                     Documentation = new StringOrMarkupContent("Create a widget"),
                     Kind = CompletionItemKind.Class,
                     InsertText = "defwidget"
                    }
            });
        }
        else
        {
            return new CompletionList();
        }
    }
}

