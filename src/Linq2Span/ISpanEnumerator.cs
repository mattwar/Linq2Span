namespace Linq2Span;

public interface ISpanEnumerator<TSpan, TElement>
{
    TElement Current { get; }
    bool MoveNext(ReadOnlySpan<TSpan> span);
}
