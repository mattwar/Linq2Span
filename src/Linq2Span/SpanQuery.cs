namespace Linq2Span;

using Enumerators;
using System.Numerics;

/// <summary>
/// A query that operates over a span of elements.
/// </summary>
/// <typeparam name="TSpan">The type of the elements in the original span.</typeparam>
/// <typeparam name="TElement">The current result element type of the query.</typeparam>
/// <typeparam name="TEnumerator">The type of the underlying span enumerator that executes the query.</typeparam>
public readonly ref struct SpanQuery<TSpan, TElement, TEnumerator>
    where TEnumerator : struct, ISpanEnumerator<TSpan, TElement>
{
    private readonly ReadOnlySpan<TSpan> _span;
    private readonly TEnumerator _enumerator;

    /// <summary>
    /// Creates a new <see cref="SpanQuery{TSpan, TElement, TEnumerator}"/>
    /// </summary>
    /// <param name="span">The span of data they query is run against.</param>
    /// <param name="enumerator">The span enumerator that executes the query.</param>
    internal SpanQuery(
        ReadOnlySpan<TSpan> span,
        TEnumerator enumerator)
    {
        _span = span;
        _enumerator = enumerator;
    }

    /// <summary>
    /// The underlying span enumerator used to execute the query.
    /// </summary>
    public TEnumerator SpanEnumerator => _enumerator;

    /// <summary>
    /// Constructs a new query with the specified enumerator.
    /// </summary>
    public SpanQuery<TSpan, TElement2, TEnumerator2> With<TElement2, TEnumerator2>(
        TEnumerator2 enumerator)
        where TEnumerator2 : struct, ISpanEnumerator<TSpan, TElement2>
    {
        return new SpanQuery<TSpan, TElement2, TEnumerator2>(_span, enumerator);
    }

    /// <summary>
    /// Constructs a new query with the specified enumerator.
    /// </summary>
    public SpanQuery<TSpan, TElement, TEnumerator2> With<TEnumerator2>(
        TEnumerator2 enumerator)
        where TEnumerator2 : struct, ISpanEnumerator<TSpan, TElement>
    {
        return new SpanQuery<TSpan, TElement, TEnumerator2>(_span, enumerator);
    }

    /// <summary>
    /// Gets the enumerator that enumertes the query results.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_span, _enumerator);
    }

    /// <summary>
    /// An enumerator that enumerates the query results.
    /// Used for foreach loops.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<TSpan> _span;
        private TEnumerator _enumerator;

        public Enumerator(ReadOnlySpan<TSpan> span, TEnumerator enumerator)
        {
            _span = span;
            _enumerator = enumerator;
        }

        public TElement Current => _enumerator.Current;

        public bool MoveNext()
        {
            return _enumerator.MoveNext(_span);
        }
    }

    /// <summary>
    /// Enumerates the query results and calls the specified action for each element.
    /// </summary>
    public void ForEach(Action<TElement, int> action)
    {
        int nextIndex = 0;
        foreach (var element in this)
        {
            action(element, nextIndex++);
        }
    }

    /// <summary>
    /// Enumerates the query results and calls the specified action for each element.
    /// </summary>
    public void ForEach(Action<TElement> action)
    {
        foreach (var element in this)
        {
            action(element);
        }
    }

    /// <summary>
    /// Executes the query and returns the results as a <see cref="List{T}"/>.
    /// </summary>
    public List<TElement> ToList()
    {
        var list = new List<TElement>();

        foreach (var element in this)
        {
            list.Add(element);
        }

        return list;
    }

    /// <summary>
    /// Executes the query and returns the results as an array.
    /// </summary>
    public TElement[] ToArray()
    {
        return ToList().ToArray();
    }

    /// <summary>
    /// Executes the query and returns the results a <see cref="HashSet{T}"/>.
    /// </summary>
    public HashSet<TElement> ToHashSet(
        IEqualityComparer<TElement>? comparer = null)
    {
        var hashset = new HashSet<TElement>(comparer ?? EqualityComparer<TElement>.Default);

        foreach (var element in this)
        {
            hashset.Add(element);
        }

        return hashset;
    }

    /// <summary>
    /// Executes the query and returns the results a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        Func<TElement, TKey> keySelector,
        Func<TElement, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);

        foreach (var element in this)
        {
            dictionary.Add(keySelector(element), valueSelector(element));
        }

        return dictionary;
    }

    /// <summary>
    /// Executes the query and returns the results a <see cref="Dictionary{TKey, TElement}"/>.
    /// </summary>
    public Dictionary<TKey, TElement> ToDictionary<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return ToDictionary(keySelector, x => x, comparer);
    }

    #region Operators

    /// <summary>
    /// Computes the aggregate of the query results.
    /// </summary>
    public TResult Aggregate<TResult>(
        TResult seed,
        Func<TResult, TElement, TResult> aggregator)
    {
        var result = seed;

        foreach (var element in this)
        {
            result = aggregator(result, element);
        }

        return result;
    }

    /// <summary>
    /// Computes the aggregate of the query results.
    /// </summary>
    public TResult Aggregate<TAccumulate, TResult>(
        TAccumulate seed,
        Func<TAccumulate, TElement, TAccumulate> aggregator,
        Func<TAccumulate, TResult> selector)
    {
        var accumulate = seed;

        foreach (var element in this)
        {
            accumulate = aggregator(accumulate, element);
        }

        return selector(accumulate);
    }

    /// <summary>
    /// Returns true if all elements of the query results satisifies the predicate.
    /// </summary>
    public bool All(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (!predicate(element))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a query that produces the results of the original query with an additional element added to the end.
    /// </summary>
    public SpanQuery<TSpan, TElement, AppendEnumerator<TSpan, TElement, TEnumerator>> Append(
        TElement element)
    {
        return With(
            new AppendEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                element
                ));
    }

    /// <summary>
    /// Returns true if the query has any results.
    /// </summary>
    public bool Any()
    {
        foreach (var element in this)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the query has any result element that satifies the predicate.
    /// </summary>
    public bool Any(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (predicate(element))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Computes the average value of the selected query results.
    /// </summary>
    public TNumber Average<TNumber>(Func<TElement, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        var total = TNumber.Zero;
        var count = TNumber.Zero;

        foreach (var element in this)
        {
            var value = selector(element);
            total = total + value;
            count += TNumber.One;
        }

        return total / count;
    }

    /// <summary>
    /// Returns a query that casts each element of the original query to the specified type.
    /// </summary>
    public SpanQuery<TSpan, TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>> Cast<TResult>()
    {
        return With<TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new CastEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator
            ));
    }

    /// <summary>
    /// Returns a query that breaks the elements of the original query into chunks.
    /// </summary>
    public SpanQuery<TSpan, TElement[], ChunkEnumerator<TSpan, TElement, TEnumerator>> Chunk(
        int size)
    {
        return With<TElement[], ChunkEnumerator<TSpan, TElement, TEnumerator>>(
            new ChunkEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                size)
            );
    }

    /// <summary>
    /// Returns a query that produces the concatenation of the results of the original query 
    /// with the specified list of elements.
    /// </summary>
    public SpanQuery<TSpan, TElement, ConcatEnumerator<TSpan, TElement, TEnumerator>> Concat(
        IEnumerable<TElement> elements)
    {
        return With(
            new ConcatEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                elements
                ));
    }

    /// <summary>
    /// Returns true if the query results contains the specified value.
    /// </summary>
    public bool Contains(
        TElement value,
        IEqualityComparer<TElement>? comparer = null)
    {
        comparer ??= EqualityComparer<TElement>.Default;

        foreach (var element in this)
        {
            if (comparer.Equals(element, value))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the number of elements in the query results.
    /// </summary>
    public int Count()
    {
        int count = 0;

        foreach (var element in this)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Returns the number of elements in the query results that satisfy the predicate.
    /// </summary>
    public int Count(Func<TElement, bool> predicate)
    {
        int count = 0;
        
        foreach (var element in this)
        {
            if (predicate(element))
                count++;
        }

        return count;
    }

    /// <summary>
    /// Returns a new query the produces the results of the original query if the results are not empty,
    /// otherwise produces a single result of the default value.
    /// </summary>
    public SpanQuery<TSpan, TElement, DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>> DefaultIfEmpty(
        TElement defaultValue)
    {
        return With(
            new DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                defaultValue
                ));
    }

    /// <summary>
    /// Returns a new query the produces the results of the original query if the results are not empty,
    /// otherwise produces a single result of the default value for the result type.
    /// </summary>
    public SpanQuery<TSpan, TElement, DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>> DefaultIfEmpty()
    {
        return With(
            new DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                default(TElement)!
                ));
    }

    /// <summary>
    /// Returns a query that produces the distinct elements of the original query.
    /// </summary>
    public SpanQuery<TSpan, TElement, DistinctByEnumerator<TSpan, TElement, TEnumerator, TElement>> Distinct(
        IEqualityComparer<TElement>? keyComparer = null)
    {
        return DistinctBy(e => e, keyComparer);
    }

    /// <summary>
    /// Returns a query that procues the distinct elements of the original query as determined by the specified key selector.
    /// </summary>
    public SpanQuery<TSpan, TElement, DistinctByEnumerator<TSpan, TElement, TEnumerator, TKey>> DistinctBy<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With(
            new DistinctByEnumerator<TSpan, TElement, TEnumerator, TKey>(
                _enumerator,
                keySelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                ));
    }

    /// <summary>
    /// Returns the element of the query results at the specified index.
    /// </summary>
    public TElement ElementAt(int index)
    {
        if (index >= 0)
        {
            foreach (var element in this)
            {
                if (index == 0)
                    return element;
                index--;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>
    /// Returns the element of the query results at the specified index.
    /// </summary>
    public TElement ElementAt(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAt(index.Value);

        var distanceFromEnd = index.Value;

        if (distanceFromEnd == 1)
            return Last();

        if (distanceFromEnd > 1)
        {
            var queue = new Queue<TElement>();

            foreach (var element in this)
            {
                if (queue.Count == distanceFromEnd)
                    queue.Dequeue();
                queue.Enqueue(element);
            }

            if (queue.Count == distanceFromEnd)
            {
                return queue.Dequeue();
            }
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>
    /// Returns the element of the query results at the specified index.
    /// If index is out of range it returns the default value for the element type.
    /// </summary>
    public TElement? ElementAtOrDefault(int index)
    {
        if (index >= 0)
        {
            foreach (var element in this)
            {
                if (index == 0)
                    return element;
                index--;
            }
        }

        return default;
    }

    /// <summary>
    /// Returns the element of the query results at the specified index.
    /// If index is out of range it returns the default value for the element type.
    /// </summary>
    public TElement? ElementAtOrDefault(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAtOrDefault(index.Value);

        var distanceFromEnd = index.Value;

        if (distanceFromEnd == 1)
            return LastOrDefault();

        if (distanceFromEnd > 1)
        {
            var queue = new Queue<TElement>();

            foreach (var element in this)
            {
                if (queue.Count == distanceFromEnd)
                    queue.Dequeue();
                queue.Enqueue(element);
            }

            if (queue.Count == distanceFromEnd)
            {
                return queue.Dequeue();
            }
        }

        return default;
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query, except for those elements in the specified list.
    /// </summary>
    public SpanQuery<TSpan, TElement, ExceptByEnumerator<TSpan, TElement, TEnumerator, TElement>> Except(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return ExceptBy(elements, x => x, comparer);
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query, except for those elements with keys in the specified list.
    /// </summary>
    public SpanQuery<TSpan, TElement, ExceptByEnumerator<TSpan, TElement, TEnumerator, TKey>> ExceptBy<TKey>(
        IEnumerable<TKey> keys,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With(
            new ExceptByEnumerator<TSpan, TElement, TEnumerator, TKey>(
                _enumerator,
                keys,
                keySelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                ));
    }

    /// <summary>
    /// Returns the first element of the query results.
    /// </summary>
    public TElement First()
    {
        foreach (var element in this)
        {
            return element;
        }

        throw Exceptions.GetSequenceIsEmpty();
    }

    /// <summary>
    /// Returns the first element of the query results that satifies the predicate.
    /// </summary>
    public TElement First(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (predicate(element))
                return element;
        }

        throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    /// <summary>
    /// Returns the first element of the query results.
    /// If the query has not results, returns the default value for the element type.
    /// </summary>
    public TElement? FirstOrDefault()
    {
        foreach (var element in this)
        {
            return element;
        }

        return default;
    }

    /// <summary>
    /// Returns the first element of the query result that satisfies the predicate.
    /// If the query has not results, returns the default value for the element type.
    /// </summary>
    public TElement? FirstOrDefault(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (predicate(element))
                return element;
        }

        return default;
    }

    public SpanQuery<TSpan, IGrouping<TKey, TElement>, GroupByEnumerator<TSpan, TElement, TEnumerator, TKey>> GroupBy<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With<IGrouping<TKey, TElement>, GroupByEnumerator<TSpan, TElement, TEnumerator, TKey>>(
            new GroupByEnumerator<TSpan, TElement, TEnumerator, TKey>(
                _enumerator,
                keySelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                ));
    }

    public SpanQuery<TSpan, TResult, GroupJoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>> GroupJoin<TInner, TKey, TResult>(
        IEnumerable<TInner> inner,
        Func<TElement, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TElement, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return With<TResult, GroupJoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>>(
            new GroupJoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>(
                _enumerator,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector,
                comparer ?? EqualityComparer<TKey>.Default
                ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query that have keys also in the specified list.
    /// </summary>
    public SpanQuery<TSpan, TElement, IntersectByEnumerator<TSpan, TElement, TEnumerator, TKey>> IntersectBy<TKey>(
        IEnumerable<TKey> keys,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With(
            new IntersectByEnumerator<TSpan, TElement, TEnumerator, TKey>(
                _enumerator,
                keys,
                keySelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query that is also in the specified list.
    /// </summary>
    public SpanQuery<TSpan, TElement, IntersectByEnumerator<TSpan, TElement, TEnumerator, TElement>> Intersect(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return IntersectBy(elements, x => x, comparer);
    }

    /// <summary>
    /// Produces the join of the result of the original query with the specified list of elements.
    /// </summary>
    public SpanQuery<TSpan, TResult, JoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>> Join<TInner, TKey, TResult>(
        IEnumerable<TInner> inner,
        Func<TElement, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TElement, TInner, TResult> resultSelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With<TResult, JoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>>(
            new JoinEnumerator<TSpan, TElement, TEnumerator, TInner, TKey, TResult>(
                _enumerator,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                ));
    }

    /// <summary>
    /// The last element in the query results.
    /// </summary>
    public TElement Last()
    {
        TElement result = default!;
        bool found = false;

        foreach (var element in this)
        {
            result = element;
            found = true;
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmpty();

        return result;
    }

    /// <summary>
    /// The last element in the query results that satisfies the predicate.
    /// </summary>
    public TElement Last(Func<TElement, bool> predicate)
    {
        TElement result = default!;
        bool found = false;

        foreach (var element in this)
        {
            if (predicate(element))
            {
                result = element;
                found = true;
            }
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();

        return result;
    }

    /// <summary>
    /// Returns the last element in the query result.
    /// If the query has no results it returns the default value for the element type.
    /// </summary>
    public TElement? LastOrDefault()
    {
        TElement? result = default;
        
        foreach (var element in this)
        {
            result = element;
        }

        return result;
    }

    /// <summary>
    /// Returns the last element in the query results that satisfies the predicate.
    /// If the query has no results it returns the default value for the element type.
    /// </summary>
    public TElement? LastOrDefault(Func<TElement, bool> predicate)
    {
        TElement? result = default;

        foreach (var element in this)
        {
            if (predicate(element))
                result = element;
        }

        return result;
    }

    /// <summary>
    /// Returns the count of the element in the query result.
    /// </summary>
    public long LongCount()
    {
        long count = 0;

        foreach (var element in this)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Returns the count of the element in the query result that satisfies the predicate.
    /// </summary>
    public long LongCount(Func<TElement, bool> predicate)
    {
        long count = 0;

        foreach (var element in this)
        {
            if (predicate(element))
                count++;
        }

        return count;
    }

    /// <summary>
    /// Returns that maximum selected value of the query results.
    /// </summary>
    public TResult Max<TResult>(
        Func<TElement, TResult> selector)
    {
        var comparer = Comparer<TResult>.Default;
        var hasValue = false;
        TResult maxValue = default!;

        foreach (var element in this)
        {
            var value = selector(element);
            if (!hasValue || comparer.Compare(value, maxValue) > 0)
            {
                maxValue = value;
                hasValue = true;
            }
        }

        if (!hasValue)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();

        return maxValue;
    }

    /// <summary>
    /// Returns that maximum element of the query results as determined by key value.
    /// </summary>
    public TElement? MaxBy<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        var hasValue = false;
        TKey maxKey = default!;
        TElement maxValue = default!;

        foreach (var element in this)
        {
            var key = keySelector(element);
            if (!hasValue || comparer.Compare(key, maxKey) > 0)
            {
                maxKey = key;
                maxValue = element;
                hasValue = true;
            }
        }

        return maxValue;
    }

    /// <summary>
    /// Returns that minimum selected value of the query results.
    /// </summary>
    public TResult Min<TResult>(
        Func<TElement, TResult> selector)
    {
        var comparer = Comparer<TResult>.Default;
        var hasValue = false;
        TResult minValue = default!;

        foreach (var element in this)
        {
            var value = selector(element);
            if (!hasValue || comparer.Compare(value, minValue) < 0)
            {
                minValue = value;
                hasValue = true;
            }
        }

        if (!hasValue)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();

        return minValue;
    }

    /// <summary>
    /// Returns that minimum element of the query results as determined by key value.
    /// </summary>
    public TElement? MinBy<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;

        var hasValue = false;
        TKey maxKey = default!;
        TElement maxValue = default!;

        foreach (var element in this)
        {
            var key = keySelector(element);
            if (!hasValue || comparer.Compare(key, maxKey) < 0)
            {
                maxKey = key;
                maxValue = element;
                hasValue = true;
            }
        }

        return maxValue;
    }

    /// <summary>
    /// Returns the elements of the original query that are of the specified type.
    /// </summary>
    public SpanQuery<TSpan, TResult, OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>> OfType<TResult>()
    {
        return With<TResult, OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator
                ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query in order.
    /// </summary>
    public SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> Order(
        IComparer<TElement>? comparer = null)
    {
        comparer ??= Comparer<TElement>.Default;
        return With(
            new OrderByEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                (a, b) => comparer.Compare(a, b)
            ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query in order of the specified key.
    /// </summary>
    public SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> OrderBy<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;
        return With(
            new OrderByEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                (a, b) => comparer.Compare(keySelector(a), keySelector(b))
            ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query in descending order of the specified key.
    /// </summary>
    public SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> OrderByDescending<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? comparer = null)
    {
        comparer ??= Comparer<TKey>.Default;
        return With(
            new OrderByEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                (a, b) => -comparer.Compare(keySelector(a), keySelector(b))
            ));
    }

    /// <summary>
    /// Returns a query that produces the elements of the original query in descending order.
    /// </summary>
    public SpanQuery<TSpan, TElement, OrderByEnumerator<TSpan, TElement, TEnumerator>> OrderDescending(
        IComparer<TElement>? comparer = null)
    {
        comparer ??= Comparer<TElement>.Default;
        return With(
            new OrderByEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                (a, b) => -comparer.Compare(a, b)
            ));
    }

    /// <summary>
    /// Returns a query that produces the prepended element followed by the elements of the result of the original query.
    /// </summary>
    public SpanQuery<TSpan, TElement, PrependSpanEnumerator<TSpan, TElement, TEnumerator>> Prepend(
        TElement element)
    {
        return With<TElement, PrependSpanEnumerator<TSpan, TElement, TEnumerator>>(
            new PrependSpanEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                element
                ));
    }

    /// <summary>
    /// Returns a query that produces the result of the original query in reverse.
    /// </summary>
    public SpanQuery<TSpan, TElement, ReverseEnumerator<TSpan, TElement, TEnumerator>> Reverse()
    {
        return With(
            new ReverseEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator
                ));
    }

    /// <summary>
    /// Returns a query that produces the selected or mapped values of the original query results.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>> Select<TResult>(
        Func<TElement, int, TResult> selector)
    {
        return With<TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new SelectEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator,
                selector
                ));
    }

    /// <summary>
    /// Returns a query that produces the selected or mapped values of the original query results.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>> Select<TResult>(
        Func<TElement, TResult> selector)
    {
        return Select((x, _) => selector(x));
    }

    /// <summary>
    /// Returns a query that produces the selected result of the original query results, flattend into a single sequence.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TResult, TResult>> SelectMany<TResult>(
        Func<TElement, int, IEnumerable<TResult>> selector)
    {
        return SelectMany(selector, (x, y) => y);
    }

    /// <summary>
    /// Returns a query that produces the selected result of the original query results, flattend into a single sequence.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TResult, TResult>> SelectMany<TResult>(
        Func<TElement, IEnumerable<TResult>> selector)
    {
        return SelectMany((x, _) => selector(x));
    }

    /// <summary>
    /// Returns a query that produces the selected result of the original query results, flattend into a single sequence.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TCollection, TResult>> SelectMany<TCollection, TResult>(
        Func<TElement, int, IEnumerable<TCollection>> collectionSelector,
        Func<TElement, TCollection, TResult> resultSelector)
    {
        return With<TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TCollection, TResult>>(
            new SelectManyEnumerator<TSpan, TElement, TEnumerator, TCollection, TResult>(
                _enumerator,
                collectionSelector,
                resultSelector
                ));
    }

    /// <summary>
    /// Returns a query that produces the selected result of the original query results, flattend into a single sequence.
    /// </summary>
    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TCollection, TResult>> SelectMany<TCollection, TResult>(
        Func<TElement, IEnumerable<TCollection>> collectionSelector,
        Func<TElement, TCollection, TResult> resultSelector)
    {
        return SelectMany((x, i) => collectionSelector(x), resultSelector);
    }

    /// <summary>
    /// Returns a query that produces the results of the original query, except for the first n elements.
    /// </summary>
    public SpanQuery<TSpan, TElement, SkipEnumerator<TSpan, TElement, TEnumerator>> Skip(
        int count)
    {
        return With(
            new SkipEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    /// <summary>
    /// Returns a query that produces the results of the original query, except for the last n elements.
    /// </summary>
    public SpanQuery<TSpan, TElement, SkipLastEnumerator<TSpan, TElement, TEnumerator>> SkipLast(
        int count)
    {
        return With(
            new SkipLastEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    /// <summary>
    /// Returns a query that produces the results of the original query, except for the first elements that satisify the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, SkipWhileEnumerator<TSpan, TElement, TEnumerator>> SkipWhile(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new SkipWhileEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
            ));
    }

    /// <summary>
    /// Returns a query that produces the results of the original query, except for the first elements that satisify the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, SkipWhileEnumerator<TSpan, TElement, TEnumerator>> SkipWhile(
        Func<TElement, bool> predicate)
    {
        return SkipWhile((x, _) => predicate(x));
    }

    /// <summary>
    /// Returns the single result element of the query.
    /// </summary>
    public TElement Single()
    {
        bool found = false;
        TElement result = default!;

        foreach (var element in this)
        {
            if (found)
                throw Exceptions.GetSequenceIsEmptyOrContainsMoreThanOneElement();
            found = true;
            result = element;
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmptyOrContainsMoreThanOneElement();

        return result;
    }

    /// <summary>
    /// Returns the single result element of the query that satifies the predicate.
    /// </summary>
    public TElement Single(Func<TElement, bool> predicate)
    {
        bool found = false;
        TElement result = default!;

        foreach (var element in this)
        {
            if (predicate(element))
            {
                if (found)
                    throw Exceptions.GetSequenceIsEmptyOrNotSatisifiedOrContainsMoreThanOneElement();

                found = true;
                result = element;
            }
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisifiedOrContainsMoreThanOneElement();

        return result;
    }

    /// <summary>
    /// Returns the single result element of the query.
    /// If the query has no results, returns the default value for the element type.
    /// </summary>
    public TElement? SingleOrDefault()
    {
        var found = false;
        TElement? result = default;

        foreach (var element in this)
        {
            if (found)
                throw Exceptions.GetSequenceContainsMoreThanOneElement();
            found = true;
            result = element;
        }

        return result;
    }

    /// <summary>
    /// Returns the single result element of the query that satifies the predicate.
    /// If the query has no results, returns the default value for the element type.
    /// </summary>
    public TElement? SingleOrDefault(Func<TElement, bool> predicate)
    {
        bool found = false;
        TElement? result = default;

        foreach (var element in this)
        {
            if (predicate(element))
            {
                if (found)
                    throw Exceptions.GetSequenceContainsMoreThanOneElement();
                found = true;
                result = element;
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the sum of the query results.
    /// </summary>
    public TNumber Sum<TNumber>(Func<TElement, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        TNumber sum = TNumber.Zero;

        foreach (var element in this)
        {
            sum = sum + selector(element);
        }

        return sum;
    }

    /// <summary>
    /// Returns a query that produces the first n elements of the original query results.
    /// </summary>
    public SpanQuery<TSpan, TElement, TakeEnumerator<TSpan, TElement, TEnumerator>> Take(
        int count)
    {
        return With(
            new TakeEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    /// <summary>
    /// Returns a query that produces the last n elements of the original query results.
    /// </summary>
    public SpanQuery<TSpan, TElement, TakeLastEnumerator<TSpan, TElement, TEnumerator>> TakeLast(
        int count)
    {
        return With(
            new TakeLastEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    /// <summary>
    /// Returns a query that produces the first elements of the original query results that satisfy the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, TakeWhileEnumerator<TSpan, TElement, TEnumerator>> TakeWhile(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new TakeWhileEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
            ));
    }

    /// <summary>
    /// Returns a query that produces the first elements of the original query results that satisfy the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, TakeWhileEnumerator<TSpan, TElement, TEnumerator>> TakeWhile(
        Func<TElement, bool> predicate)
    {
        return TakeWhile((x, _) => predicate(x));
    }

    /// <summary>
    /// Returns a query that produces the distinct union of the original query results and the specified list of elements.
    /// </summary>
    public SpanQuery<TSpan, TElement, UnionByEnumerator<TSpan, TElement, TEnumerator, TElement>> Union(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return UnionBy(elements, x => x, comparer);
    }

    /// <summary>
    /// Returns a query that produces the distinct union of the original query results and the specified list of elements,
    /// as determined by the corresponding keys.
    /// </summary>
    public SpanQuery<TSpan, TElement, UnionByEnumerator<TSpan, TElement, TEnumerator, TKey>> UnionBy<TKey>(
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return With(
            new UnionByEnumerator<TSpan, TElement, TEnumerator, TKey>(
                _enumerator,
                elements,
                keySelector,
                keyComparer ?? EqualityComparer<TKey>.Default
                )); 
    }

    /// <summary>
    /// Returns a query that produces the result of the original query that satisfy the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, WhereEnumerator<TSpan, TElement, TEnumerator>> Where(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new WhereEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
                ));
    }

    /// <summary>
    /// Returns a query that produces the result of the original query that satisfy the predicate.
    /// </summary>
    public SpanQuery<TSpan, TElement, WhereEnumerator<TSpan, TElement, TEnumerator>> Where(
        Func<TElement, bool> predicate)
    {
        return Where((x, _) => predicate(x));
    }

    #endregion
}