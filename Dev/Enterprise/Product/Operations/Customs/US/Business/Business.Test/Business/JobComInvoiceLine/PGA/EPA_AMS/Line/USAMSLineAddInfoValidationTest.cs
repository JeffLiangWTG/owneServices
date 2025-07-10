using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_OA_FinalHandeler()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR1;
			var amsLine = ams.AMSLines.AddNew();

			amsLine.US_OA_FinalHandler = ZGuid.Empty;
			AssertHasMessageError(amsLine.US_OA_FinalHandlerInfo, "You have not entered a value.");

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			amsLine.US_OA_FinalHandler = address.PK;
			AssertNoMessageError(amsLine.US_OA_FinalHandlerInfo, "You have not entered a value.");
			AssertHasMessageError(amsLine.US_OA_FinalHandlerInfo, "Organization must have Registration Code of type AMS on file.");

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "111", Core.Constants.CountryCodes.UnitedStates);
			amsLine.AddInfo.Validation.ValidateUS_OA_FinalHandler();
			AssertNoMessageError(amsLine.US_OA_FinalHandlerInfo, "Organization must have Registration Code of type AMS on file.");
			AssertHasMessageError(amsLine.US_OA_FinalHandlerInfo, "AMS code must be 10 digits.");

			cusCode.OK_CustomsRegNo = "1234567891";
			amsLine.AddInfo.Validation.ValidateUS_OA_FinalHandler();
			AssertNoMessageError(amsLine.US_OA_FinalHandlerInfo, "AMS code must be 10 digits.");
		}

		public void TestCheckUS_OA_CerFinalHandler()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ams = invoiceLine.AMSLines.AddNew();
			ams.US_Program = AMSProgramList.Codes.OR1;
			var amsLine = ams.AMSLines.AddNew();

			amsLine.US_OA_CerFinalHandler = ZGuid.Empty;
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "You have not entered a value.");

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			amsLine.US_OA_CerFinalHandler = address.PK;
			AssertNoMessageError(amsLine.US_OA_CerFinalHandlerInfo, "You have not entered a value.");
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "Organization must have Registration Code of type AMS on file.");

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "1111", Core.Constants.CountryCodes.UnitedStates);
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertNoMessageError(amsLine.US_OA_CerFinalHandlerInfo, "Organization must have Registration Code of type AMS on file.");
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "AMS code must be 3 digits.");

			cusCode.OK_CustomsRegNo = "123";
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertNoMessageError(amsLine.US_OA_CerFinalHandlerInfo, "AMS code must be 3 digits.");
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "Must have valid PGA contact info on file.");

			var contact = org.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "USP";
			contact.OC_ContactName = "Joey";
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "13222222222";
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "";
			contact.OC_Email = "test@wisetechglobal.com";
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertHasMessageError(amsLine.US_OA_CerFinalHandlerInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "13222222222";
			amsLine.AddInfo.Validation.ValidateUS_OA_CerFinalHandler();
			AssertNoMessageError(amsLine.US_OA_CerFinalHandlerInfo, "PGA contact is missing phone number or email address.");
			AssertNoMessageError(amsLine.US_OA_CerFinalHandlerInfo, "Must have valid PGA contact info on file.");
		}

		public void TestAMSLineValidateCharactorsForAddressDescription()
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

			testAMS.US_Program = AMSProgramList.Codes.EG1;
			var eg1Line = testAMS.AMSLines.AddNew();

			eg1Line.US_OA_Applicant = orgAddress1.PK;
			AssertHasWarning(eg1Line.US_OA_ApplicantInfo, addressDescriptionWarning);
			AssertHasWarning(eg1Line.US_OA_ApplicantInfo, addressCodeWarning);
			eg1Line.US_OA_Applicant = orgAddress2.PK;
			AssertNoWarning(eg1Line.US_OA_ApplicantInfo, addressDescriptionWarning);
			AssertNoWarning(eg1Line.US_OA_ApplicantInfo, addressCodeWarning);

			eg1Line.US_OA_GoodsLocation = orgAddress1.PK;
			AssertHasWarning(eg1Line.US_OA_GoodsLocationInfo, addressDescriptionWarning);
			AssertHasWarning(eg1Line.US_OA_GoodsLocationInfo, addressCodeWarning);
			eg1Line.US_OA_GoodsLocation = orgAddress2.PK;
			AssertNoWarning(eg1Line.US_OA_GoodsLocationInfo, addressDescriptionWarning);
			AssertNoWarning(eg1Line.US_OA_GoodsLocationInfo, addressCodeWarning);
		}

		public void TestUS_InspecDateTime()
		{
			testAMS.US_Program = AMSProgramList.Codes.EG1;
			var eg1Line = testAMS.AMSLines.AddNew();
			eg1Line.US_InspecDateTime = ZDateTime.Now;
			AssertNoMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InspecDateTime = ZDateTime.Empty;
			AssertHasMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EntryDate = ZDateTime.Now;
			eg1Line.US_InspecDateTime = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, USAMSLineAddInfoValidation.InspecDateTimeShouldAfterEntryDate);
			AssertHasMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, USAMSLineAddInfoValidation.TimeOfInspectionFormat);

			eg1Line.US_InspecDateTime = ZDateTime.Today.AddDays(-1).AddHours(-1);
			AssertHasMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, USAMSLineAddInfoValidation.InspecDateTimeShouldAfterEntryDate);
			AssertNoMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, USAMSLineAddInfoValidation.TimeOfInspectionFormat);
		}

		public void TestCheckUS_Packages()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_Packages = 100m;
			AssertNoMessageErrorContaining(mo1Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(mo1Line.US_PackagesInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			mo1Line.US_Packages = -100m;
			AssertHasMessageErrorContaining(mo1Line.US_PackagesInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			mo1Line.US_Packages = 0m;
			AssertHasMessageErrorContaining(mo1Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_IsDocSubmitted()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO2;
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = false;
			AssertHasMessageError(mo2Line.US_IsDocSubmittedInfo, USAMSLineAddInfoValidation.ConfirmSubmittedALLDocument);

			mo2Line.US_IsDocSubmitted = true;
			AssertNoMessageError(mo2Line.US_IsDocSubmittedInfo, USAMSLineAddInfoValidation.ConfirmSubmittedALLDocument);
		}

		public void TestUS_OA_Applicant()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";

			var amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_OA_Applicant = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(amsLine.US_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_OA_Applicant = ZGuid.Empty;
			AssertHasMessageErrorContaining(amsLine.US_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABC";
			amsLine.US_OA_Applicant = address.PK;
			amsLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(amsLine.US_OA_ApplicantInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			amsLine.US_OA_Applicant = address.PK;
			amsLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(amsLine.US_OA_ApplicantInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			amsLine.US_OA_Applicant = address.PK;
			amsLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(amsLine.US_OA_ApplicantInfo, "The state is not a valid");
		}

		public void TestUS_OA_GoodsLocation()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "EFG";

			var amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_OA_GoodsLocation = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(amsLine.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_OA_GoodsLocation = ZGuid.Empty;
			AssertHasMessageErrorContaining(amsLine.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABC";
			amsLine.US_OA_GoodsLocation = address.PK;
			amsLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(amsLine.US_OA_GoodsLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			amsLine.US_OA_GoodsLocation = address.PK;
			amsLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(amsLine.US_OA_GoodsLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			amsLine.US_OA_GoodsLocation = address.PK;
			amsLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(amsLine.US_OA_GoodsLocationInfo, "The state is not a valid");
		}

		public void TestUS_NetWeight()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_NetWeight = 100m;
			AssertNoMessageErrorContaining(mo1Line.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_NetWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(mo1Line.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_NetWeight = -2m;
			AssertHasMessageErrorContaining(mo1Line.US_NetWeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo1Line.US_NetWeight = 2m;
			AssertNoMessageErrorContaining(mo1Line.US_NetWeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);
		}

		public void TestUS_NetWeightUQ()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo1Line.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_NetWeightUQ = "";
			AssertHasMessageErrorContaining(mo1Line.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_NetWeightUQ = "~";
			AssertHasMessageErrorContaining(mo1Line.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo1Line.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PackagesUQ()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_PackagesUQ = "AC";
			AssertNoMessageErrorContaining(mo1Line.US_PackagesUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_PackagesUQ = "~";
			AssertHasMessageErrorContaining(mo1Line.US_PackagesUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_Packages = 100m;
			mo1Line.US_PackagesUQ = "";
			AssertHasMessageErrorContaining(mo1Line.US_PackagesUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_PackagesUQ = "AC";
			AssertNoMessageErrorContaining(mo1Line.US_PackagesUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PackageWeightUQ()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_PackageWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo1Line.US_PackageWeightUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_PackageWeightUQ = "~";
			AssertHasMessageErrorContaining(mo1Line.US_PackageWeightUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_PackageWeight = 200m;
			mo1Line.US_PackageWeightUQ = "";
			AssertHasMessageErrorContaining(mo1Line.US_PackageWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_PackageWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo1Line.US_PackageWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_QtyPerPackageUQ()
		{
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			var mo1Line = testAMS.AMSLines.AddNew();
			mo1Line.US_QtyPerPackageUQ = "AC";
			AssertNoMessageErrorContaining(mo1Line.US_QtyPerPackageUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_QtyPerPackageUQ = "~";
			AssertHasMessageErrorContaining(mo1Line.US_QtyPerPackageUQInfo, ListValidation.InvalidCodeMessageError);

			mo1Line.US_QtyPerPackage = 512m;
			mo1Line.US_QtyPerPackageUQ = "";
			AssertHasMessageErrorContaining(mo1Line.US_QtyPerPackageUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_QtyPerPackageUQ = "AC";
			AssertNoMessageErrorContaining(mo1Line.US_QtyPerPackageUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductNumber()
		{
			CreateAMSProductNumber();

			AssertUS_ProductNumber(AMSProgramList.Codes.EG1, "50102501");
			AssertUS_ProductNumber(AMSProgramList.Codes.EG2, "50102501");
			AssertUS_ProductNumber(AMSProgramList.Codes.MO1, "50102505");
			AssertUS_ProductNumber(AMSProgramList.Codes.MO5, "50102505");
			AssertUS_ProductNumber(AMSProgramList.Codes.MO6, "50102504");
			AssertUS_ProductNumber(AMSProgramList.Codes.PN1, "50102502");

			testAMS.US_Program = AMSProgramList.Codes.MO4;
			var amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_ProductNumber = "";
			AssertHasMessageErrorContaining(amsLine.US_ProductNumberInfo, MandatoryValidation.YouHaveNotEntered);
			amsLine.US_ProductNumber = "001forUT";
			AssertNoMessageErrorContaining(amsLine.US_ProductNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(amsLine.US_ProductNumberInfo, "The product number entered is invalid.");
			amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_ProductNumber = "50102501";
			AssertNoMessageErrorContaining(amsLine.US_ProductNumberInfo, "The product number is not supported when the AMS program is");
			testAMS.US_Program = AMSProgramList.Codes.MO1;
			amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_ProductNumber = "50102501";
			AssertHasMessageErrorContaining(amsLine.US_ProductNumberInfo, "The product number is not supported when the AMS program is");
		}

		void AssertUS_ProductNumber(ZString programCode, ZString productNumber)
		{
			testAMS.US_Program = programCode;
			var amsLine = testAMS.AMSLines.AddNew();
			amsLine.US_ProductNumber = "";
			AssertHasMessageErrorContaining(amsLine.US_ProductNumberInfo, MandatoryValidation.YouHaveNotEntered);
			amsLine.US_ProductNumber = "001forUT";
			AssertNoMessageErrorContaining(amsLine.US_ProductNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(amsLine.US_ProductNumberInfo, "The product number entered is invalid.");

			amsLine.US_ProductNumber = "50102503";
			AssertNoMessageError(amsLine.US_ProductNumberInfo, "The product number entered is invalid.");
			AssertHasMessageError(amsLine.US_ProductNumberInfo, "The product number is not supported when the AMS program is '" + programCode + "'.");

			amsLine.US_ProductNumber = productNumber;
			AssertNoMessageError(amsLine.US_ProductNumberInfo, "The product number is not supported when the AMS program is '" + programCode + "'.");
		}

		public void TestUS_LotEntity()
		{
			testAMS.US_Program = AMSProgramList.Codes.OR1;
			var or1Line = testAMS.AMSLines.AddNew();

			or1Line.US_LotEntity = "~";
			or1Line.AddInfo.Validation.ValidateUS_LotEntity();
			AssertHasMessageErrorContaining(or1Line.US_LotEntityInfo, ListValidation.InvalidCodeMessageError);

			or1Line.US_LotEntity = LotNumberQualifierList.Codes._1;
			or1Line.AddInfo.Validation.ValidateUS_LotEntity();
			AssertNoMessageErrorContaining(or1Line.US_LotEntityInfo, ListValidation.InvalidCodeMessageError);
		}

		void CreateAMSProductNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes);

			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102501", new ZString[] { Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.EG1 });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102502", new ZString[] { Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1 });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102503", new ZString[] { Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1 });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102504", new ZString[] { Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6 });
			CreateNewOrGetExistingCusCodeListAndAddAttribute(helper, "50102505", new ZString[] { Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH });
			Factory.Save();
		}

		void CreateNewOrGetExistingCusCodeListAndAddAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString[] attributeValues)
		{
			var refCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsStandardProductAndServiceCodes, code, code, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodePK = refCusCode.PK;
			foreach (var value in attributeValues)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodePK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USDAAMSProgram, value);
			}
		}

		AMS testAMS;
		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			testAMS = invoiceLine.AMSLines.AddNew();
			Factory.Save();
		}
	}
}
