namespace YuckLS.Core;

using YuckLS.Handlers;
using YuckLS.Core.Models;
using YuckLS.Services;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
//extremely experimental syntax checker
internal sealed class YuckCheck(string _text, Microsoft.Extensions.Logging.ILogger<CompletionHandler> _logger, IEwwWorkspace _workspace)
{
    private readonly SExpression _sExpression = new(_text, _logger, _workspace);
    //thread safe collection
    private System.Collections.Concurrent.ConcurrentBag<Diagnostic> _diagnostics = new();
    public async Task<List<Diagnostic>> TryGetDiagnosticsAsync(CancellationToken ctx)
    {
        List<Task> diagnosticTasks = new(){
            Task.Run(() => {
                 GetBracketPairsErrors(ctx);
            }),
            Task.Run(()=>{
                GetUnkownTypeErrors(ctx);
            }),
            Task.Run(()=>{
                GetInvalidTopLevelDefinitionErrors(ctx);
            }),
            Task.Run(()=>{
                GetInvalidPropertyDefinition(ctx);
            }),
            Task.Run(()=>{
                GetInvalidPropertyValuesErrors(ctx);
            })
        };
        await Task.WhenAll(diagnosticTasks);
        return _diagnostics.ToList();
    }
    private void GetBracketPairsErrors(CancellationToken ctx)
    {
        if (ctx.IsCancellationRequested) return;
        var unclosedBrackets = _sExpression.CheckBracketPairs();
        foreach (int pos in unclosedBrackets)
        {
            if (ctx.IsCancellationRequested) return;
            _diagnostics.Add(new()
            {
                Range = new Range(convertIndexToPosition(pos), convertIndexToPosition(pos + 1)),
                Severity = DiagnosticSeverity.Error,
                Message = "Unmatched bracket pair.",
            });
            //we need to convert the pos to a range.
        }
    }

    private void GetUnkownTypeErrors(CancellationToken ctx)
    {
        if (ctx.IsCancellationRequested) return;
        var nodes = _sExpression.GetAllNodes();
        foreach (var node in nodes)
        {
            if (ctx.IsCancellationRequested) return;
            var typeCollection = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).ToArray();
            //check if this node exists in the collection
            if (typeCollection.Where(p => p.name == node.nodeName).Count() != 0) continue;

            _diagnostics.Add(new()
            {
                Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index + node.nodeName.Length)),
                Severity = DiagnosticSeverity.Error,
                Message = $"Type or widget '{node.nodeName}' does not exist. This may cause issues. ",
            });
        }
    }
    //will probably fuse this into a generic function that just looks for definitions in the wrong place in general
    private void GetInvalidTopLevelDefinitionErrors(CancellationToken ctx)
    {
        if (ctx.IsCancellationRequested) return;
        var nodes = _sExpression.GetAllNodes();
        //plan is to split the text at the index of each node and check is the first half would be able to declare a top level widget
        foreach (var node in nodes)
        {
            if (ctx.IsCancellationRequested) return;
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
                    Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index + node.nodeName.Length)),
                    Message = $"Did not expect '{node.nodeName}' here, expected top level declaration: defwindow, defwidget, defpoll, defvar, include, deflisten.",
                    Severity = DiagnosticSeverity.Error
                });
            }
            else if (!sexpression.IsTopLevel() && yuckType.IsTopLevel)
            {
                _diagnostics.Add(new()
                {
                    Range = new Range(convertIndexToPosition(node.index), convertIndexToPosition(node.index + node.nodeName.Length)),
                    Message = $"'{node.nodeName}' should be declared at the top level and not nested within another declaration",
                    Severity = DiagnosticSeverity.Error
                });
            }
        }
    }
    private void GetInvalidPropertyDefinition(CancellationToken ctx)
    {
        if (ctx.IsCancellationRequested) return;
        var properties = _sExpression.GetAllProperyDefinitions();
        //split the text at the index of each property, get the parent type and check if that type has a defintion for this property
        foreach (var property in properties)
        {
            if (ctx.IsCancellationRequested) return;
            var part1 = _sExpression.FullText.Substring(0, property.index);
            //create a new sexpression
            var sexpression = new SExpression(part1, _logger, _workspace);
            string? parentNode = sexpression.GetParentNode();
            if (parentNode is null) continue;
            var parentType = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).Where(p => p.name == parentNode).FirstOrDefault();
            if (parentType is null) continue;

            //try to get property,
            var realProperty = parentType.properties.Where(p => p.name == property.propertyName).FirstOrDefault();
            if (realProperty is null)
            {
                _diagnostics.Add(new()
                {
                    Range = new Range(convertIndexToPosition(property.index), convertIndexToPosition(property.index + property.propertyName.Length)),
                    Message = $"'{parentType.name}' does not contain a definition for {property.propertyName}",
                    Severity = DiagnosticSeverity.Error
                });
            }
        }
    }

    private void GetInvalidPropertyValuesErrors(CancellationToken ctx)
    {
        if (ctx.IsCancellationRequested) return;
        var propertyValues = _sExpression.GetAllPropertyValues();
        foreach (var propertyValue in propertyValues)
        {
            if (ctx.IsCancellationRequested) return;
            //GET PARENT NODE AND PROPERTY
            var part1 = _sExpression.FullText.Substring(0, propertyValue.index);
            //create a new sexpression
            var sexpression = new SExpression(part1, _logger, _workspace);
            string? parentNode = sexpression.GetParentNode();
            string? parentProperty = propertyValue.property;

            if (parentNode is null || parentProperty is null) continue;

            var parentType = YuckTypesProvider.YuckTypes.Concat(_workspace.UserDefinedTypes).Where(p => p.name == parentNode).FirstOrDefault();
            if (parentType is null) continue;

            var parentPropertyType = parentType.properties.Where(p => p.name == parentProperty).FirstOrDefault();
            if (parentPropertyType is null) continue;

            var diagnosticsRange = new Range(convertIndexToPosition(propertyValue.index), convertIndexToPosition(propertyValue.index + propertyValue.propertyValue.Length));
            string readableExpectedType = "";
            switch (parentPropertyType.dataType)
            {
                case YuckDataType.YuckString:
                    readableExpectedType = "string";
                    break;
                case YuckDataType.YuckInt:
                    readableExpectedType = "int";
                    break;
                case YuckDataType.YuckBool:
                    readableExpectedType = "bool";
                    break;
                case YuckDataType.YuckDuration:
                    readableExpectedType = "duration";
                    break;
                case YuckDataType.YuckFloat:
                    readableExpectedType = "float";
                    break;
                default:
                    readableExpectedType = "custom";
                    break;
            }
            // _logger.LogError($"property is {parentProperty} and property value is {propertyValue.propertyValue}");
            //wildcard, every property should be able to take a user defined variable although with a warning that the type might not match;
            if (_workspace.UserDefinedVariables.Where(p => p.name == propertyValue.propertyValue).Count() != 0)
            {
                _diagnostics.Add(new()
                {
                    Range = diagnosticsRange,
                    Message = $"{parentProperty} expects {readableExpectedType} here but got a variable instead. This might cause issues ",
                    Severity = DiagnosticSeverity.Warning
                });
                continue;
            }
            if (parentPropertyType.dataType == YuckDataType.YuckString)
            {
                //should have single or double quotes on either side. Pretty easy
                if ((propertyValue.propertyValue.First() == '\"' && propertyValue.propertyValue.Last() == '\"')
                    || (propertyValue.propertyValue.First() == '\'' && propertyValue.propertyValue.Last() == '\'')
                )
                {
                    continue;
                }
                _diagnostics.Add(new()
                {
                    Range = diagnosticsRange,
                    Message = $"{parentProperty} expects type string here but string not given",
                    Severity = DiagnosticSeverity.Warning
                });
            }
            else if (parentPropertyType.dataType == YuckDataType.YuckInt)
            {
                //try to parse this as an int,
                if (!int.TryParse(propertyValue.propertyValue, out int placeholder))
                {
                    _diagnostics.Add(new()
                    {
                        Range = diagnosticsRange,
                        Message = $"{parentProperty} expects type int, but int not given",
                        Severity = DiagnosticSeverity.Warning
                    });
                }
            }
            else if (parentPropertyType.dataType == YuckDataType.YuckBool)
            {
                //try to parse as a bool
                if (!bool.TryParse(propertyValue.propertyValue, out bool placeholder))
                {
                    _diagnostics.Add(new()
                    {
                        Range = diagnosticsRange,
                        Message = $"{parentProperty} expects type bool, but bool not given",
                        Severity = DiagnosticSeverity.Warning
                    });
                }
            }
            else if (parentPropertyType.dataType == YuckDataType.YuckFloat)
            {
                //try to parse as a float
                if (!float.TryParse(propertyValue.propertyValue, out float placeholder))
                {
                    _diagnostics.Add(new()
                    {
                        Range = diagnosticsRange,
                        Message = $"{parentProperty} expects type float, but float not given",
                        Severity = DiagnosticSeverity.Warning
                    });
                }
            }
            else if (parentPropertyType.dataType == YuckDataType.YuckDuration)
            {
                //figure thi out later
            }
            else
            {
                //wildcard, do nothing for now
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
