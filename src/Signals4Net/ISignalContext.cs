using System.ComponentModel;

namespace Signals4Net;

public interface ISignalContext : ISupportInitialize
{
    IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default);

    IComputed<T> Computed<T>(Func<T> expression, EqualityComparer<T>? comparer = default);

    IDisposable WriteScope();

    IDisposable Effect(Action action);
}