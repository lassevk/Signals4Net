namespace Signals4Net.Tests;

public class SignalContextTests
{
    [Test]
    public void Signal_WithValues_ReturnsSignalWithValue()
    {
        var context = new SignalContext();

        IState<int> state = context.State(10);
        Assert.That(state.Value, Is.EqualTo(10));
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

    // [Test]
    // public void ThreadScope_DetectionOfMisuse()
    // {
    //     var context = new SignalContext();
    //     IState<int> state = context.State(0);
    //     IComputed<int> computed = context.Computed(() =>
    //     {
    //         Thread.Sleep(25);
    //         return state.Value + 1;
    //     });
    //
    //     var go = new ManualResetEvent(false);
    //     var f1 = new ManualResetEvent(false);
    //     var f2 = new ManualResetEvent(false);
    //     new Thread(() => misuse(f1)).Start();
    //     new Thread(() => misuse(f2)).Start();
    //     go.Set();
    //
    //     bool wasRaised = false;
    //     void misuse(ManualResetEvent finished)
    //     {
    //         go.WaitOne();
    //         try
    //         {
    //             for (int index = 0; index < 10; index++)
    //             {
    //                 state.Value++;
    //                 _ = computed.Value;
    //             }
    //         }
    //         catch (Exception)
    //         {
    //             wasRaised = true;
    //         }
    //
    //         finished.Set();
    //     }
    //
    //     f1.WaitOne();
    //     f2.WaitOne();
    //     Assert.That(wasRaised, Is.True);
    // }
}