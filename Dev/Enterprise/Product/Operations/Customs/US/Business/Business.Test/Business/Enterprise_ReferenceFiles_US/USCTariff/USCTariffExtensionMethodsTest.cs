using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCTariffExtensionMethodsTest : TestCaseWithFactory
	{
		[TestDate(2010, 02, 24)]
		public void TestGetWineOrDistilledSpiritTaxRate()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("2204100060", ZDateTime.Today);
			AssertEquals("Primary rate", "89.8178c/L", tariff.GetTaxFeeRateDescription("017", RateTypeList.Codes.Primary));
			AssertEquals("With a wrong code", "", tariff.GetTaxFeeRateDescription("016", RateTypeList.Codes.Primary));
			AssertEquals("Secondary rate", "87.1761c/L", tariff.GetTaxFeeRateDescription("017", RateTypeList.Codes.Secondary));
			AssertEquals("invalid code passed", "Select Tax Rate Type", tariff.GetTaxFeeRateDescription("017", "Y"));
		}

		[TestDate(2011, 08, 01)]
		public void TestGetDairyFeeRate()
		{
			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("0401100000", ZDateTime.Today);
			tariff.SetUpTestDataForDairyFeeWith2ComputationCode();
			AssertEquals("1.327c/CKG", tariff.GetTaxFeeRateDescription(Core.Constants.USCustoms.FeeCodes.DairyFee, ZString.Empty));
		}

		[TestDate(2009, 1, 1)]
		public void TestGetTaxFeeRateDescriptionFor2403102050()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("2403102050", ZDateTime.Today);
			AssertNotNull(tariff);
			AssertEquals("$2.41822574/KG", tariff.GetTaxFeeRateDescription("018", ZString.Empty));
		}

		public void TestGetReconTariffRequiredFeeCodes()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6205202067";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			var dutyRate_056 = tariff.DutyRates.AddNew();
			dutyRate_056.UD_TaxFeeFlag = "1";
			dutyRate_056.UD_TaxFeeClassCode = "056";
			var dutyRate_017 = tariff.DutyRates.AddNew();
			dutyRate_017.UD_TaxFeeFlag = "2";
			dutyRate_017.UD_TaxFeeClassCode = "017";
			var dutyRate_090 = tariff.DutyRates.AddNew();
			dutyRate_090.UD_TaxFeeFlag = "1";
			dutyRate_090.UD_TaxFeeClassCode = "090";

			var requiredCodes = tariff.GetReconTariffRequiredFeeCodes(false, false).ToList();
			AssertEquals(2, requiredCodes.Count);
			AssertEquals("056", requiredCodes[0]);
			AssertEquals("090", requiredCodes[1]);

			requiredCodes = tariff.GetReconTariffRequiredFeeCodes(false, true).ToList();
			AssertEquals(2, requiredCodes.Count);
			AssertEquals("056", requiredCodes[0]);
			AssertEquals("090", requiredCodes[1]);

			requiredCodes = tariff.GetReconTariffRequiredFeeCodes(true, false).ToList();
			AssertEquals(1, requiredCodes.Count);
			AssertEquals("090", requiredCodes[0]);

			requiredCodes = tariff.GetReconTariffRequiredFeeCodes(true, true).ToList();
			AssertEquals(2, requiredCodes.Count);
			AssertEquals("056", requiredCodes[0]);
			AssertEquals("090", requiredCodes[1]);
		}
	}
}
