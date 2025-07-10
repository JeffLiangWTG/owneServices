using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(ClientRatesFilterBusinessObject))]
	public class ClientRatesFilterBusinessObjectTest : RatingFilterBusinessObjectTestCase<ClientRatesFilterBusinessObject>
	{
		#region Staff Filter

		public void TestStaffFilter()
		{
			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "S1";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.StaffAssignments.OverallSalesRep = salesRep.GS_Code;

			var testQuote = Factory.New<ClientRate>();
			testQuote.TH_OH = org.PK;

			Factory.Save();

			RatingFilterBusinessObject filterBizo = (RatingFilterBusinessObject)GetNewFilterStripBusinessObject();
			RatingHeaderCollection rates = new RatingHeaderCollection(Factory);

			rates.Load(filterBizo.Filter);
			AssertEquals("1 quotation exists", 1, rates.Count);

			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).IsActive = true;
			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).Property = GlbStaff.CurrentUser.GS_Code;
			rates.Load(filterBizo.Filter);
			AssertEquals("No quotes where sales rep is current user", 0, rates.Count);

			((ModuleNkFilter)filterBizo[RateFilterHelper.Constants.StaffFilterSalesRep]).Property = salesRep.GS_Code;
			rates.Load(filterBizo.Filter);
			AssertEquals("1 quote where sales rep is the sales rep", 1, rates.Count);
		}

		#endregion

		#region TestContractNumberFilter

		public void TestContractNumberFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var rate1 = Factory.NewWithValidTestData<ClientRate>();
			rate1.TH_OH = org1.PK;
			RateEntry entry1 = rate1.AddRateEntry("FCL", "ROA", "AUSYD", "USLAX", "", "");
			entry1.TI_ContractNumber = "ABC123";

			var rate2 = Factory.NewWithValidTestData<ClientRate>();
			rate2.TH_OH = org2.PK;
			RateEntry entry2 = rate2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_ContractNumber = "";

			Factory.Save();

			ClientRatesFilterBusinessObject filter = new ClientRatesFilterBusinessObject();
			RateCollection rates = new RateCollection(Factory);
			rates.Load(filter.Filter);
			AssertEquals(2, rates.Count);

			((ModuleTextFilter)filter["Client Contract Number"]).IsActive = true;
			((ModuleTextFilter)filter["Client Contract Number"]).Property = "ABC123";

			rates.Load(filter.Filter);
			AssertEquals(1, rates.Count);
		}

		#endregion

		#region TestCommodityCodeFilter

		public void TestCommodityCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "A";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "B";

			var rate1 = Factory.NewWithValidTestData<ClientRate>();
			rate1.TH_OH = org1.PK;
			var entry1 = rate1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_RH_NKCommodityCode = "BANA";

			var rate2 = Factory.NewWithValidTestData<ClientRate>();
			rate2.TH_OH = org2.PK;
			var entry2 = rate2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_RH_NKCommodityCode = "PAIN";

			Factory.Save();
			var aa = rate1.DisplayInfo();
			var filter = new ClientRatesFilterBusinessObject();
			var rates = new RateCollection(Factory);
			rates.Load(filter.Filter);

			var actualRates = rates.Select(s => s.DisplayInfo()).ToArray();
			var expectedRates = new ZString[] { "Client Rate A", "Client Rate B" };
			AssertContainsExactElementsInAnyOrder(
				"Rate1 and Rate2 should both be in the collection",
				expectedRates,
				actualRates
			);

			((ModuleNkFilter)filter["Commodity Code"]).IsActive = true;
			((ModuleNkFilter)filter["Commodity Code"]).Property = "BANA";
			rates.Load(filter.Filter);

			actualRates = rates.Select(s => s.DisplayInfo()).ToArray();
			expectedRates = new ZString[] { "Client Rate A" };
			AssertContainsExactElementsInAnyOrder(
				"Only rate1's commodity code is BANA",
				expectedRates,
				actualRates
			);
		}

		#endregion

		#region HBL Delivery Mode Filter

		public void TestHBLDeliveryModeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var rate1 = Factory.NewWithValidTestData<ClientRate>();
			rate1.TH_OH = org1.PK;
			var entry1 = rate1.AddRateEntry("FCL", "ROA", "AUSYD", "USLAX", "", "");
			entry1.TI_HBLDeliveryMode = "Door/Door";

			var rate2 = Factory.NewWithValidTestData<ClientRate>();
			rate2.TH_OH = org2.PK;
			var entry2 = rate2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_HBLDeliveryMode = "";

			Factory.Save();

			var filter = new ClientRatesFilterBusinessObject();
			var rates = new RateCollection(Factory);
			rates.Load(filter.Filter);
			AssertEquals(2, rates.Count);

			((ModuleTextFilter)filter["HBL Delivery Mode"]).IsActive = true;
			((ModuleTextFilter)filter["HBL Delivery Mode"]).Property = "Door/Door";

			rates.Load(filter.Filter);
			AssertEquals(1, rates.Count);
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<ClientRate>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.ClientRatesCRMSecurity);
		}

		#endregion

		#region FMC Tariff ID Filter

		public void TestFMCTariffIDFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "A";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "B";

			var rate1 = Factory.NewWithValidTestData<ClientRate>();
			rate1.TH_OH = org1.PK;
			var entry1 = rate1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1.TI_FMCTariffID = "ABC";

			var rate2 = Factory.NewWithValidTestData<ClientRate>();
			rate2.TH_OH = org2.PK;
			var entry2 = rate2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry2.TI_FMCTariffID = "";

			Factory.Save();

			var filter = new ClientRatesFilterBusinessObject();
			var rates = new RateCollection(Factory);
			rates.Load(filter.Filter);

			var actual1 = rates.Select(s => s.PK).ToArray();
			var expected1 = new[] { rate1.PK, rate2.PK };
			AssertContainsExactElementsInAnyOrder(
				"If property is empty, we will load all rates.",
				expected1,
				actual1
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).IsActive = true;
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			rates.Load(filter.Filter);

			var actual2 = rates.Select(s => s.PK).ToArray();
			var expected2 = new[] { rate2.PK };
			AssertContainsExactElementsInAnyOrder(
				"When comparison operator is blank, only load rate2.",
				expected2,
				actual2
			);

			((ModuleTextFilter)filter["FMC Tariff ID"]).Property = "A";
			((ModuleTextFilter)filter["FMC Tariff ID"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			rates.Load(filter.Filter);

			var actual3 = rates.Select(s => s.PK).ToArray();
			var expected3 = new[] { rate1.PK };
			AssertContainsExactElementsInAnyOrder(
				"Only rate1 has an entry with FMC Tariff ID starting with 'A'.",
				expected3,
				actual3
			);

			AssertEquals("FMCTariffID MaxLength", 4, ((ModuleTextFilter)filter["FMC Tariff ID"]).MaxLength);
		}

		#endregion

		protected override List<RatingHeader> GetGlobalAndLocalRatingHeaders()
		{
			var localRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var globalRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			return new List<RatingHeader>() { localRate, globalRate };
		}

		protected override RatingHeaderCollection GetRatingHeaderCollection()
		{
			return new RateCollection(Factory);
		}

		protected override RatingHeader NewRatingHeader(OrgHeader client) => Helper.NewClientRate(client);

		public override void TestRatingHeaderOrganisationCaption()
		{
			var filter = new ClientRatesFilterBusinessObject();
			AssertNotNull(filter[RateFilterHelper.Constants.Client]);
			AssertNotNull(filter[RateFilterHelper.Constants.ServiceProvider]); // refer to TI_OH_Supplier
		}

		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			RateEntryFilterUtility.Constants.Codes.StartDate,
			RateEntryFilterUtility.Constants.Codes.EndDate,
			RateEntryFilterUtility.Constants.Codes.EffectiveOn,
			RateFilterHelper.Constants.Client,
			RateFilterHelper.Constants.StaffFilterSalesRep,
			RateFilterHelper.Constants.OrganizationName,
			RateEntryFilterUtility.Constants.Codes.ServiceProvider,
			RateEntryFilterUtility.Constants.Codes.ControllingCustomer,
			RateEntryFilterUtility.Constants.Codes.Consignee,
			RateEntryFilterUtility.Constants.Codes.Consignor,
			RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel,
			RateEntryFilterUtility.Constants.Codes.ProductWarehouse,
			RateEntryFilterUtility.Constants.Codes.TransitWarehouse,
			RateEntryFilterUtility.Constants.Codes.OriginDestination,
			RateEntryFilterUtility.Constants.Codes.Via,
			RateEntryFilterUtility.Constants.Codes.FirstLoad,
			RateEntryFilterUtility.Constants.Codes.LastDischarge,
			RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad,
			RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge,
			RateEntryFilterUtility.Constants.Codes.CrossTrade,
			RateEntryFilterUtility.Constants.Codes.FromToSuburb,
			RateEntryFilterUtility.Constants.Codes.FromPostcode,
			RateEntryFilterUtility.Constants.Codes.ToPostcode,
			RateEntryFilterUtility.Constants.Codes.FromToZone,
			RateEntryFilterUtility.Constants.Codes.FromToOrganization,
			RateEntryFilterUtility.Constants.Codes.FromLocationDescription,
			RateEntryFilterUtility.Constants.Codes.ToLocationDescription,
			RateEntryFilterUtility.Constants.Codes.TransportMode,
			RateEntryFilterUtility.Constants.Codes.ContainerType,
			RateEntryFilterUtility.Constants.Codes.ServiceLevel,
			RateEntryFilterUtility.Constants.Codes.HBLDeliveryMode,
			RateEntryFilterUtility.Constants.Codes.AircraftType,
			RateFilterHelper.Constants.GlobalRateFilter,
			RateEntryFilterUtility.Constants.Codes.CommodityCode,
			RateEntryFilterUtility.Constants.Codes.TransitTime,
			RateEntryFilterUtility.Constants.Codes.ClientContractNumber,
			RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider,
			RateEntryFilterUtility.Constants.Codes.Currency,
			RateEntryFilterUtility.Constants.Codes.IsNonOperatingReefer,
			RateEntryFilterUtility.Constants.Codes.FMCTariffID,
			RateLineModuleFilters.Constants.Codes.ActualPercentage,
			RateLineModuleFilters.Constants.Codes.UseOnlyActualWeightMeasure,
			RateLineModuleFilters.Constants.Codes.Condition,
			RateLineModuleFilters.Constants.Codes.ContainerOwnership,
			RateLineModuleFilters.Constants.Codes.ConversionFactor,
			RateLineModuleFilters.Constants.Codes.Currency,
			RateLineModuleFilters.Constants.Codes.ChargeCode,
			RateLineModuleFilters.Constants.Codes.FeeChargeType,
			RateLineModuleFilters.Constants.Codes.FeeChargeLevel,
			RateLineModuleFilters.Constants.Codes.Rounding,
			RateLineModuleFilters.Constants.Codes.HasOverrideChargeDescription,
			RateLineModuleFilters.Constants.Codes.IsJobLevelCharge,
			RateLineModuleFilters.Constants.Codes.UnitFactor,
			RateLineModuleFilters.Constants.Codes.UnitMultiple,
			RateLineModuleFilters.Constants.Codes.Units,
			RateLineModuleFilters.Constants.Codes.StartDate,
			RateLineModuleFilters.Constants.Codes.EndDate,
			RateLineModuleFilters.Constants.Codes.EffectiveOn,
			RateLineModuleFilters.Constants.Codes.ShowExpired,
		};
	}
}
