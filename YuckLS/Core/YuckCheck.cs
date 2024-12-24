namespace YuckLS.Core;

using YuckLS.Handlers;
using YuckLS.Services;
//extremely experimental syntax checker
internal sealed class YuckCheck(string _text, Microsoft.Extensions.Logging.ILogger<CompletionHandler> _logger, IEwwWorkspace _workspace){
	private readonly SExpression _sExpression = new(_text,_logger,_workspace);
	public void TryGetDiagnostics(){
		var unclosedBrackets = _sExpression.CheckBracketPairs();
		foreach(int pos in unclosedBrackets){
			_logger.LogError($"Unclosed bracket located at {pos}");
			_logger.LogError($"The item at this pos was {_sExpression.FullText[pos]}");
		}
	}   	
}
