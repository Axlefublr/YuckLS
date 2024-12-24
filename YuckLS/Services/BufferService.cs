namespace YuckLS.Services;
using System.Collections.Concurrent;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

///<inheritdoc/>
internal sealed class BufferService(Microsoft.Extensions.Logging.ILogger<BufferService> _logger) : IBufferService
{
    private readonly ConcurrentDictionary<DocumentUri, Buffer> _buffers = new();

    ///<inheritdoc/>
    public void Add(DocumentUri key, string text)
    {
        _logger.LogTrace($"** Adding new file to opened buffers: {key}");
        _buffers.TryAdd(key, new Buffer(text));
    }

    ///<inheritdoc/>
    public void Remove(DocumentUri key)
    {
        //  throw new NotImplementedException();
    }

    public string? GetText(DocumentUri key){
        Buffer? fullBuffer = null;
        _buffers.TryGetValue(key, out fullBuffer);

        if(fullBuffer != null){
            return fullBuffer.GetText();
        }
        //document does not exist in dictionary
        return null;
    }
    private static string Splice(string buffer, Range range, string text)
    {
        var start = GetIndex(buffer, range.Start);
        var end = GetIndex(buffer, range.End);
        return buffer[..start] + text + buffer[end..];
    }

    private static int GetIndex(string buffer, Position position)
    {
        var index = 0;
        for (var i = 0; i < position.Line; ++i)
        {
            index = buffer.IndexOf('\n', index) + 1;
        }
        return index + position.Character;
    }

    public string? GetTextTillPosition(DocumentUri key, Position position)
    {
        return _buffers[key].GetTextTillLine(position);
    }

    public void ApplyIncrementalChange(DocumentUri key, Range range, string text)
    {
        var buffer = _buffers[key];
        var newText = Splice(buffer.GetText(), range, text);
        _buffers.TryUpdate(key, new Buffer(newText), buffer);
    }

    public void ApplyFullChange(DocumentUri key, string text)
    {
        var buffer = _buffers[key];
        _buffers.TryUpdate(key, new Buffer(text), buffer);
    }
}
