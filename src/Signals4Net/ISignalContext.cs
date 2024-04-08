using System.ComponentModel;

namespace Signals4Net;

public interface ISignalContext
{
    IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default);

    IComputed<T> Computed<T>(Func<CancellationToken, Task<T>> expression, EqualityComparer<T>? comparer = default);

    void Remove(ISignal signal);

    IAsyncDisposable WriteScope();

    Task<IDisposable> AddEffectAsync(Func<CancellationToken, Task> effect, CancellationToken cancellationToken = default);
}