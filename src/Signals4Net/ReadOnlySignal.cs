namespace Signals4Net;

public abstract class ReadOnlySignal<T> : IReadOnlySignal<T>
{
    private readonly object _lock = new();
    private HashSet<Func<ISignal, Task>>? _subscribers;

    public abstract Task<T> GetValueAsync(CancellationToken cancellationToken = default);
    public abstract Task<T> PeekValueAsync(CancellationToken cancellationToken = default);

    public IDisposable Subscribe(Func<ISignal, Task> subscriber)
    {
        lock (_lock)
        {
            _subscribers ??= new();
            _subscribers.Add(subscriber);
        }

        return new ActionDisposable(() =>
        {
            lock (_lock)
            {
                _subscribers.Remove(subscriber);
            }
        });
    }

    protected Func<ISignal, Task>[] GetSubscribers()
    {
        lock (_lock)
        {
            return _subscribers?.ToArray() ?? Array.Empty<Func<ISignal, Task>>();
        }
    }
}