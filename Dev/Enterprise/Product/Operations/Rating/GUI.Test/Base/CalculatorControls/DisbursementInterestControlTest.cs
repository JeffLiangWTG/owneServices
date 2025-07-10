using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Test
{
	public class DisbursementInterestControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestDisbursementInterestControl()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var ratingHeader = Factory.NewWithValidTestData<CompanyTariff>();
			RateEntry entry = ratingHeader.AddRateEntry("AIR");
			RateLine rateLine = entry.AddRateLine(chargeCode.AC_Code);
			rateLine.TL_RateCalculator = DisbursementInterestCalculator.Code;

			using (var control = new DisbursementInterestControl())
			{
				control.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				control.SetDataBinding(ratingHeader, "");
				control.BindTo = "AIRRateEntriesForBinding.RateLines.ViewCalculator";
				AssertEquals("AIRRateEntriesForBinding.RateLines.ViewCalculator+ApplyToRateLineItems", control.ApplyToRateLineItemsGrid.BindTo);
			}
		}
	}
}
