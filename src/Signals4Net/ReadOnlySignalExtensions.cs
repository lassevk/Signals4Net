namespace Signals4Net;

public static class ReadOnlySignalExtensions
{
    public static IDisposable Subscribe<T>(this IReadOnlySignal<T> signal, Action<ISignal> subscriber)
    {
        return signal.Subscribe(s =>
        {
            subscriber(s);
            return Task.CompletedTask;
        });
    }

    public static IDisposable Subscribe<T>(this IReadOnlySignal<T> signal, Action subscriber) => signal.Subscribe(_ => subscriber());
}