using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateTransportProviderFilterBusinessObject))]
	public class RateTransportProviderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RateTransportProviderFilterBusinessObject();
		}

		#endregion

		#region Filters

		public void TestRelatedOrganisationFilter()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var cartageZone1 = Factory.NewWithValidTestData<RateTransportProvider>();
			var cartageZone2 = Factory.NewWithValidTestData<RateTransportProvider>();

			cartageZone1.TP_OH_RelatedParty = client.PK;
			cartageZone2.TP_OH_RelatedParty = client.PK;

			RateTransportProviderFilterBusinessObject filter = new RateTransportProviderFilterBusinessObject();

			ModuleGuidFilter filterRelatedOrganisation = (ModuleGuidFilter)filter["Related Organisation"];
			filterRelatedOrganisation.Property = testOrg.PK;
			filterRelatedOrganisation.IsActive = true;

			RateTransportProviderCollection collection = new RateTransportProviderCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(cartageZone1, collection);
			AssertCollectionNotContains(cartageZone2, collection);

			filterRelatedOrganisation = (ModuleGuidFilter)filter["Related Organisation"];
			filterRelatedOrganisation.Property = client.PK;
			filterRelatedOrganisation.IsActive = true;

			collection.Load(filter.Filter);

			AssertCollectionContains(cartageZone1, collection);
			AssertCollectionContains(cartageZone2, collection);

			AssertEquals(typeof(OrganisationsFindBoxCollection), filter.TransportProviders.GetType());
			AssertEquals("Should not have carrier selected", false, filter.TransportProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property4"));
			AssertEquals("Should not have Secondary type filter", false, filter.TransportProviders.FilterBusinessObjectDefaults.ContainsDefaultFor("Secondary Type:Property"));

			filterRelatedOrganisation.IsActive = false;
		}

		public void TestCountryFilter()
		{
			var sydney = Factory.NewWithValidTestData<RefCityTown>();
			sydney.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			sydney.R9_InternationalName = "Aussie Town";
			var sydneyZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			sydneyZoneSet.TP_R9_ZoneHubLocation = sydney.PK;

			var australianZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			australianZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			var newZealandZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			newZealandZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;

			Factory.Save();

			var filter = new RateTransportProviderFilterBusinessObject();
			var filterCountry = (ModuleNkFilter)filter["Country"];
			filterCountry.IsActive = true;

			var collection = new RateTransportProviderCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Nothing filtered", new[] { australianZoneSet, newZealandZoneSet, sydneyZoneSet }, collection);

			filterCountry.Property = Core.Constants.CountryCodes.Australia;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("No NZ result", new[] { australianZoneSet, sydneyZoneSet }, collection);

			filterCountry.Property = Core.Constants.CountryCodes.NewZealand;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Only NZ result", new[] { newZealandZoneSet }, collection);

			filterCountry.Property = Core.Constants.CountryCodes.China;
			filterCountry.ComparisonOperator = "not equal";
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Nothing filtered", new[] { australianZoneSet, newZealandZoneSet, sydneyZoneSet }, collection);

			filterCountry.Property = Core.Constants.CountryCodes.Australia;
			filterCountry.ComparisonOperator = "not equal";
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Only NZ result", new[] { newZealandZoneSet }, collection);
		}

		public void TestZoneTypeFilter()
		{
			var allZoneSet = Factory.New<RateTransportProvider>();
			allZoneSet.TP_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			allZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			allZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Madagascar;
			allZoneSet.Zones.AddNew().TZ_ZoneName = "All Zone";

			var ratingZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			ratingZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			ratingZoneSet.Zones.AddNew().TZ_ZoneName = "Generic Rating Zone";

			var auOperationsZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			auOperationsZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			auOperationsZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			auOperationsZoneSet.Zones.AddNew().TZ_ZoneName = "Australian Operations  Zone";

			var cnOperationsZoneSet = Factory.New<RateTransportProvider>();
			cnOperationsZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			cnOperationsZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.China;
			cnOperationsZoneSet.Zones.AddNew().TZ_ZoneName = "Chinese Operations Zone";

			var reportingZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			reportingZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Reporting;
			reportingZoneSet.Zones.AddNew().TZ_ZoneName = "Generic Reporting Zone";

			var ratingAirZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			ratingAirZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			ratingAirZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			ratingAirZoneSet.TP_ZoneMode = Core.Constants.RateMode.ULD;
			ratingAirZoneSet.Zones.AddNew().TZ_ZoneName = "Rating Air ULD Zone";

			var ratingSeaZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			ratingSeaZoneSet.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			ratingSeaZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			ratingSeaZoneSet.TP_ZoneMode = Core.Constants.RateMode.SEA;
			ratingSeaZoneSet.Zones.AddNew().TZ_ZoneName = "Rating Sea Zone";

			Factory.Save();

			var filter = new RateTransportProviderFilterBusinessObject();
			var zoneTypeFilter = (ModuleFlagsFilter)filter["Zone Types"];
			zoneTypeFilter.IsActive = true;

			var collection = new RateTransportProviderCollection(Factory);
			collection.Load(filter.Filter);
			var expectedZones = new[] { allZoneSet, auOperationsZoneSet, cnOperationsZoneSet, ratingZoneSet, reportingZoneSet, ratingAirZoneSet, ratingSeaZoneSet };
			AssertContainsExactElementsInAnyOrder("Blank filter should return all results", expectedZones, collection);

			zoneTypeFilter.Property0 = ZBool.True;
			collection.Load(filter.Filter);
			expectedZones = new[] { allZoneSet };
			AssertContainsExactElementsInAnyOrder("'All' type zone set should be found", expectedZones, collection);

			zoneTypeFilter.Property1 = ZBool.True;
			collection.Load(filter.Filter);
			expectedZones = new[] { allZoneSet, auOperationsZoneSet, cnOperationsZoneSet };
			AssertContainsExactElementsInAnyOrder("'All' and 'Ops' type zone set should be found", expectedZones, collection);

			zoneTypeFilter.Property0 = ZBool.False;
			zoneTypeFilter.Property2 = ZBool.True;
			collection.Load(filter.Filter);
			expectedZones = new[] { auOperationsZoneSet, cnOperationsZoneSet, ratingZoneSet, ratingAirZoneSet, ratingSeaZoneSet };
			AssertContainsExactElementsInAnyOrder("'Ops' and 'Rating' type zone set should be found", expectedZones, collection);

			zoneTypeFilter.Property1 = ZBool.False;
			zoneTypeFilter.Property2 = ZBool.False;
			zoneTypeFilter.Property3 = ZBool.True;
			collection.Load(filter.Filter);
			expectedZones = new[] { reportingZoneSet };
			AssertContainsExactElementsInAnyOrder("Only reporting typed zone set should be found", expectedZones, collection);

			zoneTypeFilter.Property3 = ZBool.False;
			zoneTypeFilter.Property2 = ZBool.True;
			var zoneModeFilter = (ModuleTextFilter)filter["Zone Modes"];
			zoneModeFilter.IsActive = true;
			zoneModeFilter.Property = Core.Constants.RateMode.ULD;
			collection.Load(filter.Filter);
			expectedZones = new[] { ratingAirZoneSet };
			AssertContainsExactElementsInAnyOrder("Only Rating ULD zone should be found", expectedZones, collection);

			zoneModeFilter.Property = Core.Constants.RateMode.SEA;
			collection.Load(filter.Filter);
			expectedZones = new[] { ratingSeaZoneSet };
			AssertContainsExactElementsInAnyOrder("Only Rating Sea Zone should be found", expectedZones, collection);
		}

		public void TestZoneTypeAndModeFilter()
		{
			var filter = new RateTransportProviderFilterBusinessObject();
			var zoneTypeFilter = (ModuleFlagsFilter)filter["Zone Types"];
			zoneTypeFilter.IsActive = true;
			AssertEquals(4, zoneTypeFilter.FlagNames.Length);

			var zoneModeFilter = (ModuleTextFilter)filter["Zone Modes"];
			zoneModeFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.RateModes), zoneModeFilter.List);
		}

		public void TestZoneHubFilter()
		{
			var melbourne = Factory.New<RefCityTown>();
			melbourne.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			melbourne.R9_InternationalName = "Smelbourne";

			var sydney = Factory.New<RefCityTown>();
			sydney.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			sydney.R9_InternationalName = "Cidknee";

			var melbourneProvider = Factory.New<RateTransportProvider>();
			melbourneProvider.TP_R9_ZoneHubLocation = melbourne.PK;

			var sydneyProvider = Factory.New<RateTransportProvider>();
			sydneyProvider.TP_R9_ZoneHubLocation = sydney.PK;

			var filter = new RateTransportProviderFilterBusinessObject();
			var filterZoneType = (ModuleGuidFilter)filter["Zone Hub"];
			filterZoneType.IsActive = true;

			var collection = new RateTransportProviderCollection(Factory);
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Nothing filtered", new[] { sydneyProvider, melbourneProvider }, collection);

			filterZoneType.Property = sydney.PK;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Only syd", new[] { sydneyProvider }, collection);

			filterZoneType.ComparisonOperator = "not equal";
			collection.Load(filter.Filter);

			AssertCollectionNotContains(sydneyProvider, collection);
			AssertCollectionContains(melbourneProvider, collection);

			AssertContainsExactElementsInAnyOrder("Not syd", new[] { melbourneProvider }, collection);
		}

		#endregion
	}
}
