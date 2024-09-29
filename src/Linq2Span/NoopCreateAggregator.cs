namespace Linq2Span;

internal class NoopCreateAggregator<TElement>
{
    public static readonly NoopCreateAggregator<TElement> Instance = new NoopCreateAggregator<TElement>();

    public (Func<bool> pre, Func<TElement, int, bool> aggregator, Action post) Create(
        Func<bool> pre,
        Func<TElement, int, bool> aggregator,
        Action post)
    {
        return (pre, aggregator, post);
    }
}
