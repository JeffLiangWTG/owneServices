using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CurrencyCodeListTest : TestCase
	{
		public void TestItemCount()
		{
			AssertEquals("Sars EDI User Manual entry count", 17, new CurrencyCodeList().Count);
		}
	}
}
