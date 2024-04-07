using NSubstitute;

namespace Signals4Net.Tests;

public class ActionDisposableTests
{
    [Test]
    public void Constructor_WithAction_DoesNotCallAction()
    {
        Action action = Substitute.For<Action>();
        _ = new ActionDisposable(action);

        action.DidNotReceive().Invoke();
    }

    [Test]
    public void Dispose_CallsAction()
    {
        Action? action = Substitute.For<Action>();
        new ActionDisposable(action).Dispose();

        action.Received().Invoke();
    }

    [Test]
    public void Dispose_CalledTwice_OnlyCallsActionOnce()
    {
        Action? action = Substitute.For<Action>();
        var disposable = new ActionDisposable(action);
        disposable.Dispose();
        disposable.Dispose();

        action.Received(1).Invoke();
    }
}