using System.Xml.Linq;

namespace Linq2Span;

public ref struct SpanQuery<TSpan, TElement>
{
    private readonly ReadOnlySpan<TSpan> _span;
    private readonly CreateAggregator<TElement, TSpan> _createAggregator;

    internal SpanQuery(
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
        var (pre, aggregator, post) = _createAggregator(() => true, action, () => { });

        if (pre())
        {
            for (int i = 0; i < _span.Length; i++)
            {
                if (!aggregator(_span[i], i))
                    break;
            }
        }

        post();
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


    #region query operators

    public bool All(
        Func<TElement, bool> predicate)
    {
        return this.Aggregate(true, (all, value) => all && predicate(value));
    }

    public TAggregate Aggregate<TAggregate>(
        TAggregate seed,
        Func<TAggregate, TElement, TAggregate> func)
    {
        var result = seed;
        ForEach(value => 
        {
            result = func(result, value);
        });

        return result;
    }

    public TAggregate Aggregate<TAggregate>(
        Func<TAggregate, TElement, TAggregate> func)
    {
        return Aggregate(default(TAggregate)!, func)!;
    }

    public bool Any()
    {
        var hasValue = false;

        ForEach(value =>
        {
            hasValue = true;
            return false;
        });

        return hasValue;
    }

    public bool Any(
        Func<TElement, bool> predicate)
    {
        return this.Where(predicate).Any();
    }

    public SpanQuery<TSpan, TElement> Append(
        TElement element)
    {
        return this.Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int nextIndex = 0;
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        nextIndex = index + 1;
                        return aggregator(value, index);
                    },
                    () => aggregator(element, nextIndex)
                    );
            });
    }

    /// <summary>
    /// Converts the elements to the type <see cref="TType"/>.
    /// </summary>
    public SpanQuery<TSpan, TType> Cast<TType>()
    {
        return this.Select(value => (TType)(object)value!);
    }

    public SpanQuery<TSpan, TElement[]> Chunk(
        int size)
    {
        return this.Continue<TElement[]>(
            createAggregator => (pre, aggregator, post) =>
            {
                int chunkIndex = -1;
                var chunk = new List<TElement>();
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        chunk.Add(value);

                        if (chunk.Count == size)
                        {
                            chunkIndex++;
                            if (!aggregator(chunk.ToArray(), chunkIndex))
                                return false;
                            chunk.Clear();
                        }

                        return true;
                    },
                    () =>
                    {
                        if (chunk.Count > 0)
                        {
                            aggregator(chunk.ToArray(), chunkIndex + 1);
                        }
                    });
            });

    }

    public SpanQuery<TSpan, TElement> Concat(
        IEnumerable<TElement> elements)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int count = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (!aggregator(value, count))
                            return false;
                        count++;
                        return true;
                    },
                    () => 
                    {
                        foreach (var element in elements)
                        {
                            if (!aggregator(element, count))
                                break;
                            count++;
                        }

                        post();
                    });
            });

    }

    public bool Contains(
        TElement value,
        IEqualityComparer<TElement>? comparer = null)
    {
        comparer ??= EqualityComparer<TElement>.Default;

        var contains = false;
        ForEach(val =>
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

    public int Count()
    {
        int count = 0;
        ForEach(value => count++);
        return count;
    }

    public int Count(Func<TElement, bool> predicate)
    {
        int count = 0;
        ForEach(value =>
        {
            if (predicate(value))
                count++;
        });
        return count;
    }

    public SpanQuery<TSpan, TElement?> DefaultIfEmpty()
    {
        return Continue<TElement?>(
            createAggregator => (pre, aggregator, post) =>
            {
                int count = 0;
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        count++;
                        return aggregator(value, index);
                    },
                    () =>
                    {
                        // add default item if no items existed
                        if (count == 0)
                            aggregator(default, 0);

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TElement> DefaultIfEmpty(
        TElement defaultValue)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int count = 0;
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        count++;
                        return aggregator(value, index);
                    },
                    () =>
                    {
                        // add default item if no items existed
                        if (count == 0)
                            aggregator(defaultValue, 0);

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TElement> Distinct(
        IEqualityComparer<TElement>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var hashset = new HashSet<TElement>(comparer ?? EqualityComparer<TElement>.Default);
                var list = new List<TElement>();
                return createAggregator(
                    pre,
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

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TElement> DistinctBy<TBy>(
        Func<TElement, TBy> bySelector,
        IEqualityComparer<TBy>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var hashset = new HashSet<TBy>(comparer ?? EqualityComparer<TBy>.Default);
                var list = new List<TElement>();
                return createAggregator(
                    pre,
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

                        post();
                    });
            });
    }

    public TElement ElementAt(int index)
    {
        if (index >= 0)
        {
            bool found = false;
            TElement element = default!;

            ForEach((value, _index) =>
            {
                if (index == _index)
                {
                    found = true;
                    element = value;
                    return false;
                }

                return true;
            });

            if (found)
                return element;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public TElement ElementAt(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAt(index.Value);

        var length = index.Value;
        var queue = new Queue<TElement>(length);

        if (length > 0)
        {
            int count = 0;
            ForEach((value, _index) =>
            {
                if (queue.Count == length)
                    queue.Dequeue();

                queue.Enqueue(value);

                count++;
                return true;
            });

            if (queue.Count == length)
            {
                return queue.Dequeue();
            }
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public TElement? ElementAtOrDefault(int index)
    {
        TElement? element = default;

        if (index >= 0)
        {
            ForEach((value, _index) =>
            {
                if (index == _index)
                {
                    element = value;
                    return false;
                }

                return true;
            });
        }

        return element;
    }

    public TElement? ElementAtOrDefault(Index index)
    {
        if (!index.IsFromEnd)
            return ElementAtOrDefault(index.Value);

        var length = index.Value;
        var queue = new Queue<TElement>(length);

        if (length > 0)
        {
            int count = 0;
            ForEach((value, _index) =>
            {
                if (queue.Count == length)
                    queue.Dequeue();

                queue.Enqueue(value);

                count++;
                return true;
            });

            if (queue.Count == length && length > 0)
            {
                return queue.Dequeue();
            }
        }

        return default;
    }

    public SpanQuery<TSpan, TElement> Except(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var hashset = elements.ToHashSet(comparer ?? EqualityComparer<TElement>.Default);
                var index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (!hashset.Contains(value))
                        {
                            if (!aggregator(value, index))
                                return false;
                            index++;
                        }
                        return true;
                    },
                    post
                    );
            });
    }

    public SpanQuery<TSpan, TElement> ExceptBy<TKey>(
        IEnumerable<TKey> keys,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var excludedKeys = keys.ToHashSet(comparer ?? EqualityComparer<TKey>.Default);
                var index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        var key = keySelector(value);
                        if (!excludedKeys.Contains(key))
                        {
                            if (!aggregator(value, index))
                                return false;
                            index++;
                        }
                        return true;
                    },
                    post
                    );
            });
    }

    public TElement First()
    {
        bool found = false;
        TElement first = default!;

        ForEach(value =>
        {
            first = value;
            found = true;
            return false;
        });

        return found
            ? first
            : throw Exceptions.GetSequenceIsEmptyOrNotSatisfied();
    }

    public TElement First(Func<TElement, bool> predicate)
    {
        return this.Where(predicate).First();
    }

    public TElement? FirstOrDefault()
    {
        TElement? first = default;

        ForEach(value =>
        {
            first = value;
            return false;
        });

        return first!;
    }

    public TElement? FirstOrDefault(
        Func<TElement, bool> predicate)
    {
        return this.Where(predicate).FirstOrDefault();
    }

    public SpanQuery<TSpan, IGrouping<TKey, TElement>> GroupBy<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return Continue<IGrouping<TKey, TElement>>(
            createAggregator => (pre, aggregator, post) =>
            {
                var list = new List<TElement>();
                return createAggregator(
                    pre,
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

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TResult> GroupJoin<TInner, TKey, TResult>(
        IEnumerable<TInner> inner,
        Func<TElement, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TElement, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                var outerList = new List<TElement>();
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        outerList.Add(value);
                        return true;
                    },
                    () =>
                    {
                        int resultIndex = 0;
                        var results = outerList.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
                        foreach (var result in results)
                        {
                            if (!aggregator(result, resultIndex))
                                break;
                            resultIndex++;
                        }

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TElement> Intersect(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var hashset = elements.ToHashSet(comparer ?? EqualityComparer<TElement>.Default);
                var index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (hashset.Contains(value))
                        {
                            if (!aggregator(value, index))
                                return false;
                            index++;
                        }
                        return true;
                    },
                    post
                    );
            });
    }

    public SpanQuery<TSpan, TElement> IntersectBy<TKey>(
        IEnumerable<TKey> keys,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var includedKeys = keys.ToHashSet(comparer ?? EqualityComparer<TKey>.Default);
                var index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        var key = keySelector(value);
                        if (includedKeys.Contains(key))
                        {
                            if (!aggregator(value, index))
                                return false;
                            index++;
                        }
                        return true;
                    },
                    post
                    );
            });
    }

    public SpanQuery<TSpan, TResult> Join<TInner, TKey, TResult>(
        IEnumerable<TInner> inner, 
        Func<TElement, TKey> outerKeySelector, 
        Func<TInner, TKey> innerKeySelector, 
        Func<TElement, TInner, TResult> resultSelector, 
        IEqualityComparer<TKey>? comparer = null)
    {
        return this.Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                var list = new List<TElement>();
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        int index = 0;
                        foreach (var item in list.Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer))
                        {
                            if (!aggregator(item, index))
                                break;
                            index++;
                        }

                        post();
                    });
            });
    }

    public TElement Last(
        Func<TElement, bool> predicate)
    {
        bool hasElement = false;
        TElement element = default!;
        
        ForEach(value =>
        {
            if (predicate(value))
            {
                element = value;
                hasElement = true;
            }
        });

        if (!hasElement)
            throw Exceptions.GetSequenceIsEmpty();

        return element;
    }

    public TElement Last()
    {
        return Last(x => true);
    }

    public TElement? LastOrDefault(
        Func<TElement, bool> predicate)
    {
        TElement? element = default;

        ForEach(value =>
        {
            if (predicate(value))
            {
                element = value;
            }
        });

        return element;
    }

    public TElement? LastOrDefault()
    {
        return LastOrDefault(x => true);
    }

    public long LongCount()
    {
        long count = 0;
        ForEach(value => count++);
        return count;
    }

    public long LongCount(Func<TElement, bool> predicate)
    {
        long count = 0;
        ForEach(value =>
        {
            if (predicate(value))
                count++;
        });
        return count;
    }

    public SpanQuery<TSpan, TElement> OrderBy<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
    {
        keyComparer ??= Comparer<TKey>.Default;
        return Order((a, b) => keyComparer.Compare(keySelector(a), keySelector(b)));
    }

    public SpanQuery<TSpan, TElement> OrderByDescending<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
    {
        keyComparer ??= Comparer<TKey>.Default;
        return Order((a, b) => -keyComparer.Compare(keySelector(a), keySelector(b)));
    }

    public SpanQuery<TSpan, TElement> Order(
        IComparer<TElement>? comparer = null)
    {
        comparer ??= Comparer<TElement>.Default;
        return Order(comparer.Compare);
    }

    public SpanQuery<TSpan, TElement> OrderDescending(
        IComparer<TElement>? comparer = null)
    {
        comparer ??= Comparer<TElement>.Default;
        return Order((a, b) => -comparer.Compare(a,b));
    }

    private SpanQuery<TSpan, TElement> Order(
        Comparison<TElement> comparer)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var fullComparer = comparer;

                if (aggregator.Target is OrderAggregator ord)
                {
                    fullComparer = Combine(comparer, ord.Comparer);
                    aggregator = ord.Aggregator;
                }

                var list = new List<TElement>();
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        list.Sort(fullComparer);
                        int index = 0;
                        foreach (var item in list)
                        {
                            if (!aggregator(item, index))
                                break;
                            index++;
                        }

                        post();
                    });
            });
    }

    private static Comparison<TElement> Combine(
        Comparison<TElement> primary,
        Comparison<TElement> secondary)
    {
        return (a, b) =>
        {
            var result = primary(a, b);
            if (result == 0)
                return secondary(a, b);
            return result;
        };
    }

    private class OrderAggregator
    {
        public Func<TElement, int, bool> Aggregator;
        public Comparison<TElement> Comparer { get; }

        public OrderAggregator(
            Func<TElement, int, bool> aggregator,
            Comparison<TElement> comparer)
        {
            this.Aggregator = aggregator;
            this.Comparer = comparer;
        }

        public bool Aggregate(TElement value, int index)
        {
            return this.Aggregator(value, index);
        }
    }

    /// <summary>
    /// Produces only the elements of type <see cref="TType"/>.
    /// </summary>
    public SpanQuery<TSpan, TType> OfType<TType>()
    {
        return this.Continue<TType>(
            createAggregator => (pre, aggregator, post) =>
            {
                int index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (value is TType tvalue)
                        {
                            if (!aggregator(tvalue, index))
                                return false;
                            index++;
                        }

                        return true;
                    },
                    post
                    );
            });
    }

    public SpanQuery<TSpan, TElement> Prepend(
        TElement element)
    {
        return this.Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                return createAggregator(
                    () => aggregator(element, 0),
                    (value, index) => aggregator(value, index+1),
                    post
                    );
            });
    }

    public SpanQuery<TSpan, TElement> Reverse()
    {
        return this.Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var list = new List<TElement>();
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        list.Add(value);
                        return true;
                    },
                    () =>
                    {
                        int index = 0;
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (!aggregator(list[i], index))
                                break;
                            index++;
                        }

                        post();
                    });
            });
    }

    public SpanQuery<TSpan, TResult> Select<TResult>(
        Func<TElement, TResult> selector)
    {
        return Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        var mappedValue = selector(value);
                        if (!aggregator(mappedValue, index))
                            return false;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TResult> Select<TResult>(
        Func<TElement, int, TResult> selector)
    {
        return Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        var mappedValue = selector(value, index);
                        if (!aggregator(mappedValue, index))
                            return false;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TResult> SelectMany<TResult>(
        Func<TElement, int, IEnumerable<TResult>> selector)
    {
        return Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                int resultIndex = 0;
                return createAggregator(
                    pre,
                    (value, valueIndex) =>
                    {
                        foreach (var item in selector(value, valueIndex))
                        {
                            if (!aggregator(item, resultIndex))
                                return false;
                            resultIndex++;
                        }

                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TResult> SelectMany<TResult>(
        Func<TElement, IEnumerable<TResult>> selector)
    {
        return SelectMany((value, index) => selector(value));
    }

    public SpanQuery<TSpan, TResult> SelectMany<TCollection, TResult>(
        Func<TElement, int, IEnumerable<TCollection>> collectionSelector,
        Func<TElement, TCollection, TResult> resultSelector)
    {
        return Continue<TResult>(
            createAggregator => (pre, aggregator, post) =>
            {
                int resultIndex = 0;
                return createAggregator(
                    pre,
                    (value, valueIndex) =>
                    {
                        foreach (var item in collectionSelector(value, valueIndex))
                        {
                            var result = resultSelector(value, item);
                            if (!aggregator(result, resultIndex))
                                return false;
                            resultIndex++;
                        }

                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TResult> SelectMany<TCollection, TResult>(
        Func<TElement, IEnumerable<TCollection>> collectionSelector,
        Func<TElement, TCollection, TResult> resultSelector)
    {
        return SelectMany((item, index) => collectionSelector(item), resultSelector);
    }

    public TElement Single()
    {
        int count = 0;
        TElement element = default!;

        ForEach(value =>
        {
            if (count == 0)
                element = value;
            count++;
            return count < 2;
        });

        if (count == 0 || count > 1)
            throw Exceptions.GetSequenceContainsMoreThanOneElementOrEmpty();
        return element;
    }

    public TElement Single(
        Func<TElement, bool> predicate)
    {
        return this.Where(predicate).Single();
    }

    public TElement? SingleOrDefault()
    {
        int count = 0;
        TElement? element = default;

        ForEach(value =>
        {
            if (count == 0)
                element = value;
            count++;

            return count < 2;
        });

        if (count > 1)
            throw Exceptions.GetSequenceContainsMoreThanOneElement();

        return element!;
    }

    public TElement? SingleOrDefault(
        Func<TElement, bool> predicate)
    {
        return this.Where(predicate).SingleOrDefault();
    }

    public SpanQuery<TSpan, TElement> Skip(int count)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (index >= count)
                        {
                            if (!aggregator(value, index - count))
                                return false;
                        }

                        index++;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> SkipLast(int count)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var queue = new Queue<TElement>(count);
                int index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (queue.Count == count)
                        {
                            if (!aggregator(queue.Dequeue(), index - count))
                                return false;
                        }

                        queue.Enqueue(value);
                        index++;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> SkipWhile(
        Func<TElement, int, bool> predicate)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int index = 0;
                bool skip = true;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (skip)
                        {
                            if (!predicate(value, index))
                                skip = false;
                        }

                        if (!skip)
                        {
                            if (!aggregator(value, index))
                                return false;
                        }

                        index++;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> SkipWhile(
        Func<TElement, bool> predicate)
    {
        return SkipWhile((value, index) => predicate(value));
    }

    public SpanQuery<TSpan, TElement> Take(int count)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (index < count)
                        {
                            if (!aggregator(value, index))
                                return false;
                        }

                        index++;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> TakeLast(int count)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var queue = new Queue<TElement>(count);
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        queue.Enqueue(value);
                        if (queue.Count > count)
                            queue.Dequeue();
                        return true;
                    },
                    () =>
                    {
                        int index = 0;
                        while (queue.Count > 0)
                        {
                            if (!aggregator(queue.Dequeue(), index))
                                break;
                            index++;
                        }

                        post();
                    }
                );
            });
    }

    public SpanQuery<TSpan, TElement> TakeWhile(
        Func<TElement, int, bool> predicate)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int index = 0;
                return createAggregator(
                    pre,
                    (value, _) =>
                    {
                        if (predicate(value, index))
                        {
                            if (!aggregator(value, index))
                                return false;
                        }
                        else
                        {
                            return false;
                        }

                        index++;
                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> TakeWhile(
        Func<TElement, bool> predicate)
    {
        return TakeWhile((value, index) => predicate(value));
    }

    public SpanQuery<TSpan, TElement> ThenBy<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
    {
        keyComparer ??= Comparer<TKey>.Default;
        return ThenBy((a, b) => keyComparer.Compare(keySelector(a), keySelector(b)));
    }

    public SpanQuery<TSpan, TElement> ThenByDescending<TKey>(
        Func<TElement, TKey> keySelector,
        IComparer<TKey>? keyComparer = null)
    {
        keyComparer ??= Comparer<TKey>.Default;
        return ThenBy((a, b) => -keyComparer.Compare(keySelector(a), keySelector(b)));
    }

    private SpanQuery<TSpan, TElement> ThenBy(
        Comparison<TElement> comparer)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                var fullComparer = comparer;

                if (aggregator.Target is OrderAggregator ord)
                {
                    fullComparer = Combine(comparer, ord.Comparer);
                    aggregator = ord.Aggregator;
                }

                return createAggregator(
                    pre,
                    new OrderAggregator(aggregator, fullComparer).Aggregate,
                    post
                    );
            });
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to an array.
    /// </summary>
    public TElement[] ToArray()
    {
        return ToList().ToArray();
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        Func<TElement, TKey> keySelector,
        Func<TElement, TValue> valueSelector, 
        IEqualityComparer<TKey>? keyComparer = null)
        where TKey : notnull
    {
        return this.ToList().ToDictionary(keySelector, valueSelector, keyComparer);
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to a <see cref="Dictionary{TKey, TElement}"/>.
    /// </summary>
    public Dictionary<TKey, TElement> ToDictionary<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
        where TKey : notnull
    {
        return this.ToList().ToDictionary(keySelector, keyComparer);
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to a <see cref="HashSet{TElement}"/>
    /// </summary>
    public HashSet<TElement> ToHashSet(
        IEqualityComparer<TElement>? comparer = null)
    {
        var hashSet = new HashSet<TElement>();
        ForEach(hashSet.Add);
        return hashSet;
    }

    /// <summary>
    /// Converts the <see cref="SpanQuery{TSpan, TElement}"/> to a <see cref="List{T}"/>
    /// </summary>
    public List<TElement> ToList()
    {
        var list = new List<TElement>();
        ForEach(list.Add);
        return list;
    }

    public ILookup<TKey, TValue> ToLookup<TKey, TValue>(
        Func<TElement, TKey> keySelector,
        Func<TElement, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return this.ToList().ToLookup(keySelector, valueSelector, comparer);
    }

    public ILookup<TKey, TElement> ToLookup<TKey>(
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return this.ToList().ToLookup(keySelector, comparer);
    }

    public SpanQuery<TSpan, TElement> Union(
        IEnumerable<TElement> elements,
        IEqualityComparer<TElement>? comparer = null)
    {
        return Concat(elements).Distinct(comparer);
    }

    public SpanQuery<TSpan, TElement> UnionBy<TKey>(
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        return Concat(elements).DistinctBy(keySelector, keyComparer);
    }

    public SpanQuery<TSpan, TElement> Where(
        Func<TElement, int, bool> predicate)
    {
        return Continue<TElement>(
            createAggregator => (pre, aggregator, post) =>
            {
                int filteredIndex = 0;
                return createAggregator(
                    pre,
                    (value, index) =>
                    {
                        if (predicate(value, index))
                        {
                            if (!aggregator(value, filteredIndex))
                                return false;
                            filteredIndex++;
                        }

                        return true;
                    },
                    post
                );
            });
    }

    public SpanQuery<TSpan, TElement> Where(
        Func<TElement, bool> predicate)
    {
        return Where((value, index) => predicate(value));
    }

    #endregion
}

/// <summary>
/// A function that creates a new aggregator and flushing functions 
/// given the previous aggregator and flushing functions.
/// </summary>
/// <typeparam name="TValue1">The argument type of the previous aggregator function.</typeparam>
/// <typeparam name="TValue2">The argument type of the created aggregator function.</typeparam>
/// <param name="pre">A function called before any items are enumerated.</param>
/// <param name="aggregator">The function that is called for each item enumerated.</param>
/// <param name="post">A function that is called after all items are enumerated.</param>
public delegate (Func<bool> pre, Func<TValue2, int, bool> aggregator, Action post) CreateAggregator<TValue1, TValue2>(
    Func<bool> pre,
    Func<TValue1, int, bool> aggregator,
    Action post
    );

/// <summary>
/// A function that creates a new <see cref="CreateAggregator{TValue1, TValue3}"/>
/// from a prevous <see cref="CreateAggregator{TValue1, TValue2}"/>
/// </summary>
public delegate CreateAggregator<TValue1, TValue3> CreateCreateAggregator<TValue1, TValue2, TValue3>(
    CreateAggregator<TValue2, TValue3> createAggregator
    );

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
