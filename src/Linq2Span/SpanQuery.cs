namespace Linq2Span;

using Enumerators;
using System.Numerics;

public readonly ref struct SpanQuery<TSpan, TElement, TEnumerator>
    where TEnumerator : struct, ISpanEnumerator<TSpan, TElement>
{
    private readonly ReadOnlySpan<TSpan> _span;
    private readonly TEnumerator _enumerator;

    public SpanQuery(
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

    public void ForEach(Action<TElement, int> action)
    {
        int nextIndex = 0;
        foreach (var element in this)
        {
            action(element, nextIndex++);
        }
    }

    public void ForEach(Action<TElement> action)
    {
        foreach (var element in this)
        {
            action(element);
        }
    }

    public List<TElement> ToList()
    {
        var list = new List<TElement>();

        foreach (var element in this)
        {
            list.Add(element);
        }

        return list;
    }

    public TElement[] ToArray()
    {
        return ToList().ToArray();
    }

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

    public Dictionary<TKey, TElement> ToDictionary<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return ToDictionary(keySelector, x => x, comparer);
    }

    #region Operators

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

    public bool All(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (!predicate(element))
                return false;
        }

        return true;
    }

    public SpanQuery<TSpan, TElement, AppendEnumerator<TSpan, TElement, TEnumerator>> Append(
        TElement element)
    {
        return With(
            new AppendEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                element
                ));
    }

    public bool Any()
    {
        foreach (var element in this)
        {
            return true;
        }

        return false;
    }

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

    public SpanQuery<TSpan, TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>> Cast<TResult>()
    {
        return With<TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new CastEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator
            ));
    }

    public SpanQuery<TSpan, TElement[], ChunkEnumerator<TSpan, TElement, TEnumerator>> Chunk(
        int size)
    {
        return With<TElement[], ChunkEnumerator<TSpan, TElement, TEnumerator>>(
            new ChunkEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                size)
            );
    }

    public SpanQuery<TSpan, TElement, ConcatEnumerator<TSpan, TElement, TEnumerator>> Concat(
        IEnumerable<TElement> elements)
    {
        return With(
            new ConcatEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                elements
                ));
    }

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

    public int Count()
    {
        int count = 0;

        foreach (var element in this)
        {
            count++;
        }

        return count;
    }

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

    public SpanQuery<TSpan, TElement, DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>> DefaultIfEmpty(
        TElement defaultValue)
    {
        return With(
            new DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                defaultValue
                ));
    }

    public SpanQuery<TSpan, TElement, DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>> DefaultIfEmpty()
    {
        return With(
            new DefaultIfEmptyEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                default(TElement)!
                ));
    }

    public SpanQuery<TSpan, TElement, DistinctByEnumerator<TSpan, TElement, TEnumerator, TElement>> Distinct(
        IEqualityComparer<TElement>? keyComparer = null)
    {
        return DistinctBy(e => e, keyComparer);
    }

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

    public TElement ElementAt(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAt(index.Value);

        var distanceFromEnd = index.Value;

        if (distanceFromEnd > 0)
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

    public TElement? ElementAtOrDefault(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAtOrDefault(index.Value);

        var distanceFromEnd = index.Value;

        if (distanceFromEnd > 0)
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

    public SpanQuery<TSpan, TElement, ExceptByEnumerator<TSpan, TElement, TEnumerator, TElement>> Except(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return ExceptBy(elements, x => x, comparer);
    }

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

    public TElement First()
    {
        foreach (var element in this)
        {
            return element;
        }

        throw Exceptions.GetSequenceIsEmpty();
    }

    public TElement First(Func<TElement, bool> predicate)
    {
        foreach (var element in this)
        {
            if (predicate(element))
                return element;
        }

        throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    public TElement? FirstOrDefault()
    {
        foreach (var element in this)
        {
            return element;
        }

        return default;
    }

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

    public SpanQuery<TSpan, TElement, IntersectByEnumerator<TSpan, TElement, TEnumerator, TElement>> Intersect(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return IntersectBy(elements, x => x, comparer);
    }

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

    public TElement? LastOrDefault()
    {
        TElement? result = default;
        
        foreach (var element in this)
        {
            result = element;
        }

        return result;
    }

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

    public long LongCount()
    {
        long count = 0;

        foreach (var element in this)
        {
            count++;
        }

        return count;
    }

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

    public SpanQuery<TSpan, TResult, OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>> OfType<TResult>()
    {
        return With<TResult, OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new OfTypeEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator
                ));
    }

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

    public SpanQuery<TSpan, TElement, PrependSpanEnumerator<TSpan, TElement, TEnumerator>> Prepend(
        TElement element)
    {
        return With<TElement, PrependSpanEnumerator<TSpan, TElement, TEnumerator>>(
            new PrependSpanEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                element
                ));
    }

    public SpanQuery<TSpan, TElement, ReverseEnumerator<TSpan, TElement, TEnumerator>> Reverse()
    {
        return With(
            new ReverseEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator
                ));
    }

    public SpanQuery<TSpan, TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>> Select<TResult>(
        Func<TElement, int, TResult> selector)
    {
        return With<TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new SelectEnumerator<TSpan, TElement, TEnumerator, TResult>(
                _enumerator,
                selector
                ));
    }

    public SpanQuery<TSpan, TResult, SelectEnumerator<TSpan, TElement, TEnumerator, TResult>> Select<TResult>(
        Func<TElement, TResult> selector)
    {
        return Select((x, _) => selector(x));
    }

    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TResult, TResult>> SelectMany<TResult>(
        Func<TElement, int, IEnumerable<TResult>> selector)
    {
        return SelectMany(selector, (x, y) => y);
    }

    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TResult, TResult>> SelectMany<TResult>(
        Func<TElement, IEnumerable<TResult>> selector)
    {
        return SelectMany((x, _) => selector(x));
    }
    
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

    public SpanQuery<TSpan, TResult, SelectManyEnumerator<TSpan, TElement, TEnumerator, TCollection, TResult>> SelectMany<TCollection, TResult>(
        Func<TElement, IEnumerable<TCollection>> collectionSelector,
        Func<TElement, TCollection, TResult> resultSelector)
    {
        return SelectMany((x, i) => collectionSelector(x), resultSelector);
    }

    public SpanQuery<TSpan, TElement, SkipEnumerator<TSpan, TElement, TEnumerator>> Skip(
        int count)
    {
        return With(
            new SkipEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    public SpanQuery<TSpan, TElement, SkipLastEnumerator<TSpan, TElement, TEnumerator>> SkipLast(
        int count)
    {
        return With(
            new SkipLastEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    public SpanQuery<TSpan, TElement, SkipWhileEnumerator<TSpan, TElement, TEnumerator>> SkipWhile(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new SkipWhileEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
            ));
    }

    public SpanQuery<TSpan, TElement, SkipWhileEnumerator<TSpan, TElement, TEnumerator>> SkipWhile(
        Func<TElement, bool> predicate)
    {
        return SkipWhile((x, _) => predicate(x));
    }

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

    public SpanQuery<TSpan, TElement, TakeEnumerator<TSpan, TElement, TEnumerator>> Take(
        int count)
    {
        return With(
            new TakeEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    public SpanQuery<TSpan, TElement, TakeLastEnumerator<TSpan, TElement, TEnumerator>> TakeLast(
        int count)
    {
        return With(
            new TakeLastEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                count
            ));
    }

    public SpanQuery<TSpan, TElement, TakeWhileEnumerator<TSpan, TElement, TEnumerator>> TakeWhile(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new TakeWhileEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
            ));
    }

    public SpanQuery<TSpan, TElement, TakeWhileEnumerator<TSpan, TElement, TEnumerator>> TakeWhile(
        Func<TElement, bool> predicate)
    {
        return TakeWhile((x, _) => predicate(x));
    }

    public SpanQuery<TSpan, TElement, UnionByEnumerator<TSpan, TElement, TEnumerator, TElement>> Union(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return UnionBy(elements, x => x, comparer);
    }

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

    public SpanQuery<TSpan, TElement, WhereEnumerator<TSpan, TElement, TEnumerator>> Where(
        Func<TElement, int, bool> predicate)
    {
        return With(
            new WhereEnumerator<TSpan, TElement, TEnumerator>(
                _enumerator,
                predicate
                ));
    }

    public SpanQuery<TSpan, TElement, WhereEnumerator<TSpan, TElement, TEnumerator>> Where(
        Func<TElement, bool> predicate)
    {
        return Where((x, _) => predicate(x));
    }

    #endregion
}