using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CreditTermsCodeListTest : TestCase
	{
		public void TestCreditTermsCodeListCompleteList()
		{
			var creditTermsCodeList = new CreditTermsCodeListCompleteList();
			Assert(creditTermsCodeList.ContainsCode("ADV"));
			Assert(creditTermsCodeList.ContainsCode("NEP"));
			Assert("CreditTermsCodeList - Count", creditTermsCodeList.Count == 1001);
			Assert("CreditTermsCodeList - Missing AddNumberOfDays() method call from constructor", creditTermsCodeList.ContainsCode("1"));
		}
	}
}
