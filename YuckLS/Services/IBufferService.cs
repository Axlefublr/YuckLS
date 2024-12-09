namespace YuckLS.Services;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

internal sealed class Buffer(string _text)
{
    public string GetText() => _text;

    private static readonly string[] separator = ["\n", "\r\n"];

    public string? GetTextTillLine(Position position)
    {
        string[] lines = _text.Split(separator, StringSplitOptions.None);
        string text = string.Join("\n", lines[..position.Line]);
        string line = lines[position.Line][..position.Character];
        return $"{text}\n{line}";
    }
}
//special thanks to avalonia for this implementation
//see http://www.github.com/avaloniaui/avaloniavscode
///<summary>
///Manages file buffer in memory as user makes edits
///</summary>
internal interface IBufferService
{
    ///<summary>
    ///Add a new Buffer to the concurrentDictionary that stores open buffers. Will be invoked on documentOpen
    ///</summary>
    public void Add(DocumentUri key, string text);

    ///<summary>
    ///Remove a Buffer from the concurrentDictionary that stores all buffers. Will be invoked on documentClose
    public void Remove(DocumentUri key);

    ///<summary>
    ///Apply an incremental change to an opened buffer.
    ///</summary>
    public void ApplyIncrementalChange(DocumentUri key, Range range, string text);
    
    ///<summary>
    ///Apply a full change to an opened buffer
    ///</summary>
    public void ApplyFullChange(DocumentUri key, string text);

    ///<summary>
    ///Get all text from beginning of line to current position
    ///</summary
    public string? GetTextTillPosition(DocumentUri key, Position position);
}