using System.ComponentModel;

namespace Signals4Net;

/// <summary>
/// This interface is implemented by <see cref="SignalContext"/> which is responsible for
/// collecting and maintaining all related signals.
/// </summary>
public interface ISignalContext : ISupportInitialize
{
    /// <summary>
    /// Create a state variable with the given initial state, and optionally a comparer used
    /// to detect changes.
    /// </summary>
    /// <param name="value">
    /// The initial value of this state variable.
    /// </param>
    /// <param name="comparer">
    /// Optional <see cref="EqualityComparer{T}"/> reference. If specified, will be used to detect
    /// changes to the value, in order to propagate the change to computed expressions. If not
    /// specified, the default comparer will be used.
    /// </param>
    /// <typeparam name="T">
    /// The type of value this state variable will hold.
    /// </typeparam>
    /// <returns>
    /// The created <see cref="IState{T}"/> state variable.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="comparer"/> is <c>null</c>.
    /// </exception>
    IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default);

    /// <summary>
    /// Create a computed variable from an expression, and optionally a comparer object used
    /// to detect changes.
    /// </summary>
    /// <param name="expression">
    /// The expression that will be used to evaluate the value of this computed variable.
    /// </param>
    /// <param name="comparer">
    /// Optional <see cref="EqualityComparer{T}"/> reference. If specified, will be used to detect
    /// changes to the value, in order to propagate the change to computed expressions. If not
    /// specified, the default comparer will be used.
    /// </param>
    /// <typeparam name="T">
    /// The type of value this computed variable will hold.
    /// </typeparam>
    /// <returns>
    /// The created <see cref="IComputed{T}"/> state variable.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="expression"/> is <c>null</c>.
    /// <p>- or -</p>
    /// <paramref name="comparer"/> is <c>null</c>.
    /// </exception>
    IComputed<T> Computed<T>(Func<T> expression, EqualityComparer<T>? comparer = default);

    /// <summary>
    /// Starts a write scope, inside which all writes to state variables will postpone propagating
    /// changes and firing of events until the scope is disposed.
    /// </summary>
    /// <remarks>
    /// Use this scope to assign new values to multiple state variables, preventing side-effects
    /// from <see cref="Computed{T}"/> and <see cref="Effect"/> objects, until the scope is disposed.
    /// </remarks>
    /// <returns>
    /// An <see cref="IDisposable"/> object that must be disposed when the writing to state
    /// variables is completed. Disposing this scope will start propagating changes to computed
    /// variables, and fire PropertyChanged events, where applicable.
    /// </returns>
    IDisposable WriteScope();

    /// <summary>
    /// Creates an effect that will be evaluated and executed when its dependencies change.
    /// </summary>
    /// <remarks>
    /// Note that this action will be called as part of the call to <see cref="Effect"/>
    /// in order to record the initial set of dependencies.
    /// </remarks>
    /// <param name="action">
    /// An <see cref="Action"/> delegate that will be called whenever any of its signal dependencies
    /// change.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> object that can be disposed when the effect is no longer
    /// needed. This will remove the effect from the <see cref="ISignalContext"/>.
    /// </returns>
    IDisposable Effect(Action action);
}