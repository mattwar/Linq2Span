namespace Linq2Span;

using Enumerators;
using System.Numerics;

public ref struct SpanQuery<TSpan, TElement, TEnumerator>
    where TEnumerator : struct, ISpanEnumerator<TSpan, TElement>
{
    private readonly ReadOnlySpan<TSpan> _span;
    internal readonly TEnumerator _enumerator;

    public SpanQuery(
        ReadOnlySpan<TSpan> span,
        TEnumerator enumerator)
    {
        _span = span;
        _enumerator = enumerator;
    }

    internal SpanQuery<TSpan, TElement2, TEnumerator2> With<TElement2, TEnumerator2>(
        TEnumerator2 enumerator)
        where TEnumerator2 : struct, ISpanEnumerator<TSpan, TElement2>
    {
        return new SpanQuery<TSpan, TElement2, TEnumerator2>(_span, enumerator);
    }

    internal SpanQuery<TSpan, TElement, TEnumerator2> With<TEnumerator2>(
        TEnumerator2 enumerator)
        where TEnumerator2 : struct, ISpanEnumerator<TSpan, TElement>
    {
        return new SpanQuery<TSpan, TElement, TEnumerator2>(_span, enumerator);
    }

    public void ForEach(Func<TElement, int, bool> func)
    {
        int nextIndex = 0;
        while (_enumerator.MoveNext(_span))
        {
            if (!func(_enumerator.Current, nextIndex++))
                return;
        }
    }

    public void ForEach(Func<TElement, bool> func)
    {
        while (_enumerator.MoveNext(_span))
        {
            if (!func(_enumerator.Current))
                return;
        }
    }

    public void ForEach(Action<TElement, int> action)
    {
        int nextIndex = 0;
        while (_enumerator.MoveNext(_span))
        {
            action(_enumerator.Current, nextIndex++);
        }
    }

    public void ForEach(Action<TElement> action)
    {
        while (_enumerator.MoveNext(_span))
        {
            action(_enumerator.Current);
        }
    }

    public List<TElement> ToList()
    {
        var enumerator = _enumerator;

        var list = new List<TElement>();

        while (enumerator.MoveNext(_span))
        {
            list.Add(enumerator.Current);
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
        var enumerator = _enumerator;
        var hashset = new HashSet<TElement>(comparer ?? EqualityComparer<TElement>.Default);
        while (enumerator.MoveNext(_span))
        {
            hashset.Add(enumerator.Current);
        }
        return hashset;
    }

    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        Func<TElement, TKey> keySelector,
        Func<TElement, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        var enumerator = _enumerator;
        var dictionary = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
        while (enumerator.MoveNext(_span))
        {
            var element = enumerator.Current;
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

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            result = aggregator(result, enumerator.Current);
        }

        return result;
    }

    public TResult Aggregate<TAccumulate, TResult>(
        TAccumulate seed,
        Func<TAccumulate, TElement, TAccumulate> aggregator,
        Func<TAccumulate, TResult> selector)
    {
        var accumulate = seed;

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            accumulate = aggregator(accumulate, enumerator.Current);
        }

        return selector(accumulate);
    }

    public bool All(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (!predicate(enumerator.Current))
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
        var enumerator = _enumerator;
        return enumerator.MoveNext(_span);
    }

    public bool Any(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
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
        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            var value = selector(enumerator.Current);
            total = total + value;
            count += TNumber.One;
        }
        return total / count;
    }

    public SpanQuery<TSpan, TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>> Cast<TResult>()
    {
        return With<TResult, CastEnumerator<TSpan, TElement, TEnumerator, TResult>>(
            new CastEnumerator<TSpan, TElement, TEnumerator, TResult>(_enumerator)
            );
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

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (comparer.Equals(enumerator.Current, value))
                return true;
        }

        return false;
    }

    public int Count()
    {
        var enumerator = _enumerator;
        int count = 0;
        while (enumerator.MoveNext(_span))
        {
            count++;
        }
        return count;
    }

    public int Count(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        int count = 0;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
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
            var enumerator = _enumerator;
            while (enumerator.MoveNext(_span))
            {
                if (index == 0)
                    return enumerator.Current;
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

            var enumerator = _enumerator;
            while (enumerator.MoveNext(_span))
            {
                if (queue.Count == distanceFromEnd)
                    queue.Dequeue();
                queue.Enqueue(enumerator.Current);
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
            var enumerator = _enumerator;
            while (enumerator.MoveNext(_span))
            {
                if (index == 0)
                    return enumerator.Current;
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

            var enumerator = _enumerator;
            while (enumerator.MoveNext(_span))
            {
                if (queue.Count == distanceFromEnd)
                    queue.Dequeue();
                queue.Enqueue(enumerator.Current);
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
        var enumerator = _enumerator;
        if (enumerator.MoveNext(_span))
        {
            return enumerator.Current;
        }
        throw Exceptions.GetSequenceIsEmpty();
    }

    public TElement First(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
                return enumerator.Current;
        }
        throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    public TElement? FirstOrDefault()
    {
        var enumerator = _enumerator;
        if (enumerator.MoveNext(_span))
        {
            return enumerator.Current;
        }
        return default;
    }

    public TElement? FirstOrDefault(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
                return enumerator.Current;
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

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            result = enumerator.Current;
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

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
            {
                result = enumerator.Current;
                found = true;
            }
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();

        return result;
    }

    public TElement? LastOrDefault()
    {
        var enumerator = _enumerator;
        TElement? result = default;
        while (enumerator.MoveNext(_span))
        {
            result = enumerator.Current;
        }
        return result;
    }

    public TElement? LastOrDefault(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        TElement? result = default;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
            {
                result = enumerator.Current;
            }
        }
        return result;
    }

    public long LongCount()
    {
        var enumerator = _enumerator;
        long count = 0;
        while (enumerator.MoveNext(_span))
        {
            count++;
        }
        return count;
    }

    public long LongCount(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        long count = 0;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
                count++;
        }
        return count;
    }

    public TResult Max<TResult>(
        Func<TElement, TResult> selector)
    {
        var comparer = Comparer<TResult>.Default;
        var enumerator = _enumerator;
        var hasValue = false;
        TResult maxValue = default!;

        while (enumerator.MoveNext(_span))
        {
            var value = selector(enumerator.Current);
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

        var enumerator = _enumerator;
        var hasValue = false;
        TKey maxKey = default!;
        TElement maxValue = default!;

        while (enumerator.MoveNext(_span))
        {
            var key = keySelector(enumerator.Current);
            if (!hasValue || comparer.Compare(key, maxKey) > 0)
            {
                maxKey = key;
                maxValue = enumerator.Current;
                hasValue = true;
            }
        }

        return maxValue;
    }

    public TResult Min<TResult>(
        Func<TElement, TResult> selector)
    {
        var comparer = Comparer<TResult>.Default;
        var enumerator = _enumerator;
        var hasValue = false;
        TResult minValue = default!;

        while (enumerator.MoveNext(_span))
        {
            var value = selector(enumerator.Current);
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

        var enumerator = _enumerator;
        var hasValue = false;
        TKey maxKey = default!;
        TElement maxValue = default!;

        while (enumerator.MoveNext(_span))
        {
            var key = keySelector(enumerator.Current);
            if (!hasValue || comparer.Compare(key, maxKey) < 0)
            {
                maxKey = key;
                maxValue = enumerator.Current;
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
        var enumerator = _enumerator;
        if (enumerator.MoveNext(_span))
        {
            var result = enumerator.Current;
            if (!enumerator.MoveNext(_span))
                return result;
        }

        throw Exceptions.GetSequenceIsEmptyOrContainsMoreThanOneElement();
    }

    public TElement Single(Func<TElement, bool> predicate)
    {
        bool found = false;
        TElement element = default!;

        var enumerator = _enumerator;
        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
            {
                if (!found)
                {
                    found = true;
                    element = enumerator.Current;
                }
                else
                {
                    throw Exceptions.GetSequenceIsEmptyOrNotSatisifiedOrContainsMoreThanOneElement();
                }
            }
        }

        if (!found)
            throw Exceptions.GetSequenceIsEmptyOrNotSatisifiedOrContainsMoreThanOneElement();

        return element;
    }

    public TElement? SingleOrDefault()
    {
        var enumerator = _enumerator;
        if (enumerator.MoveNext(_span))
        {
            var result = enumerator.Current;
            if (!enumerator.MoveNext(_span))
                return result;
            throw Exceptions.GetSequenceContainsMoreThanOneElement();
        }

        return default;
    }

    public TElement? SingleOrDefault(Func<TElement, bool> predicate)
    {
        var enumerator = _enumerator;
        bool found = false;
        TElement? element = default!;

        while (enumerator.MoveNext(_span))
        {
            if (predicate(enumerator.Current))
            {
                if (!found)
                {
                    found = true;
                    element = enumerator.Current;
                }
                else
                {
                    throw Exceptions.GetSequenceContainsMoreThanOneElement();
                }
            }
        }

        return element;
    }

    public TNumber Sum<TNumber>(Func<TElement, TNumber> selector)
        where TNumber : INumber<TNumber>
    {
        var enumerator = _enumerator;
        TNumber sum = TNumber.Zero;
        while (enumerator.MoveNext(_span))
        {
            sum = sum + selector(enumerator.Current);
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