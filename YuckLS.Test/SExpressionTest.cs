namespace YuckLS.Test;
using YuckLS.Core;
public class SExpressionTest{
   
    /**Test cases for isTopLevel
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
            ("
    };
    [Fact]
    public void IsTopLevelTest(){
        var instance1 = new SExpression(_isTopLevelTestCases[0]).IsTopLevel();
        var instance2 = new SExpression(_isTopLevelTestCases[1]).IsTopLevel();
        var instance3 = new SExpression(_isTopLevelTestCases[2]).IsTopLevel();
        var instance4 = new SExpression(_isTopLevelTestCases[3]).IsTopLevel();
        var instance5 = new SExpression(_isTopLevelTestCases[4]).IsTopLevel();

        Assert.False(instance1);
        Assert.True(instance2);
        Assert.False(instance3);
        Assert.False(instance4);
        Assert.True(instance5);
    }
}