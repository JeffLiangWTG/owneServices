using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class CompanyCurrencyChangeLogHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrentCompanyCurrencyChangeLog()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			var currencyChangeLog = CompanyCurrencyChangeLogHelper.GetCurrentCompanyCurrencyChangeLog(Factory);
			var expectLog = "Current Company[EDI] Currency Code Change Log: No local currency change log found.";
			AssertEquals(expectLog, currencyChangeLog);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";
			GlbCompany.CurrentCompany.Factory.Save();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			GlbCompany.CurrentCompany.Factory.Save();

			currencyChangeLog = CompanyCurrencyChangeLogHelper.GetCurrentCompanyCurrencyChangeLog(Factory);
			expectLog = "Current Company[EDI] Currency Code Change Log: Company Currency: JPY to AUD.";
			AssertEquals(expectLog, currencyChangeLog);
		}
	}
}
