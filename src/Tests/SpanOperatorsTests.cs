using Linq2Span;

namespace Tests;

[TestClass]
public class SpanOperatorsTests : TestBase
{
    protected static int[] OneToTen = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    protected static long[] OneToTen_long = OneToTen.Select(x => (long)x).ToArray();
    protected static double[] OneToTen_double = OneToTen.Select(x => (double)x).ToArray();
    protected static float[] OneToTen_float = OneToTen.Select(x => (float)x).ToArray();
    protected static decimal[] OneToTen_decimal = OneToTen.Select(x => (decimal)x).ToArray();

    [TestMethod]
    public void Test_All()
    {
        Assert.IsTrue(OneToTen.AsSpan().All(x => x > 0));
        Assert.IsFalse(OneToTen.AsSpan().All(x => x > 1));
    }

    [TestMethod]
    public void Test_Aggregate()
    {
        Assert.AreEqual(55, OneToTen.AsSpan().Aggregate(0, (total, value) => total + value));
        Assert.AreEqual(3628800, OneToTen.AsSpan().Aggregate(1, (total, value) => total * value));
    }

    [TestMethod]
    public void Test_Any()
    {
        int[] none = [];
        Assert.IsTrue(OneToTen.AsSpan().Any());
    }

    [TestMethod]
    public void Test_Any_Predicate()
    {
        Assert.IsTrue(OneToTen.AsSpan().Any(x => x > 0));
        Assert.IsFalse(OneToTen.AsSpan().Any(x => x < 0));
    }

    [TestMethod]
    public void Test_Average()
    {
        Assert.AreEqual(5.5, OneToTen_double.AsSpan().Average());
    }

    [TestMethod]
    public void Test_Contains()
    {
        Assert.IsTrue(OneToTen.AsSpan().Contains(10));
        Assert.IsTrue(OneToTen.AsSpan().Contains(1));
        Assert.IsFalse(OneToTen.AsSpan().Contains(0));
    }

    [TestMethod]
    public void Test_Count()
    {
        Assert.AreEqual(10, OneToTen.AsSpan().Count());
    }

    [TestMethod]
    public void Test_Count_Predicate()
    {
        Assert.AreEqual(5, OneToTen.AsSpan().Count(x => x > 5));
    }

    [TestMethod]
    public void Test_ElementAt()
    {
        Assert.AreEqual(3, OneToTen.AsSpan().ElementAt(2));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAt(0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpan().ElementAt(10));
    }

    [TestMethod]
    public void Test_ElementAt_Index()
    {
        Assert.AreEqual(3, OneToTen.AsSpan().ElementAt((Index)2));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAt((Index)0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpan().ElementAt((Index)10));
        Assert.AreEqual(10, OneToTen.AsSpan().ElementAt(^1));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAt(^10));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpan().ElementAt(^0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => OneToTen.AsSpan().ElementAt(^11));
    }

    [TestMethod]
    public void Test_ElementAtOrDefault()
    {
        Assert.AreEqual(3, OneToTen.AsSpan().ElementAtOrDefault(2));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAtOrDefault(0));
        Assert.AreEqual(0, OneToTen.AsSpan().ElementAtOrDefault(10));
    }

    [TestMethod]
    public void Test_ElementAtOrDefault_Index()
    {
        Assert.AreEqual(3, OneToTen.AsSpan().ElementAtOrDefault((Index)2));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAtOrDefault((Index)0));
        Assert.AreEqual(0, OneToTen.AsSpan().ElementAtOrDefault((Index)10));
        Assert.AreEqual(10, OneToTen.AsSpan().ElementAtOrDefault(^1));
        Assert.AreEqual(1, OneToTen.AsSpan().ElementAtOrDefault(^10));
        Assert.AreEqual(0, OneToTen.AsSpan().ElementAtOrDefault(^0));
        Assert.AreEqual(0, OneToTen.AsSpan().ElementAtOrDefault(^11));
    }

    [TestMethod]
    public void Test_First()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpan().First(), 1);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpan().First());
    }

    [TestMethod]
    public void Test_First_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpan().First(x => x > 5), 6);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().First(x => x > 10));
    }

    [TestMethod]
    public void Test_FirstOrDefault()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(), 1);
        Assert.AreEqual(none.AsSpan().FirstOrDefault(), 0);
    }

    [TestMethod]
    public void Test_FirstOrDefault_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(x => x > 5), 6);
        Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(x => x > 10), 0);
    }

    [TestMethod]
    public void Test_Last()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpan().Last(), 10);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpan().Last());
    }

    [TestMethod]
    public void Test_LastOrDefault()
    {
        int[] none = [];
        Assert.AreEqual(OneToTen.AsSpan().LastOrDefault(), 10);
        Assert.AreEqual(none.AsSpan().LastOrDefault(), 0);
    }

    [TestMethod]
    public void Test_Max()
    {
        Assert.AreEqual(10, OneToTen.AsSpan().Max());
    }

    [TestMethod]
    public void Test_Max_Selector()
    {
        Assert.AreEqual(20, OneToTen.AsSpan().Max(x => x * 2));
    }

    [TestMethod]
    public void Test_MaxBy()
    {
        Assert.AreEqual(1, OneToTen.AsSpan().MaxBy(x => -x));
    }

    [TestMethod]
    public void Test_Min()
    {
        Assert.AreEqual(1, OneToTen.AsSpan().Min());
    }

    [TestMethod]
    public void Test_Min_Selector()
    {
        Assert.AreEqual(2, OneToTen.AsSpan().Min(x => x * 2));
    }

    [TestMethod]
    public void Test_MinBy()
    {
        Assert.AreEqual(10, OneToTen.AsSpan().MinBy(x => -x));
    }

    [TestMethod]
    public void Test_Single()
    {
        int[] one = [1];
        int[] none = [];
        int[] two = [1, 2];
        Assert.AreEqual(one.AsSpan().Single(), 1);
        Assert.ThrowsException<InvalidOperationException>(() => none.AsSpan().Single());
        Assert.ThrowsException<InvalidOperationException>(() => two.AsSpan().Single());
    }

    [TestMethod]
    public void Test_Single_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpan().Single(x => x > 9), 10);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().Single(x => x > 10));
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().Single(x => x > 8));
    }

    [TestMethod]
    public void Test_SingleOrDefault()
    {
        int[] one = [1];
        int[] none = [];
        int[] two = [1, 2];
        Assert.AreEqual(one.AsSpan().SingleOrDefault(), 1);
        Assert.AreEqual(none.AsSpan().SingleOrDefault(), 0);
        Assert.ThrowsException<InvalidOperationException>(() => two.AsSpan().SingleOrDefault());
    }

    [TestMethod]
    public void Test_SingleOrDefault_Predicate()
    {
        Assert.AreEqual(OneToTen.AsSpan().SingleOrDefault(x => x > 9), 10);
        Assert.AreEqual(OneToTen.AsSpan().SingleOrDefault(x => x > 10), 0);
        Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().SingleOrDefault(x => x > 8));
    }

    [TestMethod]
    public void Test_Sum()
    {
        Assert.AreEqual(55, OneToTen.AsSpan().Sum());
    }

    [TestMethod]
    public void Test_Sum_Selector()
    {
        Assert.AreEqual(110, OneToTen.AsSpan().Sum(x => x * 2));
    }
}