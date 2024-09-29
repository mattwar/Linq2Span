namespace Linq2Span;

/// <summary>
/// A function that creates a new <see cref="CreateAggregator{TValue1, TValue3}"/>
/// from a prevous <see cref="CreateAggregator{TValue1, TValue2}"/>
/// </summary>
public delegate CreateAggregator<TValue1, TValue3> CreateCreateAggregator<TValue1, TValue2, TValue3>(
    CreateAggregator<TValue2, TValue3> createAggregator
    );
