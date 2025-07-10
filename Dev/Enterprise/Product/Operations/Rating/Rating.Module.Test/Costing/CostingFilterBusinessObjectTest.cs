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
	[TestedType(typeof(CostingFilterBusinessObject))]
	public class CostingFilterBusinessObjectTest : RatingFilterBusinessObjectTestCase<CostingFilterBusinessObject>
	{
		public void TestContractNumberFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var cost1 = Factory.NewWithValidTestData<Costing>();
			cost1.TH_OH = org1.PK;
			RateEntry entry1 = cost1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_ContractNumber = "ABC123";

			var cost2 = Factory.NewWithValidTestData<Costing>();
			cost2.TH_OH = org2.PK;
			RateEntry entry2 = cost2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_ContractNumber = "";

			Factory.Save();

			CostingFilterBusinessObject filter = new CostingFilterBusinessObject();
			CostingCollection costs = new CostingCollection(Factory);
			costs.Load(filter.Filter);
			AssertEquals(2, costs.Count);

			((ModuleTextFilter)filter["Carrier Contract Number"]).IsActive = true;
			((ModuleTextFilter)filter["Carrier Contract Number"]).Property = "ABC123";

			costs.Load(filter.Filter);
			AssertEquals(1, costs.Count);
		}

		public void TestCommodityCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "A";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "B";

			var cost1 = Factory.NewWithValidTestData<Costing>();
			cost1.TH_OH = org1.PK;
			var entry1 = cost1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry1.TI_RH_NKCommodityCode = "BANA";

			var cost2 = Factory.NewWithValidTestData<Costing>();
			cost2.TH_OH = org2.PK;
			var entry2 = cost2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry2.TI_RH_NKCommodityCode = "PAIN";

			Factory.Save();
			var filter = new CostingFilterBusinessObject();
			var costings = new CostingCollection(Factory);
			costings.Load(filter.Filter);

			var actualCostings1 = costings.Select(s => s.DisplayInfo()).ToArray();
			var expectedCostings1 = new ZString[] { "Costing A", "Costing B" };
			AssertContainsExactElementsInAnyOrder(
				"cost1 and cost2 should both in the collection",
				expectedCostings1,
				actualCostings1
			);

			((ModuleNkFilter)filter["Commodity Code"]).IsActive = true;
			((ModuleNkFilter)filter["Commodity Code"]).Property = "BANA";
			costings.Load(filter.Filter);

			var actualCostings2 = costings.Select(s => s.DisplayInfo()).ToArray();
			var expectedCostings2 = new ZString[] { "Costing A" };
			AssertContainsExactElementsInAnyOrder(
				"Only cost1's commodity code is BANA",
				expectedCostings2,
				actualCostings2
			);
		}

		public void TestSalesRepFilterNull()
		{
			CostingFilterBusinessObject filter = new CostingFilterBusinessObject();
			AssertNull(filter[RateFilterHelper.Constants.StaffFilterSalesRep]);
		}

		public override void TestRatingHeaderOrganisationCaption()
		{
			CostingFilterBusinessObject filter = new CostingFilterBusinessObject();
			AssertNull(filter[RateFilterHelper.Constants.Client]);
			AssertNotNull(filter[RateFilterHelper.Constants.ServiceProvider]); // refer to TH_OH
		}

		public override void TestSalesRepFilter()
		{
			Assert("Sales Rep Filter is not included", true);
		}

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<Costing>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.CostingRatesCRMSecurity);
		}

		#endregion

		protected override List<RatingHeader> GetGlobalAndLocalRatingHeaders()
		{
			var localCost = Helper.NewCosting(Helper.NewOrgHeader());
			var globalCost = Helper.NewGlobalCosting(Helper.NewOrgHeader());

			return new List<RatingHeader>() { localCost, globalCost };
		}

		protected override RatingHeaderCollection GetRatingHeaderCollection()
		{
			return new CostingCollection(Factory);
		}

		protected override RatingHeader NewRatingHeader(OrgHeader serviceProvider) =>
			Helper.NewCosting(serviceProvider);

		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			// Please keep grouped, ideally sorted also.

			RateEntryFilterUtility.Constants.Codes.AircraftType,
			RateEntryFilterUtility.Constants.Codes.CarrierContractNumber,
			RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel,
			RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider,
			RateEntryFilterUtility.Constants.Codes.CommodityCode,
			RateEntryFilterUtility.Constants.Codes.Consignee,
			RateEntryFilterUtility.Constants.Codes.Consignor,
			RateEntryFilterUtility.Constants.Codes.ContainerType,
			RateEntryFilterUtility.Constants.Codes.ContractNumberLinked,
			RateEntryFilterUtility.Constants.Codes.ControllingCustomer,
			RateEntryFilterUtility.Constants.Codes.CrossTrade,
			RateEntryFilterUtility.Constants.Codes.Currency,
			RateEntryFilterUtility.Constants.Codes.EffectiveOn,
			RateEntryFilterUtility.Constants.Codes.EndDate,
			RateEntryFilterUtility.Constants.Codes.FromLocationDescription,
			RateEntryFilterUtility.Constants.Codes.FromPostcode,
			RateEntryFilterUtility.Constants.Codes.FromToOrganization,
			RateEntryFilterUtility.Constants.Codes.FromToSuburb,
			RateEntryFilterUtility.Constants.Codes.FromToZone,
			RateEntryFilterUtility.Constants.Codes.OriginDestination,
			RateEntryFilterUtility.Constants.Codes.ProductWarehouse,
			RateEntryFilterUtility.Constants.Codes.ServiceLevel,
			RateEntryFilterUtility.Constants.Codes.ShipmentConsolidationStatus,
			RateEntryFilterUtility.Constants.Codes.StartDate,
			RateEntryFilterUtility.Constants.Codes.ToLocationDescription,
			RateEntryFilterUtility.Constants.Codes.ToPostcode,
			RateEntryFilterUtility.Constants.Codes.TransitTime,
			RateEntryFilterUtility.Constants.Codes.TransitWarehouse,
			RateEntryFilterUtility.Constants.Codes.TransportMode,
			RateEntryFilterUtility.Constants.Codes.Via,
			RateEntryFilterUtility.Constants.Codes.FirstLoad,
			RateEntryFilterUtility.Constants.Codes.LastDischarge,
			RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad,
			RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge,
			RateEntryFilterUtility.Constants.Codes.IsNonOperatingReefer,

			RateFilterHelper.Constants.GlobalRateFilter,
			RateFilterHelper.Constants.OrganizationName,
			RateFilterHelper.Constants.ServiceProvider,

			RateLineModuleFilters.Constants.Codes.ActualPercentage,
			RateLineModuleFilters.Constants.Codes.ChargeCode,
			RateLineModuleFilters.Constants.Codes.Condition,
			RateLineModuleFilters.Constants.Codes.ContainerOwnership,
			RateLineModuleFilters.Constants.Codes.ConversionFactor,
			RateLineModuleFilters.Constants.Codes.Currency,
			RateLineModuleFilters.Constants.Codes.EffectiveOn,
			RateLineModuleFilters.Constants.Codes.EndDate,
			RateLineModuleFilters.Constants.Codes.FeeChargeLevel,
			RateLineModuleFilters.Constants.Codes.FeeChargeType,
			RateLineModuleFilters.Constants.Codes.HasOverrideChargeDescription,
			RateLineModuleFilters.Constants.Codes.IsJobLevelCharge,
			RateLineModuleFilters.Constants.Codes.Rounding,
			RateLineModuleFilters.Constants.Codes.ShowExpired,
			RateLineModuleFilters.Constants.Codes.StartDate,
			RateLineModuleFilters.Constants.Codes.UnitFactor,
			RateLineModuleFilters.Constants.Codes.UnitMultiple,
			RateLineModuleFilters.Constants.Codes.Units,
			RateLineModuleFilters.Constants.Codes.UseOnlyActualWeightMeasure,
		};
	}
}
