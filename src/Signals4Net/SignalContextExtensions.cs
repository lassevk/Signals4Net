namespace Signals4Net;

public static class SignalContextExtensions
{
    public static IComputed<T> Computed<T>(this ISignalContext context, Func<T> expression, IEqualityComparer<T>? comparer = null) => context.Computed<T>(_ => Task.FromResult(expression()));
    public static IComputed<T> Computed<T>(this ISignalContext context, Func<Task<T>> expression, IEqualityComparer<T>? comparer = null) => context.Computed<T>(async _ => await expression());

    public static Task<IDisposable> AddEffectAsync(this ISignalContext context, Func<Task> effect, CancellationToken cancellationToken = default)
        => context.AddEffectAsync(async _ => await effect(), cancellationToken);

    public static Task<IDisposable> AddEffectAsync(this ISignalContext context, Action effect, CancellationToken cancellationToken = default)
        => context.AddEffectAsync(_ =>
        {
            effect();
            return Task.CompletedTask;
        }, cancellationToken);
}