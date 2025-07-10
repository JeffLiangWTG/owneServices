using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USVehicleAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVehicleValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			OrgHeader party1 = Factory.New<OrgHeader>();
			OrgAddress orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			OrgHeader party2 = Factory.New<OrgHeader>();
			OrgAddress orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			var vehicle = GetVehicle();

			vehicle.US_OA_Owner = orgAddress1.PK;
			AssertHasWarning(vehicle.US_OA_OwnerInfo, addressDescriptionWarning);
			AssertHasWarning(vehicle.US_OA_OwnerInfo, addressCodeWarning);
			vehicle.US_OA_Owner = orgAddress2.PK;
			AssertNoWarning(vehicle.US_OA_OwnerInfo, addressDescriptionWarning);
			AssertNoWarning(vehicle.US_OA_OwnerInfo, addressCodeWarning);

			vehicle.US_OA_StorageLocation = orgAddress1.PK;
			AssertHasWarning(vehicle.US_OA_StorageLocationInfo, addressDescriptionWarning);
			AssertHasWarning(vehicle.US_OA_StorageLocationInfo, addressCodeWarning);
			vehicle.US_OA_StorageLocation = orgAddress2.PK;
			AssertNoWarning(vehicle.US_OA_StorageLocationInfo, addressDescriptionWarning);
			AssertNoWarning(vehicle.US_OA_StorageLocationInfo, addressCodeWarning);
		}

		public void TestCheckUS_PGAContactName()
		{
			var vehicle = GetVehicle();
			vehicle.AddInfoValidation.ValidateUS_ContactName();
			AssertHasMessageErrorContaining(vehicle.US_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			vehicle.US_ContactName = "TEST NAME";
			AssertNoMessageErrorContaining(vehicle.US_ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactPhoneNo()
		{
			var vehicle = GetVehicle();
			vehicle.US_ContactPhoneNo = "";
			vehicle.AddInfoValidation.ValidateUS_ContactPhoneNo();
			AssertHasMessageErrorContaining(vehicle.US_ContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.US_ContactPhoneNo = "0122232323";
			AssertNoMessageErrorContaining(vehicle.US_ContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactEmail()
		{
			var vehicle = GetVehicle();
			vehicle.AddInfoValidation.ValidateUS_ContactEmail();
			AssertHasMessageErrorContaining(vehicle.US_ContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			vehicle.US_ContactEmail = "~";
			AssertNoMessageErrorContaining(vehicle.US_ContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(vehicle.US_ContactEmailInfo, "Invalid email format");
			vehicle.US_ContactEmail = "test.abc@def.com";
			AssertNoWarningContaining(vehicle.US_ContactEmailInfo, "Invalid email format");
		}

		public void TestListValidations()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = "!";
			vehicle.US_ImportCode = "~";
			vehicle.US_IndustryCode = "~";
			vehicle.US_BodyType = CommodityVehicleQualifierCodesList.Codes.V03;
			vehicle.US_BondExemption = "~";
			vehicle.US_CertifyingIndividual = "~";
			AssertHasMessageErrorContaining(vehicle.US_FormTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(vehicle.US_ImportCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(vehicle.US_IndustryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(vehicle.US_BodyTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(vehicle.US_BondExemptionInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(vehicle.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._12;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.D;
			vehicle.US_BodyType = CommodityVehicleQualifierCodesList.Codes.V00;
			vehicle.US_BondExemption = YesNoDefaultList.Codes.No;
			vehicle.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertNoMessageErrorContaining(vehicle.US_FormTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(vehicle.US_ImportCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(vehicle.US_IndustryCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(vehicle.US_BodyTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(vehicle.US_FormTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(vehicle.US_ImportCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(vehicle.US_BondExemptionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(vehicle.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);

			vehicle.US_FormType = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_FormTypeInfo, MandatoryValidation.YouHaveNotEntered);
			vehicle.US_ImportCode = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_ImportCodeInfo, MandatoryValidation.YouHaveNotEntered);
			vehicle.US_CertifyingIndividual = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCommodityValidations()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicle.AddInfoValidation.ValidateUS_ModelYear();
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_1);
			vehicle.US_ModelYear = "2014";
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_1);
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRange);
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearFormat);

			vehicle.US_ModelYear = "1234";
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRange);
			vehicle.US_ModelYear = "R234";
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearFormat);

			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.C;
			vehicle.AddInfoValidation.ValidateUS_EPARegNumber();
			AssertHasMessageErrorContaining(vehicle.US_EPARegNumberInfo, USVehicleAddInfoValidation.EPARegNoRequiredForICI);

			vehicle.US_EPARegNumber = "12345LKJ1001";
			AssertNoMessageErrorContaining(vehicle.US_EPARegNumberInfo, USVehicleAddInfoValidation.EPARegNoRequiredForICI);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._24C;
			vehicle.AddInfoValidation.ValidateUS_CBPBondNumber();
			AssertHasMessageError(vehicle.US_CBPBondNumberInfo, USVehicleAddInfoValidation.CBPBondRequiredFor3520_21);
			vehicle.US_CBPBondNumber = "1290563";
			AssertNoMessageError(vehicle.US_CBPBondNumberInfo, USVehicleAddInfoValidation.CBPBondRequiredFor3520_21);
		}

		public void TestVNEValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var vehicle = GetVehicle();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CASYD";
			vehicle.US_OA_Owner = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_Owner();
			AssertHasMessageErrorContaining(vehicle.US_OA_OwnerInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			vehicle.US_OA_Owner = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_Owner();
			AssertNoMessageErrorContaining(vehicle.US_OA_OwnerInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			vehicle.US_OA_Owner = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_Owner();
			AssertNoMessageErrorContaining(vehicle.US_OA_OwnerInfo, "The state is not a valid");

			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CASYD";
			vehicle.US_OA_StorageLocation = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_StorageLocation();
			AssertHasMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			vehicle.US_OA_StorageLocation = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_StorageLocation();
			AssertNoMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			vehicle.US_OA_StorageLocation = address.PK;
			vehicle.AddInfoValidation.ValidateUS_OA_StorageLocation();
			AssertNoMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, "The state is not a valid");
		}

		public void TestPropertiesValidation()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			vehicle.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(vehicle.US_VehicleModelInfo, MandatoryValidation.YouHaveNotEntered);
			var vehicleDetail = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetail.US_BuildYear = "2016";
			vehicle.AddInfoValidation.ValidateAll();
			Assert(vehicle.VehicleAndEngineDetails.Count > 0);
			AssertHasMessageErrorContaining(vehicle.US_VehicleModelInfo, MandatoryValidation.YouHaveNotEntered);

			AssertNoMessageErrorContaining(vehicle.US_IndustryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(vehicle.US_OA_OwnerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(vehicle.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.US_VehicleModel = "Test";
			AssertNoMessageErrorContaining(vehicle.US_VehicleModelInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			vehicle.US_OA_Owner = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(vehicle.US_OA_OwnerInfo, USVehicleAddInfoValidation.StorageLocationRequired);
			AssertNoMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._25;
			AssertHasMessageErrorContaining(vehicle.US_ExemptionRemarksInfo, USVehicleAddInfoValidation.ExemptionRemarksRequired);
			vehicle.US_ExemptionRemarks = "Test";
			AssertNoMessageErrorContaining(vehicle.US_ExemptionRemarksInfo, USVehicleAddInfoValidation.ExemptionRemarksRequired);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			AssertHasMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_21);
			vehicle.US_EnginePower = 13m;
			AssertNoMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_21);
			AssertHasMessageErrorContaining(vehicle.US_EnginePowerUQInfo, USVehicleAddInfoValidation.EnginePowerUQRequired);
			vehicle.US_EnginePowerUQ = EnginePowerUQList.Codes.HP;
			AssertNoMessageErrorContaining(vehicle.US_EnginePowerUQInfo, USVehicleAddInfoValidation.EnginePowerUQRequired);
			vehicle.US_EnginePower = ZDecimal.Zero;
			AssertHasMessageErrorContaining(vehicle.US_EnginePowerUQInfo, USVehicleAddInfoValidation.EnginePowerUQNotRequired);
			vehicle.US_EnginePowerUQ = ZString.Empty;
			AssertNoMessageErrorContaining(vehicle.US_EnginePowerUQInfo, USVehicleAddInfoValidation.EnginePowerUQNotRequired);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._24A;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.B;
			vehicle.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(vehicle.US_ImportCodeInfo, USVehicleAddInfoValidation.ImportCode24);
			AssertHasMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, USVehicleAddInfoValidation.StorageLocationRequired);

			vehicle.US_OA_StorageLocation = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(vehicle.US_OA_OwnerInfo, USVehicleAddInfoValidation.StorageLocationRequired);

			vehicle.US_IndustryCode = IndustryCodesList.Codes.D;
			AssertNoMessageErrorContaining(vehicle.US_ImportCodeInfo, USVehicleAddInfoValidation.ImportCode24);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.AddInfoValidation.ValidateUS_BondExemption();
			AssertHasMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_BondExemption = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
			AssertNoMessageErrorContaining(vehicle.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestBondPolicyAndNAICNumberNotRequiredWhenExemptFromBond()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.US_BondPolicyNo = "";
			vehicle.US_NAICNo = "";

			vehicle.US_BondExemption = "Y";
			AssertNoMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_BondExemption = "N";
			AssertHasMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);
			AssertHasMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);
		}

		public void TestStateOfIssueNotRequiredWhenExemptFromBond()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.US_StateOfIssue = "";

			vehicle.US_BondExemption = "Y";
			AssertNoMessageErrorContaining(vehicle.US_StateOfIssueInfo, USVehicleAddInfoValidation.StateRequired);

			vehicle.US_BondExemption = "N";
			AssertHasMessageErrorContaining(vehicle.US_StateOfIssueInfo, USVehicleAddInfoValidation.StateRequired);
		}

		public void TestOwnerPGADetails()
		{
			var vehicle = GetVehicle();

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			vehicle.US_OA_Owner = orgHeader.MainAddress.PK;
			vehicle.US_OA_StorageLocation = orgHeader.MainAddress.PK;
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.";
			AssertHasMessageErrorContaining(vehicle.US_OA_OwnerInfo, errorMsg);
			AssertHasMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, errorMsg);
		}

		public void TestCheckUS_FormType()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertHasMessageErrorContaining(vehicle.US_FormTypeInfo, USVehicleAddInfoValidation.AtLeastOneVehicleLineRequired);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertHasMessageErrorContaining(vehicle.US_FormTypeInfo, USVehicleAddInfoValidation.AtLeastOneVehicleLineRequired);

			vehicle.VehicleAndEngineDetails.AddNew();
			vehicle.AddInfoValidation.ValidateUS_FormType();
			AssertNoMessageErrorContaining(vehicle.US_FormTypeInfo, USVehicleAddInfoValidation.AtLeastOneVehicleLineRequired);
		}

		public void TestCheckUS_CBPBondNumber()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._24C;
			vehicle.AddInfoValidation.ValidateUS_CBPBondNumber();
			AssertHasMessageErrorContaining(vehicle.US_CBPBondNumberInfo, USVehicleAddInfoValidation.CBPBondRequiredFor3520_21);

			vehicle.US_CBPBondNumber = "1~";
			AssertNoMessageErrorContaining(vehicle.US_CBPBondNumberInfo, USVehicleAddInfoValidation.CBPBondRequiredFor3520_21);
		}

		public void TestCheckUS_ModelYear()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			vehicle.AddInfoValidation.ValidateUS_ModelYear();
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_1);

			vehicle.US_ModelYear = "2016";
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_1);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._09;
			vehicle.US_ModelYear = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_21);

			vehicle.US_ModelYear = "2016";
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredFor3520_21);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.US_ModelYear = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredForImportCode_01);

			vehicle.US_ModelYear = "2016";
			AssertNoMessageErrorContaining(vehicle.US_ModelYearInfo, USVehicleAddInfoValidation.ModelYearRequiredForImportCode_01);
		}

		public void TestCheckUS_CertOfConformity()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.A;
			AssertNoMessageErrorContaining(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.EngineFamilyNumberRequiredFor3520_1);

			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.B;
			AssertHasMessageErrorContaining(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.EngineFamilyNumberRequiredFor3520_1);

			vehicle.US_CertOfConformity = "1~";
			AssertNoMessageErrorContaining(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.EngineFamilyNumberRequiredFor3520_1);
			AssertHasMessageError(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.LengthShouldBe12);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.US_CertOfConformity = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.EngineFamilyNumberRequiredFor3520_21);

			vehicle.US_CertOfConformity = "123456789abc";
			AssertNoMessageErrorContaining(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.EngineFamilyNumberRequiredFor3520_21);
			AssertNoMessageError(vehicle.US_CertOfConformityInfo, USVehicleAddInfoValidation.LengthShouldBe12);
		}

		public void TestCheckUS_VehicleExemptionNumber()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.G;
			AssertHasMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_1);

			vehicle.US_VehicleExemptionNumber = "1~";
			AssertNoMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_1);
			AssertNoWarningContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredForImportCodeM);

			vehicle.US_VehicleExemptionNumber = ZString.Empty;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.M;
			AssertHasWarningContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredForImportCodeM);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._10;
			AssertHasMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_21);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._14;
			AssertNoMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_21);

			vehicle.US_VehicleExemptionNumber = "1~";
			AssertNoMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_21);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._02;
			vehicle.US_VehicleExemptionNumber = "";
			AssertHasMessageErrorContaining(vehicle.US_VehicleExemptionNumberInfo, USVehicleAddInfoValidation.ExemptionNumberRequiredFor3520_21);
		}

		public void TestCheckUS_OA_StorageLocation()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._24A;
			vehicle.AddInfoValidation.ValidateUS_OA_StorageLocation();
			AssertHasMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, USVehicleAddInfoValidation.StorageLocationRequired);

			var storageOrg = Factory.New<OrgHeader>();
			storageOrg.OH_Code = "TESTORG";
			vehicle.US_OA_StorageLocation = storageOrg.MainAddress.PK;
			AssertNoMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, USVehicleAddInfoValidation.StorageLocationRequired);
			AssertHasMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, OrganisationValidation.PostCodeIsRequired);

			storageOrg.MainAddress.OA_PostCode = "111111";
			vehicle.AddInfoValidation.ValidateUS_OA_StorageLocation();
			AssertNoMessageErrorContaining(vehicle.US_OA_StorageLocationInfo, OrganisationValidation.PostCodeIsRequired);
		}

		public void TestCheckUS_BondExemption()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.AddInfoValidation.ValidateUS_BondExemption();
			AssertNoMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.AddInfoValidation.ValidateUS_BondExemption();
			AssertHasMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_BondExemption = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.US_IndustryCode = ZString.Empty;
			vehicle.US_BondExemption = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);

			vehicle.US_BondExemption = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(vehicle.US_BondExemptionInfo, USVehicleAddInfoValidation.BondExemptionRequired);
		}

		public void TestCheckUS_BondPolicyNo()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.G;
			vehicle.AddInfoValidation.ValidateUS_BondPolicyNo();
			AssertHasMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_1);

			Assert(vehicle.US_BondExemption_ReadOnly);

			vehicle.US_BondPolicyNo = "1~";
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_1);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.AddInfoValidation.ValidateUS_BondPolicyNo();
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.US_BondPolicyNo = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			Assert(!vehicle.US_BondExemption_ReadOnly);

			vehicle.US_BondExemption = "Y";
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_BondExemption = "N";
			AssertHasMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_BondPolicyNo = "1~";
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.US_IndustryCode = ZString.Empty;
			vehicle.US_BondPolicyNo = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);

			vehicle.US_BondPolicyNo = "1~";
			AssertNoMessageErrorContaining(vehicle.US_BondPolicyNoInfo, USVehicleAddInfoValidation.BondPolicyNoRequired3520_21);
		}

		public void TestCheckUS_NAICNo()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._01;
			vehicle.AddInfoValidation.ValidateUS_NAICNo();
			AssertNoMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			vehicle.US_IndustryCode = IndustryCodesList.Codes.G;
			vehicle.AddInfoValidation.ValidateUS_NAICNo();
			AssertHasMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			Assert(!vehicle.US_BondExemption_ReadOnly);

			vehicle.US_BondExemption = "Y";
			AssertNoMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			vehicle.US_BondExemption = "N";
			AssertHasMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			vehicle.US_NAICNo = "1~";
			AssertNoMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.US_IndustryCode = ZString.Empty;
			vehicle.US_NAICNo = ZString.Empty;
			AssertHasMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);

			vehicle.US_NAICNo = "1~";
			AssertNoMessageErrorContaining(vehicle.US_NAICNoInfo, USVehicleAddInfoValidation.NAICNoRequired);
		}

		public void TestCheckUS_StateOfIssue()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.AddInfoValidation.ValidateUS_StateOfIssue();
			AssertHasMessageErrorContaining(vehicle.US_StateOfIssueInfo, USVehicleAddInfoValidation.StateRequired);

			vehicle.US_StateOfIssue = "1~";
			AssertNoMessageErrorContaining(vehicle.US_StateOfIssueInfo, USVehicleAddInfoValidation.StateRequired);
		}

		public void TestCheckUS_EnginePower()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicle.US_ImportCode = ImportCodesForm3520_1List.Codes.U;
			vehicle.AddInfoValidation.ValidateUS_EnginePower();
			AssertHasMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_1);

			vehicle.US_EnginePower = 100m;
			AssertNoMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_1);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._22;
			vehicle.US_EnginePower = 0m;
			AssertHasMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_21);

			vehicle.US_EnginePower = 100m;
			AssertNoMessageErrorContaining(vehicle.US_EnginePowerInfo, USVehicleAddInfoValidation.EnginePowerRequiredFor3520_21);
		}

		public void TestCheckUS_ExemptionRemarks()
		{
			var vehicle = GetVehicle();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			vehicle.US_ImportCode = ImportCodesForm3520_21List.Codes._21;
			vehicle.AddInfoValidation.ValidateUS_ExemptionRemarks();
			AssertHasMessageErrorContaining(vehicle.US_ExemptionRemarksInfo, USVehicleAddInfoValidation.ExemptionRemarksRequired);

			vehicle.US_ExemptionRemarks = "1~";
			AssertNoMessageErrorContaining(vehicle.US_ExemptionRemarksInfo, USVehicleAddInfoValidation.ExemptionRemarksRequired);
		}

		Vehicle GetVehicle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.VehicleLines.AddNew();
		}
	}
}
