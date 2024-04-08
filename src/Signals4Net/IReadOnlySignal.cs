namespace Signals4Net;

public interface IReadOnlySignal<out T>
{
    T Value { get; }
}