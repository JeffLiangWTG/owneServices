using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ZStringExtensions))]
sealed class ZStringExtensionsTest : TestCase
{
	public void TestSubStringSafeWithStartAndEndIndex() => CombineAssertions(() =>
	{
		ZString value = "SomeValueForTesting";
		AssertEquals("When Start and End Index are within Length of String", "omeVa", value.SubStringSafeWithStartAndEndIndex(1, 5));
		AssertEquals("When Start is less than length and End Index is more than Length of String", "rTesting", value.SubStringSafeWithStartAndEndIndex(11, 30));
		AssertEquals("When Start is greater than length and End Index is more than Length of String", ZString.Empty, value.SubStringSafeWithStartAndEndIndex(40, 45));
		AssertExceptionThrown<ArgumentException>("When Start is negative", () => value.SubStringSafeWithStartAndEndIndex(-1, 10));
		AssertExceptionThrown<ArgumentException>("When End is negative", () => value.SubStringSafeWithStartAndEndIndex(1, -10));
		AssertExceptionThrown<ArgumentException>("When Start is greater than end", () => value.SubStringSafeWithStartAndEndIndex(11, 10));
	});
}
