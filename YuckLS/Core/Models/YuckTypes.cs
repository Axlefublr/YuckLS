namespace YuckLS.Core.Models;

public enum YuckCompleterTypes
{
    TopLevel,
    Property,
    Widget,
    None
}

public class YuckType
{
    public string name;
    public string[]? properties;
    public string description;
    public bool IsTopLevel = false;
    public bool AreWidgetsEmbeddable = false;
    public bool IsGtkWidgetType = false;
}
/*
 * I should not hard-code these. I just don't know a better way to do it.
 */
public static class YuckTypesProvider
{
    public static YuckType[] YuckTypes = new YuckType[]{
        /*Top Level*/
        new() { name = "defwidget" , description="Define a top level widget" , IsTopLevel = true , AreWidgetsEmbeddable = true },
        new() {name = "defwindow", description = "Define a top level window" , IsTopLevel = true , AreWidgetsEmbeddable = true},
        new() {name = "defvar" , description = "Define a variable" , IsTopLevel = true},
        new() {name = "deflisten" , description = "Define a listener" , IsTopLevel = true} ,
        new() {name="include" , description = "Include another yuck source file" , IsTopLevel = true},
        new() {name = "defpoll" , description = "Define a poll" , IsTopLevel = true},
        /******
         * GTK WIDGETS 
         */
        new() {name = "combo-box-text" , description = "A combo box allowing the user to  choose between several items." , IsGtkWidgetType = true},
        new() { name = "expander" , description = "A widget that can expand and collapse, showing/hiding it's children. " , AreWidgetsEmbeddable = true , IsGtkWidgetType = true},
       new() {name = "revealer" , description = "A widget that can reveal a child with an animation." , AreWidgetsEmbeddable = true, IsGtkWidgetType = true },
       new() { name = "checkbox", description = "A checkbox that can trigger events on checked/unchecked." , IsGtkWidgetType = true,  },

       new() {name = "color-button" , description = "A button opening a color chooser window" , IsGtkWidgetType = true },
       new() { name = "color-chooser" , description = "A color chooser widget" , IsGtkWidgetType = true},
       new() { name = "scale" , description = "A slider widget" , IsGtkWidgetType = true},
       new() { name = "progress" , description = "A progress bar" , IsGtkWidgetType = true},
       new() { name = "input", description = "An input field. For this to be usable, set focusable = true on the window" , IsGtkWidgetType = true},
       new() { name = "button", description = "A button containing any widget as it's child. Events are triggered on release." , IsGtkWidgetType = true},
       new() { name = "image" , description = "A widget displaying an image" , IsGtkWidgetType = true},
       new() { name = "box" , description = "The main layout container" , IsGtkWidgetType = true , AreWidgetsEmbeddable = true},
       new() { name = "overlay" , description = "A widget that places it's children on top of each other." , IsGtkWidgetType = true, AreWidgetsEmbeddable = true},
       new() { name = "tooltip" , description = "A widget that has a custom tooltip." , IsGtkWidgetType = true , AreWidgetsEmbeddable = true},
       new() { name = "centerbox" , description = "A box that must contain EXACTLY 3 children that will be layed out at the start, center and end of the container"},
       new() { name = "scroll" , description = "A container with a single child that can scroll" , IsGtkWidgetType = true , AreWidgetsEmbeddable = true},
        new() { name = "eventbox" , description = "A container which can recieve events and must contain exactly one child." , AreWidgetsEmbeddable = true, IsGtkWidgetType = true},
      new() { name = "label" , description = "A text widget giving you more control over how the text is displayed." , IsGtkWidgetType = true},
      new() { name = "literal" , description = "A widget that allows you to render arbitrary yuck", IsGtkWidgetType = true },
        new() {name = "calendar" , description = "A widget that displays a calendar" , IsGtkWidgetType = true},
        new() {name = "stack" , description = "A widget that displays one of it's children at a time" , IsGtkWidgetType = true, AreWidgetsEmbeddable = true},
        new() {name = "transform" , description = "A widget that applies transformations to its content. They are applied in the following order: rotate -> translate -> scale", IsGtkWidgetType = true , AreWidgetsEmbeddable = true},
        new() {name = "circular-progress" ,description = "A widget that displays a circular progress bar" , IsGtkWidgetType = true},
        new() { name = "graph" , description="A widget that displays a graph showing how a given value changes over time" , IsGtkWidgetType = true},
        new() { name = "systray" , description = "Tray for system notifier icons" , IsGtkWidgetType = true}


    };


}