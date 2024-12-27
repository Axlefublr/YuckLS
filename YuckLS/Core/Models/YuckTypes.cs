namespace YuckLS.Core.Models;
using YuckLS.Services;
//really wish i had rust enums right about now
internal abstract class YuckCompletionContext
{
    protected List<CompletionItem> _items = new();
    public abstract CompletionList Completions();
}
internal class TopLevelYuckCompletionContext : YuckCompletionContext
{
    public override CompletionList Completions()
    {
        foreach (var yuckType in YuckTypesProvider.YuckTypes.Where(p => p.IsTopLevel == true))
        {
            _items.Add(new()
            {
                Label = yuckType.name,
                Documentation = new StringOrMarkupContent(yuckType.description),
                Kind = CompletionItemKind.Class,
                InsertText = yuckType.name
            });
        }
        return new CompletionList(_items);
    }
}
internal class WidgetYuckCompletionContext(IEwwWorkspace _workspace) : YuckCompletionContext
{
    public override CompletionList Completions()
    {
        var workspace = _workspace;
        //get gtk types and user defined types
        foreach (var yuckType in YuckTypesProvider.YuckTypes.Where(p => p.IsGtkWidgetType == true).Concat(workspace.UserDefinedTypes))
        {
            _items.Add(new()
            {
                Label = yuckType.name,
                Documentation = new StringOrMarkupContent(yuckType.description),
                Kind = CompletionItemKind.Class,
                InsertText = yuckType.name
            });
        }
        return new CompletionList(_items);
    }
}
internal class PropertyYuckCompletionContext : YuckCompletionContext
{
    public required YuckType parentType;

    public override CompletionList Completions()
    {
        var properties = parentType.properties;
        if (properties is null || properties.Count() == 0) return new CompletionList();
        foreach (var property in properties)
        {
            _items.Add(new()
            {
                Label = property.name,
                Documentation = new StringOrMarkupContent(property.description),
                Kind = CompletionItemKind.Property,
                InsertText = property.name
            });
        }
        return new CompletionList(_items);
    }
}
internal class PropertySuggestionCompletionContext(IEwwWorkspace _workspace) : YuckCompletionContext
{
    public required YuckType parentType;
    public required YuckProperty parentProperty;
    public override CompletionList Completions()
    {
        var suggestions = parentProperty.possibleValues;
        if (suggestions is not null && suggestions.Count() > 0)
        {
            foreach (var suggestion in suggestions)
            {
                _items.Add(new()
                {
                    Label = suggestion,
                    Kind = CompletionItemKind.EnumMember,
                    InsertText = $"\"{suggestion}\"",
                    Documentation = "A recommended variable for this type"
                });
            }
        }
        //there is definetly a neater more efficient way to do this
        foreach (var customVariable in _workspace.UserDefinedVariables.Select(p => p.name).ToArray())
        {
            _items.Add(new()
            {
                Label = customVariable,
                Kind = CompletionItemKind.Variable,
                InsertText = customVariable,
                Documentation = _workspace.UserDefinedVariables.Where(p => p.name == customVariable).First().description
            });
        }
        return new CompletionList(_items);
    }
}
public class YuckType
{
    public required string name;
    public YuckProperty[] properties = new YuckProperty[] { };
    public required string description;
    public bool IsTopLevel = false;
    public bool AreWidgetsEmbeddable = false;
    public bool IsGtkWidgetType = false;
    public bool IsUserDefined = false;
}
public class YuckVariable
{
    public required string name;
    public required string description;
}
public class YuckProperty
{
    public required string name;
    public required string description;
    public required YuckDataType? dataType;
    public string[]? possibleValues;
}

public enum YuckDataType
{
    YuckString,
    YuckInt,
    YuckBool,
    YuckDuration,
    YuckFloat,
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
        dataType = YuckDataType.YuckString,
        possibleValues = new string[] {"fill", "baseline", "center", "start" , "end"}
    },
    new() {
      name = "halign",
        description = "how to align this horizontally. possible values: 'fill', 'baseline', 'center', 'start', 'end'",
        dataType = YuckDataType.YuckString,
        possibleValues = new string[] {"fill", "baseline", "center", "start" , "end"}
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
    private static YuckProperty[] _timeOutProperty = new YuckProperty[] {
    new() {
      name = "timeout",
        description = "timeout of the command: Default: '200ms'",
        dataType = YuckDataType.YuckDuration,
    },

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
              dataType = YuckDataType.Custom,
          },
          new() {
            name = "geometry",
              description = "The geometry of the window",
              dataType = YuckDataType.Custom,
          },
          new() {
            name = "stacking",
              description = "Where the window should appear in the stack. Possible values: fg, bg. X11 ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new [] {
                "fg",
                "bg"
              },
          },
          new() {
            name = "wm-ignore",
              description = "Whether the window manager should ignore this window. This is useful for dashboard-style widgets that don't need to interact with other windows at all. Note that this makes some of the other properties not have any effect. Either true or false. X11 ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "reserve",
              description = "Specify how the window manager should make space for your window. This is useful for bars, which should not overlap any other windows. X11 ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "windowtype",
              description = "Specify what type of window this is. This will be used by your window manager to determine how it should handle your window. Possible values: normal, dock, toolbar, dialog, desktop. Default: dock if reserve is specified, normal otherwise. X11 ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {
                "normal",
                "dock",
                "toolbar",
                "dialog",
                "desktop"
              },
          },
          new() {
            name = "stacking",
              description = "Where the window should appear in the stack. Possible values: fg, bg, overlay, bottom. WAYLAND ONLY",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {
                "fg",
                "bg",
                "overlay",
                "bottom"
              },
          },
          new() {
            name = "exclusive",
              description = "Whether the compositor should reserve space for the window automatically. Either true or false. WAYLAND ONLY",
              dataType = YuckDataType.YuckBool,
          },
          new() {
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
        IsTopLevel = true,
        properties = new YuckProperty[] {
          new() {
            name = "initial",
              description = "Initial value while the listener loads",
              dataType = YuckDataType.YuckString,
          }
        }
    },
    new() {
      name = "include",
        description = "Include another yuck source file",
        IsTopLevel = true
    },
    new() {
      name = "defpoll",
        description = "Define a poll",
        IsTopLevel = true,
        properties = new YuckProperty[] {
          new() {
            name = "interval",
              description = "How frequently to update the poll",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "initial",
              description = "Initial value before the poll first loads",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "run-while",
              description = "Condition that must be fulfilled for poll to run",
              dataType = YuckDataType.YuckBool,
          }
        }
    },

    /****** GTK Widgets ******/
    new() {
      name = "combo-box-text",
        description = "A combo box allowing the user to choose between several items.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "items",
              description = "Items that should be displayed in the combo box",
              dataType = YuckDataType.YuckString,
          },

          new() {
            name = "onchange",
              description = "runs the code when a item was selected, replacing {} with the item as a string",
              dataType = YuckDataType.YuckString
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "expander",
        description = "A widget that can expand and collapse, showing/hiding its children.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "name",
              description = "Name of the expander",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "expanded",
              description = "sets if the tree is expanded",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "revealer",
        description = "A widget that can reveal a child with an animation.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[]{
          new(){
            name = "transition",
            description = "the name of the transition",
            dataType = YuckDataType.YuckString,
            possibleValues = new string[] {"slideright", "slideleft", "slideup", "slidedown", "crossfade", "none"}
          },
          new(){
            name = "reveal",
            description = "sets if the child is revealed or not",
            dataType = YuckDataType.YuckBool,
          },
          new(){
            name = "duration",
            description = "the duration of the reveal transition.",
            dataType = YuckDataType.YuckDuration
          }
        }
    },
    new() {
      name = "checkbox",
        description = "A checkbox that can trigger events on checked/unchecked.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "checked",
              description = "whether the checkbox is toggled or not when created",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "timeout",
              description = " timeout of the command. Default: 200ms",
              dataType = YuckDataType.YuckDuration,
          },
          new() {
            name = "onchecked",
              description = " action (command) to be executed when checked by the user",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "onunchecked",
              description = " similar to onchecked but when the widget is unchecked",
              dataType = YuckDataType.YuckString
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "color-button",
        description = "A button opening a color chooser window.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "use-alpha",
              description = "bool to whether or not use alpha",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "onchange",
              description = " runs the code when the color was selected",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "color-chooser",
        description = "A color chooser widget.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "use-alpha",
              description = "bool to whether or not use alpha",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "onchange",
              description = " runs the code when the color was selected",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "scale",
        description = "A slider widget.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "flipped",
              description = "flip the direction",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "marks",
              description = "draw marks",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "draw-value",
              description = "draw the value of the property",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "round-digits",
              description = "Sets the number of decimals to round the value to when it changes",
              dataType = YuckDataType.YuckInt
          },
          new() {
            name = "value",
              description = "the value",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "min",
              description = "the minimum value",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "max",
              description = "the maximum value",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "onchange",
              description = "command executed once the value is changes. The placeholder {}, used in the command will be replaced by the new value.",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "orientation",
              description = "orientation of the widget. Possible values: 'vertical', 'v', 'horizontal', 'h'",
              dataType = YuckDataType.YuckString,
              possibleValues = new[] {"vertical","horizontal"}
          }
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "progress",
        description = "A progress bar.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "flipped",
              description = "flip the direction",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "value",
              description = "value of the progress bar (between 0-100)",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "orientation",
              description = "orientation of the widget. Possible values: 'vertical', 'v', 'horizontal', 'h'",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[]{ "vertical", "horizontal"}
          },
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "input",
        description = "An input field. For this to be usable, set focusable = true on the window.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "value",
              description = "the content of the text field",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "onchange",
              description = "Command to run when the text changes. The placeholder {} will be replaced by the value",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "onaccept",
              description = " Command to run when the user hits return in the input field. The placeholder {} will be replaced by the value",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "password",
              description = " if the input is obscured",
              dataType = YuckDataType.YuckBool
          },
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "button",
        description = "A button containing any widget as its child. Events are triggered on release.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "onclick",
              description = " command to run when the button is activated either by leftclicking or keyboard",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "onmiddleclick",
              description = "command to run when the button is middleclicked",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "onrightclick",
              description = "command to run when the button is rightclicked",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_timeOutProperty).Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "image",
        description = "A widget displaying an image.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "path",
              description = " path to the image file",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "image-width",
              description = "width of the image",
              dataType = YuckDataType.YuckInt
          },
          new() {
            name = "image-height",
              description = "height of the image",
              dataType = YuckDataType.YuckInt
          },
          new() {
            name = "preserve-aspect-ratio",
              description = "whether to keep the aspect ratio when resizing an image. Default: true, false doesn't work for all image types",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "fill-svg",
              description = "sets the color of svg images",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "icon",
              description = "name of a theme icon",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "icon-size",
              description = "size of the theme icon",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "box",
        description = "The main layout container.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "orientation",
              description = "orientation of the box. possible values: 'vertical', 'v', 'horizontal', 'h'",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"vertical","horizontal"}
          },
          new() {
            name = "spacing",
              description = "spacing between elements",
              dataType = YuckDataType.YuckInt
          },
          new() {
            name = "space-evenly",
              description = "space the widgets evenly.",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "overlay",
        description = "A widget that places its children on top of each other.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {}.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "tooltip",
        description = "A widget that has a custom tooltip.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {}.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "centerbox",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        description = "A box that must contain EXACTLY 3 children that will be laid out at the start, center, and end of the container",
        properties = new YuckProperty[] {
          new() {
            name = "orientation",
              description = "orientation of the widget. Possible values: 'vertical', 'v', 'horizontal', 'h'",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"vertical","horizontal"}
          },
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "scroll",
        description = "A container with a single child that can scroll.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "hscroll",
              description = "scroll horizontally",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "vscroll",
              description = "scroll vertically",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "eventbox",
        description = "A container which can receive events and must contain exactly one child.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "onscroll",
              description = "event to execute when the user scrolls with the mouse over the widget.",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "onhover",
              description = "event to execute when the user hovers over the widget",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "onhoverlost",
              description = "event to execute when the user losts hovers over the widget",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "cursor",
              description = "Cursor to show while hovering (see gtk3-cursors for possible names)",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "ondropped",
              description = "Command to execute when something is dropped on top of this element. The placeholder {} used in the command will be replaced with the uri to the dropped thing.",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "dragvalue",
              description = "URI that will be provided when dragging from this widget",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "dragtype",
              description = "Type of value that should be dragged from this widget. Possible values: 'file', 'text'",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"file","text"}
          },
          new() {
            name = "onclick",
              description = "command to run when the widget is clicked",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "onmiddleclick",
              description = "command to run when the widget is middleclicked",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "onrightclick",
              description = "command to run when the widget is rightclicked",
              dataType = YuckDataType.YuckString
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "label",
        description = "A text widget giving you more control over how the text is displayed.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "text",
              description = "the text to display",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "truncate",
              description = "whether to truncate text (or pango markup). If show-truncated is false, or if limit-width has a value, this property has no effect and truncation is enabled.",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "limit-width",
              description = "maximum count of characters to display",
              dataType = YuckDataType.YuckInt,
          },
          new() {
            name = "truncate-left",
              description = "whether to truncate on the left side",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "show-truncated",
              description = "show whether the text was truncated. Disabling it will also disable dynamic truncation (the labels won't be truncated more than limit-width, even if there is not enough space for them), and will completly disable truncation on pango markup.",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "uindent",
              description = " whether to remove leading spaces",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "markup",
              description = "Pango markup to display",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "wrap",
              description = "Wrap the text. This mainly makes sense if you set the width of this widget.",
              dataType = YuckDataType.YuckBool,
          },
          new() {
            name = "angle",
              description = "the angle of rotation for the label (between 0 - 360)",
              dataType = YuckDataType.YuckFloat,
          },
          new() {
            name = "gravity",
              description = " the gravity of the string (south, east, west, north, auto). Text will want to face the direction of gravity.",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "xalign",
              description = " the alignment of the label text on the x axis (between 0 - 1, 0 -> left, 0.5 -> center, 1 -> right)",
              dataType = YuckDataType.YuckFloat,
          },
          new() {
            name = "yalign",
              description = "the alignment of the label text on the y axis (between 0 - 1, 0 -> bottom, 0.5 -> center, 1 -> top)",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "justify",
              description = " the justification of the label text (left, right, center, fill)",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"left","right","center","fill"}
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "literal",
        description = "A widget that allows you to render arbitrary yuck.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "content",
              description = " inline yuck that will be rendered as a widget.",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "calendar",
        description = "A widget that displays a calendar.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "day",
              description = "the selected day",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "month",
              description = "the selected month",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "year",
              description = "the selected year",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "show-details",
              description = "show details",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "show-heading",
              description = "show heading line",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "show-day-names",
              description = "show names of days",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "show-week-numbers",
              description = "show week numbers",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "onclick",
              description = "command to run when the user selects a date. The {0} placeholder will be replaced by the selected day, {1} will be replaced by the month, and {2} by the year.",
              dataType = YuckDataType.YuckString
          },
        }.Concat(_commonGtkWidgetProperties).Concat(_timeOutProperty).ToArray()
    },
    new() {
      name = "stack",
        description = "A widget that displays one of its children at a time.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "selected",
              description = "index of child which should be shown",
              dataType = YuckDataType.YuckInt
          },
          new() {
            name = "transition",
              description = "he name of the transition. Possible values: 'slideright', 'slideleft', 'slideup', 'slidedown', 'crossfade', 'none'",               possibleValues = new string[] {
                  "slideright", "slideleft" ,"slideup" ,"slidedown" , "crossfade" , "none"
              },
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "same-size",
              description = "sets whether all children should be the same size",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new(){
      name = "for",
      description = "Loop over a dataset",
      //i mean... technically
      IsGtkWidgetType = true
    },
    new() {
      name = "transform",
        description = "A widget that applies transformations to its content. They are applied in the following order: rotate -> translate -> scale.",
        IsGtkWidgetType = true,
        AreWidgetsEmbeddable = true,
        properties = new YuckProperty[] {
          new() {
            name = "rotate",
              description = "the percentage to rotate",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "transform-origin-x",
              description = "x coordinate of origin of transformation (px or %)",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "transform-origin-y",
              description = "y coordinate of origin of transformation (px or %)",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "translate-x",
              description = "the amount to translate in the x direction (px or %)",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "translate-y",
              description = "the amount to translate in the y direction (px or %)",
              dataType = YuckDataType.YuckString,
          },
          new() {
            name = "scale-x",
              description = "the amount to scale in the x direction (px or %)",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "scale-y",
              description = "the amount to scale in the y direction (px or %)",
              dataType = YuckDataType.YuckString
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "circular-progress",
        description = "A widget that displays a circular progress bar.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "value",
              description = " the value, between 0 - 100",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "start-at",
              description = "the percentage that the circle should start at",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "thickness",
              description = "the thickness of the circle",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "clockwise",
              description = "whether the progress bar spins clockwise or counter clockwise",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "graph",
        description = "A widget that displays a graph showing how a given value changes over time.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "value",
              description = "the value, between 0 - 100",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "thickness",
              description = "the thickness of the line",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "time-range",
              description = "the range of time to show",
              dataType = YuckDataType.YuckDuration,
          },
          new() {
            name = "min",
              description = "the minimum value to show (defaults to 0 if value_max is provided)",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "max",
              description = "the maximum value to show",
              dataType = YuckDataType.YuckFloat
          },
          new() {
            name = "dynamic",
              description = "whether the y range should dynamically change based on value",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "line-style",
              description = " changes the look of the edges in the graph. Values: 'miter' (default), 'round'",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "flip-x",
              description = " whether the x axis should go from high to low",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "flip-y",
              description = " whether the y axis should go from high to low",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "vertical",
              description = " if set to true, the x and y axes will be exchanged",
              dataType = YuckDataType.YuckBool
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },
    new() {
      name = "systray",
        description = "Tray for system notifier icons.",
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "spacing",
              description = "spacing between elements",
              dataType = YuckDataType.YuckInt,
          },
          new() {
            name = "orientation",
              description = "orientation of the box. possible values: 'vertical', 'v', 'horizontal', 'h'",
              dataType = YuckDataType.YuckString,
              possibleValues = new string[] {"vertical" , "horizontal"}
          },
          new() {
            name = "space-evenly",
              description = "space the widgets evenly.",
              dataType = YuckDataType.YuckBool
          },
          new() {
            name = "size",
              description = "size of icons in the tray",
              dataType = YuckDataType.YuckInt,
          },
          new() {
            name = "prepend-new",
              description = "prepend new icons",
              dataType = YuckDataType.YuckBool
          },
          new(){
            name = "icon-size",
            description = "Size of the icons",
            dataType = YuckDataType.YuckInt
          }
        }.Concat(_commonGtkWidgetProperties).ToArray()
    },

    //special types 
    new() {
      name = "geometry",
        description = "Size and positional description for a the window geometry property",
        //not ideal, will fix this later
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "x",
              description = "The x-axis position",
              dataType = YuckDataType.Custom
                },
          new() {
            name = "y",
              description = "The y-axis position",
              dataType = YuckDataType.Custom
               },
          new() {
            name = "width",
              description = "The width of the window",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "height",
              description = "The height of the window",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "anchor",
              description = "The positional anchor of the window e.g top left, top center, bottom right, e.t.c",
              dataType = YuckDataType.YuckString
          }
        }
    },
    new() {
      name = "struts",
        description = "Reserve struts for X11",
        //not ideal, will fix this later,
        IsGtkWidgetType = true,
        properties = new YuckProperty[] {
          new() {
            name = "distance",
              description = "The distance",
              dataType = YuckDataType.YuckString
          },
          new() {
            name = "side",
              description = "The side allowance",
              dataType = YuckDataType.YuckString
          }
        }
    }
  };

}
