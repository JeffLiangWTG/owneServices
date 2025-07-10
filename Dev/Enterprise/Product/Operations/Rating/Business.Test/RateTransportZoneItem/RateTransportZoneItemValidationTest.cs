using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateTransportZoneItemValidationTest : BusinessObjectValidationTestCase
	{
		#region Check column override tests

		public void TestDistanceColumns()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;
			var zone = provider.Zones.AddNew();

			var errorString = "'From Distance' must be less than 'To Distance'.";

			var zoneItemFromLessThanTo = zone.Items.AddNew();
			zoneItemFromLessThanTo.TQ_FromDistance = 1;
			zoneItemFromLessThanTo.TQ_ToDistance = 2;
			AssertNoError(zoneItemFromLessThanTo.TQ_FromDistanceInfo, errorString);
			AssertNoError(zoneItemFromLessThanTo.TQ_ToDistanceInfo, errorString);

			var zoneItemFromEqualsTo = zone.Items.AddNew();
			zoneItemFromEqualsTo.TQ_FromDistance = 1;
			zoneItemFromEqualsTo.TQ_ToDistance = 1;
			AssertHasError(zoneItemFromEqualsTo.TQ_FromDistanceInfo, errorString);
			AssertHasError(zoneItemFromEqualsTo.TQ_ToDistanceInfo, errorString);

			var zoneItemFromGreaterThanTo = zone.Items.AddNew();
			zoneItemFromGreaterThanTo.TQ_FromDistance = 2;
			zoneItemFromGreaterThanTo.TQ_ToDistance = 1;
			AssertHasError(zoneItemFromGreaterThanTo.TQ_FromDistanceInfo, errorString);
			AssertHasError(zoneItemFromGreaterThanTo.TQ_ToDistanceInfo, errorString);
		}

		public void TestPostCodeColumns()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;
			var zone = provider.Zones.AddNew();
			var postCodeL = "1000";
			var postCodeG = "2000";

			var errorString = "'From Postcode' must be less than 'To Postcode'.";

			var zoneItemFromLessThanTo = zone.Items.AddNew();
			zoneItemFromLessThanTo.TQ_FromPostCode = postCodeL;
			zoneItemFromLessThanTo.TQ_ToPostCode = postCodeG;
			AssertNoError(zoneItemFromLessThanTo.TQ_FromPostCodeInfo, errorString);
			AssertNoError(zoneItemFromLessThanTo.TQ_ToPostCodeInfo, errorString);

			var zoneItemFromEqualsTo = zone.Items.AddNew();
			zoneItemFromEqualsTo.TQ_FromPostCode = postCodeL;
			zoneItemFromEqualsTo.TQ_ToPostCode = postCodeL;
			AssertNoError(zoneItemFromEqualsTo.TQ_FromPostCodeInfo, errorString);
			AssertNoError(zoneItemFromEqualsTo.TQ_ToPostCodeInfo, errorString);

			var zoneItemFromGreaterThanTo = zone.Items.AddNew();
			zoneItemFromGreaterThanTo.TQ_FromPostCode = postCodeG;
			zoneItemFromGreaterThanTo.TQ_ToPostCode = postCodeL;
			AssertHasError(zoneItemFromGreaterThanTo.TQ_FromPostCodeInfo, errorString);
			AssertHasError(zoneItemFromGreaterThanTo.TQ_ToPostCodeInfo, errorString);

			var zoneItemHasNoFrom = zone.Items.AddNew();
			zoneItemHasNoFrom.TQ_ToPostCode = postCodeG;
			AssertHasError(zoneItemHasNoFrom.TQ_ToPostCodeInfo, "'To Postcode' can only be entered if 'From Postcode' is also entered.");
		}

		public void TestPostCodeWithCityTown()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;
			var zone = provider.Zones.AddNew();
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "3000";

			var cityTownAU = Factory.NewWithValidTestData<RefCityTown>();
			cityTownAU.R9_InternationalName = "Sydney";
			cityTownAU.R9_RN_NKCountry = "AU";
			var anotherCityTown = Factory.NewWithValidTestData<RefCityTown>();

			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_R9 = cityTownAU.PK;
			pivot1.R0_RK = postCode1.PK;

			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_R9 = cityTownAU.PK;
			pivot2.R0_RK = postCode2.PK;

			var errorString = "'From Postcode' must belong to the City/Town specified.";

			var zoneItem1 = zone.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			AssertNoError(zoneItem1.TQ_FromPostCodeInfo, errorString);
			AssertNoError(zoneItem1.TQ_ToPostCodeInfo, errorString);
			AssertNoError(zoneItem1.TQ_R9_CityTownInfo, errorString);

			zoneItem1.TQ_ToPostCode = ZString.Empty;
			zoneItem1.TQ_R9_CityTown = cityTownAU.PK;
			AssertNoError(zoneItem1.TQ_R9_CityTownInfo, errorString);

			zoneItem1.TQ_FromPostCode = postCode3.RK_CityTownPostCode;
			AssertHasError(zoneItem1.TQ_FromPostCodeInfo, errorString);
			AssertHasError(zoneItem1.TQ_R9_CityTownInfo, errorString);

			zoneItem1.TQ_R9_CityTown = ZGuid.Empty;
			AssertNoError(zoneItem1.TQ_FromPostCodeInfo, errorString);
			AssertNoError(zoneItem1.TQ_R9_CityTownInfo, errorString);

			zoneItem1.TQ_R9_CityTown = anotherCityTown.PK;
			AssertHasError(zoneItem1.TQ_FromPostCodeInfo, errorString);
			AssertHasError(zoneItem1.TQ_R9_CityTownInfo, errorString);
		}

		public void TestPostcodeAndCityTownCombinationValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_OH_RelatedParty = org.PK;
			provider.TP_RN_NKCountry = "AU";
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = "Zone1";
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			postCode1.RK_RN_NKCountry = "AU";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			postCode2.RK_RN_NKCountry = "AU";

			var cityTownAU = Factory.NewWithValidTestData<RefCityTown>();
			cityTownAU.R9_InternationalName = "Sydney";
			cityTownAU.R9_RN_NKCountry = "AU";
			var anotherCityTown = Factory.NewWithValidTestData<RefCityTown>();
			anotherCityTown.R9_InternationalName = "The Rocks";
			anotherCityTown.R9_RN_NKCountry = "AU";

			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_R9 = cityTownAU.PK;
			pivot1.R0_RK = postCode1.PK;

			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_R9 = cityTownAU.PK;
			pivot2.R0_RK = postCode2.PK;

			var pivot3 = Factory.New<RefCityPCodePivot>();
			pivot3.R0_R9 = anotherCityTown.PK;
			pivot3.R0_RK = postCode2.PK;

			var zoneItem1 = zone.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_R9_CityTown = cityTownAU.PK;

			var zoneItem2 = zone.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode2.RK_CityTownPostCode;
			zoneItem2.TQ_R9_CityTown = cityTownAU.PK;

			AssertNoErrors(zoneItem2.TQ_FromPostCodeInfo);
			AssertNoErrors(zoneItem2.TQ_ToPostCodeInfo);
			AssertNoErrors(zoneItem2.TQ_R9_CityTownInfo);

			zoneItem1.TQ_FromPostCode = postCode2.RK_CityTownPostCode;
			zoneItem1.TQ_R9_CityTown = anotherCityTown.PK;

			AssertNoErrors(zoneItem1.TQ_FromPostCodeInfo);
			AssertNoErrors(zoneItem1.TQ_ToPostCodeInfo);
			AssertNoErrors(zoneItem1.TQ_R9_CityTownInfo);

			zoneItem1.TQ_R9_CityTown = cityTownAU.PK;
			AssertHasError(zoneItem1.TQ_R9_CityTownInfo, "This country/region (AU) + city/town (Sydney) + postcode (2000) is already part of zone (Zone1) as a country/region (AU) + city/town (Sydney) + postcode (2000).");

			var zone2 = provider.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem3 = zone.Items.AddNew();
			zoneItem3.TQ_FromPostCode = postCode2.RK_CityTownPostCode;
			zoneItem3.TQ_R9_CityTown = cityTownAU.PK;
			AssertHasError(zoneItem3.TQ_R9_CityTownInfo, "This country/region (AU) + city/town (Sydney) + postcode (2000) is already part of zone (Zone1) as a country/region (AU) + city/town (Sydney) + postcode (2000).");
			AssertHasError(zoneItem3.TQ_FromPostCodeInfo, "This country/region (AU) + city/town (Sydney) + postcode (2000) is already part of zone (Zone1) as a country/region (AU) + city/town (Sydney) + postcode (2000).");

			zoneItem3.TQ_R9_CityTown = anotherCityTown.PK;
			AssertNoErrors(zoneItem3.TQ_FromPostCodeInfo);
			AssertNoErrors(zoneItem3.TQ_ToPostCodeInfo);
			AssertNoErrors(zoneItem3.TQ_R9_CityTownInfo);
		}

		#endregion

		#region Match zone set and city/town/postcode country

		public void TestCityTownCountry()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_RN_NKCountry = "AU";
			var zone = provider.Zones.AddNew();
			var item = zone.Items.AddNew();

			var cityTownAU = Factory.NewWithValidTestData<RefCityTown>();
			cityTownAU.R9_InternationalName = "Sydney";
			cityTownAU.R9_RN_NKCountry = "AU";

			var cityTownZA = Factory.NewWithValidTestData<RefCityTown>();
			cityTownZA.R9_InternationalName = "East London";
			cityTownZA.R9_RN_NKCountry = "ZA";

			var errorString = "Must belong to the country/region AU.";

			item.TQ_R9_CityTown = cityTownAU.PK;
			AssertNoError(item.TQ_R9_CityTownInfo, errorString);
			item.TQ_R9_CityTown = cityTownZA.PK;
			AssertHasError(item.TQ_R9_CityTownInfo, errorString);
			item.TQ_R9_CityTown = cityTownAU.PK;
			AssertNoError(item.TQ_R9_CityTownInfo, errorString);
		}

		public void TestPostcodeCountry()
		{
			var postCodeAU1000 = Factory.NewWithValidTestData<RefPostCode>();
			postCodeAU1000.RK_CityTownPostCode = "1000";
			postCodeAU1000.RK_RN_NKCountry = "AU";
			var postCodeAU2000 = Factory.NewWithValidTestData<RefPostCode>();
			postCodeAU2000.RK_CityTownPostCode = "2000";
			postCodeAU2000.RK_RN_NKCountry = "AU";
			var postCodeAU3000 = Factory.NewWithValidTestData<RefPostCode>();
			postCodeAU3000.RK_CityTownPostCode = "3000";
			postCodeAU3000.RK_RN_NKCountry = "AU";

			var postCodeZA1000 = Factory.NewWithValidTestData<RefPostCode>();
			postCodeZA1000.RK_CityTownPostCode = "1000";
			postCodeZA1000.RK_RN_NKCountry = "ZA";
			var postCodeZA2000 = Factory.NewWithValidTestData<RefPostCode>();
			postCodeZA2000.RK_CityTownPostCode = "2000";
			postCodeZA2000.RK_RN_NKCountry = "ZA";

			var errorString = "Postcodes must belong to the country/region ZA.";

			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_RN_NKCountry = "AU";
			var zone = provider.Zones.AddNew();
			var item = zone.Items.AddNew();
			item.TQ_FromPostCode = postCodeAU1000.RK_CityTownPostCode;
			AssertNoError(item.TQ_FromPostCodeInfo, errorString);
			item.TQ_ToPostCode = postCodeAU2000.RK_CityTownPostCode;
			AssertNoError(item.TQ_ToPostCodeInfo, errorString);
			item.TQ_ToPostCode = postCodeAU3000.RK_CityTownPostCode;
			AssertNoError(item.TQ_ToPostCodeInfo, errorString);

			var provider2 = Factory.NewWithValidTestData<RateTransportProvider>();
			provider2.TP_RN_NKCountry = "ZA";
			var zone2 = provider2.Zones.AddNew();
			var item2 = zone2.Items.AddNew();
			item2.TQ_FromPostCode = postCodeAU1000.RK_CityTownPostCode;
			AssertNoError(item2.TQ_FromPostCodeInfo, errorString);
			AssertEquals(item2.TQ_FromPostCode, postCodeZA1000.RK_CityTownPostCode);
			item2.TQ_ToPostCode = postCodeAU2000.RK_CityTownPostCode;
			AssertNoError(item2.TQ_ToPostCodeInfo, errorString);
			AssertEquals(item2.TQ_ToPostCode, postCodeZA2000.RK_CityTownPostCode);
			item2.TQ_ToPostCode = postCodeAU3000.RK_CityTownPostCode;
			AssertHasError(item2.TQ_ToPostCodeInfo, errorString);
		}

		#endregion

		#region Validate overlaps test

		public void TestOverlapsCityTown()
		{
			var prov = Factory.New<RateTransportProvider>();

			var cityTown1 = Factory.New<RefCityTown>();
			cityTown1.R9_InternationalName = "Cape Town";
			var cityTown2 = Factory.New<RefCityTown>();
			cityTown2.R9_InternationalName = "East London";

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_R9_CityTown = cityTown1.PK;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_R9_CityTown = cityTown1.PK;

			var zone3 = prov.Zones.AddNew();
			zone3.TZ_ZoneName = "Zone3";
			var zoneItem3 = zone3.Items.AddNew();
			zoneItem3.TQ_R9_CityTown = cityTown2.PK;

			Assert(RateTransportZoneItemValidation.OverlapsCityTown(zoneItem1, zoneItem2));
			Assert(!RateTransportZoneItemValidation.OverlapsCityTown(zoneItem1, zoneItem3));
		}

		public void TestOverlapsCityTownUsingPostCodes()
		{
			var prov = Factory.New<RateTransportProvider>();

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "1500";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "Johannesburg";
			cityTown.PostCodes.Add(postCode3);

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_R9_CityTown = cityTown.PK;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem2.TQ_ToPostCode = postCode2.RK_CityTownPostCode;

			Assert(RateTransportZoneItemValidation.OverlapsCityTown(zoneItem1, zoneItem2));
		}

		public void TestOverlapsDistance()
		{
			var prov = Factory.New<RateTransportProvider>();

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromDistance = 0;
			zoneItem1.TQ_ToDistance = 10;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromDistance = 5;
			zoneItem2.TQ_ToDistance = 15;

			Assert(RateTransportZoneItemValidation.OverlapsDistance(zoneItem1, zoneItem2));

			zoneItem2.TQ_FromDistance = 10;
			Assert("The 'to distance ' of Zone2 matches, not overlaps 'from distance' of Zone 1", !RateTransportZoneItemValidation.OverlapsDistance(zoneItem1, zoneItem2));

			zoneItem2.TQ_FromDistance = 11;
			Assert(!RateTransportZoneItemValidation.OverlapsDistance(zoneItem1, zoneItem2));

			var zone3 = prov.Zones.AddNew();
			zone3.TZ_ZoneName = "Zone3";
			var zoneItem3 = zone3.Items.AddNew();
			zoneItem3.TQ_R9_CityTown = Factory.New<RefCityTown>().PK;

			Assert(!RateTransportZoneItemValidation.OverlapsDistance(zoneItem3, zoneItem1));
			Assert(!RateTransportZoneItemValidation.OverlapsDistance(zoneItem3, zoneItem2));
		}

		public void TestOverlapsPostCode()
		{
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();

			var postCode1000 = Factory.NewWithValidTestData<RefPostCode>();
			postCode1000.RK_CityTownPostCode = "1000";
			var postCode1005 = Factory.NewWithValidTestData<RefPostCode>();
			postCode1005.RK_CityTownPostCode = "1005";
			var postCode1010 = Factory.NewWithValidTestData<RefPostCode>();
			postCode1010.RK_CityTownPostCode = "1010";
			var postCode1015 = Factory.NewWithValidTestData<RefPostCode>();
			postCode1015.RK_CityTownPostCode = "1015";

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1000.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode1010.RK_CityTownPostCode;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode1005.RK_CityTownPostCode;
			zoneItem2.TQ_ToPostCode = postCode1015.RK_CityTownPostCode;

			var zone3 = prov.Zones.AddNew();
			zone3.TZ_ZoneName = "Zone3";
			var zoneItem3 = zone3.Items.AddNew();
			zoneItem3.TQ_FromPostCode = postCode1010.RK_CityTownPostCode;

			var zone4 = prov.Zones.AddNew();
			zone4.TZ_ZoneName = "Zone4";
			var zoneItem4 = zone4.Items.AddNew();
			zoneItem4.TQ_FromPostCode = postCode1015.RK_CityTownPostCode;
			zoneItem4.TQ_ToPostCode = postCode1015.RK_CityTownPostCode;

			Assert(RateTransportZoneItemValidation.OverlapsPostCode(zoneItem1, zoneItem2));
			Assert(RateTransportZoneItemValidation.OverlapsPostCode(zoneItem2, zoneItem3));
			Assert(RateTransportZoneItemValidation.OverlapsPostCode(zoneItem1, zoneItem3));
			Assert(!RateTransportZoneItemValidation.OverlapsPostCode(zoneItem3, zoneItem4));
			Assert(!RateTransportZoneItemValidation.OverlapsPostCode(zoneItem1, zoneItem4));
		}

		public void TestOverlapsPostCodeUsingCityTowns()
		{
			var prov = Factory.New<RateTransportProvider>();

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1000";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "1500";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "Johannesburg";
			cityTown.PostCodes.Add(postCode3);

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode2.RK_CityTownPostCode;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_R9_CityTown = cityTown.PK;

			Assert(RateTransportZoneItemValidation.OverlapsPostCode(zoneItem1, zoneItem2));
		}

		public void TestValidateOverlappingZoneItemsByCityTown()
		{
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();

			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_InternationalName = "Sydney";

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone 1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_R9_CityTown = cityTown1.PK;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone 2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_R9_CityTown = cityTown1.PK;

			var errorMessage = string.Format("This {0} is already part of zone ({1}) as a {2}.", zoneItem2.ToDescriptiveString(), zoneItem1.Zone.TZ_ZoneName, zoneItem1.ToDescriptiveString());
			AssertHasError(zoneItem2.TQ_R9_CityTownInfo, errorMessage);

			zone1.TZ_IsActive = false;
			zoneItem2.RunPreSaveValidation();
			AssertNoError("Should be no error because zone1 has been set to inactive.", zoneItem2.TQ_R9_CityTownInfo, errorMessage);

			var errorMessag2 = string.Format("This {0} is already part of zone ({1}) as a {2}.", zoneItem1.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString());
			zoneItem1.RunPreSaveValidation();
			AssertNoError("Should be no error because zone1 has been set to inactive.", zoneItem1.TQ_R9_CityTownInfo, errorMessag2);

			zone1.TZ_IsActive = true;
			zoneItem1.RunPreSaveValidation();
			AssertHasError(zoneItem1.TQ_R9_CityTownInfo, errorMessag2);
		}

		public void TestValidateOverlappingZoneItemsByDistanceWhenFromDistanceChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromDistance = 4;
			zoneItem1.TQ_ToDistance = 30;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Zone2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromDistance = 5;
			zoneItem2.TQ_ToDistance = 40;

			var errorMessage = DistanceErrorMessageFormatted(zoneItem2);
			AssertHasErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertHasErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem2.TQ_FromDistance = 30;

			AssertNoErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertNoErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem2.TQ_FromDistance = 29;

			errorMessage = DistanceErrorMessageFormatted(zoneItem2);
			AssertHasErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertHasErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem2.TQ_FromDistance = 31;

			AssertNoErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertNoErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zone1.TZ_IsActive = false;
			zoneItem2.TQ_FromDistance = 29;

			AssertNoErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertNoErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);
		}

		public void TestValidateOverlappingZoneItemsByDistanceWhenToDistanceChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;

			var zone = prov.Zones.AddNew();
			zone.TZ_ZoneName = "Zone1";
			var zoneItem1 = zone.Items.AddNew();
			zoneItem1.TQ_FromDistance = 0;
			zoneItem1.TQ_ToDistance = 30;

			var zoneItem2 = zone.Items.AddNew();
			zoneItem2.TQ_FromDistance = 20;
			zoneItem2.TQ_ToDistance = 100;

			var errorMessage = DistanceErrorMessageFormatted(zoneItem2);
			AssertHasErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertHasErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem1.TQ_ToDistance = 20;
			zoneItem2.Validation.ValidateAll();

			AssertNoErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertNoErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem1.TQ_ToDistance = 21;
			zoneItem2.Validation.ValidateAll();

			errorMessage = DistanceErrorMessageFormatted(zoneItem2);
			AssertHasErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertHasErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);

			zoneItem1.TQ_ToDistance = 18;
			zoneItem2.Validation.ValidateAll();

			AssertNoErrorContaining(zoneItem2.TQ_FromDistanceInfo, errorMessage);
			AssertNoErrorContaining(zoneItem2.TQ_ToDistanceInfo, errorMessage);
		}

		static string DistanceErrorMessageFormatted(RateTransportZoneItem overlappingZoneItem)
		{
			return string.Format("This {0} is already part of zone (Zone1) as a",
				overlappingZoneItem.ToDescriptiveString());
		}

		public void TestValidateOverlappingZoneItemsByPostCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1";
			var postCode30 = Factory.New<RefPostCode>();
			postCode30.RK_CityTownPostCode = "30";
			var postCode5 = Factory.New<RefPostCode>();
			postCode5.RK_CityTownPostCode = "5";
			var postCode40 = Factory.New<RefPostCode>();
			postCode40.RK_CityTownPostCode = "40";
			var postCode50 = Factory.New<RefPostCode>();
			postCode50.RK_CityTownPostCode = "50";
			var postCode70 = Factory.New<RefPostCode>();
			postCode70.RK_CityTownPostCode = "70";
			var postCode31 = Factory.New<RefPostCode>();
			postCode31.RK_CityTownPostCode = "31";
			var postCode33 = Factory.New<RefPostCode>();
			postCode33.RK_CityTownPostCode = "33";

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Test1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode30.RK_CityTownPostCode;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Test2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode5.RK_CityTownPostCode;
			zoneItem2.TQ_ToPostCode = postCode40.RK_CityTownPostCode;

			var zone3 = prov.Zones.AddNew();
			zone3.TZ_ZoneName = "Test3";
			var zoneItem3 = zone3.Items.AddNew();
			zoneItem3.TQ_FromPostCode = postCode1.RK_CityTownPostCode;

			var errorString = "This {0} is already part of zone ({1}) as a {2}.";
			var error1 = string.Format(errorString, zoneItem2.ToDescriptiveString(), zoneItem1.Zone.TZ_ZoneName, zoneItem1.ToDescriptiveString());

			AssertHasError(zoneItem2.TQ_FromPostCodeInfo, error1);
			AssertHasError(zoneItem2.TQ_ToPostCodeInfo, error1);
			AssertHasError(zoneItem3.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem3.ToDescriptiveString(), zoneItem1.Zone.TZ_ZoneName, zoneItem1.ToDescriptiveString()));

			zoneItem2.TQ_FromPostCode = postCode31.RK_CityTownPostCode;

			AssertNoError(zoneItem2.TQ_FromPostCodeInfo, error1);
			AssertNoError(zoneItem2.TQ_ToPostCodeInfo, error1);

			zoneItem1.TQ_ToPostCode = postCode33.RK_CityTownPostCode;

			var errorFromCode = string.Format(errorString, zoneItem1.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString());
			var errorToCode = string.Format(errorString, zoneItem1.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString());

			AssertHasError(zoneItem1.TQ_FromPostCodeInfo, errorFromCode);
			AssertHasError(zoneItem1.TQ_ToPostCodeInfo, errorToCode);

			zone2.TZ_IsActive = false;
			zoneItem1.RunPreSaveValidation();
			AssertNoError(zoneItem1.TQ_FromPostCodeInfo, errorFromCode);
			AssertNoError(zoneItem1.TQ_ToPostCodeInfo, errorToCode);
		}

		public void TestValidateOverlappingZoneItemsByPostCodeAndCityTown()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;
			prov.TP_RN_NKCountry = "AU";

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "0161";
			postCode1.RK_RN_NKCountry = "AU";
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2015";
			postCode2.RK_RN_NKCountry = "AU";

			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_InternationalName = "Old Trafford";
			cityTown1.R9_RN_NKCountry = "AU";
			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown2.R9_InternationalName = "Old Trafford";
			cityTown2.R9_RN_NKCountry = "AU";

			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_RK = postCode1.PK;
			pivot1.R0_R9 = cityTown1.PK;
			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_RK = postCode2.PK;
			pivot2.R0_R9 = cityTown2.PK;

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Test1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode1.RK_CityTownPostCode;

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Test2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_R9_CityTown = cityTown2.PK;

			AssertNoErrors(zoneItem2.TQ_R9_CityTownInfo);
		}

		public void TestValidateOverlappingZoneItemsByPostCodeAndDifferentContries()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;
			prov.TP_RN_NKCountry = "AU";

			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "4000";
			postCode1.RK_RN_NKCountry = "CH";

			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "4099";
			postCode2.RK_RN_NKCountry = "CH";

			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "4000";
			postCode3.RK_RN_NKCountry = "BE";

			var postCode4 = Factory.New<RefPostCode>();
			postCode4.RK_CityTownPostCode = "4000";
			postCode4.RK_RN_NKCountry = "BE";

			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_RK = postCode1.PK;

			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_RK = postCode2.PK;

			var zone1 = prov.Zones.AddNew();
			zone1.TZ_ZoneName = "Test1";
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			zoneItem1.TQ_RN_NKCountry = "CH";

			var zone2 = prov.Zones.AddNew();
			zone2.TZ_ZoneName = "Test2";
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_FromPostCode = postCode3.RK_CityTownPostCode;
			zoneItem2.TQ_ToPostCode = postCode4.RK_CityTownPostCode;
			zoneItem2.TQ_RN_NKCountry = "BE";

			AssertNoErrors(zoneItem2.TQ_ToPostCodeInfo);
		}

		#endregion

		public void TestValidateZoneHasCityTownOrDistancesOrPostCodes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var prov = Factory.NewWithValidTestData<RateTransportProvider>();
			prov.TP_OH_RelatedParty = org.PK;

			var zoneDistances = prov.Zones.AddNew();
			var zoneDistancesItem = zoneDistances.Items.AddNew();
			zoneDistancesItem.TQ_FromDistance = 0;
			zoneDistancesItem.TQ_ToDistance = 1;

			var zonePostCodes = prov.Zones.AddNew();
			var zonePostCodesItem = zonePostCodes.Items.AddNew();
			zonePostCodesItem.TQ_FromPostCode = "1000";
			zonePostCodesItem.TQ_ToPostCode = "2000";

			var zoneCityTown = prov.Zones.AddNew();
			var zoneCityTownItem = zoneCityTown.Items.AddNew();
			zoneCityTownItem.TQ_R9_CityTown = Factory.NewWithValidTestData<RefCityTown>().PK;

			var zoneNone = prov.Zones.AddNew();
			var zoneNoneItem = zoneNone.Items.AddNew();
			zoneNoneItem.TQ_ToDistance = 0;

			var zoneDistancesAndPostCodes = prov.Zones.AddNew();
			var zoneDistancesAndPostCodesItem = zoneDistancesAndPostCodes.Items.AddNew();
			zoneDistancesAndPostCodesItem.TQ_FromDistance = 0;
			zoneDistancesAndPostCodesItem.TQ_ToDistance = 1;
			zoneDistancesAndPostCodesItem.TQ_FromPostCode = "1000";

			var zoneDistancesAndPostCodes2 = prov.Zones.AddNew();
			var zoneDistancesAndPostCodesItem2 = zoneDistancesAndPostCodes2.Items.AddNew();
			zoneDistancesAndPostCodesItem2.TQ_FromDistance = 1;
			zoneDistancesAndPostCodesItem2.TQ_ToDistance = 0;
			zoneDistancesAndPostCodesItem2.TQ_FromPostCode = "1000";

			var zonePostCodesAndCityTown = prov.Zones.AddNew();
			var zonePostCodesAndCityTownItem = zonePostCodesAndCityTown.Items.AddNew();
			zonePostCodesAndCityTownItem.TQ_FromPostCode = "1000";
			zonePostCodesAndCityTownItem.TQ_R9_CityTown = Factory.NewWithValidTestData<RefCityTown>().PK;

			var zoneDistancesAndCityTown = prov.Zones.AddNew();
			var zoneDistancesAndCityTownItem = zoneDistancesAndCityTown.Items.AddNew();
			zoneDistancesAndCityTownItem.TQ_FromDistance = 0;
			zoneDistancesAndCityTownItem.TQ_ToDistance = 1;
			zoneDistancesAndCityTownItem.TQ_R9_CityTown = Factory.NewWithValidTestData<RefCityTown>().PK;

			var zoneAll = prov.Zones.AddNew();
			var zoneAllItem = zoneAll.Items.AddNew();
			zoneAllItem.TQ_FromDistance = 0;
			zoneAllItem.TQ_ToDistance = 1;
			zoneAllItem.TQ_FromPostCode = "1000";
			zoneAllItem.TQ_R9_CityTown = Factory.NewWithValidTestData<RefCityTown>().PK;

			var errorString = @"You must specify either a city/town, postcode, a city/town postcode combination, postcode range or distance range.
Please set distance range to 0 if not required.";

			AssertNoRowError(zoneDistancesItem, errorString);
			AssertNoRowError(zonePostCodesItem, errorString);
			AssertNoRowError(zoneCityTownItem, errorString);
			AssertHasRowError(zoneNoneItem, errorString);

			AssertHasRowError(zoneDistancesAndPostCodesItem, errorString);
			AssertHasRowError(zoneDistancesAndPostCodesItem2, errorString);
			AssertNoRowError(zonePostCodesAndCityTownItem, errorString);
			AssertHasRowError(zoneDistancesAndCityTownItem, errorString);
			AssertHasRowError(zoneAllItem, errorString);
		}

		public void TestValidateOnlyWithHasChangesForPerformance()
		{
			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();

			var zone1 = zoneSet.Zones.AddNew();
			var zone2 = zoneSet.Zones.AddNew();
			var zone3 = zoneSet.Zones.AddNew();
			zone1.TZ_ZoneName = "zone1";
			zone2.TZ_ZoneName = "zone2";
			zone3.TZ_ZoneName = "zone3";

			var postcode1000 = Factory.NewWithValidTestData<RefPostCode>();
			var postcode2000 = Factory.NewWithValidTestData<RefPostCode>();
			postcode1000.RK_CityTownPostCode = "1000";
			postcode2000.RK_CityTownPostCode = "2000";

			var zoneItem1 = zone1.Items.AddNew();
			var zoneItem2 = zone2.Items.AddNew();
			var zoneItem3 = zone3.Items.AddNew();
			zoneItem1.TQ_FromPostCode = postcode1000.RK_CityTownPostCode;
			zoneItem2.TQ_FromPostCode = postcode2000.RK_CityTownPostCode;
			zoneItem3.TQ_FromPostCode = postcode2000.RK_CityTownPostCode;

			zoneItem2.Validation.ValidateTQ_FromPostCode();
			Factory.Save();

			var errorString = "This {0} is already part of zone ({1}) as a {2}.";

			AssertHasError(zoneItem2.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem2.ToDescriptiveString(), zoneItem3.Zone.TZ_ZoneName, zoneItem3.ToDescriptiveString()));
			AssertHasError(zoneItem3.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem3.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString()));

			var factory2 = new BusinessObjectFactory();
			zoneSet = factory2.Load<RateTransportProvider>(zoneSet.PK);

			zoneItem1 = factory2.Load<RateTransportZoneItem>(zoneItem1.PK);
			zoneItem2 = factory2.Load<RateTransportZoneItem>(zoneItem2.PK);
			zoneItem3 = factory2.Load<RateTransportZoneItem>(zoneItem3.PK);

			zoneItem1.TQ_FromPostCode = postcode2000.RK_CityTownPostCode;
			zoneItem2.Validation.ValidateTQ_FromPostCode();
			zoneItem3.Validation.ValidateTQ_FromPostCode();

			AssertHasError(zoneItem1.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem1.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString()));
			AssertNoError(zoneItem2.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem2.ToDescriptiveString(), zoneItem3.Zone.TZ_ZoneName, zoneItem3.ToDescriptiveString()));
			AssertNoError(zoneItem3.TQ_FromPostCodeInfo, string.Format(errorString, zoneItem3.ToDescriptiveString(), zoneItem2.Zone.TZ_ZoneName, zoneItem2.ToDescriptiveString()));
		}

		public void TestValidateTQ_RN_NKCountry_MandatoryAndValidCode()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zoneItem = provider.Zones.AddNew().Items.AddNew();

			zoneItem.TQ_RN_NKCountry = "";
			AssertHasErrors("No country, so should have an error.", zoneItem.TQ_RN_NKCountryInfo);

			zoneItem.TQ_RN_NKCountry = "XX";
			AssertHasErrors("Invalid country, so should have an error.", zoneItem.TQ_RN_NKCountryInfo);

			zoneItem.TQ_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			AssertNoErrors("Valid country, so should not have an error.", zoneItem.TQ_RN_NKCountryInfo);
		}

		public void TestValidateTQ_RN_NKCountry_WarningWhenOverriding()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zoneItem = provider.Zones.AddNew().Items.AddNew();
			AssertEquals("AU", zoneItem.TQ_RN_NKCountry);

			AssertNoErrors(zoneItem.TQ_RN_NKCountryInfo);
			AssertNoWarnings(zoneItem.TQ_RN_NKCountryInfo);

			provider.TP_RN_NKCountry = "US";
			zoneItem.Validation.ValidateTQ_RN_NKCountry();
			AssertNoErrors(zoneItem.TQ_RN_NKCountryInfo);
			AssertHasWarning(zoneItem.TQ_RN_NKCountryInfo, "Overriding zone country/region (US).");
		}

		public void TestIsExcludingPostCode()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zoneItem = provider.Zones.AddNew().Items.AddNew();
			zoneItem.Validation.ValidateTQ_IsExcludingPostCode();
			AssertNoErrors("no error for empty row", zoneItem.TQ_IsExcludingPostCodeInfo);

			zoneItem.TQ_FromPostCode = "2001";
			zoneItem.TQ_ToPostCode = "2015";
			zoneItem.Validation.ValidateTQ_IsExcludingPostCode();
			AssertNoErrors("no error if not ticked and there are postcodes", zoneItem.TQ_IsExcludingPostCodeInfo);

			zoneItem.TQ_IsExcludingPostCode = true;
			AssertHasErrors("error if ticked and there are postcodes", zoneItem.TQ_IsExcludingPostCodeInfo);

			zoneItem.TQ_ToPostCode = "";
			zoneItem.Validation.ValidateTQ_IsExcludingPostCode();
			AssertHasErrors("error if ticked and there is a From postcode and no To postcode", zoneItem.TQ_IsExcludingPostCodeInfo);

			zoneItem.TQ_FromPostCode = "";
			zoneItem.Validation.ValidateTQ_IsExcludingPostCode();
			AssertNoErrors("no error if ticked and there are no postcodes", zoneItem.TQ_IsExcludingPostCodeInfo);
		}

		public void TestCheckBeyondDays()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zoneItem = provider.Zones.AddNew().Items.AddNew();

			AssertNoErrors(zoneItem.BeyondDaysInfo);

			zoneItem.BeyondDays = -5;
			AssertHasError(zoneItem.BeyondDaysInfo, "Beyond Days cannot be negative.");

			zoneItem.BeyondDays = 5;
			AssertNoErrors(zoneItem.BeyondDaysInfo);
		}

		public void TestCheckBeyondHours()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zoneItem = provider.Zones.AddNew().Items.AddNew();

			AssertNoErrors(zoneItem.BeyondHoursInfo);

			zoneItem.BeyondHours = -5;
			AssertHasError(zoneItem.BeyondHoursInfo, "Beyond Hours cannot be negative.");

			zoneItem.BeyondHours = 5;
			AssertNoErrors(zoneItem.BeyondHoursInfo);

			zoneItem.BeyondHours = 25;
			AssertHasError(zoneItem.BeyondHoursInfo, "Hours must be less than 24. Please use the 'Days' component for longer times.");

			zoneItem.BeyondHours = 23;
			AssertNoErrors(zoneItem.BeyondHoursInfo);
		}
	}
}
