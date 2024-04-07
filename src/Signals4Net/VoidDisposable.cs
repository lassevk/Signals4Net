namespace Signals4Net;

internal class VoidDisposable : IDisposable
{
    public void Dispose()
    {
    }

    public static readonly VoidDisposable Instance = new();
}