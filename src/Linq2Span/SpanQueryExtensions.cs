using Linq2Span.Enumerators;
using System;
using System.Numerics;

namespace Linq2Span;

public static class SpanQueryExtensions
{
    public static SpanQuery<TElement, TElement, SpanEnumerator<TElement>> AsSpanQuery<TElement>(
        this ReadOnlySpan<TElement> span)
    {
        return new SpanQuery<TElement, TElement, SpanEnumerator<TElement>>(
            span,
            new SpanEnumerator<TElement>()
            );
    }

    public static SpanQuery<TElement, TElement, SpanEnumerator<TElement>> AsSpanQuery<TElement>(
        this Span<TElement> span)
    {
        return AsSpanQuery((ReadOnlySpan<TElement>)span);
    }

    public static SpanQuery<TElement, TElement, SpanEnumerator<TElement>> AsSpanQuery<TElement>(
        this TElement[] array)
    {
        return array.AsSpan().AsSpanQuery();
    }

    public static SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> ThenBy<TSpan, TElement, TEnumerator, TKey>(
        this SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> query,
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
        where TEnumerator : ISpanEnumerator<TSpan, TElement>
    {
        keyComparer ??= Comparer<TKey>.Default;
        return ThenBy(query, keySelector, keyComparer, isDescending: false);
    }

    public static SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> ThenByDescending<TSpan, TElement, TEnumerator, TKey>(
        this SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> query,
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
        where TEnumerator : ISpanEnumerator<TSpan, TElement>
    {
        keyComparer ??= Comparer<TKey>.Default;
        return ThenBy(query, keySelector, keyComparer, isDescending: true);
    }

    private static SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> ThenBy<TSpan, TElement, TEnumerator, TKey>(
        SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> query,
        Func<TElement, TKey> keySelector,
        IComparer<TKey> keyComparer,
        bool isDescending)
        where TEnumerator : ISpanEnumerator<TSpan, TElement>
    {
        keyComparer ??= Comparer<TKey>.Default;

        var outerComparison = query.SpanEnumerator._comparison;
        Comparison<TElement> innerComparison =
            isDescending
                ? (a, b) => -keyComparer.Compare(keySelector(a), keySelector(b))
                : (a, b) => keyComparer.Compare(keySelector(a), keySelector(b));

        Comparison<TElement> comparison = (a, b) =>
        {
            var result = outerComparison(a, b);
            if (result == 0)
                result = innerComparison(a, b);
            return result;
        };

        return query.With(
            new OrderByEnumerator<TSpan, TElement, TEnumerator>(
                query.SpanEnumerator._enumerator,
                comparison
            ));
    }

    // Operators w/ extra constraints

    public static TSource Average<TSpan, TSource, TEnumerator>(
        this SpanQuery<TSpan, TSource, TEnumerator> source
        )
        where TEnumerator : struct, ISpanEnumerator<TSpan, TSource>
        where TSource : INumber<TSource>
    {
        return source.Average(x => x);
    }

    public static TSource? Max<TSpan, TSource, TEnumerator>(
        this SpanQuery<TSpan, TSource, TEnumerator> source
        )
        where TEnumerator : struct, ISpanEnumerator<TSpan, TSource>
        where TSource : IComparable<TSource>
    {
        return source.MaxBy(x => x);
    }

    public static TSource? Min<TSpan, TSource, TEnumerator>(
        this SpanQuery<TSpan, TSource, TEnumerator> source
        )
        where TEnumerator : struct, ISpanEnumerator<TSpan, TSource>
        where TSource : IComparable<TSource>
    {
        return source.MinBy(x => x);
    }

    public static TSource Sum<TSpan, TSource, TEnumerator>(
        this SpanQuery<TSpan, TSource, TEnumerator> source
        )
        where TEnumerator : struct, ISpanEnumerator<TSpan, TSource>
        where TSource : INumber<TSource>
    {
        return source.Sum(x => x);
    }
}
