using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidationTest : SGAddInfoValidationTest
	{
		public void TestOutwardMAWB_()
		{
			Declaration.JE_MessageType = "";
			AddInfoJobDeclaration.SG_OutwardTransportMode = "";
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardMAWB = "OutwardMAWB";
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
		}

		[TestDate(2008, 6, 30)]
		public void TestInwardVesselBerth_()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			AddInfoJobDeclaration.SG_US_NKInwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKInwardVesselBerth = "KZ";
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
		}

		public void TestInwardVesselBerthForTN4Point1()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			AddInfoJobDeclaration.SG_US_NKInwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals("Vessel Berth is no longer visible/in use for TN4.1 - should not be validated.", false, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
		}

		[TestDate(2008, 6, 30)]
		public void TestOutwardVesselBerth_()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			AddInfoJobDeclaration.SG_US_NKOutwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKOutwardVesselBerth = "KZ";
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
		}

		public void TestOutwardVesselBerthForTN4Point1()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			AddInfoJobDeclaration.SG_US_NKOutwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals("Vessel Berth is no longer visible/in use for TN4.1 - should not be validated.", false, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
		}

		public void TestCountryOfFinalDestination_()
		{
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = "ZZ";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(true, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			AssertNoWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, JobDeclarationValidation.Circular18_2010);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Slovakia;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			AssertNoWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, JobDeclarationValidation.Circular18_2010);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.KoreaNorth;
			AssertHasWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, JobDeclarationValidation.Circular18_2010);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Iran;
			AssertHasWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, JobDeclarationValidation.Circular18_2010);
			AddInfoJobDeclaration.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.KoreaNorth;
			AssertNoWarning("Warning should not be shown if declaration has cleared TradeNet", AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, JobDeclarationValidation.Circular18_2010);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Curacao;
			AssertHasWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, AddInfoJobDeclarationValidation.CuracaoNotUsedBySG);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.HongKong;
			AssertNoWarning(AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo, AddInfoJobDeclarationValidation.CuracaoNotUsedBySG);
			AssertNoErrors("Valid country code", AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo);
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.NetherlandsAntilles;
			AssertNoErrors("Inactive 'AN' country code can be ignored on SG Customs", AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo);
		}

		[TestDate(2008, 6, 30)]
		public void TestCheckSG_US_NKPlaceOfCargoRelease()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfCargoRelease = "XX";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, "Please select a valid Place of Release");
			Declaration.SG_US_NKPlaceOfCargoRelease = "KZ";
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfCargoReleaseInfo);
			Declaration.SG_US_NKPlaceOfCargoRelease = "O";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, "This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
			Declaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.MajorExporterScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, "Major Exporter Scheme exemption code is not valid for Place of Release");
			Declaration.SG_US_NKPlaceOfCargoRelease = "PPW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "CW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "MW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "KW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "JW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "SW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.AISSNotValidForRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.ImportGSTDefermentScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfCargoReleaseInfo, SGCPlaces.Constants.ReceiptRelease.IGDSNotValidForRelease);
			Declaration.SG_US_NKPlaceOfCargoRelease = "PPZ";
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfCargoReleaseInfo);
		}

		[TestDate(2008, 6, 30)]
		public void TestCheckSG_US_NKPlaceOfReceipt()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfReceipt = "XX";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, "Please select a valid Place of Receipt");
			Declaration.SG_US_NKPlaceOfReceipt = "O";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, "This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
			Declaration.SG_US_NKPlaceOfReceipt = "KZ";
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.SG_US_NKPlaceOfReceipt = "SW";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
			Declaration.SG_US_NKPlaceOfReceipt = "SZ";
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
		}

		public void TestPlaceOfReceiptForMajorExporterExemption()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			var messageError = "Major Exporter Scheme exemption code is not valid for this type of Declaration";
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.MajorExporterScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, messageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, messageError);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, messageError);
			Declaration.SG_US_NKPlaceOfReceipt = "KZ";
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
		}

		public void TestPlaceOfReceiptForAISSExemption()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
			AddInfoJobDeclaration.Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
			AddInfoJobDeclaration.Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
		}

		public void TestPlaceOfReceiptForIGDSExemption()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ImportGSTDefermentScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.IGDSInvalidUseForReceipt);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.IGDSCorrectUsageForINP);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.IGDSCorrectUsageForIPT);
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DUT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfReceiptInfo);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.IGDSInvalidUseForReceipt);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.AddInfoValidation.ValidateSG_US_NKPlaceOfReceipt();
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfReceiptInfo, SGCPlaces.Constants.ReceiptRelease.IGDSInvalidUseForReceipt);
		}

		[TestDate(2008, 6, 30)]
		public void TestCheckSG_US_NKPlaceOfStorage()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_US_NKPlaceOfStorage = "XX";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfStorageInfo, "Please select a valid Place of Storage");
			Declaration.SG_US_NKPlaceOfStorage = "O";
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfStorageInfo, "This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.MajorExporterScheme;
			AssertHasMessageError(Declaration.SG_US_NKPlaceOfStorageInfo, "Major Exporter Scheme exemption code is not valid for Place of Storage");
			Declaration.SG_US_NKPlaceOfStorage = "KZ";
			AssertNoMessageErrors(Declaration.SG_US_NKPlaceOfStorageInfo);
		}

		public void TestOutwardVessel_()
		{
			AddInfoJobDeclaration.SG_OutwardVesselName = "";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardVesselName = "RV";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "RV";
		}

		public void TestTowingVessel_()
		{
			AddInfoJobDeclaration.SG_TowingVesselName = "RV";
			Validation.ValidateSG_TowingVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_TowingVesselNameInfo.HasMessageErrors());
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "RV";
			Validation.ValidateSG_TowingVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_TowingVesselNameInfo.HasMessageErrors());
		}

		public void TestOutwardVesselType()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.SouthAfrica;
			vessel.RV_Code = "RV";
			vessel.RV_NetRegisterTon = 10;
			AddInfoJobDeclaration.SG_OutwardVesselName = "RV";
			vessel.RV_VesselType = "";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			vessel.RV_VesselType = Core.Constants.VesselType.Barge;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
		}

		[TestDate(2011, 01, 01)]
		public void TestOutwardVesselNationality()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "RV";
			vessel.RV_VesselType = Core.Constants.VesselType.Barge;
			vessel.RV_NetRegisterTon = 10;
			AddInfoJobDeclaration.SG_OutwardVesselName = "RV";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			InvoiceLine.JI_Tariff = "24011010";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.SouthAfrica;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
		}

		public void TestOutwardVesselNationalityTN41()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, Universal.AttributeNames.Codes.ISSEASTORE, "Y");
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "RV";
			vessel.RV_VesselType = Core.Constants.VesselType.Barge;
			vessel.RV_NetRegisterTon = 10;
			AddInfoJobDeclaration.SG_OutwardVesselName = "RV";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			var cpc = Declaration.CPCs.AddNew();
			cpc.SG_CPCCode = "4203000";
			cpc.SG_PC1 = "12";
			cpc.SG_PC2 = "6";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			InvoiceLine.JI_Tariff = "24011010";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Bahamas;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
		}

		public void TestClaimantName()
		{
			Validation.ValidateSG_ClaimantName();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantNameInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			AddInfoJobDeclaration.SG_ClaimantCode = "TEST";
			Validation.ValidateSG_ClaimantName();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantNameInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ClaimantCode = "";
			Declaration.JE_OH_Claimant = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Validation.ValidateSG_ClaimantName();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantNameInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ClaimantName = "NAME";
			Validation.ValidateSG_ClaimantName();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantNameInfo.HasMessageErrors());
		}

		public void TestClaimantCode_()
		{
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			OrgHeader claimant = Factory.New<OrgHeader>();
			Declaration.JE_OH_Claimant = claimant.PK;
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			Declaration.JE_OH_Claimant = ZGuid.Empty;
			AddInfoJobDeclaration.SG_ClaimantName = "TEST";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ClaimantCode = "CODE";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasWarning("Claimant Code should start with 'S', T', 'M' or 'P'"));
			AddInfoJobDeclaration.SG_ClaimantCode = "P11111322";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasWarning("Claimant Code should start with 'S', T', 'M' or 'P'"));
		}

		public void TestDutyExemption()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Validation.ValidateSG_DutyExempt();
			AssertEquals(false, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_DutyExempt = true;
			Validation.ValidateSG_DutyExempt();
			AssertEquals(true, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageErrors());
			AssertEquals(true, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageError("Duty Exemption is not valid unless a Claimant is entered"));
			AddInfoJobDeclaration.SG_ClaimantCode = "S1234567E";
			Validation.ValidateSG_DutyExempt();
			AssertEquals(false, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageErrors());
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
			AddInfoJobDeclaration.SG_DutyExempt = true;
			Validation.ValidateSG_DutyExempt();
			AssertEquals(true, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageErrors());
			AssertEquals(true, AddInfoJobDeclaration.SG_DutyExemptInfo.HasMessageError("Duty Exemption is not valid for this type of declaration"));
		}

		public void TestSupplyIndicator_()
		{
			AddInfoJobDeclaration.SG_SupplyIndicator = "X";
			Validation.ValidateSG_SupplyIndicator();
			AssertEquals(true, AddInfoJobDeclaration.SG_SupplyIndicatorInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_SupplyIndicator = SupplyIndicatorCodeList.Codes.Y;
			Validation.ValidateSG_SupplyIndicator();
			AssertEquals(false, AddInfoJobDeclaration.SG_SupplyIndicatorInfo.HasMessageErrors());
		}

		public void TestApplicationProductType_()
		{
			AddInfoJobDeclaration.SG_ApplicationProductType = "X";
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(true, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			Validation.ValidateSG_ApplicationProductType();
			AssertEquals(false, AddInfoJobDeclaration.SG_ApplicationProductTypeInfo.HasMessageErrors());
		}

		public void TestDonorCountry_()
		{
			AddInfoJobDeclaration.SG_RN_NKDonorCountry = "X";
			Validation.ValidateSG_RN_NKDonorCountry();
			AssertEquals(true, AddInfoJobDeclaration.SG_RN_NKDonorCountryInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RN_NKDonorCountry = Core.Constants.CountryCodes.SouthAfrica;
			Validation.ValidateSG_RN_NKDonorCountry();
			AssertEquals(false, AddInfoJobDeclaration.SG_RN_NKDonorCountryInfo.HasMessageErrors());
		}

		public void TestEntryYear_()
		{
			AddInfoJobDeclaration.SG_EntryYear = 1899;
			Validation.ValidateSG_EntryYear();
			AssertEquals(true, AddInfoJobDeclaration.SG_EntryYearInfo.HasWarnings());
			AddInfoJobDeclaration.SG_EntryYear = 1920;
			Validation.ValidateSG_EntryYear();
			AssertEquals(false, AddInfoJobDeclaration.SG_EntryYearInfo.HasWarnings());
			AddInfoJobDeclaration.SG_EntryYear = 2101;
			Validation.ValidateSG_EntryYear();
			AssertEquals(true, AddInfoJobDeclaration.SG_EntryYearInfo.HasWarnings());
			AddInfoJobDeclaration.SG_EntryYear = -1;
			Validation.ValidateSG_EntryYear();
			AssertEquals(true, AddInfoJobDeclaration.SG_EntryYearInfo.HasErrors());
		}

		public void TestCert1Type_()
		{
			AddInfoJobDeclaration.SG_Cert1Type = "X";
			Validation.ValidateSG_Cert1Type();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert1TypeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_Cert1Type = "1";
			Validation.ValidateSG_Cert1Type();
			AssertEquals(false, AddInfoJobDeclaration.SG_Cert1TypeInfo.HasMessageErrors());
		}

		public void TestCert2Type_()
		{
			AddInfoJobDeclaration.SG_Cert2Type = "X";
			Validation.ValidateSG_Cert2Type();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert2TypeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_Cert2Type = "1";
			Validation.ValidateSG_Cert2Type();
			AssertEquals(false, AddInfoJobDeclaration.SG_Cert2TypeInfo.HasMessageErrors());
		}

		public void TestCertReferenceCurrency_()
		{
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "";
			Validation.ValidateSG_RX_NKCertReferenceCurrency();
			AssertEquals("Currency should have default to SGD when blank & CO", false, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrencyInfo.HasMessageErrors());
			AssertEquals("SGD should default here", AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency, Core.Constants.CurrencyCodes.Singapore);
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "X";
			Validation.ValidateSG_RX_NKCertReferenceCurrency();
			AssertEquals(true, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrencyInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			Validation.ValidateSG_RX_NKCertReferenceCurrency();
			AssertEquals(false, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrencyInfo.HasMessageErrors());
		}

		public void TestCert1PercCommContent()
		{
			AddInfoJobDeclaration.SG_Cert1PercCommContent = -1;
			Validation.ValidateSG_Cert1PercCommContent();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert1PercCommContentInfo.HasErrors());
			AddInfoJobDeclaration.SG_Cert1PercCommContent = 101;
			Validation.ValidateSG_Cert1PercCommContent();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert1PercCommContentInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert1PercCommContent = 55;
			Validation.ValidateSG_Cert1PercCommContent();
			AssertEquals(false, AddInfoJobDeclaration.SG_Cert1PercCommContentInfo.HasNotifications());
		}

		public void TestCert1CopiesNo()
		{
			AddInfoJobDeclaration.SG_Cert1CopiesNo = -1;
			Validation.ValidateSG_Cert1CopiesNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert1CopiesNoInfo.HasErrors());
			AddInfoJobDeclaration.SG_Cert1CopiesNo = 11;
			Validation.ValidateSG_Cert1CopiesNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert1CopiesNoInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert1CopiesNo = 5;
			Validation.ValidateSG_Cert1CopiesNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_Cert1CopiesNoInfo.HasNotifications());
		}

		public void TestCert2CopiesNo()
		{
			AddInfoJobDeclaration.SG_Cert2CopiesNo = -1;
			Validation.ValidateSG_Cert2CopiesNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert2CopiesNoInfo.HasErrors());
			AddInfoJobDeclaration.SG_Cert2CopiesNo = 11;
			Validation.ValidateSG_Cert2CopiesNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_Cert2CopiesNoInfo.HasWarnings());
			AddInfoJobDeclaration.SG_Cert2CopiesNo = 5;
			Validation.ValidateSG_Cert2CopiesNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_Cert2CopiesNoInfo.HasNotifications());
		}

		public void TestDutyUnitRate()
		{
			AddInfoJobDeclaration.SG_DutyUnitRate = -1;
			Validation.ValidateSG_DutyUnitRate();
			AssertEquals(true, AddInfoJobDeclaration.SG_DutyUnitRateInfo.HasErrors());
			AddInfoJobDeclaration.SG_DutyUnitRate = 1;
			Validation.ValidateSG_DutyUnitRate();
			AssertEquals(false, AddInfoJobDeclaration.SG_DutyUnitRateInfo.HasErrors());
		}

		public void TestExciseUnitRate()
		{
			AddInfoJobDeclaration.SG_ExciseUnitRate = -1;
			Validation.ValidateSG_ExciseUnitRate();
			AssertEquals(true, AddInfoJobDeclaration.SG_ExciseUnitRateInfo.HasErrors());
			AddInfoJobDeclaration.SG_ExciseUnitRate = 1;
			Validation.ValidateSG_ExciseUnitRate();
			AssertEquals(false, AddInfoJobDeclaration.SG_ExciseUnitRateInfo.HasErrors());
		}

		public void TestExciseUnitRateHas3Decimals()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var excisableTariff = helper.LoadOrCreateNewTariff(tariffType, "24899999");
			helper.CreateTariffExciseRate(excisableTariff, 0.427m, UnitOfQuantityCodeList.Codes.KGM);
			Factory.Save();
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "24899999";
			AssertEquals("Excise rate displayed should show exact rate from reference data", 0.427m, invoiceLine.SG_ExciseUnitRate);
		}

		public void TestMasterBillPrefixMatchesAirline()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AddInfoJobDeclaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Air;
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "";
			AddInfoJobDeclaration.SG_OutwardMAWB = "082-98739454";
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasWarnings());
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "HW652";
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasWarnings());
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "QF112";
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasWarnings());
			AddInfoJobDeclaration.SG_OutwardMAWB = "";
			AddInfoJobDeclaration.SG_IsOutwardHandCarried = true;
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasWarnings());
			AddInfoJobDeclaration.SG_OutwardMAWB = "081-49837494";
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasWarnings());
		}

		public void TestIsOutwardHandCarried()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AddInfoJobDeclaration.SG_OutwardTransportMode = Core.Constants.TransportModes.Air;
			Validation.ValidateSG_OutwardMAWB();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_IsOutwardHandCarried = true;
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
		}

		public void TestIsInwardHandCarried()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.Validation.ValidateJE_MasterBill();
			AssertEquals(true, Declaration.JE_MasterBillInfo.HasMessageErrors());
			Declaration.SG_IsInwardHandCarried = true;
			AssertEquals(false, Declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		protected override AddInfo GetNewAddInfo()
		{
			return AddInfoJobDeclaration;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SGCertificateTypeHelper.CreateCertificateTypes(Factory);
			Factory.Save();
		}

		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageType;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		protected virtual string MessageType
		{
			get
			{
				return "";
			}
		}

		protected AddInfoJobDeclarationValidation Validation
		{
			get
			{
				return AddInfoJobDeclaration.Validation;
			}
		}

		protected AddInfoJobDeclaration AddInfoJobDeclaration => Declaration.AddInfo;

		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		#endregion
	}
}
