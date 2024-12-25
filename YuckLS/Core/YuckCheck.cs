namespace YuckLS.Core;

using YuckLS.Handlers;
using YuckLS.Core.Models;
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
        GetUnkownTypeErrors();
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
                Message = "Unmatched bracket pair.",
            });
            //we need to convert the pos to a range.
        }
    }

    private void GetUnkownTypeErrors(){
        var nodes = _sExpression.GetAllNodes();
        foreach(var node in nodes){
            var typeCollection = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).ToArray();
            //check if this node exists in the collection
            if (typeCollection.Where(p=>p.name == node.nodeName).Count() != 0) continue;

            _diagnostics.Add(new(){
                Range = new Range(convertIndexToPosition(node.index),convertIndexToPosition(node.index + 1)),
                Severity = DiagnosticSeverity.Error,
                Message = $"Type or widget '{node.nodeName}' does not exist. This may cause issues. ",
            });
        }
    }
    //i dont fully understand how this method works
    private Position convertIndexToPosition(int pos)
    {
        //we need to somehow convert an index of the text to a position... after removing comments and string somehow
        int lineNumber = 0;
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
