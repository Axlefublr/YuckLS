namespace YuckLS.Core.Models;

public enum YuckCompleterTypes{
    TopLevel,
    Property,
    Widget
}

public class YuckType{
    public string name;
    public string[] ?properties;
    public string description;
}
/*
 * I should not hard-code these. I just don't know a better way to do it.
 */
public static class YuckTypesProvider{
    public static YuckType[] YuckTopLevelTypes = new YuckType[]{
        new() { name = "defwidget" , description="Define a top level widget" },
        new() {name = "defwindow", description = "Define a top level window" },
        new() {name = "defvar" , description = "Define a variable"},
        new() {name = "deflisten" , description = "Define a listener"},
        new() {name="include" , description = "Include another yuck source file"},
        new() {name = "defpoll" , description = "Define a poll"}
    };
} 