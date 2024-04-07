namespace Signals4Net;

/// <summary>
/// This signal interface is implemented by signal objects that have readable state,
/// which includes both <see cref="State{T}"/> and <see cref="Computed{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of values the signal objects hold.
/// </typeparam>
public interface IReadOnlySignal<out T>
{
    /// <summary>
    /// Gets the value of the signal object.
    /// </summary>
    T Value { get; }
}