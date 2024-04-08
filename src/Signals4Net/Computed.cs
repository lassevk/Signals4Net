namespace Signals4Net;

public class Computed<T> : ReadOnlySignal<T>, IComputed<T>, IComputedInternal
{
    private readonly ISignalContextInternal _context;
    private readonly Func<CancellationToken, Task<T>> _expression;
    private readonly EqualityComparer<T> _comparer;

    private bool _dirty;
    private T _value;

    internal Computed(ISignalContextInternal context, Func<CancellationToken, Task<T>> expression, EqualityComparer<T> comparer)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        _dirty = true;
        _value = default!;
    }

    void IComputedInternal.SetDirty()
    {
        if (_dirty)
            return;

        _dirty = true;

        // TODO: Handle versioning
        // TODO: Handle dirty/clean transitions and defer this one, if possible
        foreach (Func<ISignal, Task> subscriber in GetSubscribers())
            _context.QueueSubscriberNotification(this, subscriber);
    }

    public override async Task<T> GetValueAsync(CancellationToken cancellationToken = default)
    {
        _context.OnRead(this);
        return await PeekValueAsync(cancellationToken);
    }

    public override async Task<T> PeekValueAsync(CancellationToken cancellationToken = default)
    {
        if (!_dirty)
            return _value;

        using (ExecutionContext.SuppressFlow())
        {
            using (_context.ComputeScope(this))
            {
                T newValue = await _expression(cancellationToken);;

                // TODO: Handle versioning, if other task/thread have already changed this computed in the background
                _dirty = false;
                if (!_comparer.Equals(newValue, _value))
                    _value = newValue;

                return _value;
            }
        }
    }
}