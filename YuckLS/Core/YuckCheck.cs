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
        GetInvalidTopLevelDefinitionErrors();
        return _diagnostics;
    }
    private void GetBracketPairsErrors()
    {
        var unclosedBrackets = _sExpression.CheckBracketPairs();
        foreach (int pos in unclosedBrackets)
        {
            _diagnostics.Add(new()
            {
                Range = new Range(convertIndexToPosition(pos), convertIndexToPosition(pos + 1)),
                Severity = DiagnosticSeverity.Error,
                Message = "Unmatched bracket pair.",
            });
            //we need to convert the pos to a range.
        }
    }

    private void GetUnkownTypeErrors()
    {
        var nodes = _sExpression.GetAllNodes();
        foreach (var node in nodes)
        {
            var typeCollection = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).ToArray();
            //check if this node exists in the collection
            if (typeCollection.Where(p => p.name == node.nodeName).Count() != 0) continue;

            _diagnostics.Add(new()
            {
                Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index + 1)),
                Severity = DiagnosticSeverity.Error,
                Message = $"Type or widget '{node.nodeName}' does not exist. This may cause issues. ",
            });
        }
    }
    //will probably fuse this into a generic function that just looks for definitions in the wrong place in general
    private void GetInvalidTopLevelDefinitionErrors()
    {
        var nodes = _sExpression.GetAllNodes();
        //plan is to split the text at the index of each node and check is the first half would be able to declare a top level widget
        foreach (var node in nodes)
        {
            var yuckType = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).Where(p => p.name == node.nodeName).FirstOrDefault();
            if (yuckType is null) continue;
            //split the string,
            string part1 = _sExpression.FullText.Substring(0, node.index);
            //create a new sexpression
            var sexpression = new SExpression(part1, _logger, _workspace);
            //check that is an invalid top level declaration
            if (sexpression.IsTopLevel() && !yuckType.IsTopLevel)
            {
                _diagnostics.Add(new()
                {
                    Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index + 1)),
                    Message = $"Did not expect '{node.nodeName}' here, expected top level declaration: defwindow, defwidget, defpoll, defvar, include, deflisten.",
                    Severity = DiagnosticSeverity.Error
                });
            }
            else if(!sexpression.IsTopLevel() && yuckType.IsTopLevel){
                _diagnostics.Add(new(){
                    Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index+1)),
                    Message = $"'{node.nodeName}' should be declared at the top level and not nested within another declaration",
                    Severity = DiagnosticSeverity.Error
                });
            }
        }
    }
    //i dont fully understand how this method works
    private Position convertIndexToPosition(int pos)
    {
        //we need to somehow convert an index of the text to a position... after removing comments and string somehow
        int lineNumber = 0;
        int position = 0;
        int index = 0;

        foreach (var line in _text.Split(Environment.NewLine))
        {
            //check if position is in range of current line
            int lineLength = line.Length + 1;
            if (index + lineLength > pos)
            {
                position = pos - index + 1;
                break;
            }

            index += lineLength;
            lineNumber++;
        }
        return new Position(lineNumber, position);
    }
}
