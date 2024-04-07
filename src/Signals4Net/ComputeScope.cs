namespace Signals4Net;

internal sealed class ComputeScope : IDisposable
{
    private readonly ISignalContextInternal _context;

    public ComputeScope(ISignalContextInternal context, IComputedInternal computed)
    {
        _context = context;
        Computed = computed;
    }

    public IComputedInternal Computed { get; }
    public HashSet<ISignal> ReadSignals { get; } = new();

    public void Dispose()
    {
        _context.FinalizeComputeScope(this);
    }

    public void Read(ISignal signal)
    {
        ReadSignals.Add(signal);
    }
}