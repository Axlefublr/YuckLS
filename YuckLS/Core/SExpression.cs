using System.Text.RegularExpressions;

using YuckLS.Core.Models;
namespace YuckLS.Core;
public class SExpression
{
    private readonly string _text;
    private readonly string _refinedText;
    public SExpression(string _text)
    {
        this._text = _text.Trim();

        //delete comments from text to prevent interferance
        string[] lines = this._text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            int semicolonIndex = lines[i].IndexOf(';');
            if (semicolonIndex >= 0)
            {
                lines[i] = lines[i].Substring(0, semicolonIndex);
            }
        }

        //pop last char from text
        _refinedText = string.Join(Environment.NewLine, lines)[..^1];
    }
    private const char _openTag = '(';
    private const char _OpenProperties = ':';
    ///<summary>
    ///Try to get a completion trigger from the end of the string which is where the cursor would be. 
    ///From what i've understood for Elkowar's docs, at least 2 different inputs should trigger completion.
    ///1.) Open S-Expression like so : '(', should complete with valid widget types like Box, Window, Expression Types like defpoll, defvar ,e.t.c
    ///2.) Creating properties like so: ':' should complete with valid properties of a containing S-Expression tag e.g (defwindow  :) should autocomplete with properties like :monitor :stacking e.t.c or propertied of the monitor widget 
    ///</summary>
    public YuckCompletionContext TryGetCompletionContext()
    {
        if (_text.Last() == _openTag)
        {
            //if user's cursor is about to create a top level tag 
            if (IsTopLevel())
            {
                return new TopLevelYuckCompletionContext();
            }
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
            if (parentType == null)
            {
                return default;
            }
            //check if parentType supports GTK widget nodes
            if (parentType.AreWidgetsEmbeddable)
            {
                return new WidgetYuckCompletionContext();
            }
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
        else
        {

        }
        return default;
    }
    ///<summary>
    ///Determine is the cursor position can declare a top level widget
    ///</summary>

    //this should not be public, i dont know how to test with it private without using reflection or hacking things together
    public bool IsTopLevel()
    {
        int depth = 0;
        foreach (char c in _refinedText)
        {
            if (c == '(') depth++;
            if (c == ')') depth--;

        }
        return depth == 0 ? true : false;
    }

    ///<summary>
    ///Gets the parent node for the cursor's position. E.g (box , the parent node is box
    ///</summary>
    public string GetParentNode()
    {
        //i could not figure out how to do this in one command
        //recursively delete any tags that are closed even on multilines
        int matchCount = 0;
        string _cleanedText = _refinedText;
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

}