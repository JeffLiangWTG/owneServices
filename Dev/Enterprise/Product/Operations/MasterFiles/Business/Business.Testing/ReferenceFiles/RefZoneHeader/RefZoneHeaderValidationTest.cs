using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefZoneHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCode()
		{
			RefZoneHeader zone = Factory.NewWithValidTestData<RefZoneHeader>();

			zone.FZ_Code = "A";
			AssertHasError(zone.FZ_CodeInfo, "Zone Code must be 4 characters in length.");

			zone.FZ_Code = "ABC";
			AssertHasError(zone.FZ_CodeInfo, "Zone Code must be 4 characters in length.");

			zone.FZ_Code = "ABCD";
			AssertNoErrors(zone.FZ_CodeInfo);

			Factory.Save();

			RefZoneHeader zone2 = Factory.NewWithValidTestData<RefZoneHeader>();

			zone2.FZ_ZoneType = "ALL";
			zone2.FZ_IsActive = false;
			zone2.FZ_Code = "ACCC";
			AssertNoErrors(zone.FZ_CodeInfo);

			Factory.Save();

			RefZoneHeader zone3 = Factory.NewWithValidTestData<RefZoneHeader>();

			zone3.FZ_Code = "NEW1";
			AssertNoErrors(zone3.FZ_CodeInfo);

			zone3.FZ_Code = "ABCD";
			AssertHasError(zone3.FZ_CodeInfo, "Zone Code must be unique.");

			zone3.FZ_ZoneType = "RAT";
			zone3.FZ_Code = "ACCC";
			AssertHasError(zone3.FZ_CodeInfo, "Zone Code must be unique.");

			zone3.FZ_ZoneType = "WRS";
			zone3.FZ_Code = "BBBB";
			AssertNoErrors(zone3.FZ_CodeInfo);

			Factory.Save();

			RefZoneHeader zone4 = Factory.NewWithValidTestData<RefZoneHeader>();

			zone4.FZ_ZoneType = "ALL";
			zone4.FZ_Code = "BBBB";
			AssertNoErrors(zone4.FZ_CodeInfo);
		}

		public void TestValidateDescriptionIsUnique()
		{
			RefZoneHeader zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "ABCD";
			zone.FZ_Description = "ABCD Description";
			AssertNoErrors(zone.FZ_CodeInfo);
			AssertNoErrors(zone.FZ_DescriptionInfo);
			Factory.Save();

			zone.FZ_Description = "ABCD Description";
			AssertNoErrors(zone.FZ_DescriptionInfo);

			RefZoneHeader zone2 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone2.FZ_Code = "WZYZ";
			zone2.FZ_Description = zone.FZ_Description;
			AssertNoErrors(zone2.FZ_CodeInfo);
			AssertHasErrors("Zone description should be unique", zone2.FZ_DescriptionInfo);

			zone2.FZ_Description = "ABCD Description1";
			AssertNoErrors(zone2.FZ_DescriptionInfo);
		}

		public void TestFZ_ZoneType()
		{
			var errorMessage = "'WRS' code cannot be manually selected.";

			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "ABCD";
			zone.FZ_Description = "ABCD Description";
			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code;

			AssertEquals("Pre-condition", false, zone.IsInDatabase);
			AssertHasError("Unsaved Intl. Zone should have this error message", zone.FZ_ZoneTypeInfo, errorMessage);

			Factory.Save();
			zone.Validation.ValidateFZ_ZoneType();

			AssertEquals("Pre-condition", true, zone.IsInDatabase);
			AssertNoError("Saved WRS type Intl. Zones should not have an error message", zone.FZ_ZoneTypeInfo, errorMessage);

			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Rating.Code;
			Factory.Save();

			AssertNoError("Only the WRS type has this error", zone.FZ_ZoneTypeInfo, errorMessage);

			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code;

			AssertEquals("Pre-condition", true, zone.FZ_ZoneTypeInfo.HasChanges);
			AssertHasError("Intl. Zone where the type has been changed to 'WRS' should have this error message", zone.FZ_ZoneTypeInfo, errorMessage);

			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Rating.Code;

			AssertNoError("Only WRS code should have errors", zone.FZ_ZoneTypeInfo, errorMessage);
		}

		public void TestFZ_ZoneMode()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "BRRR";
			zone.FZ_Description = "Skiddly pap";
			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Rating.Code;
			zone.FZ_ZoneMode = zone.Lookups.ZoneModes.GetAllCodes()[0];

			zone.Validation.ValidateAll();

			AssertNoErrors("Pre-condition", zone.FZ_ZoneModeInfo);

			zone.FZ_ZoneMode = string.Empty;
			zone.Validation.ValidateFZ_ZoneMode();
			AssertHasError(zone.FZ_ZoneModeInfo, "Please enter a Mode.");

			zone.FZ_ZoneMode = "XXX";
			zone.Validation.ValidateFZ_ZoneMode();
			AssertHasError(zone.FZ_ZoneModeInfo, "Enter a valid Mode.");
		}

		public void TestGatewayValidation()
		{
			var errorMessage = "Please enter a Gateway.";

			var carrier = Factory.New<OrgHeader>();
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "ABCD";
			zone.FZ_Description = "ABCD Description";
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway;
			zone.FZ_OH_RelatedParty = carrier.PK;

			AssertNoError("Carrier is not required for other Zone Types", zone.FZ_OH_RelatedPartyInfo, errorMessage);

			zone.FZ_OH_RelatedParty = ZGuid.Empty;
			zone.Validation.ValidateFZ_ZoneType();

			AssertHasError("Carrier is required for Origin and Destination Gateway", zone.FZ_OH_RelatedPartyInfo, errorMessage);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway;
			zone.FZ_OH_RelatedParty = carrier.PK;
			zone.Validation.ValidateFZ_ZoneType();

			AssertNoError("Carrier is not required for other Zone Types", zone.FZ_OH_RelatedPartyInfo, errorMessage);

			zone.FZ_OH_RelatedParty = ZGuid.Empty;
			zone.Validation.ValidateFZ_ZoneType();

			AssertHasError("Carrier is required for Origin and Destination Gateway", zone.FZ_OH_RelatedPartyInfo, errorMessage);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;
			zone.Validation.ValidateFZ_ZoneType();

			AssertHasError("Carrier is required for HVLV Gateway", zone.FZ_OH_RelatedPartyInfo, errorMessage);
		}

		public void TestUNLOCOUniquenessChecksInHVLV()
		{
			var errorMessage = "HVLV Gateway does not support multiple UNLOCOs with the same country. Please ensure there is only one UNLOCO per country.";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSLCL1";

			var zone1 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone1.FZ_Code = "AAAA";
			zone1.FZ_Description = "AAAA Description";
			zone1.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;
			zone1.FZ_OH_RelatedParty = org.PK;
			zone1.FZ_ZoneMode = zone1.Lookups.ZoneModes.GetAllCodes()[0];

			var zone2 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone2.FZ_Code = "BBBB";
			zone2.FZ_Description = "BBBB Description";
			zone2.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;
			zone2.FZ_OH_RelatedParty = org.PK;
			zone2.FZ_ZoneMode = zone2.Lookups.ZoneModes.GetAllCodes()[0];

			var zone3 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone3.FZ_Code = "CCCC";
			zone3.FZ_Description = "CCCC Description";
			zone3.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;
			zone3.FZ_ZoneMode = zone3.Lookups.ZoneModes.GetAllCodes()[0];

			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco1.RL_Code = "Test1";
			unloco1.RL_GeoLocation = new ZGeography("POINT(77.15 28.6)");
			unloco1.RL_RN_NKCountryCode = "AU";

			zone1.UNLOCOs.Add(unloco1);

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.RL_Code = "Test2";
			unloco2.RL_GeoLocation = new ZGeography("POINT(11.15 28.6)");
			unloco2.RL_RN_NKCountryCode = "AU";

			var unloco3 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco3.RL_Code = "Test3";
			unloco3.RL_GeoLocation = new ZGeography("POINT(66.15 28.6)");
			unloco3.RL_RN_NKCountryCode = "BF";

			zone3.UNLOCOs.Add(unloco1);
			zone3.Validation.ValidateAll();
			AssertEquals(false, unloco1.HasRowErrors);

			zone2.UNLOCOs.Add(unloco2);
			zone2.Validation.ValidateAll();
			Assert(errorMessage, unloco2.HasRowErrors);

			zone1.UNLOCOs.Add(unloco1);
			zone1.Validation.ValidateAll();
			Assert(errorMessage, unloco1.HasRowErrors);
		}
	}
}
