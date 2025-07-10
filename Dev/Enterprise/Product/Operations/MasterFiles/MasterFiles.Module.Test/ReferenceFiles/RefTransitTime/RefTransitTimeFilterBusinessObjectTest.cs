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
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefTransitTimeFilterBusinessObject))]
	sealed class RefTransitTimeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestServiceLevel()
		{
			var transitTime1 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime1.RTT_RS_NKServiceLevel = "STD";

			var transitTime2 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime2.RTT_RS_NKServiceLevel = "STD";

			var transitTime3 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime3.RTT_RS_NKServiceLevel = "D2D";

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var serviceLevelFilter = (ModuleTextFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.ServiceLevel];
			serviceLevelFilter.Property = "STD";
			serviceLevelFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime2 }, results);
		}

		public void TestTransportMode()
		{
			var transitTime1 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime1.RTT_Mode = Core.Constants.RateMode.AIR;

			var transitTime2 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime2.RTT_Mode = Core.Constants.RateMode.SEA;

			var transitTime3 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime3.RTT_Mode = Core.Constants.RateMode.AIR;

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var modeFilter = (ModuleTextFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.Mode];
			modeFilter.Property = Core.Constants.RateMode.AIR;
			modeFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime3 }, results);
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

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var originZoneFilter = (ModuleTextFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.OriginZone];
			originZoneFilter.Property = "DZONE1";
			originZoneFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime3 }, results);

			originZoneFilter.IsActive = false;

			var destinationZoneFilter = (ModuleTextFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.DestinationZone];
			destinationZoneFilter.Property = "USEC";
			destinationZoneFilter.IsActive = true;

			results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime2, transitTime3 }, results);
		}

		public void TestAddIsDomesticFilters()
		{
			var filterBizO = new RefTransitTimeFilterBusinessObject();

			var isOriginDomesticFilter = (ModuleFlagsFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.IsOriginDomestic];
			AssertNotNull("'Is Origin Domestic' filter should be added.", isOriginDomesticFilter);
			var isDestinationDomesticFilter = (ModuleFlagsFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.IsDestinationDomestic];
			AssertNotNull("'Is Destination Domestic' filter should be added.", isDestinationDomesticFilter);
		}

		public void TestServiceLevelFilterDescription()
		{
			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var serviceLevelFilter = (ModuleTextFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.ServiceLevel];
			AssertEquals("'Service Level' filter name should be 'Service Level'.", RefTransitTimeFilterBusinessObject.FilterConstants.ServiceLevel, ((ResourceString)serviceLevelFilter.MultilingualDescription).EnglishText);
		}

		public void TestIsDomestic()
		{
			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());

			var internationalZone1 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			var internationalZone2 = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var isOriginDomesticFilter = (ModuleFlagsFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.IsOriginDomestic];
			isOriginDomesticFilter.Property0 = true;
			isOriginDomesticFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime3 }, results);

			isOriginDomesticFilter.IsActive = false;

			var isDestinationDomesticFilter = (ModuleFlagsFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.IsDestinationDomestic];
			isDestinationDomesticFilter.Property0 = false;
			isDestinationDomesticFilter.IsActive = true;

			results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime2, transitTime3 }, results);
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

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_FZ_OriginInternationalZone = internationalZone1.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime3.RTT_FZ_DestinationInternationalZone = internationalZone2.PK;

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var originRelatedOrgFilter = (ModuleGuidFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.OriginZoneOwner];
			originRelatedOrgFilter.Property = org1.PK;
			originRelatedOrgFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime3 }, results);

			originRelatedOrgFilter.IsActive = false;

			var isDestinationDomesticFilter = (ModuleGuidFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.DestinationZoneOwner];
			isDestinationDomesticFilter.Property = org4.PK;
			isDestinationDomesticFilter.IsActive = true;

			results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime2, transitTime3 }, results);
		}

		public void TestZoneCountries()
		{
			var australianCity = Factory.NewWithValidTestData<RefCityTown>();
			australianCity.R9_RN_NKCountry = CountryCodes.Australia;

			var belarusCity = Factory.NewWithValidTestData<RefCityTown>();
			belarusCity.R9_RN_NKCountry = CountryCodes.Belarus;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			var domesticZone1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org1.PK;
			((BusinessObject)((IRateTransportZone)domesticZone1).TransportProvider)[RateTransportProviderSchema.TP_RN_NKCountry] = Core.Constants.CountryCodes.Afghanistan;

			var domesticZone2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org2.PK;
			((BusinessObject)((IRateTransportZone)domesticZone2).TransportProvider)[RateTransportProviderSchema.TP_RN_NKCountry] = Core.Constants.CountryCodes.Algeria;

			var domesticZone3 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone3).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org3.PK;
			((BusinessObject)((IRateTransportZone)domesticZone3).TransportProvider)[RateTransportProviderSchema.TP_R9_ZoneHubLocation] = australianCity.PK;

			var domesticZone4 = Factory.NewWithValidTestData(ObjectFactory.GetType<IRateTransportZone>());
			((BusinessObject)((IRateTransportZone)domesticZone4).TransportProvider)[RateTransportProviderSchema.TP_OH_RelatedParty] = org4.PK;
			((BusinessObject)((IRateTransportZone)domesticZone4).TransportProvider)[RateTransportProviderSchema.TP_R9_ZoneHubLocation] = belarusCity.PK;

			var transitTime1 = GetValidTransitTimeWithoutZones();
			transitTime1.RTT_TZ_OriginDomesticZone = domesticZone1.PK;
			transitTime1.RTT_TZ_DestinationDomesticZone = domesticZone2.PK;

			var transitTime2 = GetValidTransitTimeWithoutZones();
			transitTime2.RTT_TZ_OriginDomesticZone = domesticZone3.PK;
			transitTime2.RTT_TZ_DestinationDomesticZone = domesticZone4.PK;

			var transitTime3 = GetValidTransitTimeWithoutZones();
			transitTime3.RTT_TZ_OriginDomesticZone = domesticZone2.PK;
			transitTime3.RTT_TZ_DestinationDomesticZone = domesticZone4.PK;

			Factory.Save();

			var filter = new RefTransitTimeFilterBusinessObject();
			var originCountryFilter = (ModuleNkFilter)filter[RefTransitTimeFilterBusinessObject.FilterConstants.OriginCountry];
			originCountryFilter.IsActive = true;
			originCountryFilter.Property = Core.Constants.CountryCodes.Australia;
			var originCollection = new RefTransitTimeCollection(Factory);
			var foundOriginTransitTimes = originCollection.Find(filter.Filter);
			AssertContainsExactElementsInAnyOrder("Expected to find zones belonging to AU", new[] { transitTime2 }, foundOriginTransitTimes);

			originCountryFilter.IsActive = false;
			var destCountryFilter = (ModuleNkFilter)filter[RefTransitTimeFilterBusinessObject.FilterConstants.DestinationCountry];
			destCountryFilter.IsActive = true;
			destCountryFilter.Property = Core.Constants.CountryCodes.Belarus;
			var destCollection = new RefTransitTimeCollection(Factory);
			var foundDestTransitTimes = destCollection.Find(filter.Filter);
			AssertContainsExactElementsInAnyOrder("Expected to find zones belonging to BY", new[] { transitTime2, transitTime3 }, foundDestTransitTimes);

			destCountryFilter.IsActive = false;
			originCountryFilter.IsActive = true;
			originCountryFilter.Property = Core.Constants.CountryCodes.Afghanistan;
			originCollection = new RefTransitTimeCollection(Factory);
			foundOriginTransitTimes = originCollection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find zones belonging to AU", new[] { transitTime1 }, foundOriginTransitTimes);
		}

		public void TestTransitTime()
		{
			var transitTime1 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime1.RTT_TransitHours = 10;

			var transitTime2 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime2.RTT_TransitHours = 30;

			var transitTime3 = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime3.RTT_TransitHours = 50;

			Factory.Save();

			var filterBizO = new RefTransitTimeFilterBusinessObject();
			var transitDaysFilter = (TransitTimeModuleFilter)filterBizO[RefTransitTimeFilterBusinessObject.FilterConstants.TransitTime];
			transitDaysFilter.ComparisonOperator = "Less than";
			transitDaysFilter.TransitDays = 1;
			transitDaysFilter.TransitHours = 8;
			transitDaysFilter.IsActive = true;

			var results = new RefTransitTimeCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { transitTime1, transitTime2 }, results);
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

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefTransitTimeFilterBusinessObject();
		}

		#endregion
	}
}
