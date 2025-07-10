using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSHarvestingDetailAddInfoValidation))]
	class NMFSHarvestingDetailAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_FirstLandingCountry()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			NMFSLine.US_SpeciesCode = "1";
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.Validation.ValidateUS_FirstLandingCountry();
			AssertNoMessageError(harvestingDetail.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(harvestingDetail.US_FirstLandingCountryInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			harvestingDetail.Validation.ValidateUS_FirstLandingCountry();
			AssertNoMessageError(harvestingDetail.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(harvestingDetail.US_FirstLandingCountryInfo, MandatoryValidation.YouHaveNotEntered);

			harvestingDetail.US_FirstLandingCountry = "~";
			AssertHasMessageError(harvestingDetail.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(harvestingDetail.US_FirstLandingCountryInfo, MandatoryValidation.YouHaveNotEntered);

			harvestingDetail.US_FirstLandingCountry = "US";
			AssertNoMessageError(harvestingDetail.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(harvestingDetail.US_FirstLandingCountryInfo, MandatoryValidation.YouHaveNotEntered);

			harvestingDetail.US_FirstLandingCountry = "ZZ";
			AssertNoMessageError(harvestingDetail.US_FirstLandingCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(harvestingDetail.US_FirstLandingCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_GearType()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_GearType = "!";
			AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(harvestingDetail.US_GearTypeInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_GearType = ZString.Empty;
			AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, ListValidation.InvalidCodeMessageError);

			foreach (var programType in new[] { NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				harvestingDetail.US_GearType = GearTypeList.Codes.Longline;
				AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_GearType = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_GearTypeInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_GearType = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_GearType();
				AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_GearType();
				AssertHasMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);

				if (programType == NMFSProgramCodeList.Codes.SIM || programType == NMFSProgramCodeList.Codes.COA)
				{
					NMFSLine.US_SpeciesCode = "2";
					harvestingDetail.US_GearType = ZString.Empty;
					AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, MandatoryValidation.YouHaveNotEntered);
				}
			}
		}

		public void TestCheckUS_HarvestedCountry()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = "!";
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountry = ZString.Empty;
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);

			foreach (var programType in new[] { NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Codes.SIM })
			{
				NMFSLine.US_ProgramType = programType;
				harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.Australia;
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_HarvestedCountry = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_HarvestedCountry = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_HarvestedCountry();
				AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_HarvestedCountry();
				AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);

				if (programType == NMFSProgramCodeList.Codes.SIM)
				{
					NMFSLine.US_SpeciesCode = "2";
					harvestingDetail.US_HarvestedCountry = ZString.Empty;
					AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
				}
			}

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountryInfo.ClearAllNotifications();

			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWatersInstallations;
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountryInfo.ClearAllNotifications();

			harvestingDetail.US_HarvestedCountry = "!";
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountryInfo.ClearAllNotifications();

			harvestingDetail.US_HarvestedCountry = ZString.Empty;
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountryInfo.ClearAllNotifications();

			harvestingDetail.US_HarvestedCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, "The Harvesting Country cannot be the same as the Origin Country.");
		}

		public void TestCheckUS_OceanAreaOfCatch()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = "!";
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_OceanAreaOfCatch = ZString.Empty;
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;

			foreach (var programType in new[] { NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
				AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_OceanAreaOfCatch = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_OceanAreaOfCatch = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_OceanAreaOfCatch();
				AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_OceanAreaOfCatch();
				AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);

				if (programType == NMFSProgramCodeList.Codes.SIM || programType == NMFSProgramCodeList.Codes.COA)
				{
					NMFSLine.US_SpeciesCode = "2";
					harvestingDetail.US_OceanAreaOfCatch = ZString.Empty;
					AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, MandatoryValidation.YouHaveNotEntered);
				}
			}
		}

		public void TestCheckUS_VesselCountry()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_VesselCountry = "!";
			AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);
			harvestingDetail.US_VesselCountry = ZString.Empty;
			AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);

			foreach (var programType in new[] { NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Codes.HMS })
			{
				NMFSLine.US_ProgramType = programType;
				harvestingDetail.US_VesselCountry = Core.Constants.CountryCodes.Australia;
				AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_VesselCountry = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_VesselCountry = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_GearStartDate()
		{
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				harvestingDetail.US_GearStartDate = ZDateTime.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_GearStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_GearStartDate();
				AssertNoMessageErrorContaining(harvestingDetail.US_GearStartDateInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_GearStartDate();
				AssertHasMessageErrorContaining(harvestingDetail.US_GearStartDateInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "2";
				harvestingDetail.US_GearStartDate = ZDateTime.Empty;
				AssertNoMessageErrorContaining(harvestingDetail.US_GearStartDateInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_GearDescription()
		{
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				harvestingDetail.US_GearDescription = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_GearDescription = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_GearDescription();
				AssertNoMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_GearDescription();
				AssertHasMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "2";
				harvestingDetail.US_GearDescription = ZString.Empty;
				AssertNoMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_ContactPartyType()
		{
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				harvestingDetail.US_ContactPartyType = "!";
				AssertNoMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, ListValidation.InvalidCodeMessageError);
				harvestingDetail.US_ContactPartyType = ZString.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_ContactPartyType();
				AssertNoMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_ContactPartyType();
				AssertHasMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "2";
				harvestingDetail.US_ContactPartyType = ZString.Empty;
				AssertNoMessageErrorContaining(harvestingDetail.US_ContactPartyTypeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_ContactParty()
		{
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				harvestingDetail.US_OA_ContactParty = ZGuid.Empty;
				AssertHasMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.ValidationModes = ValidationModes.None;
				harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
				AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
				AssertHasMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "2";
				harvestingDetail.US_OA_ContactParty = ZGuid.Empty;
				AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_OA_ContactPartyAddress()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			harvestingDetail.US_OA_ContactParty = orgHeader.MainAddress.PK;

			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, "AAAA", "BBBB", null, null, null);
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();

			var expectedError = "Either the work phone number or the email/fax is required for PGA reporting. Please add at least one data as USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.";
			AssertHasMessageError(harvestingDetail.US_OA_ContactPartyInfo, expectedError);

			harvestingDetail.US_OA_ContactParty = ZGuid.Invalid;
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, expectedError);

			harvestingDetail.US_OA_ContactParty = orgHeader.MainAddress.PK;
			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, "1234567", null, null);
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertNoMessageError(harvestingDetail.US_OA_ContactPartyInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, null, null, "23456");
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, errorMsg);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CAABC";
			harvestingDetail.US_OA_ContactParty = address.PK;
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertHasMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			harvestingDetail.US_OA_ContactParty = orgHeader.MainAddress.PK;
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			harvestingDetail.US_OA_ContactParty = orgHeader.MainAddress.PK;
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertNoMessageErrorContaining(harvestingDetail.US_OA_ContactPartyInfo, "The state is not a valid");

			address.OA_City = "KYIV";
			address.OA_Address1 = "éééÄöß";
			address.OA_Address2 = "Address2Äöß";
			address.OA_Code = "öß";
			harvestingDetail.US_OA_ContactParty = address.PK;
			harvestingDetail.AddInfoValidation.ValidateUS_OA_ContactParty();
			AssertHasWarning(harvestingDetail.US_OA_ContactPartyInfo, addressDescriptionWarning);
			AssertHasWarning(harvestingDetail.US_OA_ContactPartyInfo, addressCodeWarning);
		}

		public void TestCheckUS_HarvestedCountryForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			AssertEquals(1, NMFSLine.HarvestingDetails.Count);

			NMFSLine.US_HarvestedCountry = "~";
			AssertHasMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);

			NMFSLine.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);

			NMFSLine.US_HarvestedCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(harvestingDetail.US_HarvestedCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_OceanAreaOfCatchForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			AssertEquals(1, NMFSLine.HarvestingDetails.Count);

			NMFSLine.US_GeographicLocation = "~";
			var harvestingDetail = NMFSLine.HarvestingDetails[0];
			AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);

			NMFSLine.US_GeographicLocation = OceanGeographicAreaCodeList.Codes.CAR;
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_VesselCountryForExport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			AssertEquals(1, NMFSLine.HarvestingDetails.Count);

			NMFSLine.US_VesselCountry = "~";
			AssertHasMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);

			NMFSLine.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(harvestingDetail.US_VesselCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_OceanAreaOfCatchDesc()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = "SVH";
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			harvestingDetail.US_OceanAreaOfCatchDesc = "Test";
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchDescInfo, MandatoryValidation.YouHaveNotEntered);

			harvestingDetail.US_OceanAreaOfCatchDesc = "";
			AssertHasMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchDescInfo, MandatoryValidation.YouHaveNotEntered);

			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.CAR;
			Assert(harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc.IsEmpty);
			AssertNoMessageErrorContaining(harvestingDetail.US_OceanAreaOfCatchDescInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OceanAreaOfCatchDesc_370()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			harvestingDetail.US_OceanAreaOfCatchDesc = "Test";
			AssertNoMessageErrors(harvestingDetail.US_OceanAreaOfCatchDescInfo);

			harvestingDetail.US_OceanAreaOfCatchDesc = "";
			AssertNoMessageErrors(harvestingDetail.US_OceanAreaOfCatchDescInfo);

			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.CAR;
			Assert(harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc.IsEmpty);
			AssertNoMessageErrors(harvestingDetail.US_OceanAreaOfCatchDescInfo);
		}

		public void TestCheckUS_GearTypeWhenSIM()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			var message = "You have not entered a value.";

			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_NoSmallVessels = 1;
			harvestingDetail.AddInfoValidation.ValidateUS_GearType();
			AssertNoMessageErrorContaining(harvestingDetail.US_GearTypeInfo, message);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			harvestingDetail.AddInfoValidation.ValidateUS_GearType();
			AssertHasMessageErrorContaining(harvestingDetail.US_GearTypeInfo, message);
		}

		public void TestCheckUS_NoSmallVessels()
		{
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
				NMFSLine.US_SpeciesCode = "2";
				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				harvestingDetail.US_NoSmallVessels = ZInt.Zero;
				AssertNoMessageErrorContaining(harvestingDetail.US_NoSmallVesselsInfo, MandatoryValidation.YouHaveNotEntered);
				harvestingDetail.US_NoSmallVessels = 1;
				AssertNoMessageErrorContaining(harvestingDetail.US_GearDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "1";
				NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
				harvestingDetail.US_NoSmallVessels = ZInt.Zero;
				AssertNoMessageErrorContaining(harvestingDetail.US_NoSmallVesselsInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
				harvestingDetail.AddInfoValidation.ValidateUS_NoSmallVessels();
				AssertHasMessageErrorContaining(harvestingDetail.US_NoSmallVesselsInfo, MandatoryValidation.YouHaveNotEntered);

				harvestingDetail.US_NoSmallVessels = 1;
				AssertNoMessageErrorContaining(harvestingDetail.US_NoSmallVesselsInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = true;
				}
				return declaration;
			}
		}

		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}

		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}

		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get
			{
				if (nmfsLine == null)
				{
					nmfsLine = InvoiceLine.NMFSLines.AddNew();
				}

				nmfsLine.US_SpeciesCode = "1";
				return nmfsLine;
			}
		}

		NMFSLine nmfsLine;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "USSIM");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("RequiresFullData", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, Core.Constants.CountryCodes.UnitedStates);
			code1.Attributes.AddNew("RequiresFullData", "Yes");
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("RequiresFullData", "No");
			Factory.Save();
		}
	}
}
