using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

[ExcludeFromCodeCoverage]
public class SignalContextFactory : ISignalContextFactory
{
    public ISignalContext Create() => new SignalContext();
}