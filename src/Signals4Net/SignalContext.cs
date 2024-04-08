namespace Signals4Net;

public class SignalContext : ISignalContextInternal
{
    private readonly object _lock = new();
    private readonly Dictionary<ISignal, HashSet<ISignal>> _dependencies = new();
    private readonly Dictionary<ISignal, HashSet<ISignal>> _computesThatDependsOnSignal = new();
    private readonly List<(ISignal signal, Func<ISignal, Task> subscriber)> _pendingSubscriberNotifications = new();

    private readonly AsyncLocal<Stack<ComputeScope>?> _scopes = new();
    private readonly AsyncLocal<int> _writeScopeLevel = new();

    public IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default) => new State<T>(this, value, comparer ?? EqualityComparer<T>.Default);

    public IComputed<T> Computed<T>(Func<CancellationToken, Task<T>> expression, EqualityComparer<T>? comparer = default) => new Computed<T>(this, expression, comparer ?? EqualityComparer<T>.Default);

    public void Remove(ISignal signal)
    {
        lock (_lock)
        {
            if (!_dependencies.TryGetValue(signal, out HashSet<ISignal>? dependencies))
                return;

            foreach (ISignal dependency in dependencies)
            {
                _computesThatDependsOnSignal.TryGetValue(dependency, out HashSet<ISignal>? computesThatDependsOnSignal);
                computesThatDependsOnSignal?.Remove(signal);
            }

            _dependencies.Remove(signal);
        }
    }

    public IAsyncDisposable WriteScope()
    {
        _writeScopeLevel.Value++;

        return new AsyncActionDisposable(async () =>
        {
            _writeScopeLevel.Value--;
            if (_writeScopeLevel.Value == 0)
            {
                (ISignal signal, Func<ISignal, Task> subscriber)[]? pendingNotifications;
                lock (_lock)
                {
                    pendingNotifications = _pendingSubscriberNotifications.ToArray();
                    _pendingSubscriberNotifications.Clear();
                }

                foreach ((ISignal signal, Func<ISignal, Task> subscriber) notification in pendingNotifications)
                    await notification.subscriber(notification.signal);
            }
        });
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
            Remove(computed);
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
            if (!_computesThatDependsOnSignal.TryGetValue(currentSignal, out HashSet<ISignal>? computesThatDependsOnSignal))
                return;

            foreach (ISignal computed in computesThatDependsOnSignal)
            {
                ((IComputedInternal)computed).SetDirty();
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
            Remove(scope.Computed);
            _dependencies[scope.Computed] = scope.ReadSignals;
            foreach (ISignal dependency in scope.ReadSignals)
            {
                if (!_computesThatDependsOnSignal.TryGetValue(dependency, out HashSet<ISignal>? computesThatDependsOnSignal))
                    computesThatDependsOnSignal = _computesThatDependsOnSignal[dependency] = new();

                computesThatDependsOnSignal.Add(scope.Computed);
            }
        }
    }

    void ISignalContextInternal.QueueSubscriberNotification(ISignal signal, Func<ISignal, Task> subscriber)
    {
        lock (_lock)
        {
            _pendingSubscriberNotifications.Add((signal, subscriber));
        }
    }
}