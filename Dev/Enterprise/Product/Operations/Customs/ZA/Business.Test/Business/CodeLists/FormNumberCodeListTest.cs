using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class FormNumberCodeListTest : TestCase
	{
		public void TestItemCount()
		{
			AssertEquals("Sars EDI User Manual entry count", 8, new FormNumberCodeList().Count);
		}
	}
}
