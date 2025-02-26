using System.Diagnostics.CodeAnalysis;

namespace Mu.Messages;

[SuppressMessage(
    "ReSharper",
    "InconsistentNaming",
    Justification = "As designed")]
public interface Command
{
    public string Payload { get; set; }
}