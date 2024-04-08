namespace Signals4Net;

public class State<T> : ReadOnlySignal<T>, IState<T>
{
    private readonly ISignalContextInternal _context;
    private readonly EqualityComparer<T> _comparer;

    private T _value;
    private bool _isFrozen;

    internal State(ISignalContextInternal context, T value, EqualityComparer<T> comparer)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _value = value;
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    public override Task<T> GetValueAsync(CancellationToken cancellationToken = default)
    {
        if (!_isFrozen)
            _context.OnRead(this);

        return Task.FromResult(_value);
    }

    public override Task<T> PeekValueAsync(CancellationToken cancellationToken = default) => Task.FromResult(_value);

    public async Task SetValueAsync(T value, CancellationToken cancellationToken)
    {
        if (_isFrozen)
            throw new InvalidOperationException("State signal is frozen, value cannot be changed");

        if (_comparer.Equals(_value, value))
            return;

        _value = value;
        using (ExecutionContext.SuppressFlow())
        await using (_context.WriteScope())
        {
            _context.OnChanged(this);

            foreach (Func<ISignal, Task> subscriber in GetSubscribers())
                _context.QueueSubscriberNotification(this, subscriber);
        }
    }

    public void Freeze()
    {
        _isFrozen = true;
    }
}