using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class DutyCalculationHelperTest : TestCaseWithFactory
{
	public void TestHasCUDExchangeRateOrSpecifiedDateIsInvalid()
	{
		PopulateExchangeRateData();

		CombineAssertions(() =>
		{
			var dateForDuty = ZDateTime.Invalid;
			AssertEquals("DateForDuty is invalid", true, DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Factory, dateForDuty));

			dateForDuty = ZDateTime.Empty;
			AssertEquals("DateForDuty is empty", true, DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Factory, dateForDuty));

			dateForDuty = ZDateTime.Today;
			AssertEquals("Exchange rate CUD-EUR for Today exists", true, DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Factory, dateForDuty));

			dateForDuty = ZDateTime.Today.AddDays(-1);
			AssertEquals("Exchange rate CUD-EUR for Yesterday does not exist", false, DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Factory, dateForDuty));

			dateForDuty = ZDateTime.Today.AddDays(1);
			AssertEquals("Exchange rate CUD-EUR for Tomorrow does not exist", false, DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Factory, dateForDuty));
		});
	}

	void PopulateExchangeRateData()
	{
		RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today;
		exchangeRate.RE_ExpiryDate = ZDateTime.Today;
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 3.3333m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}
}
