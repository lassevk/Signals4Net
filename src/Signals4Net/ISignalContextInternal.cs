using System.ComponentModel;

namespace Signals4Net;

internal interface ISignalContextInternal : ISignalContext
{
    void OnRead(ISignal signal);
    void OnChanged(ISignal signal);

    IDisposable ComputeScope(IComputedInternal computed);
    IDisposable ThreadScope();
    void FinalizeComputeScope(ComputeScope scope);
    void QueuePropertyChanged(object sender, PropertyChangedEventHandler? handler, string propertyName);
}