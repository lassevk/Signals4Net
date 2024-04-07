using System.ComponentModel;

namespace Signals4Net;

/// <summary>
/// This interface is implemented by <see cref="State{T}"/> and allows for changing the
/// value of the state variable, as well as reading its current value.
/// </summary>
/// <typeparam name="T">
/// The type of value the state variable holds.
/// </typeparam>
public interface IState<T> : IReadOnlySignal<T>, ISignal
{
    /// <summary>
    /// Gets or sets the value of the state signal.
    /// </summary>
    new T Value { get; set; }
}