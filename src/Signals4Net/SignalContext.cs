using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

public class SignalContext : ISignalContextInternal
{
    private readonly object _lock = new();
    private readonly Dictionary<IComputedInternal, HashSet<ISignal>> _dependencies = new();
    private readonly Dictionary<ISignal, HashSet<IComputedInternal>> _computesThatDependsOnSignal = new();
    private readonly List<(ISignal signal, Func<ISignal, Task> subscriber)> _pendingSubscriberNotifications = new();

    private readonly AsyncLocal<Stack<ComputeScope>?> _scopes = new();
    private readonly AsyncLocal<int> _writeScopeLevel = new();

    public IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default) => new State<T>(this, value, comparer ?? EqualityComparer<T>.Default);

    public IComputed<T> Computed<T>(Func<CancellationToken, Task<T>> expression, EqualityComparer<T>? comparer = default) => new Computed<T>(this, expression, comparer ?? EqualityComparer<T>.Default);

    public IDisposable WriteScope()
    {
        ((ISupportInitialize)this).BeginInit();
        return new ActionDisposable(((ISupportInitialize)this).EndInit);
    }

    public async Task<IDisposable> AddEffectAsync(Func<CancellationToken, Task> effect, CancellationToken cancellationToken = default)
    {
        var counter = 0;
        IComputed<int> computed = Computed(async ct =>
        {
            await effect(ct);
            return ++counter;
        });

        async Task subscriber(ISignal _) => await computed.GetValueAsync(CancellationToken.None);

        IDisposable subscription = computed.Subscribe(subscriber);
        _ = await computed.GetValueAsync(cancellationToken);

        return new ActionDisposable(() =>
        {
            subscription.Dispose();
            RemoveDependencies((IComputedInternal)computed);
        });
    }

    void ISignalContextInternal.OnRead(ISignal signal)
    {
        if (_scopes.Value == null)
            return;

        _scopes.Value!.TryPeek(out ComputeScope? scope);
        scope?.Read(signal);
    }

    void ISignalContextInternal.OnChanged(ISignal signal)
    {
        lock (_lock)
        {
            flagDependenciesDirty(signal);
        }

        void flagDependenciesDirty(ISignal currentSignal)
        {
            if (!_computesThatDependsOnSignal.TryGetValue(currentSignal, out HashSet<IComputedInternal>? computesThatDependsOnSignal))
                return;

            foreach (IComputedInternal computed in computesThatDependsOnSignal)
            {
                computed.SetDirty();
                flagDependenciesDirty(computed);
            }
        }
    }

    IDisposable ISignalContextInternal.ComputeScope(IComputedInternal computed)
    {
        var scope = new ComputeScope(this, computed);
        _scopes.Value ??= new();
        _scopes.Value.Push(scope);
        return scope;
    }

    void ISignalContextInternal.FinalizeComputeScope(ComputeScope scope)
    {
        if (scope != _scopes.Value!.Pop())
            throw new InvalidOperationException("ComputeScopes finalized out of order");

        if (_scopes.Value!.Count == 0)
            _scopes.Value = null;

        lock (_lock)
        {
            RemoveDependencies(scope.Computed);
            _dependencies[scope.Computed] = scope.ReadSignals;
            foreach (ISignal dependency in scope.ReadSignals)
            {
                if (!_computesThatDependsOnSignal.TryGetValue(dependency, out HashSet<IComputedInternal>? computesThatDependsOnSignal))
                    computesThatDependsOnSignal = _computesThatDependsOnSignal[dependency] = new();

                computesThatDependsOnSignal.Add(scope.Computed);
            }
        }
    }

    private void RemoveDependencies(IComputedInternal computed)
    {
        lock (_lock)
        {
            if (!_dependencies.TryGetValue(computed, out HashSet<ISignal>? dependencies))
                return;

            foreach (ISignal signal in dependencies)
            {
                _computesThatDependsOnSignal.TryGetValue(signal, out HashSet<IComputedInternal>? computesThatDependsOnSignal);
                computesThatDependsOnSignal?.Remove(computed);
            }

            _dependencies.Remove(computed);
        }
    }

    void ISignalContextInternal.QueueSubscriberNotification(ISignal signal, Func<ISignal, Task> subscriber)
    {
        lock (_lock)
        {
            _pendingSubscriberNotifications.Add((signal, subscriber));
        }
    }

    void ISupportInitialize.BeginInit()
    {
        _writeScopeLevel.Value++;
    }

    void ISupportInitialize.EndInit()
    {
        _writeScopeLevel.Value--;
        if (_writeScopeLevel.Value > 0)
            return;

        (ISignal signal, Func<ISignal, Task> subscriber)[]? pendingNotifications;
        lock (_lock)
        {
            pendingNotifications = _pendingSubscriberNotifications.ToArray();
            _pendingSubscriberNotifications.Clear();
        }

        foreach ((ISignal signal, Func<ISignal, Task> subscriber) notification in pendingNotifications)
            notification.subscriber(notification.signal);
    }
}