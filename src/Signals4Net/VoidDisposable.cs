using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

[ExcludeFromCodeCoverage]
internal sealed class VoidDisposable : IDisposable
{
    public void Dispose()
    {
    }

    public static readonly VoidDisposable Instance = new();
}