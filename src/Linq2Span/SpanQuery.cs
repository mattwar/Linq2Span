using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Span;

public ref struct SpanQuery<TSpan, TElement>
{
    private readonly ReadOnlySpan<TSpan> _span;
    private readonly CreateAggregator<TElement, TSpan> _createAggregator;

    public SpanQuery(
        ReadOnlySpan<TSpan> span,
        CreateAggregator<TElement, TSpan> createAggregator)
    {
        _span = span;
        _createAggregator = createAggregator;
    }

    /// <summary>
    /// Continues this enumerable by specifing a new function that creates a composite aggregator
    /// from the previous one.
    /// </summary>
    public SpanQuery<TSpan, TResult> Continue<TResult>(
        CreateCreateAggregator<TResult, TElement, TSpan> createCreateAggregator)
    {
        return new SpanQuery<TSpan, TResult>(
            _span,
            createCreateAggregator(_createAggregator)
            );
    }

    /// <summary>
    /// Calls the function for each element in order.
    /// If the function return false the enumeration stops early.
    /// </summary>
    public void ForEach(Func<TElement, int, bool> action)
    {
        var (aggregator, flusher) = _createAggregator(action, () => { });

        for (int i = 0; i < _span.Length; i++)
        {
            if (!aggregator(_span[i], i))
                break;
        }

        flusher();
    }

    /// <summary>
    /// Calls the function for each element in order.
    /// If the function return false the enumeration stops early.
    /// </summary>
    public void ForEach(Func<TElement, bool> action)
    {
        ForEach((value, index) => action(value));
    }

    /// <summary>
    /// Iterates over the elements of the span and applies the action to each element.
    /// </summary>
    public void ForEach(Action<TElement, int> action) =>
        ForEach((value, index) => { action(value, index); return true; });

    /// <summary>
    /// Iterates over the elements of the span and applies the action to each element.
    /// </summary>
    public void ForEach(Action<TElement> action) =>
        ForEach(value => { action(value); return true; });

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to a <see cref="List{T}"/>
    /// </summary>
    public List<TElement> ToList()
    {
        var list = new List<TElement>();
        ForEach(list.Add);
        return list;
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to an array.
    /// </summary>
    public TElement[] ToArray()
    {
        return ToList().ToArray();
    }

#if false
    /// <summary>
    /// Gets the enumerator for this <see cref="SpanQuery{TSpan, TValue}"/>
    /// </summary>
    public IEnumerator<TElement> GetEnumerator()
    {
        return ToList().GetEnumerator();
    }
#endif

    /// <summary>
    /// Converts the elements to the type <see cref="TType"/>.
    /// </summary>
    public SpanQuery<TSpan, TType> Cast<TType>()
    {
        return this.Select(value => (TType)(object)value!);
    }

    /// <summary>
    /// Produces only the elements of type <see cref="TType"/>.
    /// </summary>
    public SpanQuery<TSpan, TType> OfType<TType>()
    {
        return this.Continue<TType>(
            previousCreateAggregator => (aggregator, flusher) =>
            {
                int index = 0;
                return previousCreateAggregator((value, _) =>
                {
                    if (value is TType tvalue)
                    {
                        if (!aggregator(tvalue, index))
                            return false;
                        index++;
                    }

                    return true;
                },
                flusher
                );
            });
    }
}

/// <summary>
/// A function that creates a new aggregator and flushing functions 
/// given the previous aggregator and flushing functions.
/// </summary>
/// <typeparam name="TValue1">The argument type of the previous aggregator function.</typeparam>
/// <typeparam name="TValue2">The argument type of the created aggregator function.</typeparam>
/// <param name="aggregator">The function that is called for each value of type <see cref="TValue"/>.</param>
/// <param name="flusher">A function that is called after all items have been enumerated.</param>
public delegate (Func<TValue2, int, bool>, Action) CreateAggregator<TValue1, TValue2>(
    Func<TValue1, int, bool> aggregator,
    Action flusher
    );

/// <summary>
/// A function that creates a new <see cref="CreateAggregator{TValue1, TValue3}"/>
/// from a prevous <see cref="CreateAggregator{TValue1, TValue2}"/>
/// </summary>
public delegate CreateAggregator<TValue1, TValue3> CreateCreateAggregator<TValue1, TValue2, TValue3>(
    CreateAggregator<TValue2, TValue3> createAggregator
    );

public static class SpanQueryExtensions
{
    #region All
    public static bool All<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Aggregate(true, (all, value) => all && predicate(value));
    }

    public static bool All<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).All(predicate);
    }

    public static bool All<TSource>(
        this Span<TSource> source,
        Func<TSource, bool> predicate)
    {
        return All((ReadOnlySpan<TSource>)source, predicate);
    }
    #endregion

    #region Aggregate
    public static TAggregate Aggregate<TSpan, TSource, TAggregate>(
        this SpanQuery<TSpan, TSource> source,
        TAggregate seed,
        Func<TAggregate, TSource, TAggregate> func)
    {
        var result = seed;
        source.ForEach(value => result = func(result, value));
        return result;
    }

    public static TAggregate Aggregate<TSpan, TSource, TAggregate>(
        this SpanQuery<TSpan, TSource> source,
        Func<TAggregate, TSource, TAggregate> func)
    {
        return Aggregate(source, default(TAggregate)!, func)!;
    }

    public static TAggregate Aggregate<TSource, TAggregate>(
        this ReadOnlySpan<TSource> source,
        TAggregate seed,
        Func<TAggregate, TSource, TAggregate> func)
    {
        return AsQuery(source).Aggregate(seed, func);
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
    public static bool Any<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        var hasValue = false;
        source.ForEach(value =>
        {
            hasValue = true;
            return false;
        });
        return hasValue;
    }

    public static bool Any<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).Any();
    }

    public static bool Any<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0;
    }

    public static bool Any<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).Any(predicate);
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

    #region AsQuery
    public static SpanQuery<TSource, TSource> AsQuery<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return new SpanQuery<TSource, TSource>(
            source,
            (aggregator, flusher) => ((value, index) => aggregator(value, index), flusher)
            );
    }

    public static SpanQuery<TSource, TSource> AsQuery<TSource>(
        this Span<TSource> source)
    {
        return AsQuery((ReadOnlySpan<TSource>)source);
    }

    public static SpanQuery<TSource, TSource> AsSpanQuery<TSource>(
        this TSource[] source)
    {
        return AsQuery((ReadOnlySpan<TSource>)source);
    }

    #endregion

    #region Average

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

    public static TSource Average<TSource>(
        this ReadOnlySpan<TSource> source)
        where TSource : INumber<TSource>
    {
        return AsQuery(source).Average();
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
        return AsQuery(source).Average(selector);
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
    public static bool Contains<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        TSource value,
        IEqualityComparer<TSource>? comparer = null)
    {
        comparer ??= EqualityComparer<TSource>.Default;

        var contains = false;
        source.ForEach(val =>
        {
            if (comparer.Equals(value, val))
            {
                contains = true;
                return false;
            }

            return true;
        });

        return contains;
    }

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
    public static int Count<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        return source.Aggregate(0, (total, value) => total + 1);
    }

    public static int Count<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).Count();
    }

    public static int Count<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length;
    }

    public static int Count<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).Count(predicate);
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
        return AsQuery(source).Count(predicate);
    }

    #endregion

    #region Distinct

    public static SpanQuery<TSpan, TSource> Distinct<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        IEqualityComparer<TSource>? comparer = null)
    {
        return source.Continue<TSource>(
            innerFnGetAggregator => (aggregator, flusher) =>
            {
                var hashset = new HashSet<TSource>(comparer ?? EqualityComparer<TSource>.Default);
                var list = new List<TSource>();
                return innerFnGetAggregator(
                    (value, _) =>
                    {
                        if (hashset.Add(value))
                            list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (!aggregator(list[i], i))
                                break;
                        }

                        flusher();
                    });
            });
    }

    public static SpanQuery<TSource, TSource> Distinct<TSource>(
        this ReadOnlySpan<TSource> source,
        IEqualityComparer<TSource>? comparer = null)
    {
        return source.AsQuery().Distinct(comparer);
    }

    public static SpanQuery<TSource, TSource> Distinct<TSource>(
        this Span<TSource> source,
        IEqualityComparer<TSource>? comparer = null)
    {
        return Distinct((ReadOnlySpan<TSource>)source, comparer);
    }

    #endregion

    #region DistinctBy

    public static SpanQuery<TSpan, TSource> DistinctBy<TSpan, TSource, TBy>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TBy> bySelector,
        IEqualityComparer<TBy>? comparer = null)
    {
        return source.Continue<TSource>(
            previousFnGetAggregator => (aggregator, flusher) =>
            {
                var hashset = new HashSet<TBy>(comparer ?? EqualityComparer<TBy>.Default);
                var list = new List<TSource>();
                return previousFnGetAggregator(
                    (value, _) =>
                    {
                        if (hashset.Add(bySelector(value)))
                            list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (!aggregator(list[i], i))
                                break;
                        }

                        flusher();
                    });
            });
    }

    public static SpanQuery<TSource, TSource> DistinctBy<TSource, TBy>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TBy> bySelector,
        IEqualityComparer<TBy>? comparer = null)
    {
        return source.AsQuery().DistinctBy(bySelector, comparer);
    }

    public static SpanQuery<TSource, TSource> DistinctBy<TSource, TBy>(
        this Span<TSource> source,
        Func<TSource, TBy> bySelector,
        IEqualityComparer<TBy>? comparer = null)
    {

        return DistinctBy((ReadOnlySpan<TSource>)source, bySelector, comparer);
    }

    #endregion

    #region First
    public static TSource First<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        bool found = false;
        TSource first = default!;

        source.ForEach(value =>
        {
            if (!found)
            {
                first = value;
                found = true;
                return false;
            }

            return true;
        });

        return found 
            ? first 
            : throw new InvalidOperationException("Sequence contains no elements");
    }

    public static TSource First<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).First();
    }

    public static TSource First<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length > 0 
            ? source[0] 
            : throw new InvalidOperationException("Sequence contains no elements");
    }

    public static TSource First<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).First(predicate);
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
    public static TSource FirstOrDefault<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        bool found = false;
        TSource? first = default;

        source.ForEach(value =>
        {
            if (!found)
            {
                first = value;
                found = true;
                return false;
            }

            return true;
        });

        return first!;
    }

    public static TSource FirstOrDefault<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).FirstOrDefault();
    }

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
        return AsQuery(source).FirstOrDefault(predicate);
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

    #region GroupBy

    public static SpanQuery<TSpan, IGrouping<TKey, TSource>> GroupBy<TSpan, TSource, TKey>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return source.Continue<IGrouping<TKey, TSource>>(
            previousFnGetAggregator => (aggregator, flusher) =>
            {
                var list = new List<TSource>();
                return previousFnGetAggregator(
                    (value, _) =>
                    {
                        list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        var groups = list.GroupBy(keySelector, comparer);
                        int index = 0;
                        foreach (var group in groups)
                        {
                            if (!aggregator(group, index))
                                break;
                            index++;
                        }

                        flusher();
                    });
            });
    }

    public static SpanQuery<TSource, IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return AsQuery(source).GroupBy(keySelector, comparer);
    }

    public static SpanQuery<TSource, IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
        this Span<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return GroupBy((ReadOnlySpan<TSource>)source, keySelector, comparer);
    }

    #endregion

    #region Select
    public static SpanQuery<TSpan, TResult> Select<TSpan, TSource, TResult>(
        this SpanQuery<TSpan, TSource> source, 
        Func<TSource, TResult> selector)
    {
        return source.Continue<TResult>(
            previousCreateAggregator => (aggregator, flusher) =>
            {
                return previousCreateAggregator((value, index) =>
                {
                    var mappedValue = selector(value);
                    if (!aggregator(mappedValue, index))
                        return false;
                    return true;
                },
                flusher
                );
            });
    }

    public static SpanQuery<TSpan, TResult> Select<TSpan, TResult>(
        this Span<TSpan> source, 
        Func<TSpan, TResult> selector)
    {
        return Select((ReadOnlySpan<TSpan>)source, selector);
    }

    public static SpanQuery<TSpan, TResult> Select<TSpan, TResult>(
        this ReadOnlySpan<TSpan> source, 
        Func<TSpan, TResult> selector)
    {
        return AsQuery(source).Select(selector);
    }

    public static SpanQuery<TSpan, TResult> Select<TSpan, TSource, TResult>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, int, TResult> selector)
    {
        return source.Continue<TResult>(
            previousCreateAggregator => (aggregator, flusher) =>
            {
                return previousCreateAggregator((value, index) =>
                {
                    var mappedValue = selector(value, index);
                    if (!aggregator(mappedValue, index))
                        return false;
                    return true;
                },
                flusher
                );
            });
    }

    public static SpanQuery<TSpan, TResult> Select<TSpan, TResult>(
        this ReadOnlySpan<TSpan> source,
        Func<TSpan, int, TResult> selector)
    {
        return AsQuery(source).Select(selector);
    }

    public static SpanQuery<TSpan, TResult> Select<TSpan, TResult>(
        this Span<TSpan> source,
        Func<TSpan, int, TResult> selector)
    {
        return Select((ReadOnlySpan<TSpan>)source, selector);
    }

    #endregion

    #region SelectMany

    public static SpanQuery<TSpan, TMany> SelectMany<TSpan, TSource, TMany>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, IEnumerable<TMany>> selector)
    {
        return source.Continue<TMany>(
            previousCreateAggregator => (aggregator, flusher) =>
            {
                int index = 0;
                return previousCreateAggregator((value, _) =>
                {
                    foreach (var item in selector(value))
                    {
                        if (!aggregator(item, index))
                            return false;
                        index++;
                    }

                    return true;
                },
                flusher
                );
            });
    }

    public static SpanQuery<TSource, TSelector> SelectMany<TSource, TSelector>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, IEnumerable<TSelector>> selector)
    {
        return AsQuery(source).SelectMany(selector);
    }

    public static SpanQuery<TSource, TSelector> SelectMany<TSource, TSelector>(
        this Span<TSource> source,
        Func<TSource, IEnumerable<TSelector>> selector)
    {
        return SelectMany((ReadOnlySpan<TSource>)source, selector);
    }

    #endregion

    #region Single
    public static TSource Single<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        int count = 0;
        TSource first = default!;

        source.ForEach(value =>
        {
            if (count == 0)
                first = value;
            count++;
            return count < 2;
        });

        return count == 1
            ? first
            : throw new InvalidOperationException("Sequence contains no elements or more than one element");
    }

    public static TSource Single<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).Single();
    }

    public static TSource Single<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length == 1
            ? source[0]
            : throw new InvalidOperationException("Sequence contains no elements or more than one element");
    }

    public static TSource Single<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).Single(predicate);
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
    public static TSource SingleOrDefault<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source)
    {
        int count = 0;
        TSource? first = default;

        source.ForEach(value =>
        {
            if (count == 0)
                first = value;
            count++;

            return count < 2;
        });

        if (count > 1)
            throw new InvalidOperationException("Sequence contains more than one element");


        return first!;
    }

    public static TSource SingleOrDefault<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return source.Where(predicate).SingleOrDefault()!;
    }

    public static TSource SingleOrDefault<TSource>(
        this ReadOnlySpan<TSource> source)
    {
        return source.Length == 1 ? source[0]
            : source.Length > 1 ? throw new InvalidOperationException("Sequence contains more than one element")
            : default!;
    }

    public static TSource SingleOrDefault<TSource>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, bool> predicate)
    {
        return AsQuery(source).SingleOrDefault(predicate);
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
    public static TNumber Sum<T, TNumber>(
        this SpanQuery<T, TNumber> source)
        where TNumber : INumber<TNumber>
    {
        return source.Aggregate(TNumber.Zero, (total, value) => total + value);
    }

    public static T Sum<T>(
        this ReadOnlySpan<T> source)
        where T : INumber<T>
    {
        return AsQuery(source).Sum();
    }

    public static T Sum<T>(
        this Span<T> source)
        where T : INumber<T>
    {
        return Sum((ReadOnlySpan<T>)source);
    }

    public static TNumber Sum<TSpan, TSource, TNumber>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return source.Select(selector).Sum();
    }

    public static TNumber Sum<TSpan, TNumber>(
        this ReadOnlySpan<TSpan> source,
        Func<TSpan, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return source.Select(selector).Sum();
    }

    public static TNumber Sum<TSpan, TNumber>(
        this Span<TSpan> source,
        Func<TSpan, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        return Sum((ReadOnlySpan<TSpan>)source, selector);
    }
    #endregion

    #region Where
    public static SpanQuery<TSpan, TSource> Where<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, int, bool> predicate)
    {
        return source.Continue<TSource>(
            previousCreateAggregator => (aggregator, flusher) =>
            {
                int filteredIndex = 0;
                return previousCreateAggregator((value, index) =>
                {
                    if (predicate(value, index))
                    {
                        if (!aggregator(value, filteredIndex))
                            return false;
                        filteredIndex++;
                    }

                    return true;
                },
                flusher
                );
            });
    }

    public static SpanQuery<T, T> Where<T>(
        this ReadOnlySpan<T> source,
        Func<T, int, bool> predicate)
    {
        return AsQuery(source).Where(predicate);
    }

    public static SpanQuery<T, T> Where<T>(
        this Span<T> source,
        Func<T, int, bool> predicate)
    {
        return Where((ReadOnlySpan<T>)source, predicate);
    }

    public static SpanQuery<TSpan, TSource> Where<TSpan, TSource>(
        this SpanQuery<TSpan, TSource> source,
        Func<TSource, bool> predicate)
    {
        return Where(source, (value, index) => predicate(value));
    }

    public static SpanQuery<T, T> Where<T>(
        this ReadOnlySpan<T> source,
        Func<T, bool> predicate)
    {
        return AsQuery(source).Where(predicate);
    }

    public static SpanQuery<T, T> Where<T>(
        this Span<T> source,
        Func<T, bool> predicate)
    {
        return Where((ReadOnlySpan<T>)source, predicate);
    }

    #endregion
}
