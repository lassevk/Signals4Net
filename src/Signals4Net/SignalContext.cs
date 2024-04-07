using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Signals4Net;

/// <summary>
/// This class is the main implementation of the signal system. All state and computed signals
/// that should be used together must be created from the same <see cref="SignalContext"/>
/// instance, or change propagation will not work.
/// </summary>
public class SignalContext : ISignalContextInternal
{
    private readonly object _lock = new();
    private readonly Stack<ComputeScope> _scopes = new();
    private readonly Dictionary<IComputedInternal, HashSet<ISignal>> _dependencies = new();
    private readonly Dictionary<ISignal, HashSet<IComputedInternal>> _computesThatDependsOnSignal = new();
    private readonly List<(object sender, PropertyChangedEventHandler handler, string propertyName)> _pendingPropertyChangedEvents = new();

    private int _writeScopeLevel;
    private int? _threadId;

    /// <inheritdoc cref="ISignalContext.State{T}"/>
    public IState<T> State<T>(T value = default!, EqualityComparer<T>? comparer = default) => new State<T>(this, value, comparer ?? EqualityComparer<T>.Default);

    /// <inheritdoc cref="ISignalContext.Computed{T}"/>
    public IComputed<T> Computed<T>(Func<T> expression, EqualityComparer<T>? comparer = default) => new Computed<T>(this, expression, comparer ?? EqualityComparer<T>.Default);

    /// <inheritdoc cref="ISignalContext.WriteScope"/>
    public IDisposable WriteScope()
    {
        ((ISupportInitialize)this).BeginInit();
        return new ActionDisposable(((ISupportInitialize)this).EndInit);
    }

    /// <inheritdoc cref="ISignalContext.Effect"/>
    public IDisposable Effect(Action action)
    {
        using IDisposable __ = ((ISignalContextInternal)this).ThreadScope();
        var counter = 0;
        IComputed<int> computed = Computed(() =>
        {
            action();
            return ++counter;
        });

        PropertyChangedEventHandler handler = (_, _) => _ = computed.Value;
        computed.PropertyChanged += handler;
        _ = computed.Value;

        return new ActionDisposable(() =>
        {
            using IDisposable _ = ((ISignalContextInternal)this).ThreadScope();
            computed.PropertyChanged -= handler;
            RemoveDependencies((IComputedInternal)computed);
        });
    }

    void ISignalContextInternal.OnRead(ISignal signal)
    {
        if (!_scopes.TryPeek(out ComputeScope? scope))
            return;

        scope.Read(signal);
    }

    void ISignalContextInternal.OnChanged(ISignal signal)
    {
        flagDependenciesDirty(signal);

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
        _scopes.Push(scope);
        return scope;
    }

    [ExcludeFromCodeCoverage]
    IDisposable ISignalContextInternal.ThreadScope()
    {
        lock (_lock)
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!_threadId.HasValue)
            {
                _threadId = currentThreadId;
                return new ActionDisposable(() =>
                {
                    if (Thread.CurrentThread.ManagedThreadId != _threadId)
                        throw new InvalidOperationException("The SignalContext class cannot be used across threads");

                    _threadId = null;
                });
            }

            if (_threadId.Value != currentThreadId)
                throw new InvalidOperationException("The SignalContext class cannot be used across threads");

            return VoidDisposable.Instance;
        }
    }

    void ISignalContextInternal.FinalizeComputeScope(ComputeScope scope)
    {
        if (scope != _scopes.Pop())
            throw new InvalidOperationException("ComputeScopes finalized out of order");

        RemoveDependencies(scope.Computed);
        _dependencies[scope.Computed] = scope.ReadSignals;
        foreach (ISignal dependency in scope.ReadSignals)
        {
            if (!_computesThatDependsOnSignal.TryGetValue(dependency, out HashSet<IComputedInternal>? computesThatDependsOnSignal))
                computesThatDependsOnSignal = _computesThatDependsOnSignal[dependency] = new();

            computesThatDependsOnSignal.Add(scope.Computed);
        }
    }

    private void RemoveDependencies(IComputedInternal computed)
    {
        if (!_dependencies.TryGetValue(computed, out HashSet<ISignal>? dependencies))
            return;

        foreach (ISignal signal in dependencies)
        {
            if (!_computesThatDependsOnSignal.TryGetValue(signal, out HashSet<IComputedInternal>? computesThatDependsOnSignal))
                continue;

            computesThatDependsOnSignal.Remove(computed);
        }

        _dependencies.Remove(computed);
    }

    void ISignalContextInternal.QueuePropertyChanged(object sender, PropertyChangedEventHandler? handler, string propertyName)
    {
        if (handler is null)
            return;

        _pendingPropertyChangedEvents.Add((sender, handler, propertyName));
    }

    void ISupportInitialize.BeginInit()
    {
        _writeScopeLevel++;
    }

    void ISupportInitialize.EndInit()
    {
        _writeScopeLevel--;
        if (_writeScopeLevel > 0)
            return;

        (object sender, PropertyChangedEventHandler handler, string propertyName)[] events = _pendingPropertyChangedEvents.ToArray();
        _pendingPropertyChangedEvents.Clear();

        foreach ((object sender, PropertyChangedEventHandler handler, string propertyName) item in events)
            item.handler(item.sender, new PropertyChangedEventArgs(item.propertyName));
    }
}