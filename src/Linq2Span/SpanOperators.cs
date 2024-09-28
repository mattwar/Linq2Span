using System.Numerics;

namespace Linq2Span;

/// <summary>
/// LINQ operators on <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/> that return to a single value.
/// For other operations, use span.AsQuery().
/// </summary>
public static class SpanOperators
{
    #region All
    public static bool All<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (!predicate(source[i]))
                return false;
        }

        return true;
    }

    public static bool All<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return All((ReadOnlySpan<TSource>)source, predicate);
    }
    #endregion

    #region Aggregate
    public static TAggregate Aggregate<TSource, TAggregate>(
        this ReadOnlySpan<TSource> source,
        TAggregate seed,
        Func<TAggregate, TSource, TAggregate> func)
    {
        var aggregate = seed;

        for (int i = 0; i < source.Length; i++)
        {
            aggregate = func(aggregate, source[i]);
        }

        return aggregate;
    }

    public static TAggregate Aggregate<TSource, TAggregate>(
        this Span<TSource> source,
        TAggregate seed,
        Func<TAggregate, TSource, TAggregate> func)
    {
        return Aggregate((ReadOnlySpan<TSource>)source, seed, func);
    }
    #endregion

    #region Any
    public static bool Any<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0;
    }

    public static bool Any<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
                return true;
        }

        return false;
    }

    public static bool Any<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return Any((ReadOnlySpan<TSource>)source, predicate);
    }

    public static bool Any<TSource>(
        this Span<TSource> source)
    {
        return Any((ReadOnlySpan<TSource>)source);
    }

    #endregion

    #region Average
    public static TSource Average<TSource>(
        this ReadOnlySpan<TSource> source)
        where TSource : INumber<TSource>
    {
        var (total, count) = source.Aggregate(
            (total: TSource.Zero, count: TSource.Zero),
            (acc, value) => (acc.total + value, acc.count + TSource.One)
            );

        return total / count;
    }

    public static TSource Average<TSource>(
        this Span<TSource> source)
        where TSource : INumber<TSource>
    {
        return Average((ReadOnlySpan<TSource>)source);
    }

    public static TNumber Average<TSource, TNumber>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        var (total, count) = source.Aggregate(
            (total: TNumber.Zero, count: TNumber.Zero),
            (acc, value) => (acc.total + selector(value), acc.count + TNumber.One)
            );

        return total / count;
    }

    public static TNumber Average<TSource, TNumber>(
        this Span<TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return Average((ReadOnlySpan<TSource>)source, selector);
    }

    #endregion

    #region Contains

    public static bool Contains<TSource>(
        this ReadOnlySpan<TSource> source,
        TSource value,
        IEqualityComparer<TSource>? comparer = null)
    {
        comparer ??= EqualityComparer<TSource>.Default;
        
        for (int i = 0; i < source.Length; i++)
        {
            if (comparer.Equals(source[i], value))
                return true;
        }

        return false;
    }

    public static bool Contains<TSource>(
        this Span<TSource> source,
        TSource value,
        IEqualityComparer<TSource>? comparer = null)
    {
        return Contains((ReadOnlySpan<TSource>)source, value, comparer);
    }

    #endregion

    #region Count
    public static int Count<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length;
    }

    public static int Count<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        int count = 0;
        
        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
                count++;
        }

        return count;
    }

    public static int Count<TSource>(
        this Span<TSource> source)
    {
        return source.Length;
    }

    public static int Count<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return Count((ReadOnlySpan<TSource>)source, predicate);
    }

    #endregion

    #region ElementAt

    public static TSource ElementAt<TSource>(
        this ReadOnlySpan<TSource> source,
        int index)
    {
        if (index >= 0 && index < source.Length)
            return source[index];

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static TSource ElementAt<TSource>(
        this ReadOnlySpan<TSource> source,
        Index index)
    {
        var offset = index.GetOffset(source.Length);

        if (offset >= 0 && offset < source.Length)
            return source[offset];

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static TSource ElementAt<TSource>(
        this Span<TSource> source,
        int index)
    {
        return ElementAt((ReadOnlySpan<TSource>)source, index);
    }

    public static TSource? ElementAt<TSource>(
        this Span<TSource> source,
        Index index)
    {
        return ElementAt((ReadOnlySpan<TSource>)source, index);
    }

    public static TSource? ElementAtOrDefault<TSource>(
        this ReadOnlySpan<TSource> source,
        int index)
    {
        if (index >= 0 && index < source.Length)
            return source[index];

        return default;
    }

    public static TSource? ElementAtOrDefault<TSource>(
        this ReadOnlySpan<TSource> source,
        Index index)
    {
        var offset = index.GetOffset(source.Length);

        if (offset >= 0 && offset < source.Length)
            return source[offset];

        return default;
    }

    public static TSource? ElementAtOrDefault<TSource>(
        this Span<TSource> source,
        int index)
    {
        return ElementAtOrDefault((ReadOnlySpan<TSource>)source, index);
    }

    public static TSource? ElementAtOrDefault<TSource>(
        this Span<TSource> source,
        Index index)
    {
        return ElementAtOrDefault((ReadOnlySpan<TSource>)source, index);
    }

    #endregion

    #region First
    public static TSource First<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0
            ? source[0]
            : throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    public static TSource First<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
                return source[i];
        }

        throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    public static TSource First<TSource>(
        this Span<TSource> source)
    {
        return First((ReadOnlySpan<TSource>)source);
    }

    public static TSource First<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return First((ReadOnlySpan<TSource>)source, predicate);
    }
    #endregion

    #region FirstOrDefault
    public static TSource FirstOrDefault<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0
            ? source[0]
            : default!;
    }

    public static TSource FirstOrDefault<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
                return source[i];
        }

        return default!;
    }

    public static TSource FirstOrDefault<TSource>(
        this Span<TSource> source)
    {
        return FirstOrDefault((ReadOnlySpan<TSource>)source);
    }

    public static TSource FirstOrDefault<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return FirstOrDefault((ReadOnlySpan<TSource>)source, predicate);
    }
    #endregion

    #region ForEach
    public static void ForEach<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, int, bool> action)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (!action(source[i], i))
                return;
        }
    }

    public static void ForEach<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> action)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (!action(source[i]))
                return;
        }
    }

    public static void ForEach<TSource>(
        this ReadOnlySpan<TSource> source,
        Action<TSource, int> action)
    {
        for (int i = 0; i < source.Length; i++)
        {
            action(source[i], i);
        }
    }

    public static void ForEach<TSource>(
        this ReadOnlySpan<TSource> source,
        Action<TSource> action)
    {
        for (int i = 0; i < source.Length; i++)
        {
            action(source[i]);
        }
    }

    public static void ForEach<TSource>(
        this Span<TSource> source,
        Func<TSource, int, bool> action)
    {
        ForEach((ReadOnlySpan<TSource>)source, action);
    }

    public static void ForEach<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> action)
    {
        ForEach((ReadOnlySpan<TSource>)source, action);
    }

    public static void ForEach<TSource>(
        this Span<TSource> source,
        Action<TSource, int> action)
    {
        ForEach((ReadOnlySpan<TSource>)source, action);
    }

    public static void ForEach<TSource>(
        this Span<TSource> source,
        Action<TSource> action)
    {
        ForEach((ReadOnlySpan<TSource>)source, action);
    }

    #endregion

    #region Last

    public static TSource Last<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0
            ? source[source.Length - 1]
            : throw Exceptions.GetSequenceIsEmpty();
    }

    public static TSource Last<TSource>(
        this Span<TSource> source)
    {
        return source.Length > 0
            ? source[source.Length - 1]
            : throw Exceptions.GetSequenceIsEmpty();
    }

    #endregion

    #region LastOrDefault

    public static TSource? LastOrDefault<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0
            ? source[source.Length - 1]
            : default;
    }

    public static TSource? LastOrDefault<TSource>(
        this Span<TSource> source)
    {
        return source.Length > 0
            ? source[source.Length - 1]
            : default;
    }

    #endregion

    #region Max

    public static TSource? Max<TSource>(
        this ReadOnlySpan<TSource> source)
        where TSource : IComparable<TSource>
    {
        var hasValue = false;
        TSource? max = default;

        for (int i = 0; i < source.Length; i++)
        {
            if (!hasValue || source[i].CompareTo(max) > 0)
            {
                max = source[i];
                hasValue = true;
            }
        }

        return max;
    }

    public static TResult? Max<TSource, TResult>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        var hasValue = false;
        TResult? max = default;

        for (int i = 0; i < source.Length; i++)
        {
            var result = selector(source[i]);
            if (!hasValue || result.CompareTo(max) > 0)
            {
                max = result;
                hasValue = true;
            }
        }

        return max;
    }

    public static TSource? Max<TSource>(
        this Span<TSource> source)
        where TSource : IComparable<TSource>
    {
        return Max((ReadOnlySpan<TSource>)source);
    }

    public static TResult? Max<TSource, TResult>(
        this Span<TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        return Max((ReadOnlySpan<TSource>)source, selector);
    }

    #endregion

    #region MaxBy

    public static TSource? MaxBy<TSource, TKey>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        var hasValue = false;
        TSource? max = default;
        TKey? maxKey = default;

        for (int i = 0; i < source.Length; i++)
        {
            var key = keySelector(source[i]);
            if (!hasValue || comparer.Compare(key, maxKey) > 0)
            {
                max = source[i];
                maxKey = key;
                hasValue = true;
            }
        }

        return max;
    }

    public static TSource? MaxBy<TSource, TKey>(
        this Span<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        return MaxBy((ReadOnlySpan<TSource>)source, keySelector, comparer);
    }

    #endregion

    #region Min

    public static TSource? Min<TSource>(
        this ReadOnlySpan<TSource> source)
        where TSource : IComparable<TSource>
    {
        var hasValue = false;
        TSource? min = default;

        for (int i = 0; i < source.Length; i++)
        {
            if (!hasValue || source[i].CompareTo(min) < 0)
            {
                min = source[i];
                hasValue = true;
            }
        }

        return min;
    }

    public static TResult? Min<TSource, TResult>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        var hasValue = false;
        TResult? min = default;

        for (int i = 0; i < source.Length; i++)
        {
            var result = selector(source[i]);
            if (!hasValue || result.CompareTo(min) < 0)
            {
                min = result;
                hasValue = true;
            }
        }

        return min;
    }

    public static TSource? Min<TSource>(
        this Span<TSource> source)
        where TSource : IComparable<TSource>
    {
        return Min((ReadOnlySpan<TSource>)source);
    }

    public static TResult? Min<TSource, TResult>(
        this Span<TSource> source,
        Func<TSource, TResult> selector)
        where TResult : IComparable<TResult>
    {
        return Min((ReadOnlySpan<TSource>)source, selector);
    }

    #endregion

    #region MinBy

    public static TSource? MinBy<TSource, TKey>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        var hasValue = false;
        TSource? min = default;
        TKey? minKey = default;

        for (int i = 0; i < source.Length; i++)
        {
            var key = keySelector(source[i]);
            if (!hasValue || comparer.Compare(key, minKey) < 0)
            {
                min = source[i];
                minKey = key;
                hasValue = true;
            }
        }

        return min;
    }

    public static TSource? MinBy<TSource, TKey>(
        this Span<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        return MinBy((ReadOnlySpan<TSource>)source, keySelector, comparer);
    }

    #endregion

    #region Single

    public static TSource Single<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        if (source.Length == 0 || source.Length > 1)
            throw Exceptions.GetSequenceContainsMoreThanOneElementOrEmpty();
        return source[0];
    }

    public static TSource Single<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        int count = 0;
        TSource element = default!;

        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
            {
                count++;
                if (count > 1)
                    throw Exceptions.GetSequenceContainsMoreThanOneElementOrEmpty();
                element = source[i];
            }
        }

        if (count != 1)
            throw Exceptions.GetSequenceContainsMoreThanOneElementOrEmpty();

        return element;
    }

    public static TSource Single<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return Single((ReadOnlySpan<TSource>)source, predicate);
    }

    public static TSource Single<TSource>(
        this Span<TSource> source)
    {
        return Single((ReadOnlySpan<TSource>)source);
    }

    #endregion

    #region SingleOrDefault

    public static TSource SingleOrDefault<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        if (source.Length > 1)
            throw Exceptions.GetSequenceContainsMoreThanOneElement();
        else if (source.Length == 0)
            return default!;
        return source[0];
    }

    public static TSource SingleOrDefault<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        int count = 0;
        TSource element = default!;

        for (int i = 0; i < source.Length; i++)
        {
            if (predicate(source[i]))
            {
                count++;
                if (count > 1)
                    throw Exceptions.GetSequenceContainsMoreThanOneElement();
                element = source[i];
            }
        }

        if (count == 0)
            return default!;

        return element;
    }

    public static TSource SingleOrDefault<TSource>(
        this Span<TSource> source)
    {
        return SingleOrDefault((ReadOnlySpan<TSource>)source);
    }

    public static TSource SingleOrDefault<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return SingleOrDefault((ReadOnlySpan<TSource>)source, predicate);
    }
    #endregion

    #region Sum
    public static TSource Sum<TSource>(
        this ReadOnlySpan<TSource> source)
        where TSource : INumber<TSource>
    {
        var total = TSource.Zero;

        for (int i = 0; i < source.Length; i++)
        {
            total += source[i];
        }

        return total;
    }

    public static TSource Sum<TSource>(
        this Span<TSource> source)
        where TSource : INumber<TSource>
    {
        return Sum((ReadOnlySpan<TSource>)source);
    }

    public static TNumber Sum<TSource, TNumber>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        var total = TNumber.Zero;

        for (int i = 0; i < source.Length; i++)
        {
            var value = selector(source[i]);
            total += value;
        }

        return total;
    }

    public static TNumber Sum<TSpan, TNumber>(
        this Span<TSpan> source,
        Func<TSpan, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return Sum((ReadOnlySpan<TSpan>)source, selector);
    }
    #endregion
}
