using NSubstitute;

namespace Signals4Net.Tests;

public class ComputedTests
{
    [Test]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new Computed<int>(null!, ct => Task.FromResult(10), EqualityComparer<int>.Default));
    }

    [Test]
    public void Constructor_WithNullExpression_ThrowsArgumentNullException()
    {
        var context = new SignalContext();
        Assert.Throws<ArgumentNullException>(() => _ = new Computed<int>(context, null!, EqualityComparer<int>.Default));
    }

    [Test]
    public void Constructor_WithNullComparer_ThrowsArgumentNullException()
    {
        var context = new SignalContext();
        Assert.Throws<ArgumentNullException>(() => _ = new Computed<int>(context, _ => Task.FromResult(10), null!));
    }

    [Test]
    public async Task Value_BasedOnSignal_ComputesCorrectValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(async () => await state.GetValueAsync() + 1);

        Assert.That(await computed.GetValueAsync(), Is.EqualTo(16));
    }

    [Test]
    public async Task GetValueAsync_SignalIsChanged_ComputesCorrectNewValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(async () => await state.GetValueAsync() + 1);
        Assert.That(await computed.GetValueAsync(), Is.EqualTo(16));
        await state.SetValueAsync(20);

        Assert.That(await computed.GetValueAsync(), Is.EqualTo(21));
    }

    [Test]
    public async Task GetValueAsync_DelaysExpressionEvaluationUntilCalled()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(async () => await state.GetValueAsync() + 1);
        await state.SetValueAsync(20);

        Assert.That(await computed.GetValueAsync(), Is.EqualTo(21));
    }

    [Test]
    public async Task Value_ReadTwice_DoesNotEvaluateExpressionTwice()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        Func<int>? expression = Substitute.For<Func<int>>();
        expression.Invoke().Returns(21);

        IComputed<int> computed = context.Computed(expression);
        Assert.That(await computed.GetValueAsync(), Is.EqualTo(21));
        Assert.That(await computed.GetValueAsync(), Is.EqualTo(21));
        expression.Received(1).Invoke();
    }

    [Test]
    public async Task Value_OfComputeBasedOnOtherCompute_IsCorrectlyEvaluated()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> compute1 = context.Computed(async () => await state.GetValueAsync() + 1);
        IComputed<int> compute2 = context.Computed(async () => await compute1.GetValueAsync() + 1);

        Assert.That(await compute2.GetValueAsync(), Is.EqualTo(17));
    }

    [Test]
    public async Task Value_OfComputeBasedOnOtherComputeWhenSignalChanges_IsCorrectlyEvaluated()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> compute1 = context.Computed(async () => await state.GetValueAsync() + 1);
        IComputed<int> compute2 = context.Computed(async () => await compute1.GetValueAsync() + 1);
        Assert.That(await compute2.GetValueAsync(), Is.EqualTo(17));

        await state.SetValueAsync(20);

        Assert.That(await compute2.GetValueAsync(), Is.EqualTo(22));
    }

    [Test]
    public async Task PropertyChanged_WhenComputeChanges_FiresEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        IComputed<int> computed = context.Computed(async () => await state.GetValueAsync() + 1);
        Assert.That(await computed.GetValueAsync(), Is.EqualTo(11));

        var fireCount = 0;
        computed.Subscribe(() => fireCount++);

        Assert.That(fireCount, Is.EqualTo(0));

        await state.SetValueAsync(15);

        Assert.That(fireCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Subscribe_WhenSignalChangesTwiceBeforeComputedIsRead_CallsSubscriberOnce()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        IComputed<int> computed = context.Computed(async () => await state.GetValueAsync() + 1);
        Assert.That(await computed.GetValueAsync(), Is.EqualTo(11));

        var fireCount = 0;
        computed.Subscribe(() => fireCount++);

        Assert.That(fireCount, Is.EqualTo(0));

        await state.SetValueAsync(15);
        await state.SetValueAsync(16);

        Assert.That(fireCount, Is.EqualTo(1));
    }
}