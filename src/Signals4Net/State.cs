using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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

    public Task SetValueAsync(T value, CancellationToken cancellationToken)
    {
        if (_isFrozen)
            throw new InvalidOperationException("State signal is frozen, value cannot be changed");

        // async will be important here when effects are put back into the mix
        // todo: refactor to be properly async to trigger effects
        if (_comparer.Equals(_value, value))
            return Task.CompletedTask;

        _value = value;
        using (ExecutionContext.SuppressFlow())
        using (_context.WriteScope())
        {
            _context.OnChanged(this);

            foreach (Func<ISignal, Task> subscriber in GetSubscribers())
                _context.QueueSubscriberNotification(this, subscriber);
        }

        return Task.CompletedTask;
    }

    public void Freeze()
    {
        _isFrozen = true;
    }
}