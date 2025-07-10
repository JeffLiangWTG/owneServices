using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFactMessagingExtensions))]
sealed class EDIFactMessagingExtensionsTest : TestCase
{
	public void TestToNorwegianAmountString() => CombineAssertions(() =>
	{
		AssertEquals("When Zero", "0", new ZDecimal(0m).ToNorwegianAmountString());
		AssertEquals("When Value with more than 3 decimals", "132,342", new ZDecimal(132.3422454m).ToNorwegianAmountString());
		AssertEquals("When Value with no decimal places", "4333", new ZDecimal(4333m).ToNorwegianAmountString());
	});
}
