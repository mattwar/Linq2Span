using System.Numerics;

namespace Linq2Span;

public static class SpanQueryExtensions
{
    // operations represented as extensions here to allow for number constraints

    public static TSource Average<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
        where TSource : INumber<TSource>
    {
        var (total, count) = source.Aggregate(
            (total: TSource.Zero, count: TSource.Zero),
            (acc, value) => (acc.total + value, acc.count + TSource.One)
            );

        return total / count;
    }

    public static TNumber Average<TSpan, TSource, TNumber>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return source.Select(selector).Average();
    }

    public static TSource? Max<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
        where TSource : IComparable<TSource>
    {
        var hasValue = false;
        TSource max = default!;
        source.ForEach(value =>
        {
            if (!hasValue || value.CompareTo(max) > 0)
                max = value;
            hasValue = true;
        });

        return max;
    }

    public static TResult? Max<TSpan, TSource, TResult>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        var hasValue = false;
        TResult? max = default;

        source.ForEach(value =>
        {
            var result = selector(value);
            if (!hasValue || result.CompareTo(max) > 0)
                max = result;
            hasValue = true;
        });

        return max;
    }

    public static TSource? MaxBy<TSpan, TSource, TKey>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;
        var hasValue = false;
        TSource? max = default;
        TKey maxKey = default!;

        source.ForEach(value =>
        {
            var key = keySelector(value);
            if (!hasValue || comparer.Compare(key, maxKey) > 0)
            {
                max = value;
                maxKey = key;
            }
            hasValue = true;
        });

        return max;
    }


    public static TSource? Min<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
        where TSource : IComparable<TSource>
    {
        var hasValue = false;
        TSource min = default!;
        source.ForEach(value =>
        {
            if (!hasValue || value.CompareTo(min) < 0)
                min = value;
            hasValue = true;
        });

        return min;
    }

    public static TResult? Min<TSpan, TSource, TResult>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        var hasValue = false;
        TResult? min = default;

        source.ForEach(value =>
        {
            var result = selector(value);
            if (!hasValue || result.CompareTo(min) < 0)
                min = result;
            hasValue = true;
        });

        return min;
    }

    public static TSource? MinBy<TSpan, TSource, TKey>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;
        var hasValue = false;
        TSource? min = default;
        TKey minKey = default!;

        source.ForEach(value =>
        {
            var key = keySelector(value);
            if (!hasValue || comparer.Compare(key, minKey) < 0)
            {
                min = value;
                minKey = key;
            }
            hasValue = true;
        });

        return min;
    }

    public static TNumber Sum<T, TNumber>(
        this SpanQuery<T, TNumber> source)
        where TNumber : INumber<TNumber>
    {
        return source.Aggregate(TNumber.Zero, (total, value) => total + value);
    }

    public static TNumber Sum<TSpan, TSource, TNumber>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return source.Select(selector).Sum();
    }
}
