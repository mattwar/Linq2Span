namespace Linq2Span;

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