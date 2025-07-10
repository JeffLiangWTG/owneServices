using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(IntercompanyTariffFilterBusinessObject))]
	public class IntercompanyTariffFilterBusinessObjectTest : RatingFilterBusinessObjectTestCase<IntercompanyTariffFilterBusinessObject>
	{
		public override void TestRatingHeaderOrganisationCaption()
		{
			var filter = new IntercompanyTariffFilterBusinessObject();

			AssertNull(filter[RateFilterHelper.Constants.Client]);
			AssertNotNull(filter[RateFilterHelper.Constants.ServiceProvider]);  // refer to TH_OH
		}

		public override void TestSalesRepFilter() => Assert("Sales Rep Filter is not included", true);

		public override void TestGlobalRateTypeFilter() => Assert("Global Rate Type Filter is not included", true);

		public void TestGatewayAgentTypeFilter()
		{
			var interCompanyTariff1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = interCompanyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry1.TI_GatewayAgentType = "SAG";

			var interCompanyTariff2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2 = interCompanyTariff2.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry2.TI_GatewayAgentType = "SSG";

			var interCompanyTariff3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3 = interCompanyTariff3.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "INBLR");
			entry3.TI_GatewayAgentType = "SSG";

			Factory.Save();

			ZQuery scope = new ZQuery(RatingHeaderSchema.PK, new ZGuid[] { interCompanyTariff1.PK, interCompanyTariff2.PK, interCompanyTariff3.PK });

			var filterBizo = GetNewFilterStripBusinessObject();
			var interCompanyTariffs = GetRatingHeaderCollection();

			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Empty Filter",
				new RatingHeader[] { interCompanyTariff1, interCompanyTariff2, interCompanyTariff3 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayAgentType]).IsActive = true;
			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayAgentType]).Property = Core.Constants.GatewayAgentType.Codes.SendingAgent;
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Gateway Agent Type SAG",
				new RatingHeader[] { interCompanyTariff1 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayAgentType]).Property = Core.Constants.GatewayAgentType.Codes.SucceedingSendingAgent;
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Gateway Agent Type SSG",
				new RatingHeader[] { interCompanyTariff2, interCompanyTariff3 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));
		}

		public void TestGatewayServiceLevelFilter()
		{
			var interCompanyTariff1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = interCompanyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry1.TI_RS_NKGatewayServiceLevel = "";

			var interCompanyTariff2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2 = interCompanyTariff2.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry2.TI_RS_NKGatewayServiceLevel = "STD";

			var interCompanyTariff3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3 = interCompanyTariff3.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "INBLR");
			entry3.TI_RS_NKGatewayServiceLevel = "DIR";

			Factory.Save();

			var scope = new ZQuery(RatingHeaderSchema.PK, new ZGuid[] { interCompanyTariff1.PK, interCompanyTariff2.PK, interCompanyTariff3.PK });

			var filterBizo = GetNewFilterStripBusinessObject();
			var interCompanyTariffs = GetRatingHeaderCollection();

			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Empty Filter",
				new RatingHeader[] { interCompanyTariff1, interCompanyTariff2, interCompanyTariff3 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel]).IsActive = true;
			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel]).Property = "STD";
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Gateway Service Level = STD",
				new RatingHeader[] { interCompanyTariff2 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel]).Property = "DEF";
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Gateway Service Level = DEF",
				Array.Empty<RatingHeader>(),
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));
		}

		public void TestShipmentGatewayServiceLevelFilter()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var interCompanyTariff1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = interCompanyTariff1.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry1.TI_RS_NKShipmentGatewayServiceLevel = "";

			var interCompanyTariff2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2 = interCompanyTariff2.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX");
			entry2.TI_RS_NKShipmentGatewayServiceLevel = "STD";

			var interCompanyTariff3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3 = interCompanyTariff3.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "INBLR");
			entry3.TI_RS_NKShipmentGatewayServiceLevel = "DIR";

			Factory.Save();

			var scope = new ZQuery(RatingHeaderSchema.PK, new ZGuid[] { interCompanyTariff1.PK, interCompanyTariff2.PK, interCompanyTariff3.PK });

			var filterBizo = GetNewFilterStripBusinessObject();
			var interCompanyTariffs = GetRatingHeaderCollection();

			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Empty Filter",
				new RatingHeader[] { interCompanyTariff1, interCompanyTariff2, interCompanyTariff3 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel]).IsActive = true;
			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel]).Property = "STD";
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Shipment Gateway Service Level = STD",
				new RatingHeader[] { interCompanyTariff2 },
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));

			((ModuleTextFilter)filterBizo[RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel]).Property = "DEF";
			interCompanyTariffs.Load(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Filter on Shipment Gateway Service Level = DEF",
				Array.Empty<RatingHeader>(),
				Factory.Load<RatingHeader>(new ZQuery(scope, filterBizo.Filter)));
		}

		protected override bool IsGlobalOnly => true;

		protected override List<RatingHeader> GetGlobalAndLocalRatingHeaders() =>
			new List<RatingHeader> { Helper.NewIntercompanyTariff(Helper.NewOrgHeader()) };

		protected override RatingHeaderCollection GetRatingHeaderCollection() =>
			new IntercompanyTariffCollection(Factory);

		protected override RatingHeader NewRatingHeader(OrgHeader serviceProvider) =>
			Helper.NewIntercompanyTariff(serviceProvider);

		protected override RateEntry AddRateEntry(RatingHeader rate, ZString category, ZString mode, ZString origin, ZString destination, ZString serviceLevel, ZString container)
		{
			var entry = base.AddRateEntry(rate, category, mode, origin, destination, serviceLevel, container);
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}

		protected override RateEntry AddRateEntry(RatingHeader rate, ZString category, ZString mode, ZString origin, ZString destination)
		{
			var entry = base.AddRateEntry(rate, category, mode, origin, destination);
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}

		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			RateFilterHelper.Constants.ServiceProvider,
			RateEntryFilterUtility.Constants.Codes.StartDate,
			RateEntryFilterUtility.Constants.Codes.EndDate,
			RateEntryFilterUtility.Constants.Codes.EffectiveOn,
			RateFilterHelper.Constants.OrganizationName,
			RateEntryFilterUtility.Constants.Codes.ControllingCustomer,
			RateEntryFilterUtility.Constants.Codes.Consignee,
			RateEntryFilterUtility.Constants.Codes.Consignor,
			RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel,
			RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel,
			RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel,
			RateEntryFilterUtility.Constants.Codes.ProductWarehouse,
			RateEntryFilterUtility.Constants.Codes.TransitWarehouse,
			RateEntryFilterUtility.Constants.Codes.OriginDestination,
			RateEntryFilterUtility.Constants.Codes.FirstLoad,
			RateEntryFilterUtility.Constants.Codes.LastDischarge,
			RateEntryFilterUtility.Constants.Codes.FirstRouteSetLoad,
			RateEntryFilterUtility.Constants.Codes.LastRouteSetDischarge,
			RateEntryFilterUtility.Constants.Codes.FromToOrganization,
			RateEntryFilterUtility.Constants.Codes.FromToSuburb,
			RateEntryFilterUtility.Constants.Codes.FromPostcode,
			RateEntryFilterUtility.Constants.Codes.ToPostcode,
			RateEntryFilterUtility.Constants.Codes.FromToZone,
			RateEntryFilterUtility.Constants.Codes.FromLocationDescription,
			RateEntryFilterUtility.Constants.Codes.ToLocationDescription,
			RateEntryFilterUtility.Constants.Codes.TransportMode,
			RateEntryFilterUtility.Constants.Codes.ContainerType,
			RateEntryFilterUtility.Constants.Codes.ServiceLevel,
			RateEntryFilterUtility.Constants.Codes.AircraftType,
			RateEntryFilterUtility.Constants.Codes.CommodityCode,
			RateEntryFilterUtility.Constants.Codes.TransitTime,
			RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider,
			RateEntryFilterUtility.Constants.Codes.GatewayAgentType,
			RateEntryFilterUtility.Constants.Codes.Currency,
			RateEntryFilterUtility.Constants.Codes.IsNonOperatingReefer,
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
