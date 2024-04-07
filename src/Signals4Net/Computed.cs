using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

/// <summary>
/// This class implements computed signals, which uses an expression to evaluate a value.
/// </summary>
/// <typeparam name="T">
/// The type of value that this computed signal will produce.
/// </typeparam>
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

    /// <summary>
    /// Gets or sets the value of this computed signal.
    /// </summary>
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

    void IComputedInternal.SetDirty()
    {
        using IDisposable _ = _context.ThreadScope();
        if (_dirty)
            return;

        _dirty = true;

        // TODO: Handle dirty/clean transitions and defer this one, if possible
        _context.QueuePropertyChanged(this, PropertyChanged, nameof(Value));
    }

    /// <summary>
    /// This event is fired when the computed signal is flagged for re-evaluation due to its
    /// dependent signals changing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public event PropertyChangedEventHandler? PropertyChanged;
}