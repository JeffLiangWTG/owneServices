using System;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class DocWrapperUtilitiesTest : TestCase
{
	public void TestToStringRounded() => CombineAssertions(() =>
	{
		AssertEquals("4 -> 4,00", "4,00", DocWrapperUtilities.ToStringRounded(4, 2));
		AssertEquals("4.562 -> 4,56", "4,56", DocWrapperUtilities.ToStringRounded(4.562, 2));
		AssertEquals("4.562 -> 5", "5", DocWrapperUtilities.ToStringRounded(4.562, 0));
		AssertExceptionThrown<ArgumentException>("When numberOfDecimalPlaces is negative", () => DocWrapperUtilities.ToStringRounded(4.562, -1));
	});
}
