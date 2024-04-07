namespace Signals4Net;

internal class ActionDisposable : IDisposable
{
    private Action? _action;

    public ActionDisposable(Action action)
    {
        _action = action;
    }

    public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
}