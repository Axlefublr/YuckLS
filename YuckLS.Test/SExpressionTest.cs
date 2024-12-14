namespace YuckLS.Test;
using YuckLS.Core;
public class SExpressionTest
{

    /**Test cases for isTopLevel()
     * All testcases must end with '(' Because that's the only thing that will trigger this method
     * */ 
    private string[] _isTopLevelTestCases = new string[] {
        //1
        @"(defwindow 
            (defvar
                (
            ",

        //2
         @"(",
        
         //3
         @"((defpoll medianame :inital '34242'))
            (defwindow media ;unclosed window
                (label :name testlabel)
                 (   
             
         ", 
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
            (
        ",
        //7
        @"(defwidget media
           ; )

          (  ",

        //8
        @"(defwidget
            )
        (defpoll 'curl ; ()fsfsfsfsf')
        (
        "
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
                (deflisten
                    (defvar
                        (                 
             
        ",
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
            ( 
           ",

        //4 
        @"(defpoll 
            (defwindow
                (label xx
                    (box )
                    (box )
                    (table )
                    (
                 
             
        ",

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
    (
    
",
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
   ( 
  "

    };
    [Fact]
    public void IsTopLevelTest()
    {
       var CompletionHandlerLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<YuckLS.Handlers.CompletionHandler>>();
        var instance1 = new SExpression(_isTopLevelTestCases[0],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance2 = new SExpression(_isTopLevelTestCases[1],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance3 = new SExpression(_isTopLevelTestCases[2],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance4 = new SExpression(_isTopLevelTestCases[3],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance5 = new SExpression(_isTopLevelTestCases[4],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance6 = new SExpression(_isTopLevelTestCases[5],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance7 = new SExpression(_isTopLevelTestCases[6],CompletionHandlerLoggerMock.Object).IsTopLevel();
        var instance8 = new SExpression(_isTopLevelTestCases[7],CompletionHandlerLoggerMock.Object).IsTopLevel();
        Assert.False(instance1);
        Assert.True(instance2);
        Assert.False(instance3);
        Assert.False(instance4);
        Assert.True(instance5);
        Assert.True(instance6);
        Assert.False(instance7);
       Assert.True(instance8);
    }

    [Fact]
    public void GetParentNodeTest()
    {
        //\(\w+[^\(]*\)
     var CompletionHandlerLoggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<YuckLS.Handlers.CompletionHandler>>();
        var instance1 = new SExpression(_getParentNodeTestCases[0],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance2 = new SExpression(_getParentNodeTestCases[1],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance3 = new SExpression(_getParentNodeTestCases[2],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance4 = new SExpression(_getParentNodeTestCases[3],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance5 = new SExpression(_getParentNodeTestCases[4],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance6 = new SExpression(_getParentNodeTestCases[5],CompletionHandlerLoggerMock.Object).GetParentNode();
        var instance7 = new SExpression(_getParentNodeTestCases[6],CompletionHandlerLoggerMock.Object).GetParentNode();
        Assert.Equal(instance1, "box");
        Assert.Equal(instance2, "defvar");
        Assert.Equal(instance3, "defwidget");
        Assert.Equal(instance4,"label");
        Assert.Equal(instance5, "defpoll");
        Assert.Equal(instance6, "defwindow");
        Assert.Equal(instance7,"box");
    } 
}