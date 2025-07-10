using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;
using UnversalTest = Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_HarmonisedTariff()
		{
			foreach (var billStatus in new[] { BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF
					, BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF
					, BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond
					, BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF })
			{
				Commodity.Bill.B0_BillStatus = billStatus;
				Commodity.BY_HarmonisedTariff = "1212";
				Commodity.BY_HarmonisedTariff = ZString.Empty;
				AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			}
			Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.GoodsAstrayRail;
			Commodity.BY_HarmonisedTariff = "1212";
			Commodity.BY_HarmonisedTariff = ZString.Empty;
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBY_HarmonisedTariffMandatoryWhenAMSHBR()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill;

				Commodity.BY_HarmonisedTariff = ZString.Empty;
				Commodity.BY_GrossWeight = 0m;
				Commodity.BY_GrossWeightUnit = ZString.Empty;
				Commodity.BY_MonetaryValue = 0m;

				AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				Commodity.BY_HarmonisedTariff = "123456";
				Commodity.Validation.ValidateAll();

				AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			}
			Commodity.BY_HarmonisedTariff = ZString.Empty;
			Commodity.Validation.ValidateAll();
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBY_MonetaryValueWhenAMSHBR()
		{
			var testHelper = new UnversalTest.UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusTaxOrFeeType("OTH", "OTH");
			testHelper.CreateTaxOrFee("DEM", 800.00, "US", 0, 0, "OTH", new ZDateTime("1995-01-01"), new ZDateTime("2060-12-31"), "Deminimus");
			Factory.Save();

			var msg = "Value is greater than the current de minimus value allowed ($800)"; // Current DEM value is $800
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill;
				Commodity.BY_HarmonisedTariff = ZString.Empty;
				Commodity.BY_GrossWeight = 0m;
				Commodity.BY_GrossWeightUnit = ZString.Empty;
				Commodity.BY_MonetaryValue = 0m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);
				Commodity.BY_MonetaryValue = 1000m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertHasMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);

				Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321HouseBill;
				Commodity.BY_MonetaryValue = 0m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);
				Commodity.BY_MonetaryValue = 1000m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertHasMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);

				Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond;
				Commodity.BY_MonetaryValue = 0m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);
				Commodity.BY_MonetaryValue = 1000m;
				commodity.Validation.ValidateBY_MonetaryValue();
				AssertHasMessageErrorContaining(Commodity.BY_MonetaryValueInfo, msg);
			}
		}

		public void TestCheckBY_MarksAndNumbers()
		{
			Commodity.Bill.ValidationModes = ValidationModes.InventoryRecord;
			Commodity.BY_MarksAndNumbers = "MARKS";
			AssertNoMessageErrorContaining(Commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			Commodity.BY_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(Commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ValidationModes = ValidationModes.ChangeEstDateOfArrival;
			Commodity.BY_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining(Commodity.BY_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			var messgae = "Marks And Numbers : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			bill.ValidationModes = ValidationModes.InventoryRecord;
			Commodity.BY_MarksAndNumbers = "MARKS****";
			AssertHasWarningContaining(Commodity.BY_MarksAndNumbersInfo, messgae);
			Commodity.BY_MarksAndNumbers = "MARKS";
			AssertNoWarningContaining(Commodity.BY_MarksAndNumbersInfo, messgae);
		}

		public void TestCheckBY_Description()
		{
			Commodity.Bill.ValidationModes = ValidationModes.InventoryRecord;
			Commodity.BY_Description = "GOODS";
			AssertNoMessageErrorContaining(Commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			Commodity.BY_Description = ZString.Empty;
			AssertHasMessageErrorContaining(Commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ValidationModes = ValidationModes.VesselDeparture;
			Commodity.BY_Description = ZString.Empty;
			AssertNoMessageErrorContaining(Commodity.BY_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			var messgae = "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			bill.ValidationModes = ValidationModes.InventoryRecord;
			Commodity.BY_Description = "GOODS***";
			AssertHasWarningContaining(Commodity.BY_DescriptionInfo, messgae);
			Commodity.BY_Description = "GOODS";
			AssertNoWarningContaining(Commodity.BY_DescriptionInfo, messgae);
		}

		public void TestCheckBY_ManifestUnitCode()
		{
			Commodity.BY_ManifestUnitCode = "PLT";
			AssertNoMessageErrorContaining(Commodity.BY_ManifestUnitCodeInfo, "The code you have selected is not in the list.");
			Commodity.BY_ManifestUnitCode = "XXX";
			AssertHasMessageErrorContaining(Commodity.BY_ManifestUnitCodeInfo, "The code you have selected is not in the list.");

			Commodity.BY_ManifestUnitCode = ZString.Empty;
			AssertHasMessageErrorContaining(Commodity.BY_ManifestUnitCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestD00_CheckBY_FormattedHarmonisedTariff()
		{
			var version = USCDataVersion.GetLastHTSAttempt(Factory);
			version.UZ_Version = 1403;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "11111010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();
			Commodity.BY_HarmonisedTariff = ZString.Empty;

			Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard;
			AssertEquals("IsTariffRequired is false", false, commodity.Bill.IsTariffRequired);
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond;
			AssertEquals("IsTariffRequired is true", true, commodity.Bill.IsTariffRequired);
			Commodity.Validation.ValidateBY_HarmonisedTariff();
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.BY_HarmonisedTariff = "123456";
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.BY_HarmonisedTariff = "";
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2014, 04, 03)]
		public void TestD00FieldsValidation()
		{
			var version = USCDataVersion.GetLastHTSAttempt(Factory);
			version.UZ_Version = 1403;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "11111010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();

			Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF;

			Commodity.BY_HarmonisedTariff = ZString.Empty;
			Commodity.BY_GrossWeight = 0m;
			Commodity.BY_GrossWeightUnit = ZString.Empty;
			Commodity.BY_MonetaryValue = 0m;

			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.BY_HarmonisedTariff = "123456";
			Commodity.Validation.ValidateAll();

			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, "not recognized");

			Commodity.BY_HarmonisedTariff = "11111012";
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, "not recognized");
			Commodity.BY_HarmonisedTariff = "11111010";
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, "not recognized");

			var inBondMoveHeader = Commodity.Bill.Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			Commodity.BY_GrossWeight = 1m;
			Commodity.BY_HarmonisedTariff = ZString.Empty;
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.BY_GrossWeight = 0m;

			Commodity.Bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF;
			Commodity.Validation.ValidateAll();

			AssertHasMessageErrorContaining(Commodity.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Commodity.BY_MonetaryValueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

			Commodity.BY_HarmonisedTariff = "11111010";
			Commodity.Validation.ValidateAll();
			Commodity.BY_GrossWeight = 1m;
			Commodity.BY_GrossWeightUnit = "KG";
			Commodity.BY_MonetaryValue = 1m;
			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_MonetaryValueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Commodity.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
		}

		CusInBondCargoDesc Commodity
		{
			get
			{
				if (commodity == null)
				{
					header = Factory.New<CusInBondHeader>();
					bill = header.Bills.AddNew();

					var moveDetail = bill.MovementDetail;
					var container = moveDetail.Containers.AddNew();
					commodity = container.Commodities.AddNew();
				}
				return commodity;
			}
		}
		CusInBondCargoDesc commodity;
		CusInBondBill bill;
		CusInBondHeader header;
	}
}
