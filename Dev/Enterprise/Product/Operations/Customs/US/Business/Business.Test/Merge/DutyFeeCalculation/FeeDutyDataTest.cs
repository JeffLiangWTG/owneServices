using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeDutyDataTest : TestCaseWithFactory
	{
		public void TestFeeDutyDataCustomsValue()
		{
			DutyDataTest dutyData = new DutyDataTest();
			dutyData.CustomsValue = 250m;

			var feeDutyData = new FeeDutyData(dutyData, "");
			AssertEquals("CustomsValue", 250m, feeDutyData.CustomsValue);
		}

		public void TestFeeDutyDataSelectedRateType()
		{
			DutyDataTest dutyData = new DutyDataTest();
			dutyData.SelectedRateType = "";

			FeeDutyData feeDutyData = new FeeDutyData(dutyData, "P");
			AssertEquals("Rate type should return the passed rate type", "P", feeDutyData.SelectedRateType);
		}

		public void TestTheRestMembers()
		{
			DutyDataTest dutyData = new DutyDataTest();
			dutyData.Tariff = "1";
			dutyData.DateForDutyCalculation = ZDateTime.BrettsBirthday.Date;
			dutyData.Quantity1 = 2m;
			dutyData.UQ1 = "3";
			dutyData.Quantity2 = 4m;
			dutyData.UQ2 = "5";
			dutyData.Quantity3 = 6m;
			dutyData.UQ3 = "7";
			dutyData.SpecialProgramsIndicatorCountry = "8";
			dutyData.SpecialProgramsIndicatorPrimary = "9";
			dutyData.SpecialProgramsIndicatorSecondary = "10";
			dutyData.CountryOfOrigin = "11";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			dutyData.Factory = factory2;

			dutyData.EntryType = "12";
			dutyData.IsAMSFeeExempt = true;
			dutyData.IsSetVLine = true;
			dutyData.IsCottonFeeExemptIndicated = true;

			DutyDataTest parentTariffDutyData = new DutyDataTest();
			dutyData.ParentTariffLine = parentTariffDutyData;
			dutyData.IsSecondaryTariffLine = true;

			//FeeDutyData should return the corresponding DutyData member which is passed in.
			FeeDutyData feeDutyData = new FeeDutyData(dutyData, "");
			AssertEquals("1", feeDutyData.Tariff);
			AssertEquals(ZDateTime.BrettsBirthday.Date, feeDutyData.DateForDutyCalculation);
			AssertEquals(2m, feeDutyData.Quantity1);
			AssertEquals("3", feeDutyData.UQ1);
			AssertEquals(true, feeDutyData.IsCottonFeeExemptIndicated);

			AssertEquals(4m, feeDutyData.Quantity2);
			AssertEquals("5", feeDutyData.UQ2);

			AssertEquals(6m, feeDutyData.Quantity3);
			AssertEquals("7", feeDutyData.UQ3);

			AssertEquals("8", feeDutyData.SpecialProgramsIndicatorCountry);
			AssertEquals("9", feeDutyData.SpecialProgramsIndicatorPrimary);
			AssertEquals("10", feeDutyData.SpecialProgramsIndicatorSecondary);

			AssertEquals("11", feeDutyData.CountryOfOrigin);
			AssertEquals(factory2, dutyData.Factory);
			AssertEquals("12", feeDutyData.EntryType);
			AssertEquals(true, feeDutyData.IsAMSFeeExempt);
			AssertEquals(true, feeDutyData.IsSetVLine);
			AssertEquals(parentTariffDutyData, feeDutyData.ParentTariffLine);
		}
	}
}
