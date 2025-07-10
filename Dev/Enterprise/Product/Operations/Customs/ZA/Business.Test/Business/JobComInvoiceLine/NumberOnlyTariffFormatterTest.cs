using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class NumberOnlyTariffFormatterTest : TestCase
	{
		public void TestFormat()
		{
			var formatter = new NumberOnlyTariffFormatter();
			AssertEquals("3901100", formatter.Format("3901 100"));
			AssertEquals("390190103", formatter.Format("3901.90.10 3"));
		}
	}
}
