namespace YuckLS.Test;
using YuckLS.Core;
public class SExpressionTest
{

    /**Test cases for isTopLevel()
     * All testcases must end with '(' Because that's the only thing that will trigger this method. Cases must not have whitespaces at the end.
     * */
    private string[] _isTopLevelTestCases = new string[] {
        //1
        @"(defwindow 
            (defvar
                (",

        //2
         @"(",
        
         //3
         @"((defpoll medianame :inital '34242'))
            (defwindow media ;unclosed window
                (label :name testlabel)
                 (", 
         //4 
         @"(defpoll medianame :initial '3313')
            (defwindow media ;unclosed window
             :geometry ("
             ,
        //5
        @"(defpoll medianame :initial '3313')
            (defwindow media 
             :geometry (geometry :anchor bottom left)
            )
            ()
            (name :ping)
            (",

        //6
        @"(defwidget media 
            ;(defpoll (defwidget))
            )
            ;(defpoll)
            (",
        //7
        @"(defwidget media
           ; )

          (",

        //8
        @"(defwidget
            )
        (defpoll 'curl ; ()fsfsfsfsf')
        (",

         //9
         @"(defwindow )
            (defpoll hyprvr :interval ""Hyprland --version|awk '{print $1; exit}'"")
            ("
    };

    /*Test cases fot GetParentNode()
     *All test cases must end with '(' and must not be top level. That is isTopLevel() must return false for it.
     */
    private string[] _getParentNodeTestCases = new string[]{
        //1 
        @"(defwindow 
            (box 
                (",

        //2
        @"(defpoll 
            (defwidget 
                deflisten
                    (defvar
                        (",
        //3
        @"(defpoll
           (defwindow
            ))
        ()()
        (defwindow
         (box
            (label :name Label-name)
            )
        )
        (defwindow 
            (defwidget 
            (",

        //4 
        @"(defpoll 
            (defwindow
                (label xx
                    (box )
                    (box )
                    (table )
                    (",

        //5 
        @"(defpoll
            ;(defwidget 
            (     
        )",
        //6
            @"
(defwindow
  (box :orientation horizontal
       :halign center
    text
    (button :onclick notify-send 'Hello' 'Hello, ${name}'
      Greet))
    (",
//7 
@"
 (box :orientation v :spacing -200  :class media-box :visible isSpotify
    (box  :height 300px :style background-image:url('${mediaart}') :class mediaart)
    ; (label :text cava :class mediaartist
    (label :text mediatitle :class mediatitle :truncate true
        :placeholder hh
     )
    (label :text mediaartist :class mediaartist
     :gddgdg
     (box )
    )
   ("
    };
    [Fact]
    public void IsTopLevelTest()
    {
        var CompletionHandlerLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<YuckLS.Handlers.CompletionHandler>>();
        var test1 = new SExpression(_isTopLevelTestCases[0], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test2 = new SExpression(_isTopLevelTestCases[1], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test3 = new SExpression(_isTopLevelTestCases[2], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test4 = new SExpression(_isTopLevelTestCases[3], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test5 = new SExpression(_isTopLevelTestCases[4], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test6 = new SExpression(_isTopLevelTestCases[5], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test7 = new SExpression(_isTopLevelTestCases[6], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test8 = new SExpression(_isTopLevelTestCases[7], CompletionHandlerLoggerMock.Object).IsTopLevel();
        var test9 = new SExpression(_isTopLevelTestCases[8], CompletionHandlerLoggerMock.Object).IsTopLevel();
        Assert.False(test1);
        Assert.True(test2);
        Assert.False(test3);
        Assert.False(test4);
        Assert.True(test5);
        Assert.True(test6);
        Assert.False(test7);
        Assert.True(test8);
        Assert.True(test9);
    }

    [Fact]
    public void GetParentNodeTest()
    {
        //\(\w+[^\(]*\)
        var CompletionHandlerLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<YuckLS.Handlers.CompletionHandler>>();
        var test1 = new SExpression(_getParentNodeTestCases[0], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test2 = new SExpression(_getParentNodeTestCases[1], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test3 = new SExpression(_getParentNodeTestCases[2], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test4 = new SExpression(_getParentNodeTestCases[3], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test5 = new SExpression(_getParentNodeTestCases[4], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test6 = new SExpression(_getParentNodeTestCases[5], CompletionHandlerLoggerMock.Object).GetParentNode();
        var test7 = new SExpression(_getParentNodeTestCases[6], CompletionHandlerLoggerMock.Object).GetParentNode();
        Assert.Equal(test1, "box");
        Assert.Equal(test2, "defvar");
        Assert.Equal(test3, "defwidget");
        Assert.Equal(test4, "label");
        Assert.Equal(test5, "defpoll");
        Assert.Equal(test6, "defwindow");
        Assert.Equal(test7, "box");
    }
}