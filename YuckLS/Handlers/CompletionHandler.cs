namespace YuckLS.Handlers;
using System.Threading;
using System.Threading.Tasks;

using YuckLS.Core;
using YuckLS.Services;

internal sealed class CompletionHandler(
        ILogger<CompletionHandler> _logger,
        TextDocumentSelector _textDocumentSelector,
        IBufferService _bufferService
        ) : CompletionHandlerBase
{
    private readonly string[] _triggerChars = { "(", ":" };
    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        var ci = new CompletionItem
        {
            Label = request.Label,
            Kind = request.Kind,
            InsertText = request.InsertText,
            //Command = Command.Create("avalonia.InsertProperty", new { repositionCaret = RepositionCaret() })
        };
        return Task.FromResult(ci);
    }

    public override async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        string text = _bufferService.GetTextTillPosition(request.TextDocument.Uri, request.Position);
        if(text is null)
            return new CompletionList();

        YuckCompleter yuckCompleter = new YuckCompleter(text,_logger);
        var completions = yuckCompleter.GetCompletions();
        if (completions is null || completions.Count() == 0)
            return new CompletionList();
        _logger.LogError($"SIZE OF COMPLETIONS IS {completions.Count()}");
        return completions; 
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = _textDocumentSelector,
            TriggerCharacters = new Container<string>(_triggerChars),
            AllCommitCharacters = new Container<string>("\n"),
            ResolveProvider = true
        };
        // throw new NotImplementedException();
    }
}