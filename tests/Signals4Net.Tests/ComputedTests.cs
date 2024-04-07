using NSubstitute;

namespace Signals4Net.Tests;

public class ComputedTests
{
    [Test]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new Computed<int>(null!, () => 10, EqualityComparer<int>.Default));
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
        Assert.Throws<ArgumentNullException>(() => _ = new Computed<int>(context, () => 10, null!));
    }

    [Test]
    public void Value_BasedOnSignal_ComputesCorrectValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(() => state.Value + 1);

        Assert.That(computed.Value, Is.EqualTo(16));
    }

    [Test]
    public void GetValueAsync_SignalIsChanged_ComputesCorrectNewValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(() => state.Value + 1);
        Assert.That(computed.Value, Is.EqualTo(16));
        state.Value = 20;

        Assert.That(computed.Value, Is.EqualTo(21));
    }

    [Test]
    public void GetValueAsync_DelaysExpressionEvaluationUntilCalled()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> computed = context.Computed(() => state.Value + 1);
        state.Value = 20;

        Assert.That(computed.Value, Is.EqualTo(21));
    }

    [Test]
    public void Value_ReadTwice_DoesNotEvaluateExpressionTwice()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        Func<int>? expression = Substitute.For<Func<int>>();
        expression.Invoke().Returns(21);

        IComputed<int> computed = context.Computed(expression);
        Assert.That(computed.Value, Is.EqualTo(21));
        Assert.That(computed.Value, Is.EqualTo(21));
        expression.Received(1).Invoke();
    }

    [Test]
    public void Value_OfComputeBasedOnOtherCompute_IsCorrectlyEvaluated()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> compute1 = context.Computed(() => state.Value + 1);
        IComputed<int> compute2 = context.Computed(() => compute1.Value + 1);

        Assert.That(compute2.Value, Is.EqualTo(17));
    }

    [Test]
    public void Value_OfComputeBasedOnOtherComputeWhenSignalChanges_IsCorrectlyEvaluated()
    {
        var context = new SignalContext();
        IState<int> state = context.State(15);
        IComputed<int> compute1 = context.Computed(() => state.Value + 1);
        IComputed<int> compute2 = context.Computed(() => compute1.Value + 1);
        Assert.That(compute2.Value, Is.EqualTo(17));

        state.Value = 20;

        Assert.That(compute2.Value, Is.EqualTo(22));
    }

    [Test]
    public void PropertyChanged_WhenComputeChanges_FiresEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        IComputed<int> computed = context.Computed(() => state.Value + 1);
        Assert.That(computed.Value, Is.EqualTo(11));

        var fireCount = 0;
        computed.PropertyChanged += (_, _) => fireCount++;

        Assert.That(fireCount, Is.EqualTo(0));

        state.Value = 15;

        Assert.That(fireCount, Is.EqualTo(1));
    }

    [Test]
    public void PropertyChanged_WhenSignalChangesTwiceBeforeComputedIsRead_FiresEventOnce()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        IComputed<int> computed = context.Computed(() => state.Value + 1);
        Assert.That(computed.Value, Is.EqualTo(11));

        var fireCount = 0;
        computed.PropertyChanged += (_, _) => fireCount++;

        Assert.That(fireCount, Is.EqualTo(0));

        state.Value = 15;
        state.Value = 16;

        Assert.That(fireCount, Is.EqualTo(1));
    }
}