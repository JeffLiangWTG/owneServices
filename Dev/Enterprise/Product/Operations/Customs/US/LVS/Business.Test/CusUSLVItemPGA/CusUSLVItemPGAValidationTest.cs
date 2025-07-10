using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVItemPGAValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckULP_DisclaimReason()
		{
			var item = Factory.New<CusUSLVItem>();
			var pga = item.CusUSLVItemPGAs.AddNew();
			pga.AgencyCode = "TST";
			pga.ULP_DisclaimReason = "T";
			Assert(!pga.ULP_DisclaimReasonInfo.HasNotifications());

			pga.ULP_DisclaimReason = "C";
			AssertHasMessageError(pga.ULP_DisclaimReasonInfo, "The code you have selected is not in the list.");

			pga.ULP_DisclaimReason = "T";
			Assert(!pga.ULP_DisclaimReasonInfo.HasNotifications());
		}

		public void TestCheckULP_DisclaimReason_NotAllowedForTariff()
		{
			var extraTariff = Factory.New<USCTariff>();
			extraTariff.UE_Tariff = "9999999999";
			extraTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			extraTariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var tariffPgaCodesTuples = new[]
			{
				(tariff: "0000000001", pgaCode: "FD2", agencyCode: "FDA"),
				(tariff: "0000000003", pgaCode: "FD4", agencyCode: "FDA"),
				(tariff: "0000000007", pgaCode: "EP4", agencyCode: "VNE"),
				(tariff: "0000000009", pgaCode: "EP2", agencyCode: "ODS"),
				(tariff: "0000000011", pgaCode: "FS4", agencyCode: "FSI"),
				(tariff: "0000000013", pgaCode: "EP6", agencyCode: "PS1"),
				(tariff: "0000000015", pgaCode: "EP8", agencyCode: "TS1"),
				(tariff: "0000000017", pgaCode: "AQ2", agencyCode: "AVS"),
				(tariff: "0000000019", pgaCode: "FW2", agencyCode: "FWS"),
				(tariff: "0000000021", pgaCode: "NM2", agencyCode: "370"),
				(tariff: "0000000023", pgaCode: "NM4", agencyCode: "AMR"),
				(tariff: "0000000025", pgaCode: "NM6", agencyCode: "HMS"),
				(tariff: "0000000026", pgaCode: "NM8", agencyCode: "SIM"),
				(tariff: "0000000028", pgaCode: "DT2", agencyCode: "OFF"),
				(tariff: "0000000030", pgaCode: "AM2", agencyCode: "MO8"),
				(tariff: "0000000032", pgaCode: "AM4", agencyCode: "MO8"),
				(tariff: "0000000033", pgaCode: "AM6", agencyCode: "MO8"),
				(tariff: "0000000034", pgaCode: "AM8", agencyCode: "OR1"),
				(tariff: "0000000035", pgaCode: "OM2", agencyCode: "OMC"),
				(tariff: "0000000037", pgaCode: "TB2", agencyCode: "TOB"),
				(tariff: "0000000040", pgaCode: "CP2", agencyCode: "CPS"),
			};

			foreach (var tuple in tariffPgaCodesTuples)
			{
				var item = SetUpTariff(tuple.tariff, tuple.pgaCode);
				Factory.Save();
				AssertValidation(item, tuple.agencyCode, tuple.tariff, tuple.pgaCode);
			}
		}

		CusUSLVItem SetUpTariff(ZString tariffCode, ZString pgaCodes)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffCode;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = pgaCodes;

			return Factory.NewWithValidTestData<CusUSLVClearance>()
				.CusUSLVConsignments
				.AddNew()
				.CusUSLVItems
				.AddNew();
		}

		void AssertValidation(CusUSLVItem item, ZString program, ZString tariffCode, ZString pgaCode)
		{
			item.ULI_Tariff = tariffCode;
			var pgaWrapper = item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().SingleOrDefault(c => !c.Requirement.IsEmpty);
			pgaWrapper.DisclaimReason = "D";
			pgaWrapper.Indicator = "C";

			var pga = pgaWrapper.PGA;
			pga.Validation.ValidateULP_DisclaimReason();
			AssertHasMessageError("AgencyCode " + program + " shouldn't be disclaimed for pga requirement " + pgaCode, pga.ULP_DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
		}
	}
}
