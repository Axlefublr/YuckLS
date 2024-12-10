using System.Text.RegularExpressions;

using YuckLS.Core.Models;
namespace YuckLS.Core;
public class SExpression
{
    private readonly string _text;
    private readonly string _refinedText;
    public SExpression(string _text)
    {
        this._text = _text;
    }
    private const char _openTag = '(';
    private const char _OpenProperties = ':';
    ///<summary>
    ///Try to get a completion trigger from the end of the string which is where the cursor would be. 
    ///From what i've understood for Elkowar's docs, at least 2 different inputs should trigger completion.
    ///1.) Open S-Expression like so : '(', should complete with valid widget types like Box, Window, Expression Types like defpoll, defvar ,e.t.c
    ///2.) Creating properties like so: ':' should complete with valid properties of a containing S-Expression tag e.g (defwindow  :) should autocomplete with properties like :monitor :stacking e.t.c or propertied of the monitor widget 
    ///</summary>
    public YuckCompleterTypes TryGetCompletionTrigger()
    {
        if (_text.Last() == _openTag)
        {
            //if user's cursor is about to create a top level tag 
            if (IsTopLevel())
            {
                return YuckCompleterTypes.TopLevel;
            }
            //a parent node must exist if the cursor is not top level
            string parentNode = GetParentNode();
            //lookup the parentNode in yuck types
            YuckType parentType = null;
            foreach(var yuckType in YuckTypesProvider.YuckTypes){
                if(yuckType.name == parentNode){
                    parentType = yuckType;
                }
            }
            //if parentType is still null, then parentNode is none standard. Perhaps custom widget?
            if(parentType == null){
                return YuckCompleterTypes.None;
            }
            //check if parentType supports GTK widget nodes
            if(parentType.AreWidgetsEmbeddable){
                return YuckCompleterTypes.Widget;
            }
        }
        return YuckCompleterTypes.None; 
    }
    ///<summary>
    ///Determine is the cursor position can declare a top level widget
    ///</summary>

    //this should not be public, i dont know how to test with it private without using reflection or hacking things together
    public bool IsTopLevel()
    {
        //pop last char from text
        var text = _text[..^1];
        //we do this by simply checking if the text is surrounded by any parenthesis
        int depth = 0;
        foreach (char c in text)
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
        //pop last char from Text
        var text = _text[..^1];
        var matches = Regex.Matches(text, @"\(\w+[^\(\)\r\n]*\s*(?!.*\))", RegexOptions.IgnoreCase); 
        if (matches.Count > 0)
        {
            //trim line breaks and remove properties from node
            var value = matches.Last().Value.Trim().Split()[0];
            if(value[0] == '(') return value.Substring(1);
        }
        return null;
    }

}