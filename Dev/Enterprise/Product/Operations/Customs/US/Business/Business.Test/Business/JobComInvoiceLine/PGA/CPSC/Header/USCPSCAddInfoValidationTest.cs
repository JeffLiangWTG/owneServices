using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCPSCAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCPSCValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
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

			AssertNoWarning(Header.US_OA_ManufacturerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_ManufacturerAddressInfo, addressCodeWarning);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_OA_ManufacturerAddress = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_ManufacturerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_ManufacturerAddressInfo, addressCodeWarning);
			Header.US_OA_ManufacturerAddress = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_ManufacturerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_ManufacturerAddressInfo, addressCodeWarning);
		}

		public void TestCheckUS_ManufacturerMonthAndYear()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_ManufacturerMonthAndYear = "";
			AssertHasMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);

			Header.US_ManufacturerMonthAndYear = "132022";
			AssertNoMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);

			Header.US_ManufacturerMonthAndYear = "122022";
			AssertNoMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);

			Header.US_ManufacturerMonthAndYear = "002022";
			AssertHasMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Header.AddInfoValidation.ValidateUS_ManufacturerMonthAndYear();
			AssertNoMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);

			Header.US_ProcessingCode = ZString.Empty;
			Header.AddInfoValidation.ValidateUS_ManufacturerMonthAndYear();
			AssertNoMessageErrorContaining(Header.US_ManufacturerMonthAndYearInfo, USCPSCAddInfoValidation.ManufacturerDateFormatIsIncorrect);
		}

		public void TestCheckUS_ProductCode()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Header.US_ProductCode = "123ABC";
			AssertNoMessageErrorContaining(Header.US_ProductCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProductCode = "";
			AssertHasMessageErrorContaining(Header.US_ProductCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_ProductCode();
			AssertNoMessageErrorContaining(Header.US_ProductCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			Header.AddInfoValidation.ValidateUS_ProductCode();
			AssertNoMessageErrorContaining(Header.US_ProductCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductCodeVersionNumber()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Header.US_ProductCodeVersionNumber = "123ABC";
			AssertNoMessageErrorContaining(Header.US_ProductCodeVersionNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProductCodeVersionNumber = "";
			AssertHasMessageErrorContaining(Header.US_ProductCodeVersionNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_ProductCodeVersionNumber();
			AssertNoMessageErrorContaining(Header.US_ProductCodeVersionNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			Header.AddInfoValidation.ValidateUS_ProductCodeVersionNumber();
			AssertNoMessageErrorContaining(Header.US_ProductCodeVersionNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CertificateExistsInfo()
		{
			Header.US_CertificateExists = "~";
			AssertHasMessageErrorContaining(Header.US_CertificateExistsInfo, ListValidation.InvalidCodeMessageError);

			Header.US_CertificateExists = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(Header.US_CertificateExistsInfo, ListValidation.InvalidCodeMessageError);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			Header.US_CertificateExists = "";
			AssertHasMessageErrorContaining(Header.US_CertificateExistsInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			AssertHasMessageErrorContaining(Header.US_CertificateExistsInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_CertificateExists = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(Header.US_CertificateExistsInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = ZString.Empty;
			Header.US_CertificateExists = ZString.Empty;
			AssertNoMessageErrorContaining(Header.US_CertificateExistsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProcessingCode()
		{
			Header.US_ProcessingCode = "~";
			AssertHasMessageErrorContaining(Header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_ProcessingCode = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.US_ProcessingCodeInfo, USCPSCAddInfoValidation.AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);

			Header.RuleAndLabs.AddNew();
			Header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, USCPSCAddInfoValidation.AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);

			Header.RuleAndLabs.RemoveAndDeleteAll();
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Header.US_ProcessingCodeInfo, USCPSCAddInfoValidation.AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);

			Header.RuleAndLabs.AddNew();
			Header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, USCPSCAddInfoValidation.AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);

			Header.RuleAndLabs.RemoveAndDeleteAll();
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			AssertNoMessageErrorContaining(Header.US_ProcessingCodeInfo, USCPSCAddInfoValidation.AtLeastOneValidateThatThereAreRuleAndLabsShouldExist);
		}

		public void TestCheckUS_ReferenceNumber()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Header.US_ReferenceNumber = "123ABC";
			AssertNoMessageErrorContaining(Header.US_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ReferenceNumber = "";
			AssertHasMessageErrorContaining(Header.US_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_ReferenceNumber();
			AssertNoMessageErrorContaining(Header.US_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductIDType()
		{
			Header.US_ProductIDType = "~";
			AssertHasMessageErrorContaining(Header.US_ProductIDTypeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_ProductIDType = ProductIDTypeCodeList.Codes.AI;
			AssertNoMessageErrorContaining(Header.US_ProductIDTypeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_ProductIDType = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_ProductIDTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Header.AddInfoValidation.ValidateUS_ProductIDType();
			AssertNoMessageErrorContaining(Header.US_ProductIDTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_ProcessingCode = ZString.Empty;
			Header.AddInfoValidation.ValidateUS_ProductIDType();
			AssertNoMessageErrorContaining(Header.US_ProductIDTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductID()
		{
			Header.US_ProductIDType = ProductIDTypeCodeList.Codes.AI;
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_ProductID = "123";
			AssertNoMessageErrorContaining(Header.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_ProductID = "";
			AssertHasMessageErrorContaining(Header.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_ProcessingCode = ZString.Empty;
			Header.AddInfoValidation.ValidateUS_ProductID();
			AssertNoMessageErrorContaining(Header.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "Test");
			var intendedUseCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, "130.000", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(intendedUseCode.PK, RefCusCodeListAttributeTypes.Codes.PGAIUCAgency, "CPS");
			Factory.Save();

			Header.US_IntendedUseCode = "~";
			AssertHasMessageErrorContaining(Header.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_IntendedUseCode = "130.000";
			AssertNoMessageErrorContaining(Header.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_IntendedUseDescription()
		{
			Header.US_IntendedUseCode = "980.000";
			Header.US_IntendedUseDescription = "123";
			AssertNoMessageErrorContaining(Header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_IntendedUseDescription = "";
			AssertHasMessageErrorContaining(Header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_IntendedUseCode = "1";
			Header.AddInfoValidation.ValidateUS_IntendedUseDescription();
			AssertNoMessageErrorContaining(Header.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SKUProductCode()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_SKUProductCode = "123";
			AssertNoMessageErrorContaining(Header.US_SKUProductCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_SKUProductCode = "";
			AssertHasMessageErrorContaining(Header.US_SKUProductCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductName()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.US_ProductName = "123";
			AssertNoMessageErrorContaining(Header.US_ProductNameInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_ProductName = "";
			AssertHasMessageErrorContaining(Header.US_ProductNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckItemIdentityNumber()
		{
			Header.US_ProcessingCode = ZString.Empty;
			Header.US_ModelNumber = ZString.Empty;
			Header.US_SerialNumber = ZString.Empty;
			Header.US_RegisteredNumber = ZString.Empty;
			Header.US_AltenateID = ZString.Empty;
			Header.AddInfoValidation.ValidateAll();
			AssertNoMessageError(Header.US_ModelNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_SerialNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_RegisteredNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_AltenateIDInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);

			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateAll();
			AssertHasMessageError(Header.US_ModelNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertHasMessageError(Header.US_SerialNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertHasMessageError(Header.US_RegisteredNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertHasMessageError(Header.US_AltenateIDInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);

			Header.US_ModelNumber = "17Len";
			Header.AddInfoValidation.ValidateAll();
			AssertNoMessageError(Header.US_ModelNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_SerialNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_RegisteredNumberInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);
			AssertNoMessageError(Header.US_AltenateIDInfo, USCPSCAddInfoValidation.AtLeastOneNumberRequired);

			Header.US_ModelNumber = "1,2,3,4,5,6";
			AssertHasMessageError(Header.US_ModelNumberInfo, ZString.Format(USCPSCAddInfoValidation.MaxCountForNumbers, "Model Numbers"));

			Header.US_ModelNumber = "1,2,3,4,5";
			AssertNoMessageError(Header.US_ModelNumberInfo, ZString.Format(USCPSCAddInfoValidation.MaxCountForNumbers, "Model Numbers"));
		}

		public void TestCheckUS_OA_ManufacturerAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			AssertNoMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, MandatoryValidation.YouHaveNotEntered);
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, "AAAA", "BBBB", null, null, null);
			Header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, "1234567", null, null);
			Header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, null, null, "23456");
			Header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(Header.US_OA_ManufacturerAddressInfo, errorMsg);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			header.US_OA_ManufacturerAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertHasMessageErrorContaining(header.US_OA_ManufacturerAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(header.US_OA_ManufacturerAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(header.US_OA_ManufacturerAddressInfo, "The state is not a valid");
		}

		public void TestUS_OA_CertifyingEntityAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			AssertNoMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertHasMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_OA_CertifyingEntityAddress = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, MandatoryValidation.YouHaveNotEntered);
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, "AAAA", "BBBB", null, null, null);
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertHasMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, "1234567", null, null);
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertHasMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, null, null, "23456");
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertNoMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, errorMsg);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			Header.US_OA_CertifyingEntityAddress = address.PK;
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertHasMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Header.US_OA_CertifyingEntityAddress = orgHeader.MainAddress.PK;
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertNoMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Header.US_OA_CertifyingEntityAddress = orgHeader.MainAddress.PK;
			Header.AddInfoValidation.ValidateUS_OA_CertifyingEntityAddress();
			AssertNoMessageErrorContaining(Header.US_OA_CertifyingEntityAddressInfo, "The state is not a valid");
		}

		public void TestUS_OA_ContactPointAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			AssertNoMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_OA_ContactPointAddress = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, MandatoryValidation.YouHaveNotEntered);
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, "AAAA", "BBBB", null, null, null);
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, "1234567", null, null);
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(orgHeader, null, null, null, null, "23456");
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertNoMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, errorMsg);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			Header.US_OA_ContactPointAddress = address.PK;
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertHasMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Header.US_OA_ContactPointAddress = orgHeader.MainAddress.PK;
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertNoMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Header.US_OA_ContactPointAddress = orgHeader.MainAddress.PK;
			Header.AddInfoValidation.ValidateUS_OA_ContactPointAddress();
			AssertNoMessageErrorContaining(Header.US_OA_ContactPointAddressInfo, "The state is not a valid");
		}

		public void TestUS_RuleCodes()
		{
			Header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;

			Header.US_NoLabTestingRequired = true;
			Header.US_RuleCodes = "";
			Header.AddInfoValidation.ValidateUS_RuleCodes();
			AssertHasMessageErrorContaining(Header.US_RuleCodesInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_RuleCodes = "111,222,111";
			Header.AddInfoValidation.ValidateUS_RuleCodes();
			AssertHasMessageErrorContaining(Header.US_RuleCodesInfo, "You have entered duplicate Citation / Exemption Numbers.");

			Header.US_NoLabTestingRequired = false;
			AssertNoMessageErrors(Header.US_RuleCodesInfo);

			Header.US_NoLabTestingRequired = true;
			Header.US_RuleCodes = "111,222";
			Header.AddInfoValidation.ValidateUS_RuleCodes();
			AssertNoMessageErrors(Header.US_RuleCodesInfo);
		}

		CPSCHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.CPSCHeaders.AddNew();
				}
				return header;
			}
		}
		CPSCHeader header;
	}
}
