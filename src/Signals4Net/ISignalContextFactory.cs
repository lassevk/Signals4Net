namespace Signals4Net;

/// <summary>
/// This interface is solely meant for dependency injection scenarios. In the event that
/// constructing a <see cref="ISignalContext"/> object requires other services, this
/// factory will take care of these dependencies.
/// </summary>
public interface ISignalContextFactory
{
    /// <summary>
    /// Create an instance of the <see cref="SignalContext"/> class.
    /// </summary>
    /// <returns>
    /// The created <see cref="SignalContext"/> instance.
    /// </returns>
    ISignalContext Create();
}