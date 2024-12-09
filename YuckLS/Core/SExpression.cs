namespace YuckLS.Core;

internal static class SExpression
{
    ///<summary>
    ///Try to get a completion trigger from the end of the string which is where the cursor would be. 
    ///From what i've understood for Elkowar's docs, at least 2 different inputs should trigger completion.
    ///1.) Open S-Expression like so : '(', should complete with valid widget types like Box, Window, Expression Types like defpoll, defvar ,e.t.c
    ///2.) Creating properties like so: ':' should complete with valid properties of a containing S-Expression tag e.g (defwindow  :) should autocomplete with properties like :monitor :stacking e.t.c or propertied of the monitor widget 
    ///</summary>
    public static string TryGetCompletionTrigger(string text)
    {
       //handle edge cases of '(' and ':'
       if(text.Last() == '(' || text.Last() == ':'){
           return "(";
       }
       return "";
    }
}

