namespace Signals4Net;

public interface IReadOnlySignal<T>
{
    Task<T> GetValueAsync(CancellationToken cancellationToken = default);

    IDisposable Subscribe(Func<ISignal, Task> subscriber);
}