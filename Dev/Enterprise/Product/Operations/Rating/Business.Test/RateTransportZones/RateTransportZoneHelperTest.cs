using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Integration.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZone))]
	public class RateTransportZoneHelperTest : EnterpriseBusinessObjectTestCase
	{
		#region Get Transport Zones Sets for International Zones

		public void TestGetTransportZoneSetsForInternationalZones()
		{
			var taxHavens = Factory.NewWithValidTestData<RefZoneHeader>();
			taxHavens.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Panama));
			taxHavens.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.BritishVirginIslands));

			var croniesInc = Factory.NewWithValidTestData<OrgHeader>();
			croniesInc.OH_IsShippingProvider = true;

			var russianZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			russianZoneSet.TP_OH_RelatedParty = croniesInc.PK;
			russianZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Russia;

			var panamaZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			panamaZoneSet.TP_OH_RelatedParty = croniesInc.PK;
			panamaZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Panama;

			var islandsZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			islandsZoneSet.TP_RN_NKCountry = Constants.CountryCodes.BritishVirginIslands;

			Factory.Save();

			var actual = RateTransportZoneHelper.GetTransportZoneSets(taxHavens, croniesInc, "", Core.Constants.RateMode.ALL, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { islandsZoneSet, panamaZoneSet }, actual);

			var islandsZoneSetForCronies = Factory.NewWithValidTestData<RateTransportProvider>();
			islandsZoneSetForCronies.TP_OH_RelatedParty = croniesInc.PK;
			islandsZoneSetForCronies.TP_RN_NKCountry = Constants.CountryCodes.BritishVirginIslands;

			actual = RateTransportZoneHelper.GetTransportZoneSets(taxHavens, croniesInc, "", Core.Constants.RateMode.ALL, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { islandsZoneSetForCronies, panamaZoneSet }, actual);

			taxHavens.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Russia));

			actual = RateTransportZoneHelper.GetTransportZoneSets(taxHavens, croniesInc, "", Core.Constants.RateMode.ALL, Factory);

			AssertContainsExactElementsInAnyOrder(new[] { russianZoneSet, islandsZoneSetForCronies, panamaZoneSet }, actual);
		}

		public void TestGetTransportZoneSetsDoesntReturnNull()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsShippingProvider = true;

			AssertExceptionThrown<ArgumentNullException>(() => RateTransportZoneHelper.GetTransportZoneSets(null, testOrg, "", Core.Constants.RateMode.ALL, Factory));

			var zoneHeaderWithoutCountries = Factory.New<RefZoneHeader>();

			var zoneSets = RateTransportZoneHelper.GetTransportZoneSets(zoneHeaderWithoutCountries, testOrg, "", Core.Constants.RateMode.ALL, Factory);
			AssertEquals(Enumerable.Empty<RateTransportProvider>(), zoneSets);
		}

		#endregion

		#region Get Transport Zone Set

		public void TestGetZoneSet_ByLocation()
		{
			var australianZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			australianZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var zone = australianZoneSet.Zones.AddNew();
			zone.TZ_ZoneName = "Australian";

			var sydneyZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			sydneyZoneSet.TP_R9_ZoneHubLocation = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney")).PK;
			zone = sydneyZoneSet.Zones.AddNew();
			zone.TZ_ZoneName = "Sydney Wide";

			Factory.Save();

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(australianZoneSet, result);

			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			result = RateTransportZoneHelper.GetMostApplicableZoneSet(sydney, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(sydneyZoneSet, result);

			var melbourne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			result = RateTransportZoneHelper.GetMostApplicableZoneSet(melbourne, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("Should fall back to the Australian zone set as no Melbourne zone set exists", australianZoneSet, result);
		}

		public void TestGetZoneSet_IfCountrySetWhenHubIsSetInDb()
		{
			var zoneSetExpected = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSetExpected.TP_R9_ZoneHubLocation = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney")).PK;
			var zone = zoneSetExpected.Zones.AddNew();
			zone.TZ_ZoneName = "Sydney Wide";

			var australianZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			australianZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var zoneAu = australianZoneSet.Zones.AddNew();
			zoneAu.TZ_ZoneName = "Australian";

			for (int i = 0; i < 17; i++)
			{
				var zoneSetExtra = Factory.NewWithValidTestData<RateTransportProvider>();
				zoneSetExtra.TP_R9_ZoneHubLocation = Factory.NewWithValidTestData<RefCityTown>().PK;
				zoneSetExtra.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			}

			Factory.Save();
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(sydney, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(zoneSetExpected, result);

			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(australianZoneSet, result);
		}

		public void TestGetZoneSet_IfCountryAndHubIsSetInDb_GetByHub()
		{
			var australianZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			australianZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var auZone = australianZoneSet.Zones.AddNew();
			auZone.TZ_ZoneName = "Australian";

			var zoneSetExpected = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSetExpected.TP_R9_ZoneHubLocation = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Sydney")).PK;
			zoneSetExpected.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var sydZone = zoneSetExpected.Zones.AddNew();
			sydZone.TZ_ZoneName = "Sydney Wide";

			Factory.Save();
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(sydney, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(zoneSetExpected, result);
		}

		public void TestGetZoneSet_ByOwner()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = owner.PK;
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var genericZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			genericZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			Factory.Save();

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, owner, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(zoneSet, result);

			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("Expected to find the generic zone", genericZoneSet, result);

			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, anotherOrg, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("Expected to fall back to generic zone", genericZoneSet, result);
		}

		public void TestGetZoneSet_ByType()
		{
			var ratingZoneSet = Factory.New<RateTransportProvider>();
			ratingZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			ratingZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			var allZoneSet = Factory.New<RateTransportProvider>();
			allZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			allZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			Factory.Save();

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(allZoneSet, result);

			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL, Factory);

			AssertEquals(ratingZoneSet, result);

			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.Reporting, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("There is no reporting specific zone set so fall back to the 'all' zone", allZoneSet, result);
		}

		public void TestGetZoneSet_ByType_ByMode()
		{
			Func<string, string, string> createTransportZone = (type, mode) =>
			{
				var zoneSet = Factory.New<RateTransportProvider>();
				zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
				zoneSet.TP_ZoneType = type;
				zoneSet.TP_ZoneMode = mode;

				var zone = zoneSet.Zones.AddNew();
				zone.TZ_ZoneName = type + "-" + mode + " Zone";

				return zone.TZ_ZoneName;
			};

			var allZone = createTransportZone(RatingConstants.RatingZoneTypes.All, Core.Constants.RateMode.ALL);
			var ratZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL);

			var airZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.AIR);
			var uldzone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ULD);

			var seaZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.SEA);
			var fclZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.FCL);

			var lroZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.LRO);
			var froZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.FRO);

			var raiZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.RAI);
			var fraZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.FRA);
			var fwlZone = createTransportZone(RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.FWL);

			Factory.Save();

			var helper = new TestHelper(Factory);
			var rateEntry = helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ALL, "", "AU");
			var rateLine = rateEntry.AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			AssertContainsExactElementsInAnyOrder("Expected only RAT for all.", new[] { ratZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertContainsExactElementsInAnyOrder("As there is no LSE Zone Mode, we fall back to Air zone", new[] { airZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertContainsExactElementsInAnyOrder(new[] { uldzone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.LCL;
			AssertContainsExactElementsInAnyOrder("As there is no LCL zone, we fall back to Sea Zone", new[] { seaZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.FCL;
			AssertContainsExactElementsInAnyOrder(new[] { fclZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.FTL;
			AssertContainsExactElementsInAnyOrder("As there is no FTL Zone, fall back to ROA, and as there was no RO", new[] { ratZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.FRO;
			AssertContainsExactElementsInAnyOrder(new[] { froZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.LRO;
			AssertContainsExactElementsInAnyOrder(new[] { lroZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.LRA;
			AssertContainsExactElementsInAnyOrder("As there is no LRA Zone, we fall back to RAI zone", new[] { raiZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.FRA;
			AssertContainsExactElementsInAnyOrder(new[] { fraZone }, rateLine.Lookups.Zones.GetAllCodes());

			rateEntry.TI_Mode = Core.Constants.RateMode.FWL;
			AssertContainsExactElementsInAnyOrder(new[] { fwlZone }, rateLine.Lookups.Zones.GetAllCodes());
		}

		public void TestGetZoneSetIgnoresInactiveZones()
		{
			var inactiveOperationsZoneSet = Factory.New<RateTransportProvider>();
			inactiveOperationsZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			inactiveOperationsZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			inactiveOperationsZoneSet.TP_IsActive = false;

			var inactiveRatingZoneSet = Factory.New<RateTransportProvider>();
			inactiveRatingZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			inactiveRatingZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			inactiveRatingZoneSet.TP_IsActive = false;

			var ratingZoneSet = Factory.New<RateTransportProvider>();
			ratingZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			ratingZoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			Factory.Save();

			var result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("Should only match the active zone", ratingZoneSet, result);

			result = RateTransportZoneHelper.GetMostApplicableZoneSet(Australia, null, RatingConstants.RatingZoneTypes.Operations, Core.Constants.RateMode.ALL, Factory);

			AssertEquals("Only active zone cannot be matched by type.", null, result);
		}

		#endregion

		#region Get Transport Zone From City/Town and Postcode

		public void TestGetZoneForPostCodeOrCityTown_ZoneItemsHaveCityTownsWithPostCodes()
		{
			var testCityNSW = Factory.New<RefCityTown>();
			testCityNSW.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			testCityNSW.R9_InternationalName = "Test City Name 1";
			testCityNSW.R9_RW_NKState = "NSW";

			var testCityWA = Factory.New<RefCityTown>();
			testCityWA.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			testCityWA.R9_InternationalName = "Test City Name 1";
			testCityWA.R9_RW_NKState = "WA";

			var anotherNSWCityTown = Factory.New<RefCityTown>();
			anotherNSWCityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			anotherNSWCityTown.R9_InternationalName = "Test City Name 2";

			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			postCode.RK_CityTownPostCode = "TEST2153";
			postCode.CityTowns.Add(testCityNSW);
			postCode.CityTowns.Add(anotherNSWCityTown);

			var parentZoneSet = Factory.New<RateTransportProvider>();
			parentZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var zone = parentZoneSet.Zones.AddNew();
			zone.Items.AddNew().TQ_R9_CityTown = testCityNSW.PK;

			Factory.Save();

			var resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, Constants.CountryCodes.Australia, string.Empty, testCityNSW.R9_InternationalName);
			AssertNull("No postcode provided", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, Constants.CountryCodes.Australia, postCode.RK_CityTownPostCode, testCityNSW.R9_InternationalName);
			AssertEquals(zone.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, Constants.CountryCodes.Australia, "4444", testCityWA.R9_InternationalName);
			AssertNull("psot code does not belong to city", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, Constants.CountryCodes.Australia, postCode.RK_CityTownPostCode, anotherNSWCityTown.R9_InternationalName);
			AssertNull("No zone for city", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, Constants.CountryCodes.Australia, postCode.RK_CityTownPostCode, string.Empty);
			AssertNull("No zone for postcode", resultZone);
		}

		public void TestGetZoneForPostCodeOrCityTown_ZoneItemsHaveCityTownsWithoutPostCodes()
		{
			#region Set up locations

			var postCode = Factory.New<RefPostCode>();
			postCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			postCode.RK_CityTownPostCode = "TEST2000";

			var cbd = Factory.New<RefCityTown>();
			cbd.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			cbd.R9_InternationalName = "Sydney CBD";
			cbd.PostCodes.Add(postCode);

			var mascot = Factory.New<RefCityTown>();
			mascot.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			mascot.R9_InternationalName = "Mascotty";
			mascot.R9_RW_NKState = "NSW";

			var alexandria = Factory.New<RefCityTown>();
			alexandria.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			alexandria.R9_InternationalName = "Alexandri";
			alexandria.R9_RW_NKState = "NSW";

			var egyptianAlexandria = Factory.New<RefCityTown>();
			egyptianAlexandria.R9_RN_NKCountry = Constants.CountryCodes.Egypt;
			egyptianAlexandria.R9_InternationalName = "Alexandri";

			#endregion

			#region Set up zones

			var parentZoneSet = Factory.New<RateTransportProvider>();
			parentZoneSet.TP_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			parentZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone1 = parentZoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			zone1.Items.AddNew().TQ_R9_CityTown = cbd.PK;

			var zone2 = parentZoneSet.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			zone2.Items.AddNew().TQ_R9_CityTown = mascot.PK;
			zone2.Items.AddNew().TQ_R9_CityTown = alexandria.PK;

			var egyptianZoneSet = Factory.New<RateTransportProvider>();
			egyptianZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Egypt;
			var egyptianZone = egyptianZoneSet.Zones.AddNew();
			egyptianZone.Items.AddNew().TQ_R9_CityTown = egyptianAlexandria.PK;

			#endregion

			Factory.Save();

			var resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cbd.R9_RN_NKCountry, "", cbd.R9_InternationalName);
			AssertNull("Cannot match CBD zone as the city/town has a post code, so just the name is not enough", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cbd.R9_RN_NKCountry, postCode.RK_CityTownPostCode, cbd.R9_InternationalName);
			AssertEquals("CBD has both name and post code, expected to be matched", zone1.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, mascot.R9_RN_NKCountry, null, mascot.R9_InternationalName);
			AssertEquals("Matched on name when there is no post code on the city/town", zone2.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, mascot.R9_RN_NKCountry, "2020", mascot.R9_InternationalName);
			AssertEquals("No zone matches this post code but we can match the name", zone2.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, alexandria.R9_RN_NKCountry, "", alexandria.R9_InternationalName);
			AssertEquals("Should not have been confused with the Egyptian zone", zone2.PK, resultZone.PK);
		}

		public void TestGetZoneForPostCodeOrCityTown_PostCodeOnly()
		{
			var zoneSet = Factory.New<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone1 = zoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";

			var item11 = zone1.Items.AddNew();
			var postCode2000 = Factory.New<RefPostCode>();
			postCode2000.RK_CityTownPostCode = "2000";
			item11.TQ_FromPostCode = postCode2000.RK_CityTownPostCode;
			var postCode2999 = Factory.New<RefPostCode>();
			postCode2999.RK_CityTownPostCode = "2999";
			item11.TQ_ToPostCode = postCode2999.RK_CityTownPostCode;

			var item12 = zone1.Items.AddNew();
			var postCode3000 = Factory.New<RefPostCode>();
			postCode3000.RK_CityTownPostCode = "3000";
			item12.TQ_FromPostCode = postCode3000.RK_CityTownPostCode;
			var postCode3500 = Factory.New<RefPostCode>();
			postCode3500.RK_CityTownPostCode = "3500";
			item12.TQ_ToPostCode = postCode3500.RK_CityTownPostCode;

			var zone2 = zoneSet.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";

			var item21 = zone2.Items.AddNew();
			var postCode3501 = Factory.New<RefPostCode>();
			postCode3501.RK_CityTownPostCode = "3501";
			item21.TQ_FromPostCode = postCode3501.RK_CityTownPostCode;
			var postCode4000 = Factory.New<RefPostCode>();
			postCode4000.RK_CityTownPostCode = "4000";
			item21.TQ_ToPostCode = postCode4000.RK_CityTownPostCode;

			var item22 = zone2.Items.AddNew();
			var postCode5000 = Factory.New<RefPostCode>();
			postCode5000.RK_CityTownPostCode = "5000";
			item22.TQ_FromPostCode = postCode5000.RK_CityTownPostCode;
			var postCode7000 = Factory.New<RefPostCode>();
			postCode7000.RK_CityTownPostCode = "7000";
			item22.TQ_ToPostCode = postCode7000.RK_CityTownPostCode;

			var item23 = zone2.Items.AddNew();
			var postCodeA1000 = Factory.New<RefPostCode>();
			postCodeA1000.RK_CityTownPostCode = "A1000";
			item23.TQ_FromPostCode = postCodeA1000.RK_CityTownPostCode;
			var postCodeB3000 = Factory.New<RefPostCode>();
			postCodeB3000.RK_CityTownPostCode = "B3000";
			item23.TQ_ToPostCode = postCodeB3000.RK_CityTownPostCode;

			Factory.Save();

			var result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "2000", "");
			AssertEquals("Zone 1 should be retrieved", zone1.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "3600", "");
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "9500", "");
			AssertNull("No zone should be retrieved", result);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "2999", "");
			AssertEquals("Zone 1 should be retrieved", zone1.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "6000", "");
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "A2000", "");
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "A3000", "");
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "B3001", "");
			AssertNull("No zone should be retrieved", result);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "B2500A", "");
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, "Z2500", "");
			AssertNull("No zone should be retrieved", result);
		}

		public void TestGetZoneForPostCodeOrCityTown_PostCodeAndCityTown()
		{
			var bellaVista = Factory.New<RefCityTown>();
			bellaVista.R9_InternationalName = "Bella vista";

			var baulko = Factory.New<RefCityTown>();
			baulko.R9_InternationalName = "Baulkham Hills";

			var commonPostcode = Factory.New<RefPostCode>();
			commonPostcode.RK_CityTownPostCode = "2153";

			bellaVista.PostCodes.Add(commonPostcode);
			baulko.PostCodes.Add(commonPostcode);

			var zoneSet = Factory.New<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var zone = zoneSet.Zones.AddNew();
			zone.Items.AddNew().TQ_FromPostCode = commonPostcode.RK_CityTownPostCode;

			Factory.Save();

			var resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, string.Empty, bellaVista.R9_InternationalName);
			AssertNull("No zone for just bella vista, and no postcode provided", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, commonPostcode.RK_CityTownPostCode, bellaVista.R9_InternationalName);
			AssertEquals(zone.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, commonPostcode.RK_CityTownPostCode, baulko.R9_InternationalName);
			AssertEquals(zone.PK, resultZone.PK);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, string.Empty, baulko.R9_InternationalName);
			AssertNull("No zone for just bella vista, and no postcode provided", resultZone);

			resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(zoneSet, Constants.CountryCodes.Australia, commonPostcode.RK_CityTownPostCode, string.Empty);
			AssertEquals(zone.PK, resultZone.PK);
		}

		public void TestGetZoneForPostCodeOrCityTown_IsExcludingPostCode()
		{
			#region Set up locations

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			postCode1.RK_CityTownPostCode = "TEST2001";

			var cityWithPostcode1 = Factory.New<RefCityTown>();
			cityWithPostcode1.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			cityWithPostcode1.R9_InternationalName = "City 1";
			cityWithPostcode1.PostCodes.Add(postCode1);

			var cityWithoutPostcode = Factory.New<RefCityTown>();
			cityWithoutPostcode.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			cityWithoutPostcode.R9_InternationalName = "Mascotty";
			cityWithoutPostcode.R9_RW_NKState = "NSW";

			#endregion

			#region Set up zones

			var parentZoneSet = Factory.New<RateTransportProvider>();
			parentZoneSet.TP_OH_RelatedParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			parentZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone1 = parentZoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone2";
			var item1 = zone1.Items.AddNew();
			item1.TQ_R9_CityTown = cityWithPostcode1.PK;
			item1.TQ_IsExcludingPostCode = true;

			var zone2 = parentZoneSet.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone3";
			var item2 = zone2.Items.AddNew();
			item2.TQ_R9_CityTown = cityWithoutPostcode.PK;
			item2.TQ_IsExcludingPostCode = true;

			#endregion

			Factory.Save();

			CombineAssertions(() =>
			{
				var resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cityWithPostcode1.R9_RN_NKCountry, "", cityWithPostcode1.R9_InternationalName);
				AssertEquals("Given a city with a post code and an address with just a name, can match on name only", zone1.PK, resultZone?.PK ?? ZGuid.Empty);

				resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cityWithPostcode1.R9_RN_NKCountry, "2222", cityWithPostcode1.R9_InternationalName);
				AssertEquals("Given a city with a post code and an address with a name and different postcode, can match on name", zone1.PK, resultZone?.PK ?? ZGuid.Empty);

				resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cityWithPostcode1.R9_RN_NKCountry, postCode1.RK_CityTownPostCode, "");
				AssertNull("Given a city with a post code and an address with postcode only, can not match", resultZone);

				resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cityWithoutPostcode.R9_RN_NKCountry, "", cityWithoutPostcode.R9_InternationalName);
				AssertEquals("Given a city with no post code and an address with just a name, can match on name only", zone2.PK, resultZone?.PK ?? ZGuid.Empty);

				resultZone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(parentZoneSet, cityWithoutPostcode.R9_RN_NKCountry, "2222", cityWithoutPostcode.R9_InternationalName);
				AssertEquals("Given a city with no post code and an address with a name and different postcode, can match on name", zone2.PK, resultZone?.PK ?? ZGuid.Empty);
			});
		}

		#endregion

		#region Get Transport Zone For Distance

		public void TestGetZoneFromDistance()
		{
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			var transportZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			transportZoneSet.TP_OH_RelatedParty = transportProvider.PK;
			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone1 = transportZoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone 1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromDistance = 0;
			zoneItem1.TQ_ToDistance = 25;

			var zone2 = transportZoneSet.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone 2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromDistance = 25;
			zoneItem2.TQ_ToDistance = 50;

			var genericZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			genericZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			Factory.Save();

			var result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 0);
			AssertNull("Expected no zone to be found as result must be greater than zero", result);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 0.001);
			AssertEquals("Zone 1 should be retrieved", zone1.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 5);
			AssertEquals("Zone 1 should be retrieved", zone1.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 25);
			AssertEquals("Zone 1 should be retrieved", zone1.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 49);
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 50);
			AssertEquals("Zone 2 should be retrieved", zone2.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 80);
			AssertNull("No zone in set covers a distance greater than 80", result);
		}

		public void TestGetZoneFromDistance_ZoneItemHasDifferentCountry()
		{
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			var transportZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			transportZoneSet.TP_OH_RelatedParty = transportProvider.PK;
			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var chinaZone = transportZoneSet.Zones.AddNew();
			chinaZone.TZ_ZoneName = "China Zone";
			var chinaZoneItem = chinaZone.Items.AddNew();
			chinaZoneItem.TQ_FromDistance = 0;
			chinaZoneItem.TQ_ToDistance = 1000;
			chinaZoneItem.TQ_RN_NKCountry = "CN";

			var malaysiaZone = transportZoneSet.Zones.AddNew();
			malaysiaZone.TZ_ZoneName = "Malaysia Zone";
			var malaysiaItemZone = malaysiaZone.Items.AddNew();
			malaysiaItemZone.TQ_FromDistance = 0;
			malaysiaItemZone.TQ_ToDistance = 1000;
			malaysiaItemZone.TQ_RN_NKCountry = "MY";

			Factory.Save();

			var result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Australia, 100);
			AssertNull("Expected no zone to be found as there is no zone item for AU", result);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.China, 100);
			AssertEquals("China Zone should be retrieved", chinaZone.PK, result.PK);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, null, 100);
			AssertNull("No zones should be returned when there is no country code", result);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, string.Empty, 100);
			AssertNull("No zones should be returned when there is no country code", result);

			result = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, Constants.CountryCodes.Malaysia, 100);
			AssertEquals("Malaysia Zone should be retrieved", malaysiaZone.PK, result.PK);
		}

		#endregion

		#region Get Zone Item

		public void TestIsCityTownMatching()
		{
			var postCode = Factory.NewWithValidTestData<RefPostCode>();
			postCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			postCode.RK_CityTownPostCode = "TEST1000";

			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			postCode2.RK_CityTownPostCode = "TEST2000";

			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			cityTown.R9_InternationalName = "International Space Station";
			cityTown.PostCodes.Add(postCode);

			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var zone = prov.Zones.AddNew();

			var zoneItem = zone.Items.AddNew();

			zoneItem.TQ_R9_CityTown = cityTown.PK;

			Factory.Save();

			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "International Space Station"));
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION"));
			Assert(!RateTransportZoneHelper.IsCityTownMatching(zoneItem, "Russian Space Station"));

			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", false, "TEST1000"));
			Assert(!RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", false, "2149"));

			zoneItem.TQ_R9_CityTown = ZGuid.Empty;
			zoneItem.TQ_FromPostCode = ZString.Empty;
			zoneItem.TQ_ToPostCode = ZString.Empty;
			Assert(!RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", true, "TEST1000"));
			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", true, "TEST1000"));
			zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", true, "TEST1000"));

			zoneItem.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", true, "TEST1000"));
		}

		public void TestIsCityTownMatchingUsingPostCodes()
		{
			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "1500";
			var postCode4 = Factory.New<RefPostCode>();
			postCode4.RK_CityTownPostCode = "1000";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "International Space Station";
			cityTown.PostCodes.Add(postCode3);

			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem.TQ_ToPostCode = postCode2.RK_CityTownPostCode;

			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "International Space Station"));
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION"));
			Assert(!RateTransportZoneHelper.IsCityTownMatching(zoneItem, "IntERNATioNAl sPACe sTatION", false));
			Assert(!RateTransportZoneHelper.IsCityTownMatching(zoneItem, "Russian Space Station"));

			cityTown.PostCodes.RemoveFromRelationship(postCode3);
			cityTown.PostCodes.Add(postCode4);
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "International Space Station"));

			zoneItem.TQ_ToPostCode = ZString.Empty;
			Assert(RateTransportZoneHelper.IsCityTownMatching(zoneItem, "International Space Station"));
		}

		public void TestIsPostCodeWithinRange()
		{
			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();

			var zoneItem = zone.Items.AddNew();
			var fromPostCode = Factory.New<RefPostCode>();
			fromPostCode.RK_CityTownPostCode = "1000";
			zoneItem.TQ_FromPostCode = fromPostCode.RK_CityTownPostCode;
			var toPostCode = Factory.New<RefPostCode>();
			toPostCode.RK_CityTownPostCode = "2000";
			zoneItem.TQ_ToPostCode = toPostCode.RK_CityTownPostCode;

			Assert(RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "1000"));
			Assert(RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "1500"));
			Assert(!RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "500"));
		}

		public void TestIsPostCodeWithinRangeUsingCityTowns()
		{
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();

			var postCode1 = Factory.NewWithValidTestData<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			var postCode2 = Factory.NewWithValidTestData<RefPostCode>();
			postCode2.RK_CityTownPostCode = "1500";

			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_InternationalName = "International Space Station";
			cityTown.PostCodes.Add(postCode1);
			cityTown.PostCodes.Add(postCode2);

			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_R9_CityTown = cityTown.PK;

			Factory.Save();

			var citiesPKsWithPostCode = RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(Factory, "1000");
			Assert(RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "1000", checkPostCodeCities: true, citiesPKsWithPostCode));

			citiesPKsWithPostCode = RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(Factory, "1500");
			Assert(RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "1500", checkPostCodeCities: true, citiesPKsWithPostCode));

			citiesPKsWithPostCode = RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(Factory, "500");
			Assert(!RateTransportZoneHelper.IsPostCodeWithinRange(zoneItem, "500", checkPostCodeCities: true, citiesPKsWithPostCode));
		}

		#endregion

		#region IRateTransportZoneHelper

		public void TestGetZoneName()
		{
			var fromPostCode = Factory.NewWithValidTestData<RefPostCode>();
			fromPostCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			fromPostCode.RK_CityTownPostCode = "TEST1000";

			var toPostCode = Factory.NewWithValidTestData<RefPostCode>();
			toPostCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			toPostCode.RK_CityTownPostCode = "TEST2000";

			var neverlandCityTown = Factory.NewWithValidTestData<RefCityTown>();
			neverlandCityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			neverlandCityTown.R9_InternationalName = "Neverland";

			var neverlandPostCode = Factory.NewWithValidTestData<RefPostCode>();
			neverlandPostCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			neverlandPostCode.RK_CityTownPostCode = "TEST6666";
			neverlandCityTown.PostCodes.Add(neverlandPostCode);

			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = transportProvider.PK;
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone1 = zoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone 1";
			var zoneItem1 = zone1.Items.AddNew();
			AssertEquals(Constants.CountryCodes.Australia, zoneItem1.TQ_RN_NKCountry);
			zoneItem1.TQ_FromPostCode = fromPostCode.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = toPostCode.RK_CityTownPostCode;

			var zone2 = zoneSet.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone 2";
			var zoneItem2 = zone2.Items.AddNew();
			AssertEquals(Constants.CountryCodes.Australia, zoneItem2.TQ_RN_NKCountry);
			zoneItem2.TQ_R9_CityTown = neverlandCityTown.PK;

			Factory.Save();

			IRateTransportZoneHelper helper = new RateTransportZoneHelper();

			var result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "", "");
			AssertEquals("No zone should be retrieved", "", result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "TEST1200", "");
			AssertEquals("Zone1 name should be retrieved from postcode", "Zone 1", result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "TEST2000", "");
			AssertEquals("Zone1 name should be retrieved from postcode", "Zone 1", result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "Alwaysland", "");
			AssertEquals("No zone should be retrieved - no matching postcode/suburb", "", result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "TEST6666", "Neverland");
			AssertEquals("Zone2 name should be retrieved from suburb", "Zone 2", result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "", "Neverland");
			AssertEquals("Nothing retrieved if no postcode passed in and city has linked postcodes", string.Empty, result);

			result = helper.GetZoneName(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "", "Alwaysland");
			AssertEquals("No zone should be retrieved - no matching postcode/suburb", string.Empty, result);
		}

		public void TestIsBeyond()
		{
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = transportProvider.PK;
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var zone = zoneSet.Zones.AddNew();
			zone.TZ_ZoneName = "Zone 1";
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_IsBeyond = true;
			var zoneItemFromPostCode = Factory.NewWithValidTestData<RefPostCode>();
			zoneItemFromPostCode.RK_CityTownPostCode = "1000";
			zoneItem.TQ_FromPostCode = zoneItemFromPostCode.RK_CityTownPostCode;
			var zoneItemToPostCode = Factory.NewWithValidTestData<RefPostCode>();
			zoneItemToPostCode.RK_CityTownPostCode = "2000";
			zoneItem.TQ_ToPostCode = zoneItemToPostCode.RK_CityTownPostCode;

			Factory.Save();

			IRateTransportZoneHelper helper = new RateTransportZoneHelper();

			var result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "", "");
			AssertEquals("No zone should be retrieved", false, result);

			result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "1200", "");
			AssertEquals("Zone should be retrieved from postcode", true, result);

			result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "2000", "");
			AssertEquals("Zone should be retrieved from postcode", true, result);

			result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "Alwaysland", "");
			AssertEquals("No zone should be retrieved - no matching postcode", false, result);

			zoneItem.TQ_IsBeyond = false;

			result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "1200", "");
			AssertEquals("Zone should be retrieved from postcode", false, result);

			result = helper.IsBeyond(Factory, transportProvider, Australia, Constants.CountryCodes.Australia, "2000", "");
			AssertEquals("Zone should be retrieved from postcode", false, result);
		}

		public void TestIsBeyond_ZoneItemHasDifferentCountry()
		{
			var germany = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Germany);
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = transportProvider.PK;
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Germany;

			var zoneItemFromPostCode = Factory.NewWithValidTestData<RefPostCode>();
			zoneItemFromPostCode.RK_CityTownPostCode = "1000";

			var zoneItemToPostCode = Factory.NewWithValidTestData<RefPostCode>();
			zoneItemToPostCode.RK_CityTownPostCode = "2000";

			var belgiumZone = zoneSet.Zones.AddNew();
			belgiumZone.TZ_ZoneName = "Belgium Zone";
			var belgiumZoneItem = belgiumZone.Items.AddNew();
			belgiumZoneItem.TQ_IsBeyond = true;
			belgiumZoneItem.TQ_FromPostCode = zoneItemFromPostCode.RK_CityTownPostCode;
			belgiumZoneItem.TQ_ToPostCode = zoneItemToPostCode.RK_CityTownPostCode;
			belgiumZoneItem.TQ_RN_NKCountry = Constants.CountryCodes.Belgium;

			var franceZone = zoneSet.Zones.AddNew();
			franceZone.TZ_ZoneName = "France Zone";
			var franceZoneItem = franceZone.Items.AddNew();
			franceZoneItem.TQ_IsBeyond = true;
			franceZoneItem.TQ_FromPostCode = zoneItemFromPostCode.RK_CityTownPostCode;
			franceZoneItem.TQ_ToPostCode = zoneItemToPostCode.RK_CityTownPostCode;
			franceZoneItem.TQ_RN_NKCountry = Constants.CountryCodes.France;

			Factory.Save();

			IRateTransportZoneHelper helper = new RateTransportZoneHelper();

			var result = helper.IsBeyond(Factory, transportProvider, germany, Constants.CountryCodes.Australia, "1200", "");
			AssertEquals(false, result);

			result = helper.IsBeyond(Factory, transportProvider, germany, Constants.CountryCodes.France, "1200", "");
			AssertEquals(true, result);

			result = helper.IsBeyond(Factory, transportProvider, germany, Constants.CountryCodes.Belgium, "1500", "");
			AssertEquals(true, result);

			belgiumZoneItem.TQ_IsBeyond = false;
			franceZoneItem.TQ_IsBeyond = false;

			result = helper.IsBeyond(Factory, transportProvider, germany, Constants.CountryCodes.France, "1200", "");
			AssertEquals(false, result);

			result = helper.IsBeyond(Factory, transportProvider, germany, Constants.CountryCodes.Belgium, "1500", "");
			AssertEquals(false, result);
		}

		#endregion

		#region Performance

		public void TestGetZoneNameDbHits()
		{
			TestCaseHelper.ClearTable(RateTransportProviderSchema.Constants.TableName);
			Factory.ResetDatabaseLoadCount();

			var factory = new BusinessObjectFactory();

			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = ZGuid.Empty;
			provider.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			var zone1 = provider.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone 1";
			var zoneItem1 = zone1.Items.AddNew();
			var zoneItem1FromPostCode = factory.NewWithValidTestData<RefPostCode>();
			zoneItem1FromPostCode.RK_CityTownPostCode = "1000";
			zoneItem1.TQ_FromPostCode = zoneItem1FromPostCode.RK_CityTownPostCode;
			var zoneItem1ToPostCode = factory.NewWithValidTestData<RefPostCode>();
			zoneItem1ToPostCode.RK_CityTownPostCode = "2000";
			zoneItem1.TQ_ToPostCode = zoneItem1ToPostCode.RK_CityTownPostCode;

			var zone2 = provider.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone 2";
			var zoneItem2 = zone2.Items.AddNew();
			var zoneItem2CityTown = factory.NewWithValidTestData<RefCityTown>();
			zoneItem2CityTown.R9_InternationalName = "Neverland";
			zoneItem2.TQ_R9_CityTown = zoneItem2CityTown.PK;

			factory.Save();

			IRateTransportZoneHelper helper = new RateTransportZoneHelper();

			var result = helper.GetZoneName(Factory, null, Australia, Constants.CountryCodes.Australia, "1200", "");
			AssertEquals("Zone 1", result);

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateTransportZonesSchema.Constants.TableName, 1 },
				{ RateTransportProviderSchema.Constants.TableName, 1 },
				{ RefCountrySchema.Constants.TableName, 1 },
			};

			AssertMaxDbHits(tableHitDictionary, Factory);
		}

		public void TestGetZoneNameDbHits_RefCityPCodePivot()
		{
			TestCaseHelper.ClearTable(RateTransportProviderSchema.Constants.TableName);
			Factory.ResetDatabaseLoadCount();

			var factory = new BusinessObjectFactory();

			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = ZGuid.Empty;
			provider.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			provider.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = "Zone 1";
			var zoneItem1 = zone.Items.AddNew();
			var zoneItem1FromPostCode = factory.NewWithValidTestData<RefPostCode>();
			zoneItem1FromPostCode.RK_CityTownPostCode = "2000";
			zoneItem1.TQ_FromPostCode = zoneItem1FromPostCode.RK_CityTownPostCode;
			var zoneItem1ToPostCode = factory.NewWithValidTestData<RefPostCode>();
			zoneItem1ToPostCode.RK_CityTownPostCode = "3000";
			zoneItem1.TQ_ToPostCode = zoneItem1ToPostCode.RK_CityTownPostCode;

			var zoneItem2 = zone.Items.AddNew();
			var zoneItem2CityTown = factory.NewWithValidTestData<RefCityTown>();
			zoneItem2CityTown.R9_InternationalName = "Sydney Metro";
			zoneItem2CityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			var zoneItem2PostCode = zoneItem2CityTown.PostCodes.AddNew();
			zoneItem2PostCode.RK_CityTownPostCode = "2000";
			var zoneItem3PostCode = zoneItem2CityTown.PostCodes.AddNew();
			zoneItem3PostCode.RK_CityTownPostCode = "2200";
			var zoneItem4PostCode = zoneItem2CityTown.PostCodes.AddNew();
			zoneItem4PostCode.RK_CityTownPostCode = "2300";
			var zoneItem5PostCode = zoneItem2CityTown.PostCodes.AddNew();
			zoneItem5PostCode.RK_CityTownPostCode = "2400";
			zoneItem2.TQ_R9_CityTown = zoneItem2CityTown.PK;

			var zoneItem3 = zone.Items.AddNew();
			var zoneItem3CityTown = factory.NewWithValidTestData<RefCityTown>();
			zoneItem3CityTown.R9_InternationalName = "Sydney Suburban";
			zoneItem3CityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			var zoneItem6PostCode = zoneItem3CityTown.PostCodes.AddNew();
			zoneItem6PostCode.RK_CityTownPostCode = "2800";
			zoneItem3.TQ_R9_CityTown = zoneItem3CityTown.PK;

			var zoneItem4 = zone.Items.AddNew();
			var zoneItem4CityTown = factory.NewWithValidTestData<RefCityTown>();
			zoneItem4CityTown.R9_InternationalName = "Sydney Suburban";
			zoneItem4CityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			zoneItem4CityTown.R9_RW_NKState = "NSW";
			var zoneItem7PostCode = zoneItem4CityTown.PostCodes.AddNew();
			zoneItem7PostCode.RK_CityTownPostCode = "2800";
			zoneItem4.TQ_R9_CityTown = zoneItem4CityTown.PK;

			factory.Save();

			IRateTransportZoneHelper helper = new RateTransportZoneHelper();

			var result = helper.GetZoneName(Factory, null, Australia, Constants.CountryCodes.Australia, "2200", "");
			AssertEquals("Pre-condition, we loaded the zone for the criteria", "Zone 1", result);

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateTransportZonesSchema.Constants.TableName, 1 },
				{ RateTransportProviderSchema.Constants.TableName, 1 },
				{ RefCountrySchema.Constants.TableName, 1 },
			};

			AssertMaxDbHits(tableHitDictionary, Factory);
		}

		#endregion

		#region Implementation

		RefCountry australia;
		RefCountry Australia
		{
			get { return australia ?? (australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia)); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RateTransportProvider>().Zones.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory newFactory)
		{
			var prov = newFactory.NewWithValidTestData<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var item = zone.Items.AddNew();
			item.TQ_ToDistance = 10;

			return zone;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion

		#region Get City Via Post Code

		public void TestGetCitiesWithPostCode()
		{
			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_InternationalName = "International Space Station";

			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_InternationalName = "Another City";

			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_InternationalName = "Yet Another City";

			var cityTown3 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown3.R9_InternationalName = "Crowded City";

			var postCode = Factory.NewWithValidTestData<RefPostCode>();
			postCode.RK_CityTownPostCode = "1000";

			cityTown.PostCodes.Add(postCode);
			cityTown1.PostCodes.Add(postCode);
			cityTown2.PostCodes.Add(postCode);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should returns all cities with post code 1000",
				RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(Factory, "1000"), new List<ZGuid> { cityTown.PK, cityTown1.PK, cityTown2.PK });

			AssertContainsExactElementsInAnyOrder("Should returns empty list for post code 1758",
				RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(Factory, "1758"), new List<ZGuid>());
		}

		#endregion
	}

	[CodeAlive("Address DeadCodeTest::TestNoDeadCode, not in scope for WI00610205 - Activate IDE0051 in Enterprise/Product/Operations/Freight; usage in Freight.Business/TestConfiguration.xml and Freight.DataTransfer.Test/TestConfiguration.xml")]
	public class RateTransportZoneTestHelper : IRateTransportZoneTestHelper
	{
		#region IRateTransportZoneTestHelper Members

		void IRateTransportZoneTestHelper.CreateRateTransportZones(object newFactory, IOrgHeader transportProvider)
		{
			this.factory = (BusinessObjectFactory)newFactory;
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_OH_RelatedParty = transportProvider.PK;
			provider.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var previousZone = string.Empty;
			var previousPostalRegion = ZString.Empty;
			RateTransportZone zone = null;

			foreach (var zoneTestData in Zones)
			{
				var zoneTestDataElements = new List<string>(zoneTestData.Split(','));

				if (!zoneTestDataElements[0].Equals(previousZone))
				{
					previousZone = zoneTestDataElements[0];
					zone = provider.Zones.AddNew();
					zone.TZ_ZoneName = zoneTestDataElements[0];
				}

				var currentPostalRegion = ZString.Format("{0},{1},{2}", zoneTestDataElements[1], zoneTestDataElements[2], zoneTestDataElements[3]);
				if (!currentPostalRegion.Equals(previousPostalRegion))
				{
					previousPostalRegion = currentPostalRegion;
					var zoneItem = zone.Items.AddNew();

					if (!string.IsNullOrEmpty(zoneTestDataElements[1]))
					{
						var postCode = Factory.New<RefPostCode>();
						postCode.RK_CityTownPostCode = zoneTestDataElements[1];
						zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
						postCode = Factory.New<RefPostCode>();
						postCode.RK_CityTownPostCode = zoneTestDataElements[2];
						zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
					}
					else
					{
						var cityTown = Factory.New<RefCityTown>();
						cityTown.R9_InternationalName = zoneTestDataElements[3];
						zoneItem.TQ_R9_CityTown = cityTown.PK;
					}
				}
			}
			Factory.Save();
		}

		void IRateTransportZoneTestHelper.AddTestZone(string zoneName, string postcodeFrom, string postcodeTo, string postcodeCitySuburb)
		{
			Zones.Add(string.Format(zoneSetupFormatMask, zoneName, postcodeFrom, postcodeTo, postcodeCitySuburb));
		}

		#endregion

		List<string> Zones
		{
			get { return zones ?? (zones = new List<string>()); }
		}
		List<string> zones;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		const string zoneSetupFormatMask = "{0},{1},{2},{3}";
	}
}
