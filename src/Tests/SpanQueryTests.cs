using Linq2Span;

namespace Tests;


[TestClass]
public class SpanQueryTests : TestBase
{
    protected static int[] OneToTen = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    protected static long[] OneToTen_long = OneToTen.Select(x => (long)x).ToArray();
    protected static double[] OneToTen_double = OneToTen.Select(x => (double)x).ToArray();
    protected static float[] OneToTen_float = OneToTen.Select(x => (float)x).ToArray();
    protected static decimal[] OneToTen_decimal = OneToTen.Select(x => (decimal)x).ToArray();

    [TestMethod]
    public void Test_All()
    {
        Assert.IsTrue(OneToTen.AsSpanQuery().All(x => x > 0));
        Assert.IsFalse(OneToTen.AsSpanQuery().All(x => x > 1));
    }

    [TestMethod]
    public void Test_Aggregate()
    {
        Assert.AreEqual(55, OneToTen.AsSpanQuery().Aggregate(0, (total, value) => total + value));
        Assert.AreEqual(3628800, OneToTen.AsSpanQuery().Aggregate(1, (total, value) => total * value));
    }

    [TestMethod]
    public void Test_Any()
    {
        int[] none = [];
        Assert.IsTrue(OneToTen.AsSpanQuery().Any());
        Assert.IsFalse(none.AsSpanQuery().Any());

        Assert.IsTrue(OneToTen.AsSpanQuery().Where(x => x > 0).Any());
        Assert.IsFalse(OneToTen.AsSpanQuery().Where(x => x < 0).Any());
    }

    [TestMethod]
    public void Test_Any_Predicate()
    {
        Assert.IsTrue(OneToTen.AsSpanQuery().Any(x => x > 0));
        Assert.IsFalse(OneToTen.AsSpanQuery().Any(x => x < 0));
    }

    [TestMethod]
    public void Test_Append()
    {
        AssertAreEquivalent(
            OneToTen.Append(11).ToArray(),
            OneToTen.AsSpanQuery().Append(11).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Append(11).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Append(11).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Average()
    {
        Assert.AreEqual(5.5, OneToTen_double.AsSpanQuery().Average());
    }

    [TestMethod]
    public void Test_Cast()
    {
        AssertAreEquivalent(
            OneToTen.AsSpanQuery().Cast<object>().ToArray(),
            OneToTen.Cast<object>().ToArray()
            );
    }

    [TestMethod]
    public void Test_Chunk()
    {
        AssertAreEquivalent(
            OneToTen.Chunk(3).ToArray(),
            OneToTen.AsSpanQuery().Chunk(3).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Chunk(3).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Chunk(3).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Concat()
    {
        AssertAreEquivalent(
            OneToTen.Concat(OneToTen).ToArray(),
            OneToTen.AsSpanQuery().Concat(OneToTen).ToArray()
            );

        // post concat indices
        AssertAreEquivalent(
            OneToTen.Concat(OneToTen).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Concat(OneToTen).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Contains()
    {
        Assert.IsTrue(OneToTen.AsSpanQuery().Contains(10));
        Assert.IsTrue(OneToTen.AsSpanQuery().Contains(1));
        Assert.IsFalse(OneToTen.AsSpanQuery().Contains(0));
    }

    [TestMethod]
    public void Test_Count()
    {
        Assert.AreEqual(10, OneToTen.AsSpanQuery().Count());
    }

    [TestMethod]
    public void Test_Count_Predicate()
    {
        Assert.AreEqual(5, OneToTen.AsSpanQuery().Count(x => x > 5));
    }

    [TestMethod]
    public void Test_DefaultIfEmpty()
    {
        int[] none = [];
        int[] one = [1];
        AssertAreEquivalent(
            none.DefaultIfEmpty().ToArray(),
            none.AsSpanQuery().DefaultIfEmpty().ToArray()
            );
        AssertAreEquivalent(
            one.DefaultIfEmpty().ToArray(),
            one.AsSpanQuery().DefaultIfEmpty().ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            none.DefaultIfEmpty().Select((x, i) => i).ToArray(),
            none.AsSpanQuery().DefaultIfEmpty().Select((x, i) => i).ToArray()
            );

        AssertAreEquivalent(
            one.DefaultIfEmpty().Select((x, i) => i).ToArray(),
            one.AsSpanQuery().DefaultIfEmpty().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_DefaultIfEmpty_Default()
    {
        int[] none = [];
        int[] one = [1];

        AssertAreEquivalent(
            none.DefaultIfEmpty(2).ToArray(),
            none.AsSpanQuery().DefaultIfEmpty(2).ToArray()
            );
        AssertAreEquivalent(
            one.DefaultIfEmpty(2).ToArray(),
            one.AsSpanQuery().DefaultIfEmpty(2).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            none.DefaultIfEmpty(2).Select((x, i) => i).ToArray(),
            none.AsSpanQuery().DefaultIfEmpty(2).Select((x, i) => i).ToArray()
            );
        AssertAreEquivalent(
            one.DefaultIfEmpty(2).Select((x, i) => i).ToArray(),
            one.AsSpanQuery().DefaultIfEmpty(2).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Distinct()
    {
        int[] nondistinct = [1, 2, 3, 4, 5, 1, 2, 3, 4, 5];
        string[] nondistinct2 = ["one", "One", "Two", "TWO"];
        
        AssertAreEquivalent(
            nondistinct.Distinct().ToArray(),
            nondistinct.AsSpanQuery().Distinct().ToArray()
            );
        AssertAreEquivalent(
            nondistinct2.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            nondistinct2.AsSpanQuery().Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            nondistinct.Distinct().Select((x, i) => i).ToArray(),
            nondistinct.AsSpanQuery().Distinct().Select((x, i) => i).ToArray()
            );
        AssertAreEquivalent(
            nondistinct2.Distinct(StringComparer.OrdinalIgnoreCase).Select((x, i) => i).ToArray(),
            nondistinct2.AsSpanQuery().Distinct(StringComparer.OrdinalIgnoreCase).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_DistinctBy()
    {
        string[] nondistinct = ["one", "two", "three", "four", "five"];
        AssertAreEquivalent(
            nondistinct.DistinctBy(x => x.Length).ToArray(),
            nondistinct.AsSpanQuery().DistinctBy(x => x.Length).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            nondistinct.DistinctBy(x => x.Length).Select((x, i) => i).ToArray(),
            nondistinct.AsSpanQuery().DistinctBy(x => x.Length).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_ElementAt()
    {
        Assert.AreEqual(3, OneToTen.AsSpanQuery().ElementAt(2));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAt(0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpanQuery().ElementAt(10));
    }

    [TestMethod]
    public void Test_ElementAt_Index()
    {
        Assert.AreEqual(3, OneToTen.AsSpanQuery().ElementAt((Index)2));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAt((Index)0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpanQuery().ElementAt((Index)10));
        Assert.AreEqual(10, OneToTen.AsSpanQuery().ElementAt(^1));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAt(^10));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpanQuery().ElementAt(^0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpanQuery().ElementAt(^11));
    }

    [TestMethod]
    public void Test_ElementAtOrDefault()
    {
        Assert.AreEqual(3, OneToTen.AsSpanQuery().ElementAtOrDefault(2));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAtOrDefault(0));
        Assert.AreEqual(0, OneToTen.AsSpanQuery().ElementAtOrDefault(10));
    }

    [TestMethod]
    public void Test_ElementAtOrDefault_Index()
    {
        Assert.AreEqual(3, OneToTen.AsSpanQuery().ElementAtOrDefault((Index)2));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAtOrDefault((Index)0));
        Assert.AreEqual(0, OneToTen.AsSpanQuery().ElementAtOrDefault((Index)10));
        Assert.AreEqual(10, OneToTen.AsSpanQuery().ElementAtOrDefault(^1));
        Assert.AreEqual(1, OneToTen.AsSpanQuery().ElementAtOrDefault(^10));
        Assert.AreEqual(0, OneToTen.AsSpanQuery().ElementAtOrDefault(^0));
        Assert.AreEqual(0, OneToTen.AsSpanQuery().ElementAtOrDefault(^11));
    }

    [TestMethod]
    public void Test_Except()
    {
        AssertAreEquivalent(
            OneToTen.Except([2, 4, 6]).ToArray(),
            OneToTen.AsSpanQuery().Except([2, 4, 6]).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Except([2, 4, 6]).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Except([2, 4, 6]).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_ExceptBy()
    {
        AssertAreEquivalent(
            OneToTen.ExceptBy([2, 4, 6], x => x * 2).ToArray(),
            OneToTen.AsSpanQuery().ExceptBy([2, 4, 6], x => x * 2).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.ExceptBy([2, 4, 6], x => x * 2).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().ExceptBy([2, 4, 6], x => x * 2).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_First()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpanQuery().First(), 1);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpanQuery().First());
    }

    [TestMethod]
    public void Test_First_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpanQuery().First(x => x > 5), 6);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().First(x => x > 10));
    }

    [TestMethod]
    public void Test_FirstOrDefault()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(), 1);
        Assert.AreEqual(none.AsSpanQuery().FirstOrDefault(), 0);
    }

    [TestMethod]
    public void Test_FirstOrDefault_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(x => x > 5), 6);
        Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(x => x > 10), 0);
    }

    [TestMethod]
    public void Test_foreach()
    {
        var items = new List<int>();

        foreach (var x in OneToTen.AsSpanQuery().Where(x => x < 5))
        {
            items.Add(x);
        }

        AssertAreEquivalent(
            OneToTen.Where(x => x < 5).ToArray(),
            items.ToArray()
            );
    }

    [TestMethod]
    public void Test_ForEach()
    {
        var items = new List<int>();

        OneToTen.AsSpanQuery().ForEach(x =>
        {
            items.Add(x);
            if (items.Count > 10)
                throw new Exception("Runaway query");
        });

        AssertAreEquivalent(
            OneToTen,
            items.ToArray()
            );
    }

    [TestMethod]
    public void Test_ForEach_Index()
    {
        var items = new List<int>();

        OneToTen.AsSpanQuery().ForEach((x, i) =>
        {
            items.Add(x);
            if (items.Count > 10)
                throw new Exception("Runaway query");
        });

        AssertAreEquivalent(
            OneToTen,
            items.ToArray()
            );
    }

    [TestMethod]
    public void Test_ForEach_Abort()
    {
        var items = new List<int>();

        OneToTen.AsSpanQuery().ForEach(x =>
        {
            items.Add(x);
            return items.Count < 5;
        });

        AssertAreEquivalent(
            OneToTen.Take(5).ToArray(),
            items.ToArray()
            );
    }

    [TestMethod]
    public void Test_ForEach_Index_Abort()
    {
        var items = new List<int>();

        OneToTen.AsSpanQuery().ForEach((x, i) =>
        {
            items.Add(x);
            return items.Count < 5;
        });

        AssertAreEquivalent(
            OneToTen.Take(5).ToArray(),
            items.ToArray()
            );
    }

    [TestMethod]
    public void Test_GroupBy()
    {
        AssertAreEquivalent(
            OneToTen.GroupBy(x => x / 2).ToArray(),
            OneToTen.AsSpanQuery().GroupBy(x => x / 2).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.GroupBy(x => x / 2).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().GroupBy(x => x / 2).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_GroupJoin()
    {
        AssertAreEquivalent(
            OneToTen.GroupJoin(OneToTen, x => x, y => y, (x, ygroup) => x + ygroup.Sum()).ToArray(),
            OneToTen.AsSpanQuery().GroupJoin(OneToTen, x => x, y => y, (x, ygroup) => x + ygroup.Sum()).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.GroupJoin(OneToTen, x => x, y => y, (x, ygroup) => x + ygroup.Sum()).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().GroupJoin(OneToTen, x => x, y => y, (x, ygroup) => x + ygroup.Sum()).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Intersect()
    {
        AssertAreEquivalent(
            OneToTen.Intersect([2, 4, 6]).ToArray(),
            OneToTen.AsSpanQuery().Intersect([2, 4, 6]).ToArray()
            );

        // results are distinct
        var duplicates = OneToTen.Concat(OneToTen).ToArray();
        AssertAreEquivalent(
            duplicates.Intersect([2, 4, 6]).ToArray(),
            duplicates.AsSpanQuery().Intersect([2, 4, 6]).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Intersect([2, 4, 6]).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Intersect([2, 4, 6]).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_IntersectBy()
    {
        AssertAreEquivalent(
            OneToTen.IntersectBy([2, 4, 6], x => x * 2).ToArray(),
            OneToTen.AsSpanQuery().IntersectBy([2, 4, 6], x => x * 2).ToArray()
            );

        // results are distinct
        var duplicates = OneToTen.Concat(OneToTen).ToArray();
        AssertAreEquivalent(
            duplicates.IntersectBy([2, 4, 6], x => x * 2).ToArray(),
            duplicates.AsSpanQuery().IntersectBy([2, 4, 6], x => x * 2).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.IntersectBy([2, 4, 6], x => x * 2).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().IntersectBy([2, 4, 6], x => x * 2).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Join()
    {
        AssertAreEquivalent(
            OneToTen.Join(OneToTen, x => x, y => y, (x, y) => x = y).ToArray(),
            OneToTen.AsSpanQuery().Join(OneToTen, x => x, y => y, (x, y) => x = y).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Join(OneToTen, x => x, y => y, (x, y) => x = y).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Join(OneToTen, x => x, y => y, (x, y) => x = y).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Last()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpanQuery().Last(), 10);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpanQuery().Last());
    }

    [TestMethod]
    public void Test_LastOrDefault()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpanQuery().LastOrDefault(), 10);
        Assert.AreEqual(none.AsSpanQuery().LastOrDefault(), 0);
    }

    [TestMethod]
    public void Test_LongCount()
    {
        Assert.AreEqual(10, OneToTen.AsSpanQuery().LongCount());
    }

    [TestMethod]
    public void Test_LongCount_Predicate()
    {
        Assert.AreEqual(5, OneToTen.AsSpanQuery().LongCount(x => x > 5));
    }

    [TestMethod]
    public void Test_Max()
    {
        Assert.AreEqual(10, OneToTen.AsSpanQuery().Max());
    }

    [TestMethod]
    public void Test_Max_Selector()
    {
        Assert.AreEqual(20, OneToTen.AsSpanQuery().Max(x => x * 2));
    }

    [TestMethod]
    public void Test_MaxBy()
    {
        Assert.AreEqual(1, OneToTen.AsSpanQuery().MaxBy(x => -x));
    }

    [TestMethod]
    public void Test_Min()
    {
        Assert.AreEqual(1, OneToTen.AsSpanQuery().Min());
    }

    [TestMethod]
    public void Test_Min_Selector()
    {
        Assert.AreEqual(2, OneToTen.AsSpanQuery().Min(x => x * 2));
    }

    [TestMethod]
    public void Test_MinBy()
    {
        Assert.AreEqual(10, OneToTen.AsSpanQuery().MinBy(x => -x));
    }

    [TestMethod]
    public void Test_OfType()
    {
        object[] values = [1, 1.0, 1m, "one"];

        AssertAreEquivalent(
            values.OfType<int>().ToArray(),
            values.AsSpanQuery().OfType<int>().ToArray()
            );

        AssertAreEquivalent(
            values.OfType<double>().ToArray(),
            values.AsSpanQuery().OfType<double>().ToArray()
            );

        AssertAreEquivalent(
            values.OfType<decimal>().ToArray(),
            values.AsSpanQuery().OfType<decimal>().ToArray()
            );

        AssertAreEquivalent(
            values.OfType<string>().ToArray(),
            values.AsSpanQuery().OfType<string>().ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            values.OfType<double>().Select((x, i) => i).ToArray(),
            values.AsSpanQuery().OfType<double>().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Order()
    {
        AssertAreEquivalent(
            OneToTen.Order().ToArray(),
            OneToTen.AsSpanQuery().Order().ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Order().Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Order().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_OrderDescending()
    {
        AssertAreEquivalent(
            OneToTen.OrderDescending().ToArray(),
            OneToTen.AsSpanQuery().OrderDescending().ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.OrderDescending().Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().OrderDescending().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_OrderBy()
    {
        AssertAreEquivalent(
            OneToTen.OrderBy(x => -x).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => -x).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.OrderBy(x => -x).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => -x).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_OrderByDescending()
    {
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Prepend()
    {
        AssertAreEquivalent(
            OneToTen.Prepend(0).ToArray(),
            OneToTen.AsSpanQuery().Prepend(0).ToArray()
            );

        AssertAreEquivalent(
            OneToTen.Prepend(0).Prepend(-1).ToArray(),
            OneToTen.AsSpanQuery().Prepend(0).Prepend(-1).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Prepend(0).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Prepend(0).Select((x, i) => i).ToArray()
            );

        AssertAreEquivalent(
            OneToTen.Prepend(0).Prepend(-1).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Prepend(0).Prepend(-1).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Reverse()
    {
        AssertAreEquivalent(
            OneToTen.Reverse().ToArray(),
            OneToTen.AsSpanQuery().Reverse().ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Reverse().Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Reverse().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Select()
    {
        AssertAreEquivalent(
            OneToTen.Select(x => x * 2).ToArray(),
            OneToTen.AsSpanQuery().Select(x => x * 2).ToArray()
            );
    }

    [TestMethod]
    public void Test_Select_Index()
    {
        AssertAreEquivalent(
            Enumerable.Range(0, 10).ToArray(),
            OneToTen.AsSpanQuery().Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SelectMany()
    {
        AssertAreEquivalent(
            OneToTen.SelectMany(x => new[] { x, x }).ToArray(),
            OneToTen.AsSpanQuery().SelectMany(x => new[] { x, x }).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SelectMany(x => new[] { x, x }).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SelectMany(x => new[] { x, x }).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SelectMany_Index()
    {
        AssertAreEquivalent(
            OneToTen.SelectMany((x, i) => new[] { x, i }).ToArray(),
            OneToTen.AsSpanQuery().SelectMany((x, i) => new[] { x, i }).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SelectMany((x, i) => new[] { x, i }).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SelectMany((x, i) => new[] { x, i }).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SelectMany_Result()
    {
        AssertAreEquivalent(
            OneToTen.SelectMany(x => new[] { x, x }, (x, y) => x + y).ToArray(),
            OneToTen.AsSpanQuery().SelectMany(x => new[] { x, x }, (x, y) => x + y).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SelectMany(x => new[] { x, x }, (x, y) => x + y).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SelectMany(x => new[] { x, x }, (x, y) => x + y).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SelectMany_Index_Result()
    {
        AssertAreEquivalent(
            OneToTen.SelectMany((x, i) => new[] { x, i }, (x, y) => x + y).ToArray(),
            OneToTen.AsSpanQuery().SelectMany((x, i) => new[] { x, i }, (x, y) => x + y).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SelectMany((x, i) => new[] { x, i }, (x, y) => x + y).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SelectMany((x, i) => new[] { x, i }, (x, y) => x + y).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Single()
    {
        int[] one = [1];
        int[] none = [];
        int[] two = [1, 2];
        Assert.AreEqual(one.AsSpanQuery().Single(), 1);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpanQuery().Single());
        Assert.ThrowsException<InvalidOperationException>(() => two.AsSpanQuery().Single());
    }

    [TestMethod]
    public void Test_Single_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpanQuery().Single(x => x > 9), 10);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().Single(x => x > 10));
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().Single(x => x > 8));
    }

    [TestMethod]
    public void Test_SingleOrDefault()
    {
        int[] one = [1];
        int[] none = [];
        int[] two = [1, 2];
        Assert.AreEqual(one.AsSpanQuery().SingleOrDefault(), 1);
        Assert.AreEqual(none.AsSpanQuery().SingleOrDefault(), 0);
        Assert.ThrowsException<InvalidOperationException>(() => two.AsSpanQuery().SingleOrDefault());
    }

    [TestMethod]
    public void Test_SingleOrDefault_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpanQuery().SingleOrDefault(x => x > 9), 10);
        Assert.AreEqual(OneToTen.AsSpanQuery().SingleOrDefault(x => x > 10), 0);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().SingleOrDefault(x => x > 8));
    }

    [TestMethod]
    public void Test_Skip()
    {
        AssertAreEquivalent(
            OneToTen.Skip(5).ToArray(),
            OneToTen.AsSpanQuery().Skip(5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Skip(5).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Skip(5).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SkipLast()
    {
        AssertAreEquivalent(
            OneToTen.SkipLast(5).ToArray(),
            OneToTen.AsSpanQuery().SkipLast(5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SkipLast(5).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SkipLast(5).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SkipWhile()
    {
        AssertAreEquivalent(
            OneToTen.SkipWhile(x => x < 6).ToArray(),
            OneToTen.AsSpanQuery().SkipWhile(x => x < 6).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SkipWhile(x => x < 6).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SkipWhile(x => x < 6).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_SkipWhile_Index()
    {
        AssertAreEquivalent(
            OneToTen.SkipWhile((x, i) => i < 6).ToArray(),
            OneToTen.AsSpanQuery().SkipWhile((x, i) => i < 6).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.SkipWhile((x, i) => i < 6).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().SkipWhile((x, i) => i < 6).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Sum()
    {
        Assert.AreEqual(55, OneToTen.AsSpanQuery().Sum());
    }

    [TestMethod]
    public void Test_Sum_Selector()
    {
        Assert.AreEqual(110, OneToTen.AsSpanQuery().Sum(x => x * 2));
    }

    [TestMethod]
    public void Test_Take()
    {
        AssertAreEquivalent(
            OneToTen.Take(5).ToArray(),
            OneToTen.AsSpanQuery().Take(5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Take(5).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Take(5).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_TakeLast()
    {
        AssertAreEquivalent(
            OneToTen.TakeLast(5).ToArray(),
            OneToTen.AsSpanQuery().TakeLast(5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.TakeLast(5).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().TakeLast(5).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_TakeWhile()
    {
        AssertAreEquivalent(
            OneToTen.TakeWhile(x => x < 6).ToArray(),
            OneToTen.AsSpanQuery().TakeWhile(x => x < 6).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.TakeWhile(x => x < 6).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().TakeWhile(x => x < 6).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_TakeWhile_Index()
    {
        AssertAreEquivalent(
            OneToTen.TakeWhile((x, i) => i < 6).ToArray(),
            OneToTen.AsSpanQuery().TakeWhile((x, i) => i < 6).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.TakeWhile((x, i) => i < 6).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().TakeWhile((x, i) => i < 6).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_ThenBy()
    {
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x & 2).ThenBy(x => x & 1).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x & 2).ThenBy(x => x & 1).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x & 2).ThenBy(x => x & 1).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x & 2).ThenBy(x => x & 1).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_ThenByDescending()
    {
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x & 2).ThenByDescending(x => x & 1).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x & 2).ThenByDescending(x => x & 1).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.OrderBy(x => x & 2).ThenByDescending(x => x & 1).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().OrderBy(x => x & 2).ThenByDescending(x => x & 1).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Union()
    {
        AssertAreEquivalent(
            OneToTen.Union([2, 11, 15]).ToArray(),
            OneToTen.AsSpanQuery().Union([2, 11, 15]).ToArray()
            );

        // results are distinct
        var duplicates = OneToTen.Concat(OneToTen).ToArray();
        AssertAreEquivalent(
            duplicates.Union([2, 11, 15]).ToArray(),
            duplicates.AsSpanQuery().Union([2, 11, 15]).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Union([2, 11, 15]).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Union([2, 11, 15]).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_UnionBy()
    {
        AssertAreEquivalent(
            OneToTen.UnionBy([2, 11, 15], x => x % 2).ToArray(),
            OneToTen.AsSpanQuery().UnionBy([2, 11, 15], x => x % 2).ToArray()
            );

        // results are distinct
        var duplicates = OneToTen.Concat(OneToTen).ToArray();
        AssertAreEquivalent(
            duplicates.UnionBy([2, 11, 15], x => x * 2).ToArray(),
            duplicates.AsSpanQuery().UnionBy([2, 11, 15], x => x * 2).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.UnionBy([2, 11, 15], x => x % 2).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().UnionBy([2, 11, 15], x => x % 2).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Where()
    {
        AssertAreEquivalent(
            OneToTen.Where(x => x > 5).ToArray(),
            OneToTen.AsSpanQuery().Where(x => x > 5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Where(x => x > 5).Select((x, i) => i).ToArray(),
            OneToTen.AsSpanQuery().Where(x => x > 5).Select((x, i) => i).ToArray()
            );
    }

    [TestMethod]
    public void Test_Where_Index()
    {
        AssertAreEquivalent(
            OneToTen.Where((x, i) => i > 5).ToArray(),
            OneToTen.AsSpanQuery().Where((x, i) => i > 5).ToArray()
            );

        // post op indices
        AssertAreEquivalent(
            OneToTen.Where((x, i) => i > 5).ToArray(),
            OneToTen.AsSpanQuery().Where((x, i) => i > 5).ToArray()
            );
    }
}