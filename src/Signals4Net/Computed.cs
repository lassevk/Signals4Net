using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

public class Computed<T> : IComputed<T>, IComputedInternal
{
    private readonly ISignalContextInternal _context;
    private readonly Func<T> _expression;
    private readonly EqualityComparer<T> _comparer;

    private bool _dirty;
    private T _value;

    internal Computed(ISignalContextInternal context, Func<T> expression, EqualityComparer<T> comparer)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        _dirty = true;
        _value = default!;
    }

    public T Value
    {
        get
        {
            if (!_dirty)
                return _value;

            using IDisposable _ = _context.ThreadScope();
            using (_context.ComputeScope(this))
            {
                _value = _expression();
            }

            _dirty = false;

            _context.OnRead(this);
            return _value;
        }
    }

    public void SetDirty()
    {
        using IDisposable _ = _context.ThreadScope();
        if (_dirty)
            return;

        _dirty = true;

        // TODO: Handle dirty/clean transitions and defer this one, if possible
        _context.QueuePropertyChanged(this, PropertyChanged, nameof(Value));
    }

    [ExcludeFromCodeCoverage]
    public event PropertyChangedEventHandler? PropertyChanged;
}