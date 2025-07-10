using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FishingInformationAddInfoValidation))]
	public class FishingInformationAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_MethodOfHarvest()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var fishing = invoiceLine.FishingInformations.AddNew();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(fishing.US_MethodOfHarvestInfo, "??", SourceTypeCodesList.Codes.Vessel);
		}

		public void TestCheckUS_VesselName()
		{
			SetUpTariffData();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestVessel";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301930000";
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			var fishing = invoiceLine.FishingInformations.AddNew();
			AssertNoMessageErrors(fishing.US_VesselNameInfo);
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(fishing.US_VesselNameInfo, "??", "TestVessel");
			fishing.US_VesselName = "TestVessel";
			AssertNoMessageErrors(fishing.US_VesselNameInfo);
		}

		public void TestCheckUS_VesselCountry()
		{
			SetUpTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301930000";
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			var fishing = invoiceLine.FishingInformations.AddNew();
			AssertNoMessageError(fishing.US_VesselCountryInfo, "You have not entered a value.");
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			AssertHasMessageError(fishing.US_VesselCountryInfo, "You have not entered a value.");
			fishing.US_VesselCountry = "CA";
			AssertNoMessageError(fishing.US_VesselCountryInfo, "You have not entered a value.");
		}

		public void TestCheckUS_VesselIMO()
		{
			SetUpTariffData();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestVessel";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301930000";
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			var fishing = invoiceLine.FishingInformations.AddNew();
			AssertNoMessageErrors(fishing.US_VesselIMOInfo);
			fishing.US_MethodOfHarvest = SourceTypeCodesList.Codes.Vessel;
			AssertHasMessageError(fishing.US_VesselIMOInfo, "You have not entered a value.");
			fishing.US_VesselIMO = "TestVessel";
			AssertNoMessageErrors(fishing.US_VesselIMOInfo);
		}

		public void TestCheckUS_HarvestedCountry()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var fishing = invoiceLine.FishingInformations.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(fishing.US_HarvestedCountryInfo, "??", "CA");
		}

		void SetUpTariffData()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0301930000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			helper.CreateCusApplicability(condition, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
