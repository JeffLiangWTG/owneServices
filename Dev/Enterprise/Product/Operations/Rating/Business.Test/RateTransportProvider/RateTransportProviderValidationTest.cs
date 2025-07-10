using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class RateTransportProviderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTP_RN_NKCountry()
		{
			var prov = Factory.New<RateTransportProvider>();
			prov.TP_RN_NKCountry = "";
			AssertNull("Pre-condition", prov.ZoneHubLocation);
			AssertHasErrors("No country, so should have an error.", prov.TP_RN_NKCountryInfo);

			prov.TP_RN_NKCountry = "XX";
			AssertHasErrors("Invalid country, so should have an error.", prov.TP_RN_NKCountryInfo);

			prov.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			AssertNoErrors("Valid country, so should not have an error.", prov.TP_RN_NKCountryInfo);

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.China;
			prov.TP_R9_ZoneHubLocation = cityTown.PK;

			Assert(prov.TP_RN_NKCountry.IsEmpty);
			AssertNotNull("Pre-condition", prov.ZoneHubLocation);
			AssertNoErrors("Transport Zone has city town, so country can be empty.", prov.TP_RN_NKCountryInfo);
		}

		#region ZoneMode

		public void TestCheckTP_ZoneMode()
		{
			TestCheckTP_ZoneMode(RatingConstants.RatingZoneTypes.Rating);
			TestCheckTP_ZoneMode(RatingConstants.RatingZoneTypes.All);
			TestCheckTP_ZoneMode(RatingConstants.RatingZoneTypes.Reporting);
			TestCheckTP_ZoneMode(RatingConstants.RatingZoneTypes.Operations);
		}

		void TestCheckTP_ZoneMode(string zoneType)
		{
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_RN_NKCountry = "ZA";
			provider.TP_ZoneType = zoneType;

			var lookup = new RateTransportProviderLookups(provider);
			var modes = lookup.ZoneModes.GetAllCodes();

			foreach (var mode in modes)
			{
				provider.TP_ZoneMode = mode;
				AssertNoErrors(provider.TP_ZoneModeInfo);
			}

			AssertCollectionNotContains("Precondition", "XYZ", modes);
			provider.TP_ZoneMode = "XYZ";
			AssertHasError(provider.TP_ZoneModeInfo, "Enter a valid Zone Mode.");
		}

		#endregion

		public void TestValidateDuplicateTransportProviders()
		{
			var prov1 = Factory.New<RateTransportProvider>();
			var prov2 = Factory.New<RateTransportProvider>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			prov1.TP_OH_RelatedParty = ZGuid.Empty;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_OH_RelatedParty = ZGuid.Empty;
			AssertHasError("Generic zones in the same country with same type, so should have an error.", prov2.TP_OH_RelatedPartyInfo, "Transport zone set cannot be created because there is an existing transport zone set with same owner, location, zone type and zone mode.");

			prov1.TP_IsActive = false;
			prov2.RunPreSaveValidation();
			Assert("provider 1 is set to inactive, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_IsActive = true;
			prov1.TP_OH_RelatedParty = ZGuid.Empty;
			prov1.TP_RN_NKCountry = "AU";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "US";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_OH_RelatedParty = ZGuid.Empty;
			Assert("Generic zones in different countries, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_OH_RelatedParty = org1.PK;
			AssertHasError("Zones with the same owner in the same country with same type, so should have an error.", prov2.TP_OH_RelatedPartyInfo, "Transport zone set cannot be created because there is an existing transport zone set with same owner, location, zone type and zone mode.");

			prov1.TP_IsActive = false;
			prov2.RunPreSaveValidation();
			Assert("Zones with the same owner in different countries but one is inactive, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_IsActive = true;
			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "AU";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "US";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_OH_RelatedParty = org1.PK;
			Assert("Zones with the same owner in different countries, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_OH_RelatedParty = org2.PK;
			Assert("Zones with different owners, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			prov2.TP_OH_RelatedParty = org1.PK;
			Assert("Zones with the same owner in the same country but with different zone types, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.All;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			prov2.TP_OH_RelatedParty = org1.PK;
			Assert("Zones with the same owner in the same country and but different zone type, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov1.TP_ZoneMode = Core.Constants.RateMode.ALL;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			prov2.TP_ZoneMode = Core.Constants.RateMode.ALL;
			prov2.TP_OH_RelatedParty = org1.PK;
			Assert("Zones with the same owner in the same country with thye same zone mode but different zone Type, so should have no error", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov1.TP_ZoneMode = Core.Constants.RateMode.AIR;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov2.TP_ZoneMode = Core.Constants.RateMode.AIR;
			Assert("Zones with the same zone type and mode in the same country but one is more specific (having zone owner), so should have no error", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov1.TP_ZoneMode = Core.Constants.RateMode.SEA;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov2.TP_ZoneMode = Core.Constants.RateMode.FCL;
			prov2.TP_OH_RelatedParty = org1.PK;
			Assert("Zones with the same zone type and same owner in the same country but different zone mode, so should have no error", !prov2.TP_OH_RelatedPartyInfo.HasErrors());

			prov1.TP_OH_RelatedParty = org1.PK;
			prov1.TP_RN_NKCountry = "ZA";
			prov1.TP_R9_ZoneHubLocation = ZGuid.Empty;
			prov1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov1.TP_ZoneMode = Core.Constants.RateMode.LRA;

			prov2.TP_RN_NKCountry = "ZA";
			prov2.TP_R9_ZoneHubLocation = ZGuid.Empty;
			prov2.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			prov2.TP_ZoneMode = Core.Constants.RateMode.LRA;
			prov2.TP_OH_RelatedParty = org1.PK;
			AssertHasError("Zones with all the same owner, country, zone Type and Zone Mode, so should have error", prov2.TP_OH_RelatedPartyInfo, "Transport zone set cannot be created because there is an existing transport zone set with same owner, location, zone type and zone mode.");

			prov1.TP_IsActive = false;
			prov2.RunPreSaveValidation();
			Assert("provider 1 is set to inactive, so should have no error.", !prov2.TP_OH_RelatedPartyInfo.HasErrors());
		}

		public void TestValidateDuplicateTransportProviders_CountriesVsCityTowns()
		{
			var auProvider = Factory.New<RateTransportProvider>();
			auProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			auProvider.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var melProvider = Factory.New<RateTransportProvider>();
			melProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			melProvider.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			AssertHasErrors("Melbourne provider is direct copy of Australian provider so should be invalid", melProvider.TP_OH_RelatedPartyInfo);

			var melbourne = Factory.New<RefCityTown>();
			melbourne.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			melbourne.R9_InternationalName = "Melbourne01";
			melProvider.TP_R9_ZoneHubLocation = melbourne.PK;

			AssertNoErrors("Melbourne provider should be considered valid and distinct from the Australian provider", melProvider.TP_OH_RelatedPartyInfo);

			var sydney = Factory.New<RefCityTown>();
			sydney.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			sydney.R9_InternationalName = "Sydney01";

			var sydProvider = Factory.New<RateTransportProvider>();
			sydProvider.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			sydProvider.TP_R9_ZoneHubLocation = sydney.PK;

			AssertNoErrors("Sydney and Melbourne have distinct city/towns so should not be considered duplicates", sydProvider.TP_OH_RelatedPartyInfo);

			sydProvider.TP_R9_ZoneHubLocation = melbourne.PK;

			AssertHasError(sydProvider.TP_OH_RelatedPartyInfo, "Transport zone set cannot be created because there is an existing transport zone set with same owner, location, zone type and zone mode.");
		}

		public void TestValidateTP_ZoneType()
		{
			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_ZoneType = ZString.Empty;

			AssertHasErrors("Empty is not a valid zone type", zoneSet.TP_ZoneTypeInfo);
			AssertExceptionThrown<ZSaveException>("Constraint_TP_ZoneType should stop an empty zone type from being saved", () => Factory.Save());

			zoneSet.TP_ZoneType = "XXX";

			AssertHasErrors(zoneSet.TP_ZoneTypeInfo);
			AssertExceptionThrown<ZSaveException>("Constraint_TP_ZoneType should stop an invalid zone type from being saved", () => Factory.Save());

			foreach (var zoneType in zoneSet.Lookups.ZoneTypes.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				zoneSet.TP_ZoneType = zoneType;
				AssertNoErrors(zoneSet.TP_ZoneTypeInfo);
			}

			zoneSet.TP_ZoneType = RatingConstants.RatingZoneTypes.All;
			AssertNoErrors(zoneSet.TP_ZoneTypeInfo);
			AssertNoExceptionThrown("Constraint_TP_ZoneType should allow a valid type to be saved", () => Factory.Save());
		}

		public void TestValidate_ZoneOwner()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var zoneSet1 = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet1.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
			zoneSet1.TP_RN_NKCountry = "AU";
			zoneSet1.TP_OH_RelatedParty = owner.PK;

			var zoneSet2 = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet2.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			zoneSet2.TP_RN_NKCountry = "AU";
			zoneSet2.TP_OH_RelatedParty = owner.PK;

			Factory.Save();

			var zoneSet3 = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet3.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
			zoneSet3.TP_RN_NKCountry = "AU";
			AssertNoErrors(zoneSet3.TP_OH_RelatedPartyInfo);
			zoneSet3.TP_OH_RelatedParty = owner.PK;
			AssertHasErrors("The validation error should be added", zoneSet3.TP_OH_RelatedPartyInfo);
		}
	}
}
