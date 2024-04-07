namespace Signals4Net;

public interface IState<T> : IReadOnlySignal<T>, ISignal
{
    new T Value { get; set; }
}