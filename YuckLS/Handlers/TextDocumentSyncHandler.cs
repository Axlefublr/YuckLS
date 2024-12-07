namespace YuckLS.Handlers;

internal sealed class TextDocumentSyncHandler(ILogger<TextDocumentSyncHandler> _logger) : TextDocumentSyncHandlerBase
{
    private readonly TextDocumentSelector _textDocumentSelector = new TextDocumentSelector(
            new TextDocumentFilter
            {
                Pattern = "**/*.yuck"
            }
        );
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, uri.Scheme!, "yuck");
    }

    public override async Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogTrace("FIle was opened");
        return Unit.Value;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        //document changed 
        _logger.LogTrace($"You've changed this yuck document at {request.ContentChanges.First()}");
        return Task.FromResult(Unit.Value);
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogTrace($"did save file {request.TextDocument.Uri}");
        return Task.FromResult(Unit.Value);
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Unit.Value);
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSyncRegistrationOptions
        {
            DocumentSelector = _textDocumentSelector,
            Change = OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities.TextDocumentSyncKind.Incremental,
            Save = new SaveOptions() { IncludeText = false }

        };
    }
}