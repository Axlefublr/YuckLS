using YuckLS.Core.Models;
namespace YuckLS.Core;
public class SExpression(string _text)
{

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
       if(_text.Last() == _openTag){
           if(IsTopLevel()){
               return YuckCompleterTypes.TopLevel;
           }
           return YuckCompleterTypes.Widget;
       }
       else{
           return YuckCompleterTypes.Widget;
       }
    }
    ///<summary>
    ///Determine is the cursor position can declare a top level widget
    ///</summary>
    public bool IsTopLevel()
    {
        //pop last char from text
        var text = _text[..^1];
        //we do this by simply checking if the text is surrounded by any parenthesis
        int depth = 0;
        foreach(char c in text){
            if(c == '(') depth ++;
            if(c == ')') depth--;

        }
        return depth == 0 ? true : false;
    }

}