namespace Mu.Messages;

// ReSharper disable once InconsistentNaming
public interface Command
{
    public string Payload { get; set; }
}