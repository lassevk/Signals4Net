namespace Signals4Net;

internal interface ISignalContextInternal : ISignalContext
{
    void OnRead(ISignal signal);
    void OnChanged(ISignal signal);

    IDisposable ComputeScope(IComputedInternal computed);
    void QueueSubscriberNotification(ISignal signal, Func<ISignal, Task> subscriber);
}