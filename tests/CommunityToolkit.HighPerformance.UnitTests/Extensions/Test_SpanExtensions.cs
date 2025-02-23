// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.HighPerformance.UnitTests.Extensions;

[TestClass]
public class Test_SpanExtensions
{
    [TestMethod]
    public void Test_SpanExtensions_DangerousGetReference()
    {
        Span<int> data = new[] { 1, 2, 3, 4, 5, 6, 7 };

        ref int r0 = ref Unsafe.AsRef(in data.DangerousGetReference());
        ref int r1 = ref Unsafe.AsRef(in data[0]);

        Assert.IsTrue(Unsafe.AreSame(ref r0, ref r1));
    }

    [TestMethod]
    public void Test_SpanExtensions_DangerousGetReferenceAt_Zero()
    {
        Span<int> data = new[] { 1, 2, 3, 4, 5, 6, 7 };

        ref int r0 = ref Unsafe.AsRef(in data.DangerousGetReference());
        ref int r1 = ref Unsafe.AsRef(in data.DangerousGetReferenceAt(0));

        Assert.IsTrue(Unsafe.AreSame(ref r0, ref r1));
    }

    [TestMethod]
    public void Test_SpanExtensions_DangerousGetReferenceAt_Index()
    {
        Span<int> data = new[] { 1, 2, 3, 4, 5, 6, 7 };

        ref int r0 = ref Unsafe.AsRef(in data.DangerousGetReferenceAt(5));
        ref int r1 = ref Unsafe.AsRef(in data[5]);

        Assert.IsTrue(Unsafe.AreSame(ref r0, ref r1));
    }

    [TestMethod]
    public void Test_SpanExtensions_IndexOf_Empty()
    {
        static void Test<T>()
        {
            T? a = default;

            int indexOfA = default(Span<T?>).IndexOf(ref a);

            Assert.AreEqual(-1, indexOfA);

            Span<T?> data = new T?[] { default };

            int indexOfData = data.Slice(1).IndexOf(ref data[0]);

            Assert.AreEqual(-1, indexOfData);
        }

        Test<byte>();
        Test<int>();
        Test<Guid>();
        Test<string>();
        Test<object>();
        Test<char>();
        Test<IEnumerable<int>>();
    }

    [TestMethod]
    public void Test_SpanExtensions_IndexOf_NotEmpty()
    {
        static void Test<T>()
        {
            Span<T?> data = new T?[] { default, default, default, default };

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(i, data.IndexOf(ref data[i]));
            }
        }

        Test<byte>();
        Test<int>();
        Test<Guid>();
        Test<string>();
        Test<object>();
        Test<char>();
        Test<IEnumerable<int>>();
    }

    [TestMethod]
    public void Test_SpanExtensions_IndexOf_NotEmpty_OutOfRange()
    {
        static void Test<T>()
        {
            // Before start
            Span<T?> data = new T?[] { default, default, default, default };

            int index = data.Slice(1).IndexOf(ref data[0]);

            Assert.AreEqual(-1, index);

            // After end
            data = new T?[] { default, default, default, default };

            index = data.Slice(0, 2).IndexOf(ref data[2]);

            Assert.AreEqual(-1, index);

            // Local variable
            T?[]? dummy = new T?[] { default };

            data = new T?[] { default, default, default, default };

            index = data.IndexOf(ref dummy[0]);

            Assert.AreEqual(-1, index);
        }

        Test<byte>();
        Test<int>();
        Test<Guid>();
        Test<string>();
        Test<object>();
        Test<char>();
        Test<IEnumerable<int>>();
    }

    [TestMethod]
    public void Test_SpanExtensions_Enumerate()
    {
        Span<int> data = new[] { 1, 2, 3, 4, 5, 6, 7 };

        int i = 0;

        foreach (HighPerformance.Enumerables.SpanEnumerable<int>.Item item in data.Enumerate())
        {
            Assert.IsTrue(Unsafe.AreSame(ref data[i], ref item.Value));
            Assert.AreEqual(i, item.Index);

            i++;
        }
    }

    [TestMethod]
    public void Test_SpanExtensions_Enumerate_Empty()
    {
        Span<int> data = Array.Empty<int>();

        foreach (HighPerformance.Enumerables.SpanEnumerable<int>.Item _ in data.Enumerate())
        {
            Assert.Fail("Empty source sequence");
        }
    }

    [TestMethod]
    public void Test_SpanExtensions_CopyTo_RefEnumerable()
    {
        int[,] array = new int[4, 5];

        int[] values1 = { 10, 20, 30, 40, 50 };
        int[] values2 = { 11, 22, 33, 44, 55 };

        // Copy a span to a target row and column with valid lengths
        values1.AsSpan().CopyTo(array.GetRow(0));
        values2.AsSpan(0, 4).CopyTo(array.GetColumn(1));

        int[,] result =
        {
            { 10, 11, 30, 40, 50 },
            { 0, 22, 0, 0, 0 },
            { 0, 33, 0, 0, 0 },
            { 0, 44, 0, 0, 0 }
        };

        CollectionAssert.AreEqual(array, result);

        // Try to copy to a valid row and an invalid column (too short for the source span)
        bool shouldBeTrue = values1.AsSpan().TryCopyTo(array.GetRow(2));
        bool shouldBeFalse = values2.AsSpan().TryCopyTo(array.GetColumn(3));

        Assert.IsTrue(shouldBeTrue);
        Assert.IsFalse(shouldBeFalse);

        result = new[,]
        {
            { 10, 11, 30, 40, 50 },
            { 0, 22, 0, 0, 0 },
            { 10, 20, 30, 40, 50 },
            { 0, 44, 0, 0, 0 }
        };

        CollectionAssert.AreEqual(array, result);
    }
}
