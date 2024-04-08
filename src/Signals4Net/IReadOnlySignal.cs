namespace Signals4Net;

public interface IReadOnlySignal<T>
{
    Task<T> GetValueAsync(CancellationToken cancellationToken = default);

    Task<T> PeekValueAsync(CancellationToken cancellationToken = default);

    IDisposable Subscribe(Func<ISignal, Task> subscriber);
}