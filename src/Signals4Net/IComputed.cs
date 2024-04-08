namespace Signals4Net;

public interface IComputed<out T> : IReadOnlySignal<T>, ISignal
{
}