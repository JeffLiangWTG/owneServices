using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateTransportZoneFilterBusinessObject))]
	public class RateTransportZoneFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestZonesFilteredByZoneOwner()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var provider1 = Factory.NewWithValidTestData<RateTransportProvider>();
			provider1.TP_OH_RelatedParty = org1.PK;
			provider1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			var provider2 = Factory.NewWithValidTestData<RateTransportProvider>();
			provider2.TP_OH_RelatedParty = org2.PK;
			provider2.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			provider2.TP_RN_NKCountry = CountryCodes.Belarus;

			var zone11 = Factory.NewWithValidTestData<RateTransportZone>();
			zone11.TZ_TP = provider1.PK;
			var zone12 = Factory.NewWithValidTestData<RateTransportZone>();
			zone12.TZ_TP = provider1.PK;
			var zone2 = Factory.NewWithValidTestData<RateTransportZone>();
			zone2.TZ_TP = provider2.PK;

			var standardProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			standardProvider.TP_OH_RelatedParty = ZGuid.Empty;
			standardProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			var standardZone = Factory.NewWithValidTestData<RateTransportZone>();
			standardZone.TZ_TP = standardProvider.PK;

			Factory.Save();

			var filter = new RateTransportZoneFilterBusinessObject();
			var filterRelatedOrganisation = (ModuleGuidFilter)filter[RateTransportZoneFilterBusinessObject.FilterConstants.ZoneOwner];
			filterRelatedOrganisation.Property = org1.PK;
			filterRelatedOrganisation.IsActive = true;

			var collection = new RateTransportZonesCollection(Factory);
			var foundZones = collection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find zones belonging to org1", new[] { zone11, zone12 }, foundZones);

			filterRelatedOrganisation.Property = ZGuid.Empty;
			filterRelatedOrganisation.ComparisonOperator = "is blank";
			foundZones = collection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find standard zone", new[] { standardZone }, foundZones);

			filterRelatedOrganisation.ComparisonOperator = "is not blank";
			foundZones = collection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find all zones but the standard zone", new[] { zone11, zone12, zone2 }, foundZones);
		}

		public void TestZonesFilteredByCountry()
		{
			var australianCity = Factory.NewWithValidTestData<RefCityTown>();
			australianCity.R9_RN_NKCountry = CountryCodes.Australia;

			var belarusCity = Factory.NewWithValidTestData<RefCityTown>();
			belarusCity.R9_RN_NKCountry = CountryCodes.Belarus;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var provider1 = Factory.NewWithValidTestData<RateTransportProvider>();
			provider1.TP_OH_RelatedParty = org1.PK;
			provider1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			provider1.TP_RN_NKCountry = CountryCodes.Australia;

			var provider2 = Factory.NewWithValidTestData<RateTransportProvider>();
			provider2.TP_OH_RelatedParty = org2.PK;
			provider2.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			provider2.TP_RN_NKCountry = CountryCodes.Belarus;
			provider2.TP_R9_ZoneHubLocation = belarusCity.PK;

			var zone11 = Factory.NewWithValidTestData<RateTransportZone>();
			zone11.TZ_TP = provider1.PK;
			var zone12 = Factory.NewWithValidTestData<RateTransportZone>();
			zone12.TZ_TP = provider1.PK;

			var zone2 = Factory.NewWithValidTestData<RateTransportZone>();
			zone2.TZ_TP = provider2.PK;

			var standardProvider = Factory.NewWithValidTestData<RateTransportProvider>();
			standardProvider.TP_OH_RelatedParty = ZGuid.Empty;
			standardProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			standardProvider.TP_R9_ZoneHubLocation = australianCity.PK;

			var standardZone = Factory.NewWithValidTestData<RateTransportZone>();
			standardZone.TZ_TP = standardProvider.PK;

			Factory.Save();

			var filter = new RateTransportZoneFilterBusinessObject();
			var countryFilter = (ModuleNkFilter)filter[RateTransportZoneFilterBusinessObject.FilterConstants.Country];
			countryFilter.IsActive = true;

			countryFilter.Property = CountryCodes.Australia;
			var collection = new RateTransportZonesCollection(Factory);
			var foundZones = collection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find zones belonging to org1", new[] { zone11, zone12, standardZone }, foundZones);

			countryFilter.Property = CountryCodes.Belarus;
			foundZones = collection.Find(filter.Filter);

			AssertContainsExactElementsInAnyOrder("Expected to find standard zone", new[] { zone2 }, foundZones);
		}

		public void TestZoneTypeFilter()
		{
			var allZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.All, RateMode.ALL);
			var allZone1 = AddZone(allZoneSet, "ALL 1");
			var allZone2 = AddZone(allZoneSet, "ALL 2");
			var allZone3 = AddZone(allZoneSet, "ALL 3");

			var ratingZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			var ratingZone1 = AddZone(ratingZoneSet, "RAT 1");
			var ratingZone2 = AddZone(ratingZoneSet, "RAT 2");

			var opsAustraliaZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Operations, RateMode.ALL);
			var opsZoneAustralia = AddZone(opsAustraliaZoneSet, "OPS AU");

			var opsChinaZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Operations, RateMode.ALL, CountryCodes.China);
			var opsZoneChina = AddZone(opsChinaZoneSet, "OPS CN");

			var reportingZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Reporting, RateMode.ALL);
			var reportingZone = AddZone(reportingZoneSet, "RPT");

			Factory.Save();

			var filter = new RateTransportZoneFilterBusinessObject();
			var zoneTypeFilter = (ModuleFlagsFilter)filter[RateTransportZoneFilterBusinessObject.FilterConstants.ZoneType];
			zoneTypeFilter.IsActive = true;

			var collection = new RateTransportZonesCollection(Factory);
			var foundZones = collection.Find(filter.Filter);

			var expectedZones = new[]
			{
				allZone1, allZone2, allZone3, opsZoneAustralia, opsZoneChina, ratingZone1, ratingZone2, reportingZone
			};

			AssertContainsExactElementsInAnyOrder("Blank filter should return all results", expectedZones, foundZones);

			zoneTypeFilter.Property0 = ZBool.True;
			foundZones = collection.Find(filter.Filter);

			expectedZones = new[]
			{
				allZone1, allZone2, allZone3
			};

			AssertContainsExactElementsInAnyOrder("'All' type zones should be found", expectedZones, foundZones);

			zoneTypeFilter.Property1 = ZBool.True;
			foundZones = collection.Find(filter.Filter);

			expectedZones = new[]
			{
				allZone1, allZone2, allZone3, opsZoneAustralia, opsZoneChina
			};

			AssertContainsExactElementsInAnyOrder("'All' and 'Ops' type zones should be found", expectedZones, foundZones);
		}

		public void TestZoneModeFilter()
		{
			var seaAUZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, RateMode.SEA);
			var seaAUZone = AddZone(seaAUZoneSet, "RAT SEA AU");

			var lseZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, RateMode.LSE);
			var lseZone = AddZone(lseZoneSet, "RAT LSE");

			var seaCNZoneSet = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, RateMode.SEA, CountryCodes.China);
			var seaCNZone = AddZone(seaCNZoneSet, "RAT SEA for CN");

			Factory.Save();

			var filter = new RateTransportZoneFilterBusinessObject();
			var zoneModeFilter = (ModuleTextFilter)filter[RateTransportZoneFilterBusinessObject.FilterConstants.ZoneMode];
			zoneModeFilter.IsActive = true;

			zoneModeFilter.Property = RateMode.LSE;
			var collection = new RateTransportZonesCollection(Factory);
			var foundZones = collection.Find(filter.Filter);

			var expectedZones = new[] { lseZone };
			AssertContainsExactElementsInAnyOrder(expectedZones, foundZones);

			zoneModeFilter.Property = RateMode.SEA;
			foundZones = collection.Find(filter.Filter);
			expectedZones = new[] { seaCNZone, seaAUZone };
			AssertContainsExactElementsInAnyOrder(expectedZones, foundZones);

			zoneModeFilter.Property = RateMode.MAI;
			foundZones = collection.Find(filter.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<RateTransportProvider>(), foundZones);
		}

		RateTransportProvider GetZoneSet(ZString type, string mode, string country = CountryCodes.Australia)
		{
			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			zoneSet.TP_ZoneType = type;
			zoneSet.TP_ZoneMode = mode;
			zoneSet.TP_RN_NKCountry = country;

			return zoneSet;
		}

		static RateTransportZone AddZone(RateTransportProvider parent, ZString name)
		{
			var zone = parent.Zones.AddNew();
			zone.TZ_ZoneName = name;

			return zone;
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RateTransportZoneFilterBusinessObject();
		}

		#endregion
	}
}
