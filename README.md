# LINQ to Span

This project provides an implementation of LINQ 
for the `Span<T>` and `ReadOnlySpan<T>` types.

# Why is this a thing?

Normally, you cannot use LINQ with `Span<T>` because it is a stack-only type 
and LINQ methods are implemented over heap allocated `IEnumerable<T>`.

If you have data in the form of a span, the only way to use LINQ operators on it
is to convert it to a heap allocated `IEnumerable<T>` by copying it to a list or array first.

This project exists to give you a way to use LINQ operators directly on a span without copying the data.


# What is included?

Included are a full set of LINQ operators that work on a new type `SpanQuery` 
that you can get using the `AsSpanQuery()` extension method,
and a separate limited set of operators that work directly on `Span<T>` and `ReadOnlySpan<T>`.
The limited operators only include operators that return a single value, 
like the `Any`, `Aggregate`, `Count`, `ElementAt` and `First`,
that operate on the span immediately, 
as opposed to defering execution until the full query is assembled.

# How it works

The type `SpanQuery` is a ref struct that captures the span,
restricting your use of it to the stack.
It also accumulates your LINQ operations as delegates, as you call them,
just like how LINQ works with `IEnumerable<T>` and `IQueryable<T>`.
When you call an operator like `Count` or `ToList` 
or iterate the results using `ForEach`,
the operations are executed on the span to determine the results.

You can avoid list allocations for queries that produce multiple values by using the `ForEach` operator
instead of converting the results to an allocated list using `ToList`.

*Note: The implementation is not allocation free, as the operations are captured as delegates.*

*Note: Some operators such as `Reverse` and `OrderBy` will still cause duplication of the span, or a subset, 
even when the final operation is an aggregate or `ForEach`. Use of these operators should be limited.*

# Example Usage

```csharp
Span<int> span = stackalloc int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

var count = span.AsSpanQuery()
	.Where(i => i % 2 == 0)
	.Select(i => i * i)
	.ForEach(i => Console.WriteLine(i));
```

# How to Access

The builds are published on nuget here: [Linq2Span](https://www.nuget.org/packages/Linq2Span/)

# How to Contribute

The project is hosted on github here: [Linq2Span](https://github.com/mattwar/Linq2Span)

Feel free to submit a pull request if you have fixes for bugs,
implementations of missing operators or significant peformance improvements.



