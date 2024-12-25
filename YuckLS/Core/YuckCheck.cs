namespace YuckLS.Core;

using YuckLS.Handlers;
using YuckLS.Services;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
//extremely experimental syntax checker
internal sealed class YuckCheck(string _text, Microsoft.Extensions.Logging.ILogger<CompletionHandler> _logger, IEwwWorkspace _workspace)
{
    private readonly SExpression _sExpression = new(_text, _logger, _workspace);
    private List<Diagnostic> _diagnostics = new();
    public List<Diagnostic> TryGetDiagnostics()
    {
        GetBracketPairsErrors();
        return _diagnostics;
    }
    private void GetBracketPairsErrors()
    {
        var unclosedBrackets = _sExpression.CheckBracketPairs();
        foreach (int pos in unclosedBrackets)
        {
            _diagnostics.Add(new()
            {
                Range = new Range(convertIndexToPosition(pos), convertIndexToPosition(pos+1)),
                Severity = DiagnosticSeverity.Error,
                Message = "Unmatched bracket pair",
            });
            _logger.LogError($"Position is {pos}");
            //we need to convert the pos to a range.
        }
    }
    //i dont fully understand how this method works
    private Position convertIndexToPosition(int pos)
    {
        //we need to somehow convert an index of the text to a position... after removing comments and string somehow
        int lineNumber = 1;
        int position = 0;
        int index = 0;

        foreach(var line in _text.Split(Environment.NewLine)){
            //check if position is in range of current line
            int lineLength = line.Length + 1; 
            if(index + lineLength > pos){
                position = pos - index + 1;
                break;
            }

            index += lineLength;
            lineNumber ++;
        }
        return new Position(lineNumber,position);
    }
}
