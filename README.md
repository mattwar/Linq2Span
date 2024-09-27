# LINQ to Span

This project provides an implementation of LINQ 
for the `Span<T>` and `ReadOnlySpan<T>` types.

Normally, you cannot use LINQ with `Span<T>` because it is a stack-only type 
and LINQ methods are implemented over `IEnumerable<T>` and relies on those enumerables as being heap allocated objects.

You can convert a span to an enumerable by coping it into a list or array, 
but that is likely not desirable in scenarios where using spans are important.

This implementation captures the span in a ref struct called `SpanQuery`, along with the operations required to perform the query.
Ideally, allowing you to compute an aggregate like `Sum` or `Count`, pick an indivual element with `First` or `Single`,
or simply filter the span to a smaller size that is reasonable to copy.

The implementation is not allocation free, as the operations are captured as delegates.


Feel free to submit a pull request if you have fixes for bugs,
implementations of missing operators or significant peformance improvements.





