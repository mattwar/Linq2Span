using Linq2Span;

namespace Tests
{
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
            Assert.IsTrue(OneToTen.AsSpan().All(x => x > 0));
            Assert.IsFalse(OneToTen.AsSpan().All(x => x > 1));

            Assert.IsTrue(OneToTen.AsSpanQuery().All(x => x > 0));
            Assert.IsFalse(OneToTen.AsSpanQuery().All(x => x > 1));
        }

        [TestMethod]
        public void Test_Aggregate()
        {
            Assert.AreEqual(55, OneToTen.AsSpan().Aggregate(0, (total, value) => total + value));
            Assert.AreEqual(3628800, OneToTen.AsSpan().Aggregate(1, (total, value) => total * value));

            Assert.AreEqual(55, OneToTen.AsSpanQuery().Aggregate(0, (total, value) => total + value));
            Assert.AreEqual(3628800, OneToTen.AsSpanQuery().Aggregate(1, (total, value) => total * value));
        }

        [TestMethod]
        public void Test_Any()
        {
            int[] none = [];

            Assert.IsTrue(OneToTen.AsSpan().Any());

            Assert.IsTrue(OneToTen.AsSpanQuery().Any());
        }

        [TestMethod]
        public void Test_Any_Predicate()
        {
            Assert.IsTrue(OneToTen.AsSpan().Any(x => x > 0));
            Assert.IsFalse(OneToTen.AsSpan().Any(x => x < 0));

            Assert.IsTrue(OneToTen.AsSpanQuery().Any(x => x > 0));
            Assert.IsFalse(OneToTen.AsSpanQuery().Any(x => x < 0));
        }

        [TestMethod]
        public void Test_Average()
        {
            Assert.AreEqual(5.5, OneToTen_double.AsSpan().Average());
            Assert.AreEqual(5.5, OneToTen_double.AsSpanQuery().Average());
        }

        [TestMethod]
        public void Test_Cast()
        {
            // not supported on span directly

            AssertAreEquivalent(
                OneToTen.AsSpanQuery().Cast<object>().ToArray(),
                (object)OneToTen.Cast<object>().ToArray()
                );
        }

        [TestMethod]
        public void Test_Contains()
        {
            Assert.IsTrue(OneToTen.AsSpan().Contains(10));
            Assert.IsTrue(OneToTen.AsSpan().Contains(1));
            Assert.IsFalse(OneToTen.AsSpan().Contains(0));

            Assert.IsTrue(OneToTen.AsSpanQuery().Contains(10));
            Assert.IsTrue(OneToTen.AsSpanQuery().Contains(1));
            Assert.IsFalse(OneToTen.AsSpanQuery().Contains(0));
        }

        [TestMethod]
        public void Test_Count()
        {
            Assert.AreEqual(10, OneToTen.AsSpan().Count());
            Assert.AreEqual(5, OneToTen.AsSpan().Count(x => x > 5));
        }

        [TestMethod]
        public void Test_Count_Predicate()
        {
            Assert.AreEqual(10, OneToTen.AsSpanQuery().Count());
            Assert.AreEqual(5, OneToTen.AsSpanQuery().Count(x => x > 5));
        }

        [TestMethod]
        public void Test_Distinct()
        {
            int[] nondistinct = [1, 2, 3, 4, 5, 1, 2, 3, 4, 5];
            string[] nondistinct2 = ["one", "One", "Two", "TWO"];

            AssertAreEquivalent([1, 2, 3, 4, 5], nondistinct.AsSpan().Distinct().ToArray());
            AssertAreEquivalent(["one", "Two"], nondistinct2.AsSpan().Distinct(StringComparer.OrdinalIgnoreCase).ToArray());

            AssertAreEquivalent([1, 2, 3, 4, 5], nondistinct.AsSpanQuery().Distinct().ToArray());
            AssertAreEquivalent(["one", "Two"], nondistinct2.AsSpanQuery().Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        }

        [TestMethod]
        public void Test_DistinctBy()
        {
            string[] nondistinct = ["one", "two", "three", "four", "five"];
            AssertAreEquivalent(["one", "three", "four"], nondistinct.AsSpan().DistinctBy(x => x.Length).ToArray());
            AssertAreEquivalent(["one", "three", "four"], nondistinct.AsSpanQuery().DistinctBy(x => x.Length).ToArray());
        }

        [TestMethod]
        public void Test_First()
        {
            int[] none = [];

            Assert.AreEqual(OneToTen.AsSpan().First(), 1);
            Assert.ThrowsException<InvalidOperationException>(() => none.AsSpan().First());

            Assert.AreEqual(OneToTen.AsSpanQuery().First(), 1);
            Assert.ThrowsException<InvalidOperationException>(() => none.AsSpanQuery().First());
        }

        [TestMethod]
        public void Test_First_Predicate()
        {
            Assert.AreEqual(OneToTen.AsSpan().First(x => x > 5), 6);
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().First(x => x > 10));

            Assert.AreEqual(OneToTen.AsSpanQuery().First(x => x > 5), 6);
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().First(x => x > 10));
        }

        [TestMethod]
        public void Test_FirstOrDefault()
        {
            int[] none = [];

            Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(), 1);
            Assert.AreEqual(none.AsSpan().FirstOrDefault(), 0);

            Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(), 1);
            Assert.AreEqual(none.AsSpanQuery().FirstOrDefault(), 0);
        }

        [TestMethod]
        public void Test_FirstOrDefault_Predicate()
        {
            Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(x => x > 5), 6);
            Assert.AreEqual(OneToTen.AsSpan().FirstOrDefault(x => x > 10), 0);

            Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(x => x > 5), 6);
            Assert.AreEqual(OneToTen.AsSpanQuery().FirstOrDefault(x => x > 10), 0);
        }

        [TestMethod]
        public void Test_GroupBy()
        {
            AssertAreEquivalent(
                OneToTen.GroupBy(x => x / 2).ToArray(),
                OneToTen.AsSpan().GroupBy(x => x / 2).ToArray()
                );

            AssertAreEquivalent(
                OneToTen.GroupBy(x => x / 2).ToArray(),
                OneToTen.AsSpanQuery().GroupBy(x => x / 2).ToArray()
                );
        }

        [TestMethod]
        public void Test_Select()
        {
            AssertAreEquivalent(
                OneToTen.Select(x => x * 2).ToArray(),
                OneToTen.AsSpan().Select(x => x * 2).ToArray()
                );

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
                OneToTen.AsSpan().Select((x, i) => i).ToArray()
                );

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
                OneToTen.AsSpan().SelectMany(x => new[] { x, x }).ToArray()
                );

            AssertAreEquivalent(
                OneToTen.SelectMany(x => new[] { x, x }).ToArray(),
                OneToTen.AsSpanQuery().SelectMany(x => new[] { x, x }).ToArray()
                );
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

            Assert.AreEqual(one.AsSpanQuery().Single(), 1);
            Assert.ThrowsException<InvalidOperationException>(() => none.AsSpanQuery().Single());
            Assert.ThrowsException<InvalidOperationException>(() => two.AsSpanQuery().Single());
        }

        [TestMethod]
        public void Test_Single_Predicate()
        {
            Assert.AreEqual(OneToTen.AsSpan().Single(x => x > 9), 10);
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().Single(x => x > 10));
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().Single(x => x > 8));

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

            Assert.AreEqual(one.AsSpan().SingleOrDefault(), 1);
            Assert.AreEqual(none.AsSpan().SingleOrDefault(), 0);
            Assert.ThrowsException<InvalidOperationException>(() => two.AsSpan().SingleOrDefault());

            Assert.AreEqual(one.AsSpanQuery().SingleOrDefault(), 1);
            Assert.AreEqual(none.AsSpanQuery().SingleOrDefault(), 0);
            Assert.ThrowsException<InvalidOperationException>(() => two.AsSpanQuery().SingleOrDefault());
        }

        [TestMethod]
        public void Test_SingleOrDefault_Predicate()
        {
            Assert.AreEqual(OneToTen.AsSpan().SingleOrDefault(x => x > 9), 10);
            Assert.AreEqual(OneToTen.AsSpan().SingleOrDefault(x => x > 10), 0);
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpan().SingleOrDefault(x => x > 8));

            Assert.AreEqual(OneToTen.AsSpanQuery().SingleOrDefault(x => x > 9), 10);
            Assert.AreEqual(OneToTen.AsSpanQuery().SingleOrDefault(x => x > 10), 0);
            Assert.ThrowsException<InvalidOperationException>(() => OneToTen.AsSpanQuery().SingleOrDefault(x => x > 8));
        }

        [TestMethod]
        public void Test_Sum()
        {
            Assert.AreEqual(55, OneToTen.AsSpan().Sum());

            Assert.AreEqual(55, OneToTen.AsSpanQuery().Sum());
        }

        [TestMethod]
        public void Test_Sum_Selector()
        {
            Assert.AreEqual(110, OneToTen.AsSpan().Sum(x => x * 2));

            Assert.AreEqual(110, OneToTen.AsSpanQuery().Sum(x => x * 2));
        }

        [TestMethod]
        public void Test_Where()
        {
            AssertAreEquivalent([6, 7, 8, 9, 10], OneToTen.AsSpan().Where(x => x > 5).ToArray());

            AssertAreEquivalent([6, 7, 8, 9, 10], OneToTen.AsSpanQuery().Where(x => x > 5).ToArray());
        }

        [TestMethod]
        public void Test_Where_Index()
        {
            AssertAreEquivalent([7, 8, 9, 10], OneToTen.AsSpan().Where((x, i) => i > 5).ToArray());

            AssertAreEquivalent([7, 8, 9, 10], OneToTen.AsSpanQuery().Where((x, i) => i > 5).ToArray());
        }
    }
}