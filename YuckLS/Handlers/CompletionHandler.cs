using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
[assembly: InternalsVisibleTo("YuckLS.Test")]
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