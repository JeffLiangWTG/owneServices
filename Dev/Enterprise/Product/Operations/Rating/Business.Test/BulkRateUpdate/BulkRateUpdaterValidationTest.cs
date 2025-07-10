using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Test.BulkRateUpdate
{
	public class BulkRateUpdaterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGroup()
		{
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModuleInfo, "Please enter a value.");
			TestUpdater.Module = "FWD";
			AssertNoErrors(TestUpdater.ModuleInfo);
			TestUpdater.Module = "###";
			AssertHasError(TestUpdater.ModuleInfo, "Enter a valid selection.");
			TestUpdater.Module = "SHP";
			AssertNoErrors(TestUpdater.ModuleInfo);
			TestUpdater.Module = "CUS";
			AssertNoErrors(TestUpdater.ModuleInfo);
			TestUpdater.Module = "WRH";
			AssertNoErrors(TestUpdater.ModuleInfo);
		}

		public void TestValidateType()
		{
			TestUpdater.Module = "FWD";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.TypeInfo, "Please enter a value.");
			TestUpdater.Type = "FCL";
			AssertNoErrors(TestUpdater.TypeInfo);
			TestUpdater.Type = "###";
			AssertHasError(TestUpdater.TypeInfo, "Enter a valid selection.");
			TestUpdater.Type = "DST";
			AssertNoErrors(TestUpdater.TypeInfo);

			TestUpdater.Module = "SHP";
			AssertHasError(TestUpdater.TypeInfo, "Enter a valid selection.");
			TestUpdater.Type = "SCO";
			AssertNoErrors(TestUpdater.TypeInfo);

			// Customs
			TestUpdater.Module = "CUS";
			TestUpdater.Type = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.TypeInfo, "Please enter a value.");
			TestUpdater.Type = Category.CFC;
			AssertNoErrors(TestUpdater.TypeInfo);
			TestUpdater.Type = "###";
			AssertHasError(TestUpdater.TypeInfo, "Enter a valid selection.");
			TestUpdater.Type = Category.CDS;
			AssertNoErrors(TestUpdater.TypeInfo);

			TestUpdater.Module = "WRH";
			TestUpdater.Type = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.TypeInfo, "Please enter a value.");
			TestUpdater.Type = Category.WHS;
			AssertNoErrors(TestUpdater.TypeInfo);
			TestUpdater.Type = "###";
			AssertHasError(TestUpdater.TypeInfo, "Enter a valid selection.");
			TestUpdater.Type = Category.TRW;
			AssertNoErrors(TestUpdater.TypeInfo);
			TestUpdater.Type = Category.TWU;
			AssertNoErrors(TestUpdater.TypeInfo);
		}

		public void TestValidateMode()
		{
			AssertEquals(0, TestUpdater.Modes.Count);
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "FCL";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModeInfo, "Please enter a value.");
			TestUpdater.Mode = "SEA";
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = "###";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");
			TestUpdater.Mode = "RAI";
			AssertNoErrors(TestUpdater.ModeInfo);

			TestUpdater.Type = "LCL";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");
			TestUpdater.Mode = "LCL";
			AssertNoErrors(TestUpdater.ModeInfo);

			// Customs
			TestUpdater.Module = "CUS";
			TestUpdater.Type = Category.CFC;
			TestUpdater.Mode = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModeInfo, "Please enter a value.");
			TestUpdater.Mode = Mode.SEA;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = "###";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");
			TestUpdater.Mode = Mode.RAI;
			AssertNoErrors(TestUpdater.ModeInfo);

			TestUpdater.Type = Category.CLC;
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");
			TestUpdater.Mode = Mode.LCL;
			AssertNoErrors(TestUpdater.ModeInfo);

			TestUpdater.Module = "WRH";
			TestUpdater.Type = Category.WHS;
			TestUpdater.Mode = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModeInfo, "Please enter a value.");
			TestUpdater.Mode = Mode.ALL;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = "###";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");

			TestUpdater.Type = Category.TRW;
			TestUpdater.Mode = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModeInfo, "Please enter a value.");
			TestUpdater.Mode = Mode.ALL;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = "###";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");

			TestUpdater.Type = Category.TWU;
			TestUpdater.Mode = "";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.ModeInfo, "Please enter a value.");
			TestUpdater.Mode = Mode.ALL;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = Mode.AIR;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = Mode.SEA;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = Mode.ROA;
			AssertNoErrors(TestUpdater.ModeInfo);
			TestUpdater.Mode = "###";
			AssertHasError(TestUpdater.ModeInfo, "Enter a valid selection.");
		}

		public void TestValidateOrigin()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.OriginInfo);
			TestUpdater.Origin = "AUSYD";
			AssertNoErrors(TestUpdater.OriginInfo);
			TestUpdater.Origin = "#####";
			AssertHasError(TestUpdater.OriginInfo, "Enter a valid selection.");
			TestUpdater.Origin = "US";
			AssertNoErrors(TestUpdater.OriginInfo);
		}

		public void TestValidateDestination()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.DestinationInfo);
			TestUpdater.Destination = "AUEC";
			AssertNoErrors(TestUpdater.DestinationInfo);
			TestUpdater.Destination = "#####";
			AssertHasError(TestUpdater.DestinationInfo, "Enter a valid selection.");
			TestUpdater.Destination = "GBLON";
			AssertNoErrors(TestUpdater.DestinationInfo);
		}

		public void TestValidateServiceLevel()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.ServiceLevelInfo);
			TestUpdater.ServiceLevel = "STD";
			AssertNoErrors(TestUpdater.ServiceLevelInfo);
			TestUpdater.ServiceLevel = "###";
			AssertHasError(TestUpdater.ServiceLevelInfo, "Enter a valid selection.");
			TestUpdater.ServiceLevel = "D2D";
			AssertNoErrors(TestUpdater.ServiceLevelInfo);
		}

		public void TestValidateGatewayServiceLevel()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.GatewayServiceLevelInfo);
			TestUpdater.GatewayServiceLevel = "STD";
			AssertNoErrors(TestUpdater.GatewayServiceLevelInfo);
			TestUpdater.GatewayServiceLevel = "###";
			AssertHasError(TestUpdater.GatewayServiceLevelInfo, "Enter a valid selection.");
			TestUpdater.GatewayServiceLevel = "D2D";
			AssertNoErrors(TestUpdater.GatewayServiceLevelInfo);
		}

		public void TestValidateShipmentGatewayServiceLevel()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = false;
			Factory.Save();

			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.ShipmentGatewayServiceLevelInfo);
			TestUpdater.ShipmentGatewayServiceLevel = "STD";
			AssertNoErrors(TestUpdater.ShipmentGatewayServiceLevelInfo);
			TestUpdater.ShipmentGatewayServiceLevel = "###"; // not exists
			AssertHasError(TestUpdater.ShipmentGatewayServiceLevelInfo, "Enter a valid selection.");
			TestUpdater.ShipmentGatewayServiceLevel = "DIR"; // exists, but it's NOT gateway
			AssertHasError(TestUpdater.ShipmentGatewayServiceLevelInfo, "Enter a valid selection.");
			TestUpdater.ShipmentGatewayServiceLevel = "DEF";
			AssertNoErrors(TestUpdater.ShipmentGatewayServiceLevelInfo);
		}

		public void TestValidateCommodityCode()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.CommodityCodeInfo);
			TestUpdater.CommodityCode = "GEN";
			AssertNoErrors(TestUpdater.CommodityCodeInfo);
			TestUpdater.CommodityCode = "###";
			AssertHasError(TestUpdater.CommodityCodeInfo, "Enter a valid selection.");
			TestUpdater.CommodityCode = "HAZ";
			AssertNoErrors(TestUpdater.CommodityCodeInfo);
		}

		public void TestValidateContainerType()
		{
			TestUpdater.RunPreSaveValidation();

			Assert(!TestUpdater.ContainerTypes.HasErrors());

			var container = TestUpdater.ContainerTypes.AddNew();
			container.RC_Code = "ThisCodeDoesntExist";
			AssertEquals(true, TestUpdater.HasErrors);
			AssertHasError(container.RC_CodeInfo, "Enter a valid Container Code.");
			Assert(TestUpdater.ContainerTypes.HasErrors());

			container.RC_Code = "20GP";
			AssertNoErrors(container.RC_CodeInfo);
			Assert(!TestUpdater.ContainerTypes.HasErrors());
		}

		public void TestValidateSupplier()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.SupplierInfo);
			TestUpdater.Supplier = ZGuid.Invalid;
			AssertHasError(TestUpdater.SupplierInfo, "Enter a valid selection.");
			TestUpdater.Supplier = ZGuid.NewZGuid();
			AssertNoErrors(TestUpdater.SupplierInfo);
			TestUpdater.Supplier = ZGuid.Missing;
			AssertHasError(TestUpdater.SupplierInfo, "The selected selection is no longer valid. Please choose a new selection from the list.");
		}

		public void TestValidateCarrier()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.CarrierInfo);
			TestUpdater.Carrier = ZGuid.Invalid;
			AssertHasError(TestUpdater.CarrierInfo, "Enter a valid selection.");
			TestUpdater.Carrier = ZGuid.NewZGuid();
			AssertNoErrors(TestUpdater.CarrierInfo);
			TestUpdater.Carrier = ZGuid.Missing;
			AssertHasError(TestUpdater.CarrierInfo, "The selected selection is no longer valid. Please choose a new selection from the list.");
		}

		public void TestValidateCarrierServiceLevel()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsAirLine = true;
			carrier1.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			var level = carrier1.MiscServ.CarrierServiceLevels.AddNew();
			level.PL_Code = "MKT";
			level.PL_CarrierServiceLevelDescription = "Market Rate";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			level = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			level.PL_Code = "EXP";
			level.PL_CarrierServiceLevelDescription = "Expensive";

			Factory.Save();

			TestUpdater.CarrierServiceLevel = "STD";
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.CarrierServiceLevelInfo);

			TestUpdater.CarrierServiceLevel = "MKT";
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.CarrierServiceLevelInfo, "Enter a valid selection.");

			TestUpdater.Carrier = carrier1.PK;
			TestUpdater.CarrierServiceLevel = "MKT";
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.CarrierServiceLevelInfo);

			TestUpdater.Carrier = carrier2.PK;
			TestUpdater.CarrierServiceLevel = "EXP";
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.CarrierServiceLevelInfo);
		}

		[TestDate(2010, 10, 10)]
		public void TestValidateStartDate()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.StartDateInfo);

			TestUpdater.StartDate = ZDate.Invalid;
			AssertHasError(TestUpdater.StartDateInfo, "Enter a valid selection.");

			var today = ZDate.Today;
			TestUpdater.StartDate = today;
			AssertNoErrors(TestUpdater.StartDateInfo);

			TestUpdater.StartDate = today.AddYears(-3);
			AssertHasWarning(TestUpdater.StartDateInfo, "The date '10-Oct-2007' is more than 1 year old.");
		}

		[TestDate(2010, 10, 10)]
		public void TestValidateEndDate()
		{
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.EndDateInfo);

			TestUpdater.EndDate = ZDate.Invalid;
			AssertHasError(TestUpdater.EndDateInfo, "Enter a valid selection.");

			var today = ZDate.Today;
			TestUpdater.EndDate = today;
			AssertNoErrors(TestUpdater.EndDateInfo);

			TestUpdater.EndDate = today.AddYears(-3);
			AssertHasWarning(TestUpdater.EndDateInfo, "The date '10-Oct-2007' is more than 1 year old.");

			TestUpdater.StartDate = today;
			TestUpdater.EndDate = today.AddDays(-10);

			AssertHasError(TestUpdater.EndDateInfo, "End Date cannot be earlier than the the Start Date.");
		}

		[TestDate(2016, 02, 10)]
		public void TestValidateNewEntryStartDate()
		{
			TestUpdater.CreateNewEntry = true;
			TestUpdater.RunPreSaveValidation();
			AssertHasError(TestUpdater.NewEntryStartDateInfo, "Please enter a value.");

			TestUpdater.NewEntryStartDate = ZDate.Invalid;
			AssertHasError(TestUpdater.NewEntryStartDateInfo, "Enter a valid selection.");

			var today = ZDate.Today;
			TestUpdater.NewEntryStartDate = today;
			AssertNoErrors(TestUpdater.NewEntryStartDateInfo);

			TestUpdater.NewEntryStartDate = today.AddYears(-3);
			AssertHasWarning(TestUpdater.NewEntryStartDateInfo, "The date '10-Feb-2013' is more than 1 year old.");

			TestUpdater.CreateNewEntry = false;
			TestUpdater.NewEntryStartDate = ZDate.Invalid;
			AssertNoErrors(TestUpdater.NewEntryStartDateInfo);
		}

		[TestDate(2016, 02, 10)]
		public void TestValidateNewEntryEndDate()
		{
			TestUpdater.CreateNewEntry = true;
			TestUpdater.RunPreSaveValidation();
			AssertNoErrors(TestUpdater.NewEntryEndDateInfo);

			TestUpdater.NewEntryEndDate = ZDate.Invalid;
			AssertHasError(TestUpdater.NewEntryEndDateInfo, "Enter a valid selection.");

			var today = ZDate.Today;
			TestUpdater.NewEntryEndDate = today;
			AssertNoErrors(TestUpdater.NewEntryEndDateInfo);

			TestUpdater.NewEntryEndDate = today.AddYears(-3);
			AssertHasWarning(TestUpdater.NewEntryEndDateInfo, "The date '10-Feb-2013' is more than 1 year old.");

			TestUpdater.CreateNewEntry = false;
			TestUpdater.NewEntryEndDate = ZDate.Invalid;
			AssertNoErrors(TestUpdater.NewEntryEndDateInfo);
		}

		public void TestValidateShowClientRates()
		{
			try
			{
				Env.Security.ClientRatesBulkUpdate.IsAllowed = false;
				TestUpdater.ShowClientRates = true;
				Assert(!TestUpdater.ShowClientRates);
				AssertHasWarning(TestUpdater.ShowClientRatesInfo, "You do not have security rights to update Client Rates.");

				fTestUpdater = null;
				Env.Security.ClientRatesBulkUpdate.IsAllowed = true;
				TestUpdater.ShowClientRates = true;
				Assert(TestUpdater.ShowClientRates);
				AssertNoWarnings(TestUpdater.ShowClientRatesInfo);
			}
			finally
			{
				Env.Security.ClientRatesBulkUpdate.IsAllowed = true;
			}
		}

		public void TestValidateShowIntercompanyTariffs()
		{
			try
			{
				Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed = false;
				TestUpdater.ShowIntercompanyTariffs = true;
				Assert(!TestUpdater.ShowIntercompanyTariffs);
				AssertHasWarning(TestUpdater.ShowIntercompanyTariffsInfo, "You do not have security rights to update Intercompany Tariffs.");

				fTestUpdater = null;
				Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed = true;
				TestUpdater.ShowIntercompanyTariffs = true;
				Assert(TestUpdater.ShowIntercompanyTariffs);
				AssertNoWarnings(TestUpdater.ShowIntercompanyTariffsInfo);
			}
			finally
			{
				Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed = true;
			}
		}

		public void TestValidateShowActiveQuotes()
		{
			try
			{
				Env.Security.QuotationBulkUpdate.IsAllowed = false;
				TestUpdater.ShowActiveQuotes = true;
				Assert(!TestUpdater.ShowActiveQuotes);
				AssertHasWarning(TestUpdater.ShowActiveQuotesInfo, "You do not have security rights to update Quotes.");

				fTestUpdater = null;
				Env.Security.QuotationBulkUpdate.IsAllowed = true;
				TestUpdater.ShowActiveQuotes = true;
				Assert(TestUpdater.ShowActiveQuotes);
				AssertNoWarnings(TestUpdater.ShowActiveQuotesInfo);
			}
			finally
			{
				Env.Security.QuotationBulkUpdate.IsAllowed = true;
			}
		}

		public void TestValidateShowCostings()
		{
			try
			{
				Env.Security.CostingRatesBulkUpdate.IsAllowed = false;
				TestUpdater.ShowCostings = true;
				Assert(!TestUpdater.ShowCostings);
				AssertHasWarning(TestUpdater.ShowCostingsInfo, "You do not have security rights to update Costings.");

				fTestUpdater = null;
				Env.Security.CostingRatesBulkUpdate.IsAllowed = true;
				TestUpdater.ShowCostings = true;
				Assert(TestUpdater.ShowCostings);
				AssertNoWarnings(TestUpdater.ShowCostingsInfo);
			}
			finally
			{
				Env.Security.CostingRatesBulkUpdate.IsAllowed = true;
			}
		}

		public void TestValidateShowCompanyTariffs()
		{
			try
			{
				Env.Security.CompanyTariffRatesBulkUpdate.IsAllowed = false;
				TestUpdater.ShowCompanyTariffs = true;
				Assert(!TestUpdater.ShowCompanyTariffs);
				AssertHasWarning(TestUpdater.ShowCompanyTariffsInfo, "You do not have security rights to update Company Tariffs.");

				fTestUpdater = null;
				Env.Security.CompanyTariffRatesBulkUpdate.IsAllowed = true;
				TestUpdater.ShowCompanyTariffs = true;
				Assert(TestUpdater.ShowCompanyTariffs);
				AssertNoWarnings(TestUpdater.ShowCompanyTariffsInfo);
			}
			finally
			{
				Env.Security.CompanyTariffRatesBulkUpdate.IsAllowed = true;
			}
		}

		public void TestValidateCreateNewEntry()
		{
			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.CreateNewEntry = true;
			AssertHasWarning(TestUpdater.CreateNewEntryInfo, "You cannot choose to create new entries (trade lanes) on Quotations.");

			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.CreateNewEntry = false;
			AssertNoWarnings(TestUpdater.CreateNewEntryInfo);

			TestUpdater.ShowActiveQuotes = false;
			TestUpdater.CreateNewEntry = true;
			AssertNoWarnings(TestUpdater.CreateNewEntryInfo);
		}

		public void TestValidateModuleSelection()
		{
			var message = "Rates of Intercompany Tariffs cannot be updated in bulk together with rates of other Tariffs & Rates modules as Global Charge Codes are used for Intercompany Tariffs.";

			void CheckValidation(Action<ZBool> action)
			{
				action(true);
				AssertNoError(TestUpdater.ShowIntercompanyTariffsInfo, message);
				TestUpdater.ShowIntercompanyTariffs = true;
				AssertHasError(TestUpdater.ShowIntercompanyTariffsInfo, message);
				TestUpdater.ShowIntercompanyTariffs = false;
				AssertNoError(TestUpdater.ShowIntercompanyTariffsInfo, message);
				TestUpdater.ShowIntercompanyTariffs = true;
				action(false);
				AssertNoError(TestUpdater.ShowIntercompanyTariffsInfo, message);
			}

			CheckValidation((x) => TestUpdater.ShowClientRates = x);
			CheckValidation((x) => TestUpdater.ShowActiveQuotes = x);
			CheckValidation((x) => TestUpdater.ShowCompanyTariffs = x);
			CheckValidation((x) => TestUpdater.ShowCostings = x);

			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.ShowClientRates = true;
			TestUpdater.ShowCompanyTariffs = true;
			TestUpdater.ShowCostings = true;
			AssertNoError(TestUpdater.ShowIntercompanyTariffsInfo, message);
			TestUpdater.ShowIntercompanyTariffs = true;
			AssertHasError(TestUpdater.ShowIntercompanyTariffsInfo, message);
			TestUpdater.ShowIntercompanyTariffs = false;
			AssertNoError(TestUpdater.ShowIntercompanyTariffsInfo, message);
		}

		#region Implementation

		BulkRateUpdater TestUpdater
		{
			get { return fTestUpdater ?? (fTestUpdater = new BulkRateUpdater()); }
		}

		BulkRateUpdater fTestUpdater;

		#endregion
	}
}
