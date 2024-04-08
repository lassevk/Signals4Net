namespace Signals4Net.Tests;

public class EffectTests
{
    [Test]
    public async Task Effect_WhenSignalChanges_ActionIsCalled()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        _ = context.AddEffectAsync(async () =>
        {
            _ = await state.GetValueAsync();
            callCount++;
        });

        Assert.That(callCount, Is.EqualTo(1));

        await state.SetValueAsync(await state.GetValueAsync() + 1);

        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Effect_WhenStateChangesTwice_IsCalledBothTimes()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        _ = context.AddEffectAsync(async () =>
        {
            _ = await state.GetValueAsync();
            callCount++;
        });

        Assert.That(callCount, Is.EqualTo(1));

        await state.SetValueAsync(1);
        await state.SetValueAsync(2);

        Assert.That(callCount, Is.EqualTo(3));
    }

    [Test]
    public async Task Effect_AfterSubscriptionIsDisposed_IsNoLongerCalled()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        IDisposable subscription = await context.AddEffectAsync(async () =>
        {
            _ = await state.GetValueAsync();
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        await state.SetValueAsync(1);

        Assert.That(callCount, Is.EqualTo(2));
        callCount = 0;

        subscription.Dispose();

        await state.SetValueAsync(2);

        Assert.That(callCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Effect_WhenStateChangesTwiceInsideWriteScope_IsOnlyCalledOnce()
    {
        var context = new SignalContext();

        IState<int> state = context.State(0);
        var callCount = 0;
        IDisposable subscription = context.AddEffectAsync(async () =>
        {
            _ = await state.GetValueAsync();
            callCount++;
        });
        Assert.That(callCount, Is.EqualTo(1));

        using (context.WriteScope())
        {
            await state.SetValueAsync(1);
            await state.SetValueAsync(2);
        }

        Assert.That(callCount, Is.EqualTo(2));
    }
}