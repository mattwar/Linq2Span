using Linq2Span;

namespace Tests;

[TestClass]
public class CombinedTests : TestBase
{
    protected static int[] OneToTen = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    protected static long[] OneToTen_long = OneToTen.Select(x => (long)x).ToArray();
    protected static double[] OneToTen_double = OneToTen.Select(x => (double)x).ToArray();
    protected static float[] OneToTen_float = OneToTen.Select(x => (float)x).ToArray();
    protected static decimal[] OneToTen_decimal = OneToTen.Select(x => (decimal)x).ToArray();

    [TestMethod]
    public void Test_SkipTake()
    {
        AssertAreEquivalent(
            OneToTen.Skip(2).Take(2).ToArray(),
            OneToTen.AsSpanQuery().Skip(2).Take(2).ToArray()
            );
    }

    [TestMethod]
    public void Test_EnumerateTwice()
    {
        var query = OneToTen.AsSpanQuery().Skip(2).Take(2);
        var expected = OneToTen.Skip(2).Take(2).ToArray();

        AssertAreEquivalent(
            expected,
            query.ToArray()
            );

        AssertAreEquivalent(
            expected,
            query.ToArray()
            );
    }
}

