namespace Signals4Net.Tests;

public class EffectTests
{
    [Test]
    public void Effect_WhenSignalChanges_ActionIsCalled()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        context.Effect(() =>
        {
            _ = state.Value;
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        state.Value++;

        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public void Effect_WhenStateChangesTwice_IsCalledBothTimes()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        context.Effect(() =>
        {
            _ = state.Value;
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        state.Value++;
        state.Value++;

        Assert.That(callCount, Is.EqualTo(3));
    }

    [Test]
    public void Effect_AfterSubscriptionIsDisposed_IsNoLongerCalled()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        IDisposable subscription = context.Effect(() =>
        {
            _ = state.Value;
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        state.Value++;

        Assert.That(callCount, Is.EqualTo(2));

        subscription.Dispose();

        state.Value++;

        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public void Effect_WhenStateChangesTwiceInsideWriteScope_IsOnlyCalledOnce()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        IDisposable subscription = context.Effect(() =>
        {
            _ = state.Value;
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        using (context.WriteScope())
        {
            state.Value++;
            state.Value++;
        }

        Assert.That(callCount, Is.EqualTo(2));
    }
}