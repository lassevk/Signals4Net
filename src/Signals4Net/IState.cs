namespace Signals4Net;

public interface IState<T> : IReadOnlySignal<T>, ISignal
{
    Task SetValueAsync(T value, CancellationToken cancellationToken = default);

    async Task MutateAsync(Func<T, T> mutator, CancellationToken cancellationToken = default)
    {
        await SetValueAsync(mutator(await GetValueAsync(cancellationToken)), cancellationToken);
    }

    async Task MutateAsync(Func<T, Task<T>> mutator, CancellationToken cancellationToken = default)
    {
        await SetValueAsync(await mutator(await GetValueAsync(cancellationToken)), cancellationToken);
    }

    void Freeze();
}