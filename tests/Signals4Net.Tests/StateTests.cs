namespace Signals4Net.Tests;

public class StateTests
{
    [Test]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new State<int>(null!, 10, EqualityComparer<int>.Default));
    }

    [Test]
    public void Constructor_WithNullComparer_ThrowsArgumentNullException()
    {
        var context = new SignalContext();
        Assert.Throws<ArgumentNullException>(() => _ = new State<int>(context, 10, null!));
    }

    [Test]
    public async Task SetValue_NewValue_ChangesValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        await state.SetValueAsync(15);
        Assert.That(await state.GetValueAsync(), Is.EqualTo(15));
    }

    [Test]
    public async Task PropertyChanged_WhenNewValueIsAssigned_FiresEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        var fireCounter = 0;
        state.Subscribe(_ => fireCounter++);

        await state.SetValueAsync(15);
        Assert.That(await state.GetValueAsync(), Is.EqualTo(15));
        Assert.That(fireCounter, Is.EqualTo(1));
    }

    [Test]
    public async Task Subscribe_WhenSameValueIsAssigned_DoesNotCallSubscriber()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        var fireCounter = 0;
        state.Subscribe(() => fireCounter++);

        await state.SetValueAsync(10);
        Assert.That(fireCounter, Is.EqualTo(0));
    }

    [Test]
    public async Task Subscribe_WhenSignalChanges_CallsSubscriber()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        var fireCount = 0;

        state.Subscribe(() => fireCount++);

        Assert.That(fireCount, Is.EqualTo(0));

        await state.SetValueAsync(15);

        Assert.That(fireCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Subscribe_WhenSignalChangesButInWriteScope_CallsSubscriberOnlyAfterScopeHasBeenDisposed()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        var fireCount = 0;
        state.Subscribe(() => fireCount++);

        IDisposable scope = context.WriteScope();

        Assert.That(fireCount, Is.EqualTo(0));

        await state.SetValueAsync(15);

        Assert.That(fireCount, Is.EqualTo(0));

        scope.Dispose();
        Assert.That(fireCount, Is.EqualTo(1));
    }
}