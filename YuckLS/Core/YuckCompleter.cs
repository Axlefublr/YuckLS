namespace YuckLS.Core;
using YuckLS.Handlers;
using YuckLS.Core.Models;
internal sealed class YuckCompleter(string _text, ILogger<CompletionHandler> _logger)
{
    private readonly SExpression _sExpression = new(_text);
    public CompletionList GetCompletions()
    {
        var completionContext = _sExpression.TryGetCompletionContext();
        if (completionContext == default) return new CompletionList();
        var items = new List<CompletionItem>();
        if (completionContext is TopLevelYuckCompletionContext topLevelCompletionContext)
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
        else if (completionContext is WidgetYuckCompletionContext widgetCompletionContext)
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
        else if(completionContext is PropertyYuckCompletionContext propertyCompletionContext ){
            var properties =  propertyCompletionContext.parentType.properties;
            if(properties is null || properties.Count() == 0) return new CompletionList();
            foreach(var property in properties){
                items.Add(new(){
                    Label = property.name,
                    Documentation = new StringOrMarkupContent(property.description),
                    Kind = CompletionItemKind.Property,
                    InsertText = property.name
                });
            }
            _logger.LogError("Trying to suggest properties");
        }
        else
        {
            return new CompletionList();
        }
        return new CompletionList(items);
    }
}