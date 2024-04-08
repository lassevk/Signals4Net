namespace Signals4Net.Tests;

public class SignalContextTests
{
    [Test]
    public async Task Signal_WithValues_ReturnsSignalWithValue()
    {
        var context = new SignalContext();

        IState<int> state = context.State(10);
        Assert.That(await state.GetValueAsync(), Is.EqualTo(10));
    }

    [Test]
    public void ComputeScope_DetectionOfCorruptCodeStructure()
    {
        var context = new SignalContext();
        IComputed<bool> c1 = context.Computed(() => true);
        IComputed<bool> c2 = context.Computed(() => false);
        IDisposable scope1 = ((ISignalContextInternal)context).ComputeScope((IComputedInternal)c1);
        IDisposable scope2 = ((ISignalContextInternal)context).ComputeScope((IComputedInternal)c2);

        Assert.Throws<InvalidOperationException>(() => scope1.Dispose());
    }
}