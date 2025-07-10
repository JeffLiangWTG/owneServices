using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Test
{
	public class PercentageControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestExcludeGSTShouldBeHiddenForCertainCountries()
		{
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = usCompany.PK;

			var ratingHeader = Factory.NewWithValidTestData<CompanyTariff>();
			ratingHeader.TH_GC = usCompany.PK;

			RateEntry entry = ratingHeader.AddRateEntry("AIR");
			RateLine rateLine = entry.AddRateLine(chargeCode.AC_Code);
			rateLine.TL_RateCalculator = PercentageCalculator.Code;

			using (PercentageControl control = new PercentageControl())
			{
				control.SetDataBinding(ratingHeader, "");
				AssertEquals("Exclude GST should not be visible for US company", false, control.IncludeTaxCheckbox.Visible);
			}
		}

		public void TestCheckboxesAlignedAtTheBottom()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;

			var ratingHeader = Factory.NewWithValidTestData<CompanyTariff>();
			ratingHeader.TH_GC = company.PK;

			RateEntry entry = ratingHeader.AddRateEntry("AIR");
			RateLine rateLine = entry.AddRateLine(chargeCode.AC_Code);
			rateLine.TL_RateCalculator = PercentageCalculator.Code;

			using (PercentageControl control = new PercentageControl())
			{
				control.SetDataBinding(ratingHeader, "");
				AssertEquals(control.PartThereofCheckBox.Location.Y, control.GreaterChargeCheckbox.Location.Y);
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestExcludeGSTShouldBeHiddenForCertainCountries_BulkRateUpdate()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			BulkRateUpdater updater = new BulkRateUpdater();
			updater.ActionsLine.TL_RateCalculator = PercentageCalculator.Code;

			using (PercentageControl control = new PercentageControl())
			{
				control.SetDataBinding(updater, "");
			}
		}
	}
}
