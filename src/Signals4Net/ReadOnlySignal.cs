namespace Signals4Net;

public abstract class ReadOnlySignal<T> : IReadOnlySignal<T>
{
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif
    private HashSet<Func<ISignal, Task>>? _subscribers;

    public abstract Task<T> GetValueAsync(CancellationToken cancellationToken = default);
    public abstract Task<T> PeekValueAsync(CancellationToken cancellationToken = default);

    public IDisposable Subscribe(Func<ISignal, Task> subscriber)
    {
        lock (_lock)
        {
            _subscribers ??= [];
            _subscribers.Add(subscriber);
        }

        return new ActionDisposable(() =>
        {
            lock (_lock)
            {
                _subscribers.Remove(subscriber);
                if (_subscribers.Count == 0)
                    _subscribers = null;
            }
        });
    }

    protected Func<ISignal, Task>[] GetSubscribers()
    {
        lock (_lock)
        {
            return _subscribers?.ToArray() ?? [];
        }
    }
}