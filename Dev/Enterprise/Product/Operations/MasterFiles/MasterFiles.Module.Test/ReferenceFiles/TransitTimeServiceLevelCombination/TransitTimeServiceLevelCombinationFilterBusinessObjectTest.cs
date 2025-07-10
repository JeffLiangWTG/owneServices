using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TransitTimeServiceLevelCombinationFilterBusinessObject))]
	class TransitTimeServiceLevelCombinationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAddIsDomesticFilters()
		{
			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();

			var isOriginDomesticFilter = (ModuleFlagsFilter)filterBizO["Is Origin Domestic"];
			AssertNull("'Is Origin Domestic' filter should not be added.", isOriginDomesticFilter);
			var isDestinationDomesticFilter = (ModuleFlagsFilter)filterBizO["Is Destination Domestic"];
			AssertNull("'Is Destination Domestic' filter should not be added.", isDestinationDomesticFilter);
		}

		public void TestServiceLevelFilterDescription()
		{
			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();
			var serviceLevelFilter = (ModuleTextFilter)filterBizO["Service Level"];
			AssertEquals("'Service Level' filter name should be 'Code'.", "Code", ((ResourceString)serviceLevelFilter.MultilingualDescription).EnglishText);
		}

		public void TestDefaultFilter()
		{
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone1[RateTransportZonesSchema.TZ_ZoneName] = "DZONE1";

			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone2[RateTransportZonesSchema.TZ_ZoneName] = "DZONE2";

			var internationalZone1 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			var internationalZone2 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime3.RTT_RS_NKServiceLevel = "STD";

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "SL1";
			serviceLevel1.RS_DefaultTransitHours = 10;

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "SL2";

			Factory.Save();

			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();
			var stdViews = new[] { transitTime1, transitTime2, transitTime3 };
			var nonStdViews = Factory.Load<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_Code, SQLComparisonOperator.NotEqual, "STD"));
			var results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			var resultTransitTimePKs = results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_RTT);

			foreach (var pk in stdViews.Select(t => t.PK))
			{
				AssertCollectionContains(pk, results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_RTT));
			}
			AssertCollectionContains("SL1", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
			AssertCollectionContains("SL2", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
		}

		public void TestFilterFallback()
		{
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone1[RateTransportZonesSchema.TZ_ZoneName] = "DZONE1";

			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone2[RateTransportZonesSchema.TZ_ZoneName] = "DZONE2";

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime1.RTT_RS_NKServiceLevel = "AAA";

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "AAA";
			serviceLevel1.RS_DefaultTransitHours = 10;

			Factory.Save();

			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();
			var results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertNotNull(results.FirstOrDefault(x => x.PK == transitTime1.PK));
			AssertNotNull(results.FirstOrDefault(x => x.PK == serviceLevel1.PK));
		}

		public void TestZones()
		{
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone1[RateTransportZonesSchema.TZ_ZoneName] = "DZONE1";

			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			domesticZone2[RateTransportZonesSchema.TZ_ZoneName] = "DZONE2";

			var internationalZone1 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			var internationalZone2 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime3.RTT_RS_NKServiceLevel = "STD";

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			serviceLevel1.RS_Code = "SL1";
			serviceLevel1.RS_DefaultTransitHours = 10;

			var serviceLevel2 = Factory.New<RefServiceLevel>();
			serviceLevel2.RS_Code = "SL2";

			Factory.Save();

			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();
			var originZoneFilter = (ModuleGuidFilter)filterBizO["Origin Zone"];
			originZoneFilter.IsActive = true;

			originZoneFilter.Property = domesticZone1.PK;
			var results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertContainsSameElementsInView(new[] { transitTime1, transitTime3 }, results);
			AssertCollectionContains("SL1", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
			AssertCollectionNotContains("SL2", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));

			originZoneFilter.Property = internationalZone1.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertEquals("Should only find service level without matching transit time but has default transit hours more than 0 when filtering by international zones.", 1, results.Length);
			AssertCollectionContains("SL1", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
			AssertCollectionNotContains("SL2", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));

			originZoneFilter.IsActive = false;

			var destinationZoneFilter = (ModuleGuidFilter)filterBizO["Destination Zone"];
			destinationZoneFilter.IsActive = true;

			destinationZoneFilter.Property = domesticZone2.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertContainsSameElementsInView(new[] { transitTime1 }, results);
			AssertCollectionContains("SL1", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
			AssertCollectionNotContains("SL2", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));

			destinationZoneFilter.Property = internationalZone2.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertEquals("Should only find service level without matching transit time but has default transit hours more than 0 when filtering by international zones.", 1, results.Length);
			AssertCollectionContains("SL1", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
			AssertCollectionNotContains("SL2", results.Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_Code));
		}

		public void TestRelatedOrgs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org1.PK;

			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org2.PK;

			var internationalZone1 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			internationalZone1.FZ_OH_RelatedParty = org3.PK;

			var internationalZone2 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));
			internationalZone2.FZ_OH_RelatedParty = org4.PK;

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;
			transitTime3.RTT_RS_NKServiceLevel = "STD";

			Factory.Save();

			var filterBizO = new TransitTimeServiceLevelCombinationFilterBusinessObject();
			var originRelatedOrgFilter = (ModuleGuidFilter)filterBizO["Origin Zone Owner/Carrier"];
			originRelatedOrgFilter.IsActive = true;

			originRelatedOrgFilter.Property = org1.PK;
			var results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertContainsSameElementsInView(new[] { transitTime1, transitTime3 }, results);

			originRelatedOrgFilter.Property = org3.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertEquals("Should not find any transit time when filtering by international zones.", 0, results.Length);

			originRelatedOrgFilter.IsActive = false;

			var isDestinationDomesticFilter = (ModuleGuidFilter)filterBizO["Destination Zone Owner/Carrier"];
			isDestinationDomesticFilter.IsActive = true;

			isDestinationDomesticFilter.Property = org2.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertContainsSameElementsInView(new[] { transitTime1 }, results);

			isDestinationDomesticFilter.Property = org4.PK;
			results = GetTransitTimeServiceLevelCombinationCollection().Find(filterBizO.Filter);
			AssertEquals("Should not find any transit time when filtering by international zones.", 0, results.Length);
		}

		#region Implementation

		RefTransitTime GetValidTransitTimeWithoutZones()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_TZ_OriginDomesticZone = ZGuid.Empty;
			transitTime.RTT_FZ_OriginInternationalZone = ZGuid.Empty;
			transitTime.RTT_TZ_DestinationDomesticZone = ZGuid.Empty;
			transitTime.RTT_FZ_DestinationInternationalZone = ZGuid.Empty;
			return transitTime;
		}

		IBusinessObjectCollection GetTransitTimeServiceLevelCombinationCollection()
		{
			return new TransitTimeServiceLevelCombinationCollection(Factory, null, null, null, null, null);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TransitTimeServiceLevelCombinationFilterBusinessObject();
		}

		void AssertContainsSameElementsInView(RefTransitTime[] transitTimes, BusinessObject[] views)
		{
			AssertContainsExactElementsInAnyOrder(transitTimes.Select(t => t.PK),
				views.Where(v => !((TransitTimeServiceLevelCombinationView)v).TSC_RTT.IsEmpty).Select(v => ((TransitTimeServiceLevelCombinationView)v).TSC_RTT));
		}

		#endregion
	}
}
