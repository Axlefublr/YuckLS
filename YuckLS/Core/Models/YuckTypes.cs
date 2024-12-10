namespace YuckLS.Core.Models;

//really wish i had rust enums right about now
public abstract class YuckCompletionContext { }
public class TopLevelYuckCompletionContext : YuckCompletionContext { }
public class WidgetYuckCompletionContext : YuckCompletionContext { }
public class PropertyYuckCompletionContext : YuckCompletionContext
{
    public YuckType parentType;
}

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
    public YuckProperty[] properties;
    public string description;
    public bool IsTopLevel = false;
    public bool AreWidgetsEmbeddable = false;
    public bool IsGtkWidgetType = false;
}
public class YuckProperty
{
    public string name;
    public string description;
    public YuckDataType? dataType;
    public string[]? possibleValues;
}

public enum YuckDataType
{
    YuckString,
    YuckInt,
    YuckBool,
    Custom
};
/*
 * I should not hard-code these. I just don't know a better way to do it.
 */
public static class YuckTypesProvider
{
    private static readonly YuckProperty[] _commonGtkWidgetProperties = new YuckProperty[] {
    new() {
      name = "class",
      description = "css class name",
      dataType = YuckDataType.YuckString
    },
    new() {
      name = "valign",
      description = "how to align this vertically. possible values: 'fill', 'baseline', 'center', 'start', 'end'",
      dataType = YuckDataType.YuckString
    },
    new() {
      name = "halign",
      description = "how to align this horizontally. possible values: 'fill', 'baseline', 'center', 'start', 'end'",
      dataType = YuckDataType.YuckString
    },
    new() {
      name = "vexpand",
      description = "should this container expand vertically. Default: false.",
      dataType = YuckDataType.YuckBool,
    },
    new() {
      name = "hexpand",
      description = "should this container expand horizontally. Default: false.",
      dataType = YuckDataType.YuckBool
    },
    new() {
      name = "width",
      description = "int width of this element. note that this can not restrict the size if the contents stretch it",
      dataType = YuckDataType.YuckInt
    },
    new() {
      name = "height",
      description = "int width of this element. note that this can not restrict the size if the contents stretch it",
      dataType = YuckDataType.YuckInt
    },
    new() {
      name = "active",
      description = "If this widget can be interacted with",
      dataType = YuckDataType.YuckBool,
    },
    new() {
      name = "tooltip",
      description = " tooltip text (on hover)",
      dataType = YuckDataType.YuckString,
    },
    new() {
      name = "visible",
      description = "visibility of the widget",
      dataType = YuckDataType.YuckBool,
    },
    new() {
      name = "style",
      description = " inline scss style applied to the widget",
      dataType = YuckDataType.YuckString,
    },
    new() {
      name = "css",
      description = "scss code applied to the widget, i.e.: button {color: red;}",
      dataType = YuckDataType.YuckString
    }
  };
    public static readonly YuckType[] YuckTypes = new YuckType[] {
    /* Top Level */
    new() {
      name = "defwidget",
      description = "Define a top level widget",
      IsTopLevel = true,
      AreWidgetsEmbeddable = true,
    },
    new() {
      name = "defwindow",
      description = "Define a top level window",
      IsTopLevel = true,
      AreWidgetsEmbeddable = true,
      properties = new YuckProperty[] {
          new() {
              name = "monitor",
              description = "The monitor index to display this window on",
              dataType = YuckDataType.YuckString,
          },
          new(){
              name = "geometry",
              description = "The geometry of the window",
              dataType = YuckDataType.Custom,
          },
          new(){
              name = "stacking",
              description = "Where the window should appear in the stack. Possible values: fg, bg. X11 ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new[] {"fg","bg"},
          },
          new(){
              name = "wm-ignore",
              description = "Whether the window manager should ignore this window. This is useful for dashboard-style widgets that don't need to interact with other windows at all. Note that this makes some of the other properties not have any effect. Either true or false. X11 ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new(){
              name = "reserve",
              description = "Specify how the window manager should make space for your window. This is useful for bars, which should not overlap any other windows. X11 ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new(){
              name = "windowtype",
              description = "Specify what type of window this is. This will be used by your window manager to determine how it should handle your window. Possible values: normal, dock, toolbar, dialog, desktop. Default: dock if reserve is specified, normal otherwise. X11 ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"normal","dock","toolbar","dialog","desktop"},
          },
          new(){
              name = "stacking",
              description = "Where the window should appear in the stack. Possible values: fg, bg, overlay, bottom. WAYLAND ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] { "fg" , "bg" , "overlay" , "bottom"},
          },
          new(){
              name = "exclusive",
              description = "Whether the compositor should reserve space for the window automatically. Either true or false. WAYLAND ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new(){
              name = "focusable",
              description = "Whether the window should be able to be focused. This is necessary for any widgets that use the keyboard to work. Either true or false. WAYLAND ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new() {
              name = "namespace",
              description = "Set the wayland layersurface namespace eww uses. Accepts a string value. WAYLAND ONLY",
              dataType = YuckDataType.YuckString
          }

       }
    },
    new() {
      name = "defvar",
      description = "Define a variable",
      IsTopLevel = true
    },
    new() {
      name = "deflisten",
      description = "Define a listener",
      IsTopLevel = true
    },
    new() {
      name = "include",
      description = "Include another yuck source file",
      IsTopLevel = true
    },
    new() {
      name = "defpoll",
      description = "Define a poll",
      IsTopLevel = true
    },

    /****** GTK Widgets ******/
    new() {
      name = "combo-box-text",
      description = "A combo box allowing the user to choose between several items.",
      IsGtkWidgetType = true
    },
    new() {
      name = "expander",
      description = "A widget that can expand and collapse, showing/hiding its children.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "revealer",
      description = "A widget that can reveal a child with an animation.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "checkbox",
      description = "A checkbox that can trigger events on checked/unchecked.",
      IsGtkWidgetType = true
    },
    new() {
      name = "color-button",
      description = "A button opening a color chooser window.",
      IsGtkWidgetType = true
    },
    new() {
      name = "color-chooser",
      description = "A color chooser widget.",
      IsGtkWidgetType = true
    },
    new() {
      name = "scale",
      description = "A slider widget.",
      IsGtkWidgetType = true
    },
    new() {
      name = "progress",
      description = "A progress bar.",
      IsGtkWidgetType = true
    },
    new() {
      name = "input",
      description = "An input field. For this to be usable, set focusable = true on the window.",
      IsGtkWidgetType = true
    },
    new() {
      name = "button",
      description = "A button containing any widget as its child. Events are triggered on release.",
      IsGtkWidgetType = true
    },
    new() {
      name = "image",
      description = "A widget displaying an image.",
      IsGtkWidgetType = true
    },
    new() {
      name = "box",
      description = "The main layout container.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "overlay",
      description = "A widget that places its children on top of each other.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "tooltip",
      description = "A widget that has a custom tooltip.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "centerbox",
      description = "A box that must contain EXACTLY 3 children that will be laid out at the start, center, and end of the container"
    },
    new() {
      name = "scroll",
      description = "A container with a single child that can scroll.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "eventbox",
      description = "A container which can receive events and must contain exactly one child.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "label",
      description = "A text widget giving you more control over how the text is displayed.",
      IsGtkWidgetType = true
    },
    new() {
      name = "literal",
      description = "A widget that allows you to render arbitrary yuck.",
      IsGtkWidgetType = true
    },
    new() {
      name = "calendar",
      description = "A widget that displays a calendar.",
      IsGtkWidgetType = true
    },
    new() {
      name = "stack",
      description = "A widget that displays one of its children at a time.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "transform",
      description = "A widget that applies transformations to its content. They are applied in the following order: rotate -> translate -> scale.",
      IsGtkWidgetType = true,
      AreWidgetsEmbeddable = true
    },
    new() {
      name = "circular-progress",
      description = "A widget that displays a circular progress bar.",
      IsGtkWidgetType = true
    },
    new() {
      name = "graph",
      description = "A widget that displays a graph showing how a given value changes over time.",
      IsGtkWidgetType = true
    },
    new() {
      name = "systray",
      description = "Tray for system notifier icons.",
      IsGtkWidgetType = true
    }
  };

}