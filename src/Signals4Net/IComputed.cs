namespace Signals4Net;

/// <summary>
/// This interface is implemented by <see cref="Computed{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of values the computed signal will produce.
/// </typeparam>
public interface IComputed<out T> : IReadOnlySignal<T>, ISignal
{
}