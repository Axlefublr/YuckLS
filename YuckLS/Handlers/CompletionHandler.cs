namespace YuckLS.Handlers;
using System.Threading;
using System.Threading.Tasks;
internal sealed class CompletionHandler : CompletionHandlerBase
{
   public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return default;
        throw new NotImplementedException();
    }

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        return default;
        throw new NotImplementedException();
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return default;
       // throw new NotImplementedException();
    }
}  