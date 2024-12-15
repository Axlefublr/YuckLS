using System.Text.RegularExpressions;
using YuckLS.Core.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YuckLS.Test")]
namespace YuckLS.Core;
internal class SExpression
{
    private readonly string _text;
    private readonly string _completionText;
    private readonly ILogger<YuckLS.Handlers.CompletionHandler> _logger;


    public SExpression(string _text, ILogger<YuckLS.Handlers.CompletionHandler> _logger)
    {
        //removed trimming because we need white space to also trigger completion
        //        this._text = _text.Trim();
        this._text = _text;
        this._logger = _logger;
        _completionText = this._text;
        //recursively delete char in quotes to prevent interferance

        _completionText = RemoveAllQuotedChars(_completionText);
        //delete comments from text to prevent interferance, this must be dont after characters in quotes have been removed or completer might break 
        string[] lines = this._completionText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            int semicolonIndex = lines[i].IndexOf(';');
            if (semicolonIndex >= 0)
            {
                lines[i] = lines[i].Substring(0, semicolonIndex);
            }
        }
        //pop last char from text
        _completionText = string.Join(Environment.NewLine, lines)[..^1];
    }


    private const char _openTag = '(';
    private const char _OpenProperties = ':';
    private const char _openCompleterForProperties = ' ';
    ///<summary>
    ///Try to get a completion trigger from the end of the string which is where the cursor would be. 
    ///From what i've understood for Elkowar's docs, at least 2 different inputs should trigger completion.
    ///1.) Open S-Expression like so : '(', should complete with valid widget types like Box, Window, Expression Types like defpoll, defvar ,e.t.c
    ///2.) Creating properties like so: ':' should complete with valid properties of a containing S-Expression tag e.g (defwindow  :) should autocomplete with properties like :monitor :stacking e.t.c or propertied of the monitor widget 
    ///</summary>
    public YuckCompletionContext TryGetCompletionContext()
    {
        _logger.LogError($"Triggering completer for  {_text.Last()}");
        if (_text.Last() == _openTag)
        {
            //if user's cursor is about to create a top level tag 
            if (IsTopLevel()) return new TopLevelYuckCompletionContext();
            //a parent node must exist if the cursor is not top level
            string parentNode = GetParentNode();
            //lookup the parentNode in yuck types
            YuckType parentType = null;
            foreach (var yuckType in YuckTypesProvider.YuckTypes)
            {
                if (yuckType.name == parentNode)
                {
                    parentType = yuckType;
                }
            }
            //if parentType is still null, then parentNode is none standard. Perhaps custom widget?
            if (parentType == null) return default;
            //check if parentType supports GTK widget nodes
            if (parentType.AreWidgetsEmbeddable) return new WidgetYuckCompletionContext();
        }


        else if (_text.Last() == _OpenProperties)
        {
            //try to get the parentNode 
            string parentNode = GetParentNode();
            if (parentNode == null) return default;

            //try to parse the parentNode to a yuck type. Will deal with custom types later
            YuckType parentType = YuckTypesProvider.YuckTypes?.Where(type => type.name == parentNode)?.First();
            if (parentType == null) return default;

            return new PropertyYuckCompletionContext() { parentType = parentType };
        }


        else if (_text.Last() == _openCompleterForProperties)
        {
            string parentNode = GetParentNode();
            string parentPropertyNode = GetParentProperty();
            if (parentPropertyNode == null || parentNode == null) return default;
            
            YuckType parentType = YuckTypesProvider.YuckTypes?.Where(type => type.name == parentNode)?.First();
            YuckProperty parentProperty = parentType.properties.Where(type => type.name == parentPropertyNode)?.First();

            if(parentType is null || parentProperty is null) return default; 
            return new PropertySuggestionCompletionContext() { parentType = parentType, parentProperty = parentProperty};
        }


        else
        { }

        return default;
    }
    ///<summary>
    ///Determine is the cursor position can declare a top level widget
    ///</summary>

    internal protected bool IsTopLevel()
    {
        int depth = 0;
        foreach (char c in _completionText)
        {
            if (c == '(')
            {
                depth++;
            }
            if (c == ')')
            {
                depth--;
            }
        }
        return depth == 0;
    }

    ///<summary>
    ///Gets the parent node for the cursor's position. E.g (box , the parent node is box
    ///</summary>
    internal protected string GetParentNode()
    {
        //i could not figure out how to do this in one command
        //recursively delete any tags that are closed even on multilines
        int matchCount = 0;
        string _cleanedText = _completionText;
        string patternForClosedNodes = @"\(\w+[^\(]*?\)";
        do
        {
            matchCount = Regex.Matches(_cleanedText, patternForClosedNodes, RegexOptions.IgnoreCase).Count;
            _cleanedText = Regex.Replace(_cleanedText, patternForClosedNodes, "", RegexOptions.IgnoreCase);
        } while (matchCount > 0);
        var matches = Regex.Matches(_cleanedText, @"\(\w+", RegexOptions.IgnoreCase);
        if (matches.Count > 0)
        {
            //trim line breaks and remove properties from node
            var value = matches.Last().Value.Trim().Split()[0];
            if (value[0] == '(') return value.Substring(1);
        }
        return null;
    }
    ///<summary>
    ///Will attempt to remove all characters in quotes by looping through every character and checking if the current character can close an open open quote 
    ///</summary>
    ///
    private string RemoveAllQuotedChars(string text)
    {
        //copy text to another variable
        string textCopy = text;
        bool hasEncounteredOpenQuote = false;
        char quote = default;
        int indexOfOpenQuote = 0;
        //temporary(hopefully) very bad very inefficient solution to a very simple problem
        bool shouldRestartLoop = false;
        for (int i = 0; i < textCopy.Length; i++)
        {
            //if we havent found a quote opener, skip to next char 
            if (!hasEncounteredOpenQuote)
            {
                if (text[i] == '\"' || text[i] == '\'' || text[i] == '`')
                {
                    hasEncounteredOpenQuote = true;
                    quote = text[i];
                    indexOfOpenQuote = i;
                }
            }
            else
            {
                //if the current char mathches the open quote 
                if (text[i] == quote)
                {
                    //we need to check that the current quote is not escaped to start another quote 
                    if (text[i - 1] == '\\') { continue; }

                    //if it is not escaped, then it is the end of the original quote. Delete the Substring from the textCopy 
                    textCopy = textCopy.Remove(indexOfOpenQuote, (i - indexOfOpenQuote) + 1);

                    //reset variables 
                    //i cant figure out how to fix the indexing after deleting parts of the string. Feel so stupid rn. 
                    /*hasEncounteredOpenQuote = false;*/
                    /*quote = default;*/
                    /*indexOfOpenQuote = 0;*/
                    /**/
                    /*i = indexOfOpenQuote -1;*/
                    //restart loop 
                    shouldRestartLoop = true;
                    break;

                }
            }
        }
        //temp
        if (shouldRestartLoop) return RemoveAllQuotedChars(textCopy);
        return textCopy;
    }

    ///<summary>
    ///Pretty much just do some simple string parsing to get the property on which the completion request was invoked on. e.g (box :height , height would be the property.
    private string GetParentProperty()
    {
        //select last line 
        string lastLine = _completionText.Split(Environment.NewLine).Last();
        string lastNode = lastLine.Split(" ").Last().Trim();
       
        if(lastNode[0] == ':' && lastNode.Length > 0) return lastNode[1..].Trim();
        return null;
    }
}