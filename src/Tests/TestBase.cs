﻿using Linq2Span;
using System.Reflection;

namespace Tests
{
    [TestClass]
    public class TestBase
    {
        protected void AssertAreEquivalent<TValue>(
            TValue[] expectedValues,
            TValue[] actualValues
            )
        {
            AssertAreEquivalent((object)expectedValues, (object)actualValues);
        }

        public static void AssertAreEquivalent(object? expected, object? actual, string path = "")
        {
            if (expected == actual)
                return;

            if (expected == null && actual != null)
            {
                Assert.Fail($"{path}: expected: {expected} actual: {actual}");
            }
            else if (expected != null && actual == null)
            {
                Assert.Fail($"{path}: expected: {expected} actual: {actual}");
            }

            var expectedType = expected!.GetType();
            var actualType = actual!.GetType();

            if (expectedType != actualType)
            {
                Assert.Fail($"{path}: Type expected: {expectedType.Name} actual: {actualType.Name}");
            }

            if (expectedType.IsAssignableTo(typeof(IEquatable<>).MakeGenericType(expectedType)))
            {
                if (!object.Equals(expected, actual))
                {
                    Assert.Fail($"{path}: expected: {expected} actual: {actual}");
                }
            }
            else if (expectedType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
            {
                var expectedList = ((System.Collections.IEnumerable)expected).OfType<object>().ToList();
                var actualList = ((System.Collections.IEnumerable)actual).OfType<object>().ToList();

                if (actualList.Count != expectedList.Count)
                {
                    Assert.Fail($"{path}: count expected: {expectedList.Count} actual: {actualList.Count}");
                }

                for (int i = 0; i < expectedList.Count; i++)
                {
                    var expectedItem = expectedList[i];
                    var actualItem = actualList[i];
                    AssertAreEquivalent(expectedItem, actualItem, $"{path}[{i}]");
                }
            }
            else
            {
                var props = expectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (var prop in props)
                {
                    if (prop.GetIndexParameters().Length == 0)
                    {
                        var expectedPropValue = prop.GetValue(expected);
                        var actualPropValue = prop.GetValue(actual);
                        AssertAreEquivalent(expectedPropValue, actualPropValue, $"{path}.{prop.Name}");
                    }
                }
            }
        }
    }
}