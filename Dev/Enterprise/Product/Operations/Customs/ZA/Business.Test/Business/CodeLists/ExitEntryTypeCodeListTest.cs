using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ExitEntryTypeCodeListTest : TestCase
	{
		public void TestItemCount()
		{
			AssertEquals("Sars EDI User Manual entry count", 5, new ExitEntryTypeCodeList().Count);
		}
	}
}
