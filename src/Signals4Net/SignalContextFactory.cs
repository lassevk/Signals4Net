using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

/// <inheritdoc cref="ISignalContextFactory"/>
[ExcludeFromCodeCoverage]
public class SignalContextFactory : ISignalContextFactory
{
    /// <inheritdoc cref="ISignalContextFactory.Create"/>
    public ISignalContext Create() => new SignalContext();
}