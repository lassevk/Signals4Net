using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

public class State<T> : IState<T>
{
    private readonly ISignalContextInternal _context;
    private readonly EqualityComparer<T> _comparer;

    private T _value;

    internal State(ISignalContextInternal context, T value, EqualityComparer<T> comparer)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _value = value;
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    public T Value
    {
        get
        {
            using IDisposable _ = _context.ThreadScope();
            _context.OnRead(this);
            return _value;
        }
        set
        {
            if (_comparer.Equals(_value, value))
                return;

            _value = value;
            using (_context.ThreadScope())
            using (_context.WriteScope())
            {
                _context.OnChanged(this);
                _context.QueuePropertyChanged(this, PropertyChanged, nameof(Value));
            }

        }
    }

    [ExcludeFromCodeCoverage]
    public event PropertyChangedEventHandler? PropertyChanged;
}