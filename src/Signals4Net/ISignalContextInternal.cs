using System.ComponentModel;

namespace Signals4Net;

internal interface ISignalContextInternal : ISignalContext
{
    void OnRead(ISignal signal);
    void OnChanged(ISignal signal);

    IDisposable ComputeScope(IComputedInternal computed);
    void FinalizeComputeScope(ComputeScope scope);
    void QueueSubscriberNotification(ISignal signal, Func<ISignal, Task> subscriber);
}