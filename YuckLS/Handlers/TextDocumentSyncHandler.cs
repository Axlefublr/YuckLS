using YuckLS.Services;

namespace YuckLS.Handlers;

internal sealed class TextDocumentSyncHandler(
        ILogger<TextDocumentSyncHandler> _logger,
        ILanguageServerConfiguration _configuration,
        IBufferService _bufferService,
        TextDocumentSelector _textDocumentSelector,
        IEwwWorkspace _ewwWorkspace
        ) : TextDocumentSyncHandlerBase
{
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, uri.Scheme!, "yuck");
    }

    public override async Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _ewwWorkspace.LoadWorkspace();
        var conf = await _configuration.GetScopedConfiguration(request.TextDocument.Uri, cancellationToken);
        _bufferService.Add(request.TextDocument.Uri, request.TextDocument.Text);
        return Unit.Value;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        foreach (var change in request.ContentChanges)
        {
            if (change.Range != null)
            {
                _bufferService.ApplyIncrementalChange(request.TextDocument.Uri, change.Range, change.Text);
            }
            else
            {
                _bufferService.ApplyFullChange(request.TextDocument.Uri, change.Text);
            }
        }
        return Task.FromResult(Unit.Value);
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        //reload workspace when buffer is saved , i'm not sure that this is the most ideal way to do this 
        _ewwWorkspace.LoadWorkspace();
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