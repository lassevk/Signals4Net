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
    public void SetValue_NewValue_ChangesValue()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        state.Value = 15;
        Assert.That(state.Value, Is.EqualTo(15));
    }

    [Test]
    public void PropertyChanged_WhenNewValueIsAssigned_FiresEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        var fireCounter = 0;
        state.PropertyChanged += (_, _) => fireCounter++;

        state.Value = 15;
        Assert.That(state.Value, Is.EqualTo(15));
        Assert.That(fireCounter, Is.EqualTo(1));
    }

    [Test]
    public void PropertyChanged_WhenSameValueIsAssigned_DoesNotFireEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);

        var fireCounter = 0;
        state.PropertyChanged += (_, _) => fireCounter++;

        state.Value = 10;
        Assert.That(fireCounter, Is.EqualTo(0));
    }

    [Test]
    public void PropertyChanged_WhenSignalChanges_FiresEvent()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        var fireCount = 0;
        state.PropertyChanged += (_, _) => fireCount++;

        Assert.That(fireCount, Is.EqualTo(0));

        state.Value = 15;

        Assert.That(fireCount, Is.EqualTo(1));
    }

    [Test]
    public void PropertyChanged_WhenSignalChangesButInWriteScope_FiresEventOnlyAfterScopeHasBeenDisposed()
    {
        var context = new SignalContext();
        IState<int> state = context.State(10);
        var fireCount = 0;
        state.PropertyChanged += (_, _) => fireCount++;

        IDisposable scope = context.WriteScope();

        Assert.That(fireCount, Is.EqualTo(0));

        state.Value = 15;

        Assert.That(fireCount, Is.EqualTo(0));

        scope.Dispose();
        Assert.That(fireCount, Is.EqualTo(1));
    }
}