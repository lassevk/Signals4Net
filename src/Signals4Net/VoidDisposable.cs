using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

[ExcludeFromCodeCoverage]
internal class VoidDisposable : IDisposable
{
    public void Dispose()
    {
    }

    public static readonly VoidDisposable Instance = new();
}