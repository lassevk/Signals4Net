namespace Signals4Net;

internal class AsyncActionDisposable : IAsyncDisposable
{
    private Func<Task>? _disposeAction;

    public AsyncActionDisposable(Func<Task> disposeAction)
    {
        _disposeAction = disposeAction;
    }

    public async ValueTask DisposeAsync()
    {
        Func<Task>? action = Interlocked.Exchange(ref _disposeAction, null);
        if (action != null)
            await action();
    }
}