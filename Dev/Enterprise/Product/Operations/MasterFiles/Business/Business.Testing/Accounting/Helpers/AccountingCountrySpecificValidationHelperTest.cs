using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class AccountingCountrySpecificValidationHelperTest : TestCaseWithFactory
	{
		public void TestIsEmptyPortugalIVA()
		{
			Assert(AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA(string.Empty));
			Assert(AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA("999999990"));
			Assert(AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA("PT999999990"));
			Assert(!AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA("FR999999990"));
			Assert(!AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA("999999991"));
			Assert(!AccountingCountrySpecificValidationHelper.IsEmptyPortugalIVA("PT123456789"));
		}
	}
}
