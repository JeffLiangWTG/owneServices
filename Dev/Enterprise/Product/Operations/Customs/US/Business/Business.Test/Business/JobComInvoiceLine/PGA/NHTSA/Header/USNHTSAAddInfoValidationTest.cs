using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using BoxNumberList = Enterprise.Customs.US.Business.DepartmentOfTransportBoxNumberList;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USNHTSAAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNHTSAValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			Header.US_OA_NHTOwner = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_NHTOwnerInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_NHTOwnerInfo, addressCodeWarning);
			Header.US_OA_NHTOwner = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_NHTOwnerInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_NHTOwnerInfo, addressCodeWarning);

			Header.US_NHTFabricatingMFRAddress = orgAddress1.PK;
			AssertHasWarning(Header.US_NHTFabricatingMFRAddressInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_NHTFabricatingMFRAddressInfo, addressCodeWarning);
			Header.US_NHTFabricatingMFRAddress = orgAddress2.PK;
			AssertNoWarning(Header.US_NHTFabricatingMFRAddressInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_NHTFabricatingMFRAddressInfo, addressCodeWarning);

			Header.US_NHTOriginalMFRAddress = orgAddress1.PK;
			AssertHasWarning(Header.US_NHTOriginalMFRAddressInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_NHTOriginalMFRAddressInfo, addressCodeWarning);
			Header.US_NHTOriginalMFRAddress = orgAddress2.PK;
			AssertNoWarning(Header.US_NHTOriginalMFRAddressInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_NHTOriginalMFRAddressInfo, addressCodeWarning);

			Header.US_OA_NHTRetailer = orgAddress1.PK;
			AssertHasWarning(Header.US_OA_NHTRetailerInfo, addressDescriptionWarning);
			AssertHasWarning(Header.US_OA_NHTRetailerInfo, addressCodeWarning);
			Header.US_OA_NHTRetailer = orgAddress2.PK;
			AssertNoWarning(Header.US_OA_NHTRetailerInfo, addressDescriptionWarning);
			AssertNoWarning(Header.US_OA_NHTRetailerInfo, addressCodeWarning);
		}
		public void TestFWSValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "CA";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			NHTSAValidateState(Header, Header.US_OA_NHTOwnerInfo);
			NHTSAValidateState(Header, Header.US_NHTFabricatingMFRAddressInfo);
			NHTSAValidateState(Header, Header.US_NHTOriginalMFRAddressInfo);
			NHTSAValidateState(Header, Header.US_OA_NHTRetailerInfo);
		}

		void NHTSAValidateState(NHTSAHeader header, ZPropertyInfo propertyInfo)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "CATST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "CATST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");
		}

		public void TestCheckUS_NHTProgramCode()
		{
			Header.US_NHTProgramCode = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_NHTProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTProgramCode = "ABC";
			AssertHasMessageErrorContaining(Header.US_NHTProgramCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OEI;
			Header.US_NHTBoxNumber = BoxNumberList.Codes._2A;
			AssertHasMessageErrorContaining(Header.US_NHTProgramCodeInfo, USNHTSAAddInfoValidation.OnlyREIAndMVSAreAllowedFor2A);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			AssertHasMessageErrorContaining(Header.US_NHTProgramCodeInfo, USNHTSAAddInfoValidation.AtLeastOneDetailsLineRequired);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			AssertHasMessageErrorContaining(Header.US_NHTProgramCodeInfo, USNHTSAAddInfoValidation.AtLeastOneDetailsLineRequired);

			Header.NHTSADetails.AddNew();
			Header.AddInfo.Validation.ValidateUS_NHTProgramCode();
			AssertNoMessageErrors(Header.US_NHTProgramCodeInfo);
		}

		public void TestCheckUS_NHTProgramCodeOnProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var header = pivot.NHTSALines.AddNew();
			header.AddInfo.Validation.ValidateUS_NHTProgramCode();
			AssertNoMessageErrors(header.US_NHTProgramCodeInfo);

			header.US_NHTProgramCode = "~";
			AssertHasMessageErrorContaining(header.US_NHTProgramCodeInfo, ListValidation.InvalidCodeMessageError);

			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			AssertNoMessageErrors(header.US_NHTProgramCodeInfo);
		}

		public void TestCheckUS_NHTElectronicImage()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._2B;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertNoMessageErrors(Header.US_NHTElectronicImageInfo);

			Header.US_NHTElectronicImage = true;
			AssertHasWarningContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsNotRequired);

			Header.US_NHTElectronicImage = false;
			var document = Header.NHTSADocuments.AddNew();
			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._872;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertNoMessageErrors(Header.US_NHTElectronicImageInfo);

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._165;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._06;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertNoMessageErrors(Header.US_NHTElectronicImageInfo);

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._874;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._12;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._09;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertNoMessageErrors(Header.US_NHTElectronicImageInfo);

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._958;
			Header.AddInfo.Validation.ValidateUS_NHTElectronicImage();
			AssertHasMessageErrorContaining(Header.US_NHTElectronicImageInfo, ValidationConstants.NHTSA.ElectronicImageIsRequired);

			Header.US_NHTElectronicImage = true;
			AssertNoMessageErrors(Header.US_NHTElectronicImageInfo);
		}

		public void TestCheckUS_NHTOwner()
		{
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertNoMessageErrors(Header.US_OA_NHTOwnerInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._2B;
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertHasMessageErrorContaining(Header.US_OA_NHTOwnerInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._05;
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertHasMessageErrorContaining(Header.US_OA_NHTOwnerInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.MainAddress.State = "UT";
			Header.US_OA_NHTOwner = orgHeader.MainAddress.PK;
			AssertNoMessageErrors(Header.US_OA_NHTOwnerInfo);

			DeclarationTestHelper.AddPGAContact(Header.OwnerAddress, "AAAA", "BBBB", null, null, null);
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertNoMessageErrors(Header.US_OA_NHTOwnerInfo);

			DeclarationTestHelper.AddPGAContact(Header.OwnerAddress, null, null, "1234567", null, null);
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertNoMessageErrors(Header.US_OA_NHTOwnerInfo);

			DeclarationTestHelper.AddPGAContact(Header.OwnerAddress, null, null, null, null, "23456");
			Header.AddInfo.Validation.ValidateUS_OA_NHTOwner();
			AssertNoMessageErrors(Header.US_OA_NHTOwnerInfo);
		}

		public void TestCheckUS_NHTFabricatingMFRAddress()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			Header.AddInfo.Validation.ValidateUS_NHTFabricatingMFRAddress();
			AssertNoMessageErrors(Header.US_NHTFabricatingMFRAddressInfo);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			Header.AddInfo.Validation.ValidateUS_NHTFabricatingMFRAddress();
			AssertHasMessageErrorContaining(Header.US_NHTFabricatingMFRAddressInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.MainAddress.State = "UT";
			DeclarationTestHelper.AddPGAContact(orgHeader, "TIM", "VAN", "034 34343", "1111111111111111111111111111111111111111111111111111@fda.com", null);

			Header.US_NHTFabricatingMFRAddress = orgHeader.MainAddress.PK;
			AssertNoMessageErrors(Header.US_NHTFabricatingMFRAddressInfo);

			var detailsLine = Header.NHTSADetails.AddNew();
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_REITYPList.Codes.REI1;
			AssertHasMessageErrorContaining(Header.US_NHTFabricatingMFRAddressInfo, string.Format(USNHTSAAddInfoValidation.ManufacturerCodeIsRequired, "Tire", "REI1", "TMC"));

			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_REITYPList.Codes.REI7;
			AssertHasMessageErrorContaining(Header.US_NHTFabricatingMFRAddressInfo, string.Format(USNHTSAAddInfoValidation.ManufacturerCodeIsRequired, "Glazing", "REI7", "GMC"));

			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.GlazingManufacturerCode, "12123", Core.Constants.CountryCodes.UnitedStates);
			Header.AddInfo.Validation.ValidateUS_NHTFabricatingMFRAddress();
			AssertNoMessageErrors(Header.US_NHTFabricatingMFRAddressInfo);
		}

		public void TestCheckUS_OA_NHTRetailer()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			Header.AddInfo.Validation.ValidateUS_OA_NHTRetailer();
			AssertNoMessageErrors(Header.US_OA_NHTRetailerInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._2A;
			Header.AddInfo.Validation.ValidateUS_OA_NHTRetailer();
			AssertHasMessageErrorContaining(Header.US_OA_NHTRetailerInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.MainAddress.State = "UT";
			DeclarationTestHelper.AddPGAContact(orgHeader, "TIM", "VAN", "034 34343", "1111111111111111111111111111111111111111111111111111@fda.com", null);

			Header.US_OA_NHTRetailer = orgHeader.MainAddress.PK;
			AssertNoMessageErrors(Header.US_OA_NHTRetailerInfo);
		}

		public void TestCheckUS_NHTEmbassyNationality()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._06;
			Header.AddInfo.Validation.ValidateUS_NHTEmbassyNationality();
			AssertHasMessageErrorContaining(Header.US_NHTEmbassyNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTEmbassyNationality = "XX";
			AssertHasMessageErrorContaining(Header.US_NHTEmbassyNationalityInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTEmbassyNationality = "US";
			AssertNoMessageErrors(Header.US_NHTEmbassyNationalityInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._01;
			Header.US_NHTEmbassyNationality = "XX";
			Header.AddInfo.Validation.ValidateUS_NHTEmbassyNationality();
			AssertHasWarningContaining(Header.US_NHTEmbassyNationalityInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckUS_NHTBoxNumber()
		{
			Header.AddInfo.Validation.ValidateUS_NHTBoxNumber();
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTBoxNumber = "XX";
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._01;
			AssertNoMessageErrors(Header.US_NHTBoxNumberInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._2B;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 872 - Signed Manufacturer’s Compliance Letter is required when Box 2B is used.");

			var document = Header.NHTSADocuments.AddNew();
			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._872;
			AssertNoMessageErrors(Header.US_NHTBoxNumberInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 165 - NHTSA HS-474 DOT Conformance Bond is required when Box 03 is used.");

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._165;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 165 - NHTSA HS-474 DOT Conformance Bond is required when Box 03 is used.");

			Header.US_NHTBoxNumber = BoxNumberList.Codes._06;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 874 - Official Orders is required when Box 06 is used.");

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._874;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 874 - Official Orders is required when Box 06 is used.");

			Header.US_NHTBoxNumber = BoxNumberList.Codes._09;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 875 - Incomplete Vehicle Document and 958 - Motor Vehicle Equipment Manufacturer’s Written Statement are required when Box 09 is used.");

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._958;
			var document2 = Header.NHTSADocuments.AddNew();
			document2.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 875 - Incomplete Vehicle Document and 958 - Motor Vehicle Equipment Manufacturer’s Written Statement are required when Box 09 is used.");

			Header.US_NHTBoxNumber = BoxNumberList.Codes._12;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 874 - Official Orders is required when Box 12 is used.");

			document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._874;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Document 874 - Official Orders is required when Box 12 is used.");
		}

		public void TestCheckUS_NHTBoxNumberOnProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var header = pivot.NHTSALines.AddNew();
			header.AddInfo.Validation.ValidateUS_NHTBoxNumber();
			AssertNoMessageErrors(header.US_NHTBoxNumberInfo);

			header.US_NHTBoxNumber = "~";
			AssertHasMessageErrorContaining(header.US_NHTBoxNumberInfo, ListValidation.InvalidCodeMessageError);

			header.US_NHTBoxNumber = BoxNumberList.Codes._01;
			AssertNoMessageErrors(header.US_NHTBoxNumberInfo);
		}

		public void TestCheckUS_NHTTravelDocType()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._05;
			Header.AddInfo.Validation.ValidateUS_NHTTravelDocType();
			AssertHasMessageErrorContaining(Header.US_NHTTravelDocTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTTravelDocType = "X";
			AssertHasMessageErrorContaining(Header.US_NHTTravelDocTypeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTTravelDocType = TravelDocumentTypeCodeList.Codes._4;
			AssertNoMessageErrors(Header.US_NHTTravelDocTypeInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._04;
			Header.US_NHTTravelDocType = "X";
			AssertHasWarningContaining(Header.US_NHTTravelDocTypeInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckUS_NHTTravelDocNumber()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._05;
			Header.AddInfo.Validation.ValidateUS_NHTTravelDocNumber();
			AssertHasMessageErrorContaining(Header.US_NHTTravelDocNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTTravelDocNumber = "111111";
			AssertNoMessageErrors(Header.US_NHTTravelDocNumberInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._04;
			Header.US_NHTTravelDocNumber = ZString.Empty;
			AssertNoMessageErrors(Header.US_NHTTravelDocNumberInfo);
		}

		public void TestCheckUS_NHTTravelDocNationality()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._05;
			Header.AddInfo.Validation.ValidateUS_NHTTravelDocNationality();
			AssertHasMessageErrorContaining(Header.US_NHTTravelDocNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTTravelDocNationality = "XX";
			AssertHasMessageErrorContaining(Header.US_NHTTravelDocNationalityInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTTravelDocNationality = "US";
			AssertNoMessageErrors(Header.US_NHTTravelDocNationalityInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._04;
			Header.US_NHTTravelDocNationality = "XX";
			AssertHasWarningContaining(Header.US_NHTTravelDocNationalityInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckUS_NHTDOTSuretyCode()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			Header.AddInfo.Validation.ValidateUS_NHTDOTSuretyCode();
			AssertHasMessageErrorContaining(Header.US_NHTDOTSuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTDOTSuretyCode = "ABC";
			AssertHasMessageErrorContaining(Header.US_NHTDOTSuretyCodeInfo, ValidationConstants.NHTSA.InvalidDOTSuretyCodeFormat);

			Header.US_NHTDOTSuretyCode = "123";
			AssertNoMessageErrors(Header.US_NHTDOTSuretyCodeInfo);
		}

		public void TestCheckUS_NHTDOTBondNumber()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			Header.AddInfo.Validation.ValidateUS_NHTDOTBondNumber();
			AssertHasMessageErrorContaining(Header.US_NHTDOTBondNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTDOTBondNumber = "123445";
			AssertNoMessageErrors(Header.US_NHTDOTBondNumberInfo);
		}

		public void TestCheckUS_NHTDOTBondType()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			Header.AddInfo.Validation.ValidateUS_NHTDOTBondType();
			AssertHasMessageErrorContaining(Header.US_NHTDOTBondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTDOTBondType = "X";
			AssertHasMessageErrorContaining(Header.US_NHTDOTBondTypeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTDOTBondType = DOTBondQualifierList.Codes.Single;
			AssertNoMessageErrors(Header.US_NHTDOTBondTypeInfo);

			Header.US_NHTBoxNumber = BoxNumberList.Codes._04;
			Header.US_NHTDOTBondType = "X";
			AssertHasWarningContaining(Header.US_NHTDOTBondTypeInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckUS_NHTDOTBondAmount()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			Header.AddInfo.Validation.ValidateUS_NHTDOTBondAmount();
			AssertHasMessageErrorContaining(Header.US_NHTDOTBondAmountInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTDOTBondAmount = 123445;
			AssertNoMessageErrors(Header.US_NHTDOTBondAmountInfo);
		}

		public void TestCheckUS_CertifyingIndividual()
		{
			Header.US_CertifyingIndividual = ZString.Empty;
			AssertHasMessageErrorContaining(Header.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_CertifyingIndividual = PartyTypeList.Codes.Shipper;
			AssertHasMessageErrorContaining(Header.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CertifyingIndividualOnProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var header = pivot.NHTSALines.AddNew();
			header.AddInfo.Validation.ValidateUS_CertifyingIndividual();
			AssertNoMessageErrors(header.US_CertifyingIndividualInfo);

			header.US_CertifyingIndividual = "~";
			AssertHasMessageErrorContaining(header.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);

			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertNoMessageErrors(header.US_CertifyingIndividualInfo);
		}

		public void TestCheckUS_PGAContactName()
		{
			Header.AddInfo.Validation.ValidateUS_PGAContactName();
			AssertHasMessageErrorContaining(Header.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_PGAContactName = "TEST NAME";
			AssertNoMessageErrorContaining(Header.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactPhoneNo()
		{
			Header.AddInfo.Validation.ValidateUS_PGAContactPhoneNo();
			AssertHasMessageErrorContaining(Header.US_PGAContactPhoneNoInfo, "You have not entered a value.");
			Header.US_PGAContactPhoneNo = "123";
			AssertNoMessageErrorContaining(Header.US_PGAContactPhoneNoInfo, "You have not entered a value.");
			Header.US_PGAContactPhoneNo = "2345678901";
			AssertNoMessageErrorContaining(Header.US_PGAContactPhoneNoInfo, "You have not entered a value.");
		}

		public void TestCheckUS_PGAContactEmail()
		{
			Header.AddInfo.Validation.ValidateUS_PGAContactEmail();
			AssertHasMessageErrorContaining(Header.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			Header.US_PGAContactEmail = "~";
			AssertNoMessageErrorContaining(Header.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(Header.US_PGAContactEmailInfo, "Invalid email format");
			Header.US_PGAContactEmail = "test.abc@def.com";
			AssertNoWarningContaining(Header.US_PGAContactEmailInfo, "Invalid email format");
		}

		public void TestCheckPermitAndLicenses()
		{
			Header.US_NHTBoxNumber = BoxNumberList.Codes._03;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH0' is required when Box Number is '03'.");
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH3' is required when Box Number is '03'.");

			var details = Header.NHTSADetails.AddNew();
			var lpco0 = details.PermitAndLicenses.AddNew();
			lpco0.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH0' is required when Box Number is '03'.");
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH3' is required when Box Number is '03'.");
			var lpco1 = details.PermitAndLicenses.AddNew();
			lpco1.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH3' is required when Box Number is '03'.");

			details.PermitAndLicenses.RemoveAndDeleteAll();
			Header.US_NHTBoxNumber = BoxNumberList.Codes._07;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '07' and Original Vehicle Manufacturer is not reported.");
			lpco0 = details.PermitAndLicenses.AddNew();
			lpco0.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '07' and Original Vehicle Manufacturer is not reported.");

			details.PermitAndLicenses.RemoveAndDeleteAll();
			Header.US_NHTBoxNumber = BoxNumberList.Codes._10;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '10'.");
			lpco0 = details.PermitAndLicenses.AddNew();
			lpco0.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '10'.");

			details.PermitAndLicenses.RemoveAndDeleteAll();
			Header.US_NHTBoxNumber = BoxNumberList.Codes._13;
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH0' is required when Box Number is '13'.");
			AssertHasMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '13'.");
			lpco0 = details.PermitAndLicenses.AddNew();
			lpco0.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH0' is required when Box Number is '13'.");
			lpco1 = details.PermitAndLicenses.AddNew();
			lpco1.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertNoMessageErrorContaining(Header.US_NHTBoxNumberInfo, "Permit And License with LPCO Type 'NH2' is required when Box Number is '13'.");
		}

		public void TestValidatePGAContact()
		{
			var org1 = Factory.New<OrgHeader>();
			var orgAdd1 = org1.Addresses.AddNew();
			orgAdd1.State = "UT";
			var org2 = Factory.New<OrgHeader>();
			var orgAdd2 = org2.Addresses.AddNew();
			orgAdd2.State = "UT";
			DeclarationTestHelper.AddPGAContact(org2, "TIM", "VAN", "034 34343", "1111111111111111111111111111111111111111111111111111@fda.com", null);
			var org1Wrapper = OrgHeaderWrapper.New(org1);
			var org2Wrapper = OrgHeaderWrapper.New(org2);
			var org3 = Factory.New<OrgHeader>();
			DeclarationTestHelper.AddPGAContact(org3, "TIM", "VAN", "034 34343", "111111111@fda.com", null);

			foreach (var propertyInfo in new[] { Header.US_NHTOriginalMFRAddressInfo, Header.US_NHTFabricatingMFRAddressInfo, Header.US_OA_NHTRetailerInfo })
			{
				propertyInfo.Value = orgAdd1.PK;
				AssertNoMessageErrors(propertyInfo);

				propertyInfo.Value = orgAdd2.PK;
				AssertNoMessageErrors(propertyInfo);
				propertyInfo.Value = ZGuid.Empty;
			}
		}

		#region Implementation

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}

				return header;
			}
		}
		NHTSAHeader header;

		#endregion
	}
}
