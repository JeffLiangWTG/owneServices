using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class OrgAddressMessageDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUS_IndividualName()
		{
			var organization = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organization);
			var messageData = new OrgAddressMessageData(wrapper);
			var errorMsg = "Format of name should be Last Name(max 40 characters), First Name(max 40 characters), Middle Initial(optional, max 18 characters)";

			messageData.US_IndividualName = ZString.Empty;
			AssertHasMessageErrorContaining(messageData.US_IndividualNameInfo, MandatoryValidation.YouHaveNotEntered);
			messageData.US_IndividualName = "Donald";
			AssertNoMessageErrorContaining(messageData.US_IndividualNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);

			messageData.US_IndividualName = "Donald Trump";
			AssertHasMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);

			messageData.US_IndividualName = "Trump, Donald";
			AssertNoMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);

			messageData.US_IndividualName = "Trump, Donald, J";
			AssertNoMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);

			messageData.US_IndividualName = "Trump, Donald, J,";
			AssertHasMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);

			messageData.US_IndividualName = "Trump, Donald, Jaaaaaaaaabbbbbbbbbb";
			AssertHasMessageErrorContaining(messageData.US_IndividualNameInfo, errorMsg);
		}

		public void TestCheckUS_NameQualifier()
		{
			AddressData.US_NameQualifier = "";
			AssertNoMessageErrors(AddressData.US_NameQualifierInfo);

			AddressData.US_NameQualifier = "~";
			AssertHasMessageErrorContaining(AddressData.US_NameQualifierInfo, OrgAddressMessageDataValidation.InvalidNameQualifier);

			AddressData.US_NameQualifier = "";
			AddressData.US_AlternativeImporterName = "AAAAAA";
			AssertHasMessageErrorContaining(AddressData.US_NameQualifierInfo, OrgAddressMessageDataValidation.NameQualifierShouldBeEnteredForAlternativeName);

			AddressData.US_NameQualifier = ImporterADDNameQualifierList.Codes.BusinessUnderAnotherName;
			AssertNoMessageErrors(AddressData.US_NameQualifierInfo);
		}

		public void TestCheckUS_AlternativeNameTriggersValidateUS_NameQualifier()
		{
			AddressData.US_AlternativeImporterName = "";
			AssertNoMessageErrors(AddressData.US_AlternativeImporterNameInfo);

			AddressData.US_NameQualifier = ImporterADDNameQualifierList.Codes.A_Division;
			AssertHasMessageErrorContaining(AddressData.US_AlternativeImporterNameInfo, OrgAddressMessageDataValidation.AlternativeNameShouldBeEnteredForAType);

			AddressData.US_AlternativeImporterName = "AAAA";
			AssertNoMessageErrors(AddressData.US_AlternativeImporterNameInfo);
		}

		public void TestCheckUS_ActionCode()
		{
			AddressData.US_ActionCode = "";
			AssertHasMessageErrorContaining(AddressData.US_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_ActionCode = "~";
			AssertNoMessageErrorContaining(AddressData.US_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(AddressData.US_ActionCodeInfo, OrgAddressMessageDataValidation.InvalidActionCode);

			AddressData.US_ActionCode = ImporterADDActionCodeList.Codes.AddImporterNumber;
			AssertNoMessageErrorContaining(AddressData.US_ActionCodeInfo, OrgAddressMessageDataValidation.InvalidActionCode);
		}

		public void TestCheckUS_ImporterNumber()
		{
			AddressData.US_ImporterNumber = "";
			AssertNoMessageErrors(AddressData.US_ImporterNumberInfo);
			AddressData.US_ActionCode = ImporterADDActionCodeList.Codes.ChangeImporter;
			AddressData.Validation.ValidateUS_ImporterName();

			AddressData.US_ImporterNumber = "12345";
			AssertHasMessageErrorContaining(AddressData.US_ImporterNumberInfo, OrgAddressMessageDataValidation.ImporterNumberNotInRightFormatForNew5106);
			AddressData.US_ImporterNumber = "12-1234568";
			AssertNoMessageErrorContaining(AddressData.US_ImporterNumberInfo, OrgAddressMessageDataValidation.ImporterNumberNotInRightFormatForNew5106);

			AddressData.US_ActionCode = ImporterADDActionCodeList.Codes.RequestCBPNumber;
			AddressData.US_ImporterNumber = "12-1234568";
			AssertHasMessageErrorContaining(AddressData.US_ImporterNumberInfo, OrgAddressMessageDataValidation.ImporterNumberNotRequiredForCBPNumberRequest);
			AddressData.US_ImporterNumber = "";
			AssertNoMessageErrorContaining(AddressData.US_ImporterNumberInfo, OrgAddressMessageDataValidation.ImporterNumberNotRequiredForCBPNumberRequest);
		}

		public void TestCheckUS_ImporterType()
		{
			AddressData.US_RN_NKCountry1 = "US";
			AssertHasMessageError(AddressData.US_ImporterTypeInfo, OrgAddressMessageDataValidation.ImporterTypeMandatory);

			AddressData.US_ImporterType = "~";
			AssertNoMessageError(AddressData.US_ImporterTypeInfo, OrgAddressMessageDataValidation.ImporterTypeMandatory);
			AssertHasMessageErrorContaining(AddressData.US_ImporterTypeInfo, OrgAddressMessageDataValidation.InvalidImporterType);

			AddressData.US_ImporterType = ImporterTypeList.Codes.Corporation;
			AssertNoMessageErrorContaining(AddressData.US_ImporterTypeInfo, OrgAddressMessageDataValidation.InvalidImporterType);
		}

		public void TestUS_ImporterName()
		{
			AddressData.US_ImporterName = "";
			AssertHasMessageErrorContaining(AddressData.US_ImporterNameInfo, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_ImporterName = "ABC Importer";
			AssertNoMessageErrorContaining(AddressData.US_ImporterNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OA_Address1()
		{
			AddressData.US_OA_Address1 = ZGuid.Empty;
			AssertHasMessageErrorContaining(AddressData.US_OA_Address1Info, MandatoryValidation.YouHaveNotEntered);

			var address = Organisation.Addresses.AddNew();
			AddressData.US_OA_Address1 = address.PK;
			AssertNoMessageErrorContaining(AddressData.US_RN_NKCountry1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OA_Address2()
		{
			AddressData.US_OA_Address2 = ZGuid.Empty;
			AssertNoMessageErrors(AddressData.US_OA_Address2Info);

			AddressData.US_OA_Address1 = Organisation.MainAddress.PK;
			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AssertHasMessageError(AddressData.US_OA_Address2Info, OrgAddressMessageDataValidation.PhysicalLocationAddressSameAsMailingAddress);

			var address = Organisation.Addresses.AddNew();
			AddressData.US_OA_Address2 = address.PK;
			AssertNoMessageError(AddressData.US_OA_Address2Info, OrgAddressMessageDataValidation.PhysicalLocationAddressSameAsMailingAddress);
		}

		public void TestCheckUS_RN_NKCountry1()
		{
			AddressData.US_RN_NKCountry1 = ZString.Empty;
			AssertHasMessageErrorContaining(AddressData.US_RN_NKCountry1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_RN_NKCountry1 = "US";
			AssertNoMessageErrorContaining(AddressData.US_RN_NKCountry1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CityAddress1()
		{
			AddressData.US_OA_Address1 = Organisation.MainAddress.PK;
			AssertHasMessageErrorContaining(AddressData.US_CityAddress1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_CityAddress1 = "Chicago";
			AssertNoMessageErrorContaining(AddressData.US_CityAddress1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CityAddress2()
		{
			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_CityAddress2 = "";
			AssertHasMessageErrorContaining(AddressData.US_CityAddress2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_CityAddress2 = "Chicago";
			AssertNoMessageErrorContaining(AddressData.US_CityAddress2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_StateAddress1()
		{
			AddressData.US_RN_NKCountry1 = "MX";
			AddressData.US_StateAddress1 = "";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_StateAddress1 = "~~";
			AssertNoMessageErrorContaining(AddressData.US_StateAddress1Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(AddressData.US_StateAddress1Info, OrgAddressMessageDataValidation.StateForImportersOtherThanUS_CA);

			AddressData.US_RN_NKCountry1 = "CN";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress1Info, OrgAddressMessageDataValidation.StateForImportersOtherThanUS_CA);
		}

		public void TestCheckUS_StateAddress2()
		{
			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_RN_NKCountry2 = "US";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_StateAddress2 = "~~";
			AssertNoMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, "This is an invalid US state.");

			AddressData.US_StateAddress2 = USStatesList.Codes.Alabama;
			AssertNoMessageErrorContaining(AddressData.US_StateAddress2Info, "This is an invalid US state.");

			AddressData.US_RN_NKCountry2 = "KR";
			AddressData.US_StateAddress2 = "AA";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, OrgAddressMessageDataValidation.StateForImportersOtherThanUS_CA);

			AddressData.US_StateAddress2 = "";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_StateAddress2 = OrgAddressMessageData.ForeignBasedImporterStateCode;
			AssertNoMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_RN_NKCountry2 = "MX";
			AddressData.US_StateAddress2 = "FN";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, "This is an invalid MX state.");

			AddressData.US_StateAddress2 = "";
			AssertHasMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_StateAddress2 = MexicoStateList.Codes.Aguascalientes;
			AssertNoMessageErrorContaining(AddressData.US_StateAddress2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(AddressData.US_StateAddress2Info, OrgAddressMessageDataValidation.StateForImportersOtherThanUS_CA);
		}

		public void TestCheckZipCode()
		{
			AddressData.US_RN_NKCountry1 = "AU";
			AddressData.US_ZipAddress1 = "";
			AssertNoMessageErrorContaining((ZPropertyInfoString)AddressData.US_ZipAddress1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_RN_NKCountry1 = "CA";
			AddressData.US_ZipAddress1 = "";
			AssertHasMessageErrorContaining((ZPropertyInfoString)AddressData.US_ZipAddress1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_RN_NKCountry1 = "FR";
			AddressData.US_ZipAddress1 = "";
			AssertNoMessageErrorContaining((ZPropertyInfoString)AddressData.US_ZipAddress1Info, MandatoryValidation.YouHaveNotEntered);
			AddressData.US_ZipAddress1 = "1234";
			AssertHasMessageErrorContaining((ZPropertyInfoString)AddressData.US_ZipAddress1Info, string.Format(ZipCodeValidation.InvalidPostCodeLength, 5));
		}
		public void TestCheckUS_LineTwoAddress()
		{
			AddressData.US_RN_NKCountry1 = "CA";
			AddressData.US_RN_NKCountry2 = "CA";

			AddressData.US_LineTwoAddress1 = "uioweruo";
			AssertHasMessageErrorContaining(AddressData.US_LineTwoAddress1Info, OrgAddressMessageDataValidation.LineTwoAddressEnteredForUSBasedImporter);
			AddressData.US_LineTwoAddress2 = "789435798";
			AssertHasMessageErrorContaining(AddressData.US_LineTwoAddress2Info, OrgAddressMessageDataValidation.LineTwoAddressEnteredForUSBasedImporter);

			AddressData.US_RN_NKCountry1 = "US";
			AddressData.US_RN_NKCountry2 = "US";
			AssertNoMessageErrorContaining(AddressData.US_LineTwoAddress1Info, OrgAddressMessageDataValidation.LineTwoAddressEnteredForUSBasedImporter);
			AssertNoMessageErrorContaining(AddressData.US_LineTwoAddress2Info, OrgAddressMessageDataValidation.LineTwoAddressEnteredForUSBasedImporter);
		}

		public void TestValidityOfUSZipCodes()
		{
			AddressData.US_RN_NKCountry1 = "US";

			USCZipCode zipCodeForXX = Factory.New<USCZipCode>();
			zipCodeForXX.UZ_State = "XX";
			zipCodeForXX.UZ_BeginZipCodeRange = "00100";
			zipCodeForXX.UZ_EndZipCodeRange = "00299";

			AssertForUSZipCode((ZPropertyInfoString)AddressData.US_ZipAddress1Info, (ZPropertyInfoString)AddressData.US_StateAddress1Info);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_RN_NKCountry2 = "US";
			AssertForUSZipCode((ZPropertyInfoString)AddressData.US_ZipAddress2Info, (ZPropertyInfoString)AddressData.US_StateAddress2Info);
		}

		void AssertForUSZipCode(ZPropertyInfoString zipCodeInfo, ZPropertyInfoString stateInfo)
		{
			stateInfo.Value = "XX";
			zipCodeInfo.Value = "00489";
			AssertHasMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "00100", "00299"));

			zipCodeInfo.Value = "00210";
			AssertNoMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "00100", "00299"));

			zipCodeInfo.Value = "123";
			AssertHasMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeDigitsForUS);

			zipCodeInfo.Value = "123456789";
			AssertNoMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeDigitsForUS);
		}

		public void TestValidityOfUSZipCodesWhenMultipleRanges()
		{
			AddressData.US_RN_NKCountry1 = "US";

			USCZipCode zipCodeForXXRange1 = Factory.New<USCZipCode>();
			zipCodeForXXRange1.UZ_State = "XX";
			zipCodeForXXRange1.UZ_BeginZipCodeRange = "22000";
			zipCodeForXXRange1.UZ_EndZipCodeRange = "24699";

			USCZipCode zipCodeForXXRange2 = Factory.New<USCZipCode>();
			zipCodeForXXRange2.UZ_State = "XX";
			zipCodeForXXRange2.UZ_BeginZipCodeRange = "20100";
			zipCodeForXXRange2.UZ_EndZipCodeRange = "20199";

			AssertForMultipleUSZipCode((ZPropertyInfoString)AddressData.US_ZipAddress1Info, (ZPropertyInfoString)AddressData.US_StateAddress1Info);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_RN_NKCountry2 = "US";
			AssertForMultipleUSZipCode((ZPropertyInfoString)AddressData.US_ZipAddress2Info, (ZPropertyInfoString)AddressData.US_StateAddress2Info);
		}

		void AssertForMultipleUSZipCode(ZPropertyInfoString zipCodeInfo, ZPropertyInfoString stateInfo)
		{
			stateInfo.Value = "XX";
			zipCodeInfo.Value = "00489";
			AssertHasMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "22000", "24699"));

			zipCodeInfo.Value = "22410";
			AssertNoMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "22000", "24699"));

			zipCodeInfo.Value = "20195";
			AssertNoMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "22000", "24699"));

			zipCodeInfo.Value = "";
			AssertNoMessageErrorContaining(zipCodeInfo, ZString.Format(ZipCodeValidation.InvalidUSZIPCodeEntered, "XX", "22000", "24699"));
		}

		public void TestValidateMXPostCodes()
		{
			AddressData.US_RN_NKCountry1 = "MX";

			AssertForMXPostCode((ZPropertyInfoString)AddressData.US_ZipAddress1Info);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_RN_NKCountry2 = "MX";
			AssertForMXPostCode((ZPropertyInfoString)AddressData.US_ZipAddress2Info);
		}

		void AssertForMXPostCode(ZPropertyInfoString zipCodeInfo)
		{
			zipCodeInfo.Value = "2222";
			AssertHasMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForMX);

			zipCodeInfo.Value = "2222A";
			AssertHasMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForMX);

			zipCodeInfo.Value = "22221";
			AssertNoMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForMX);
		}

		public void TestValidateStatesFor5106()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			addressData.US_RN_NKCountry1 = "US";

			addressData.US_StateAddress1 = "XX";
			AssertHasMessageErrorContaining(addressData.US_StateAddress1Info, "This is an invalid US state.");

			addressData.US_StateAddress1 = "DC";
			AssertNoMessageErrorContaining(addressData.US_StateAddress1Info, "This is an invalid US state.");
		}

		public void TestValidateCAPostCodes()
		{
			AddressData.US_RN_NKCountry1 = "CA";

			AssertForCAPostCode((ZPropertyInfoString)AddressData.US_ZipAddress1Info);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.US_RN_NKCountry2 = "CA";
			AssertForCAPostCode((ZPropertyInfoString)AddressData.US_ZipAddress2Info);
		}

		public void TestCheckUS_NumberOfEntries()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_NumberOfEntries = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_NumberOfEntriesInfo);

			AddressData.US_NumberOfEntries = "~";
			AssertHasMessageErrorContaining(AddressData.US_NumberOfEntriesInfo, ListValidation.InvalidCodeMessageError);

			AddressData.US_NumberOfEntries = NumberOfEntriesPlanningList.Codes._01;
			AssertNoMessageErrors(AddressData.US_NumberOfEntriesInfo);
		}

		public void TestCheckUS_ProgramCode1()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ProgramCode1 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_ProgramCode1Info);

			AddressData.US_ProgramCode1 = "~";
			AssertHasMessageErrorContaining(AddressData.US_ProgramCode1Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_ProgramCode1 = ImporterProgramCodeList.Codes.CTPAT;
			AssertNoMessageErrors(AddressData.US_ProgramCode1Info);
		}

		public void TestCheckUS_ProgramCode2()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ProgramCode2 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_ProgramCode2Info);

			AddressData.US_ProgramCode2 = "~";
			AssertHasMessageErrorContaining(AddressData.US_ProgramCode2Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_ProgramCode2 = ImporterProgramCodeList.Codes.CTPAT;
			AssertNoMessageErrors(AddressData.US_ProgramCode2Info);
		}

		public void TestCheckUS_ProgramCode3()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ProgramCode3 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_ProgramCode3Info);

			AddressData.US_ProgramCode3 = "~";
			AssertHasMessageErrorContaining(AddressData.US_ProgramCode3Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_ProgramCode3 = ImporterProgramCodeList.Codes.CTPAT;
			AssertNoMessageErrors(AddressData.US_ProgramCode3Info);
		}

		public void TestCheckUS_ProgramCode4()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ProgramCode4 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_ProgramCode4Info);

			AddressData.US_ProgramCode4 = "~";
			AssertHasMessageErrorContaining(AddressData.US_ProgramCode4Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_ProgramCode4 = ImporterProgramCodeList.Codes.CTPAT;
			AssertNoMessageErrors(AddressData.US_ProgramCode4Info);
		}

		public void TestCheckUS_UtlOtherDescription()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_UtlOtherDescription = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_UtlOtherDescriptionInfo);

			AddressData.US_UtlOtherIndicator = true;
			AssertHasMessageErrorContaining(AddressData.US_UtlOtherDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_UtlOtherDescription = "A";
			AssertNoMessageErrors(AddressData.US_UtlOtherDescriptionInfo);
		}

		public void TestCheckUS_AddressType1()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_AddressType1 = ZString.Empty;
			AssertHasMessageErrorContaining(AddressData.US_AddressType1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_AddressType1 = "~";
			AssertNoMessageErrorContaining(AddressData.US_AddressType1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(AddressData.US_AddressType1Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._01;
			AssertNoMessageErrors(AddressData.US_AddressType1Info);
		}

		public void TestCheckUS_AddressExplanation1()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._01;
			AddressData.US_AddressExplanation1 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_AddressExplanation1Info);

			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._08;
			AssertHasMessageErrorContaining(AddressData.US_AddressExplanation1Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_AddressExplanation1 = "A";
			AssertNoMessageErrors(AddressData.US_AddressExplanation1Info);
		}

		public void TestCheckUS_AddressType2()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_AddressType2 = ZString.Empty;
			AssertNoMessageErrorContaining(AddressData.US_AddressType2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AddressData.Validation.ValidateUS_AddressType2();
			AssertHasMessageErrorContaining(AddressData.US_AddressType2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_AddressType2 = "~";
			AssertNoMessageErrorContaining(AddressData.US_AddressType2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(AddressData.US_AddressType2Info, ListValidation.InvalidCodeMessageError);

			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._01;
			AssertNoMessageErrors(AddressData.US_AddressType2Info);
		}

		public void TestCheckUS_AddressExplanation2()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._01;
			AddressData.US_AddressExplanation2 = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_AddressExplanation2Info);

			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._08;
			AssertHasMessageErrorContaining(AddressData.US_AddressExplanation2Info, MandatoryValidation.YouHaveNotEntered);

			AddressData.US_AddressExplanation2 = "A";
			AssertNoMessageErrors(AddressData.US_AddressExplanation2Info);
		}

		public void TestCheckUS_ImporterPhoneNumber()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ImporterPhoneNumber = ZString.Empty;
			AssertHasMessageError(AddressData.US_ImporterPhoneNumberInfo, OrgAddressMessageDataValidation.ImporterPhoneNumberRequired);

			AddressData.US_ImporterPhoneNumber = "+123456";
			AssertNoMessageError(AddressData.US_ImporterPhoneNumberInfo, OrgAddressMessageDataValidation.ImporterPhoneNumberRequired);
			AssertHasMessageError(AddressData.US_ImporterPhoneNumberInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);

			AddressData.US_ImporterPhoneNumber = "(0)123456";
			AssertHasMessageError(AddressData.US_ImporterPhoneNumberInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);

			AddressData.US_ImporterPhoneNumber = "123456";
			AssertNoMessageError(AddressData.US_ImporterPhoneNumberInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
		}

		public void TestCheckUS_ImporterEmail()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ImporterEmail = ZString.Empty;
			AssertHasMessageError(AddressData.US_ImporterEmailInfo, OrgAddressMessageDataValidation.ImporterEmailRequired);

			AddressData.US_ImporterEmail = "aaa";
			AssertNoMessageError(AddressData.US_ImporterEmailInfo, OrgAddressMessageDataValidation.ImporterEmailRequired);
			AssertHasWarningContaining(AddressData.US_ImporterEmailInfo, OrgAddressMessageDataValidation.ImporterEmailInvalidFormat);

			AddressData.US_ImporterEmail = "TEST@ABC.COM";
			AssertNoMessageError(AddressData.US_ImporterEmailInfo, OrgAddressMessageDataValidation.ImporterEmailRequired);
			AssertNoWarningContaining(AddressData.US_ImporterEmailInfo, OrgAddressMessageDataValidation.ImporterEmailInvalidFormat);
		}

		public void TestCheckUS_ImporterFaxNumber()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ImporterFaxNumber = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_ImporterFaxNumberInfo);

			AddressData.US_ImporterFaxNumber = "+123456";
			AssertHasMessageError(AddressData.US_ImporterFaxNumberInfo, OrgAddressMessageDataValidation.InvalidFaxNumberFormat);

			AddressData.US_ImporterFaxNumber = "(0)12345";
			AssertHasMessageError(AddressData.US_ImporterFaxNumberInfo, OrgAddressMessageDataValidation.InvalidFaxNumberFormat);

			AddressData.US_ImporterFaxNumber = "12345";
			AssertNoMessageError(AddressData.US_ImporterFaxNumberInfo, OrgAddressMessageDataValidation.InvalidFaxNumberFormat);
		}

		public void TestCheckUS_ImporterWebsite()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_ImporterWebsite = ZString.Empty;
			AssertNoWarningContaining(AddressData.US_ImporterWebsiteInfo, OrgAddressMessageDataValidation.WebsiteURLInvalidFormat);

			AddressData.US_ImporterWebsite = "A";
			AssertHasWarningContaining(AddressData.US_ImporterWebsiteInfo, OrgAddressMessageDataValidation.WebsiteURLInvalidFormat);

			AddressData.US_ImporterWebsite = "www.cargowise.com";
			AssertNoWarningContaining(AddressData.US_ImporterWebsiteInfo, OrgAddressMessageDataValidation.WebsiteURLInvalidFormat);

			AddressData.US_ImporterWebsite = "http://www.wisetechglobal.com";
			AssertNoWarningContaining(AddressData.US_ImporterWebsiteInfo, OrgAddressMessageDataValidation.WebsiteURLInvalidFormat);
		}

		public void TestCheckUS_NAICSCode()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_NAICSCode = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_NAICSCodeInfo);

			AddressData.US_NAICSCode = "A1";
			AssertHasMessageErrorContaining(AddressData.US_NAICSCodeInfo, OrgAddressMessageDataValidation.NAICSCodeInvalidFormat);

			AddressData.US_NAICSCode = "A23456";
			AssertHasMessageErrorContaining(AddressData.US_NAICSCodeInfo, OrgAddressMessageDataValidation.NAICSCodeInvalidFormat);

			AddressData.US_NAICSCode = "123456";
			AssertNoMessageErrorContaining(AddressData.US_NAICSCodeInfo, OrgAddressMessageDataValidation.NAICSCodeInvalidFormat);
		}

		public void TestCheckUS_DUNS()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_DUNS = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_DUNSInfo);

			AddressData.US_DUNS = "1";
			AssertHasMessageErrorContaining(AddressData.US_DUNSInfo, OrgAddressMessageDataValidation.DUNSInvalidFormat);

			AddressData.US_DUNS = "ABCDEFGH1";
			AssertHasMessageErrorContaining(AddressData.US_DUNSInfo, OrgAddressMessageDataValidation.DUNSInvalidFormat);

			AddressData.US_DUNS = "123456789";
			AssertNoMessageErrorContaining(AddressData.US_DUNSInfo, OrgAddressMessageDataValidation.DUNSInvalidFormat);
		}

		public void TestCheckUS_YearEstablished()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_YearEstablished = ZString.Empty;
			AssertNoMessageErrors(AddressData.US_YearEstablishedInfo);

			AddressData.US_YearEstablished = "1";
			AssertHasMessageErrorContaining(AddressData.US_YearEstablishedInfo, OrgAddressMessageDataValidation.YearInvalidFormat);

			AddressData.US_YearEstablished = "123A";
			AssertHasMessageErrorContaining(AddressData.US_YearEstablishedInfo, OrgAddressMessageDataValidation.YearInvalidFormat);

			AddressData.US_YearEstablished = "2019";
			AssertNoMessageErrorContaining(AddressData.US_YearEstablishedInfo, OrgAddressMessageDataValidation.YearInvalidFormat);
		}

		public void TestCheckUS_CountryISOCode()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_CountryISOCode = "";
			AssertNoMessageErrors(AddressData.US_CountryISOCodeInfo);

			AddressData.US_CountryISOCode = "~";
			AssertHasMessageErrorContaining(AddressData.US_CountryISOCodeInfo, ListValidation.InvalidCodeMessageError);

			AddressData.US_CountryISOCode = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageErrorContaining(AddressData.US_CountryISOCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_AcknowledgeAndSign()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			AddressData.US_AcknowledgeAndSign = false;
			AssertHasMessageErrorContaining(AddressData.US_AcknowledgeAndSignInfo, OrgAddressMessageDataValidation.ShouldBeSigned);

			AddressData.US_AcknowledgeAndSign = true;
			AssertNoMessageErrorContaining(AddressData.US_AcknowledgeAndSignInfo, OrgAddressMessageDataValidation.ShouldBeSigned);
		}

		public void TestCheckUS_IndividualPhone()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			addressData.US_IndividualPhone = ZString.Empty;
			AssertNoMessageError(addressData.US_IndividualPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_IndividualPhone = "+12345";
			AssertHasMessageError(addressData.US_IndividualPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_IndividualPhone = "A12345";
			AssertHasMessageError(addressData.US_IndividualPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_IndividualPhone = "12345";
			AssertNoMessageError(addressData.US_IndividualPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
		}

		public void TestCheckUS_BrokerPhone()
		{
			addressData = new OrgAddressMessageData(Wrapper);
			addressData.US_BrokerPhone = ZString.Empty;
			AssertNoMessageError(addressData.US_BrokerPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_BrokerPhone = "+12345";
			AssertHasMessageError(addressData.US_BrokerPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_BrokerPhone = "A12345";
			AssertHasMessageError(addressData.US_BrokerPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
			addressData.US_BrokerPhone = "12345";
			AssertNoMessageError(addressData.US_BrokerPhoneInfo, OrgAddressMessageDataValidation.InvalidPhoneNumberFormat);
		}

		void AssertForCAPostCode(ZPropertyInfoString zipCodeInfo)
		{
			zipCodeInfo.Value = "2222";
			AssertHasMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForCA);

			zipCodeInfo.Value = "A2B12AA";
			AssertHasMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForCA);

			zipCodeInfo.Value = "A2B 2A2";
			AssertNoMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForCA);

			zipCodeInfo.Value = "A2B2A2";
			AssertNoMessageErrorContaining(zipCodeInfo, ZipCodeValidation.InvalidPostCodeForCA);
		}

		OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;

		OrgHeaderWrapper Wrapper
		{
			get { return wrapper ?? (wrapper = OrgHeaderWrapper.New(Organisation)); }
		}
		OrgHeaderWrapper wrapper;

		OrgAddressMessageData AddressData
		{
			get { return addressData ?? (addressData = new OrgAddressMessageData(Wrapper)); }
		}
		OrgAddressMessageData addressData;
	}
}
