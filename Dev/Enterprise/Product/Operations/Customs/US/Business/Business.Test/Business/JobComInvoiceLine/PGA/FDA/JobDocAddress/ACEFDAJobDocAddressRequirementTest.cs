using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class ACEFDAJobDocAddressRequirementTest : TestCaseWithFactory
	{
		public void TestValidateE2_AddressType()
		{
			docAddress.E2_AddressType = "XXX";
			AssertHasMessageErrorContaining(docAddress.E2_AddressTypeInfo, ListValidation.InvalidCodeMessageError);

			docAddress.E2_AddressType = "";
			AssertHasErrorContaining(docAddress.E2_AddressTypeInfo, MandatoryValidation.MustBeEntered);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertNoErrors(docAddress.E2_AddressTypeInfo);

			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			docAddress.E2_AddressType = DocAddressTypes.Codes.FSVPImporter;
			AssertHasMessageErrorContaining(docAddress.E2_AddressTypeInfo, ListValidation.InvalidCodeMessageError);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertNoMessageErrorContaining(docAddress.E2_AddressTypeInfo, ListValidation.InvalidCodeMessageError);

			var docAddress2 = docAddresses.AddNew();
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertHasErrorContaining(docAddress2.E2_AddressTypeInfo, string.Format(ACEFDAJobDocAddressRequirement.DuplicateAddressTypesErrorMessage, docAddress2.AddressDescription));
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Laboratory;
			AssertNoErrors(docAddress2.E2_AddressTypeInfo);

			docAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
			AssertHasErrorContaining(docAddress2.E2_AddressTypeInfo, ACEFDAJobDocAddressRequirement.MultipleProducerTypesErrorMessage);
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Grower;
			AssertHasErrorContaining(docAddress2.E2_AddressTypeInfo, ACEFDAJobDocAddressRequirement.MultipleProducerTypesErrorMessage);
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Consolidator;
			AssertHasErrorContaining(docAddress2.E2_AddressTypeInfo, ACEFDAJobDocAddressRequirement.MultipleProducerTypesErrorMessage);
			docAddress2.E2_AddressType = DocAddressTypes.Codes.Shipper;
			AssertNoErrors(docAddress2.E2_AddressTypeInfo);
			docAddresses.RemoveAndDelete(docAddress2);
		}

		public void TestValidateE2_CompanyName()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_CompanyName();

			docAddress.E2_CompanyName = "";
			AssertHasErrorContaining(docAddress.E2_CompanyNameInfo, MandatoryValidation.MustBeEntered);

			docAddress.E2_CompanyName = "测试 COMPANY";
			AssertHasWarningContaining(docAddress.E2_CompanyNameInfo, "Shipper: Company Name : US Customs only accepts standard English alphabetic characters");

			docAddress.E2_CompanyName = "COMPANY";
			AssertNoErrorContaining(docAddress.E2_CompanyNameInfo, MandatoryValidation.MustBeEntered);
			AssertNoWarningContaining(docAddress.E2_CompanyNameInfo, "Shipper: Company Name : US Customs only accepts standard English alphabetic characters");
		}

		public void TestValidateE2_Address1()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_Address1();

			docAddress.E2_Address1 = "";
			AssertHasErrorContaining(docAddress.E2_Address1Info, MandatoryValidation.MustBeEntered);

			docAddress.E2_Address1 = "测试 ADDRESS 1";
			AssertHasWarningContaining(docAddress.E2_Address1Info, "Shipper: Address Line 1 : US Customs only accepts standard English alphabetic characters");

			docAddress.E2_Address1 = "ADDRESS 1";
			AssertNoErrorContaining(docAddress.E2_Address1Info, MandatoryValidation.MustBeEntered);
			AssertNoWarningContaining(docAddress.E2_Address1Info, "Shipper: Address Line 1 : US Customs only accepts standard English alphabetic characters");
		}

		public void TestValidateE2_Address2()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_Address2();

			docAddress.E2_Address2 = "";
			AssertNoErrors(docAddress.E2_Address2Info);

			docAddress.E2_Address2 = "测试 ADDRESS 2";
			AssertHasWarningContaining(docAddress.E2_Address2Info, "Shipper: Address Line 2 : US Customs only accepts standard English alphabetic characters");

			docAddress.E2_Address2 = "ADDRESS 2";
			AssertNoWarningContaining(docAddress.E2_Address2Info, "Shipper: Address Line 2 : US Customs only accepts standard English alphabetic characters");
		}

		public void TestValidateE2_RN_NKCoutryCode()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_RN_NKCountryCode();

			docAddress.E2_RN_NKCountryCode = "";
			AssertHasErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, MandatoryValidation.MustBeEntered);

			docAddress.E2_RN_NKCountryCode = "KR";
			AssertNoErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateE2_State()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_State();

			docAddress.E2_RN_NKCountryCode = "CA";
			docAddress.E2_State = "";
			AssertNoErrorContaining(docAddress.E2_StateInfo, MandatoryValidation.MustBeEntered);
			AssertHasMessageErrorContaining(docAddress.E2_StateInfo, "State should not be empty.");
			docAddress.E2_State = "WN";
			AssertNoMessageErrorContaining(docAddress.E2_StateInfo, "State should not be empty.");

			docAddress.E2_RN_NKCountryCode = "US";
			docAddress.E2_State = "";
			AssertHasErrorContaining(docAddress.E2_StateInfo, MandatoryValidation.MustBeEntered);
			AssertHasMessageErrorContaining(docAddress.E2_StateInfo, "State should not be empty.");
			docAddress.E2_State = "MI";
			AssertNoErrorContaining(docAddress.E2_StateInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(docAddress.E2_StateInfo, "State should not be empty.");

			docAddress.E2_RN_NKCountryCode = "CN";
			docAddress.E2_State = "";
			AssertNoMessageErrorContaining(docAddress.E2_StateInfo, "State should not be empty.");
		}

		public void TestValidateE2_City()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_City();

			docAddress.E2_City = "";
			AssertHasErrorContaining(docAddress.E2_CityInfo, MandatoryValidation.MustBeEntered);

			docAddress.E2_City = "测试 CITY";
			AssertHasWarningContaining(docAddress.E2_CityInfo, "Shipper: City : US Customs only accepts standard English alphabetic characters");

			docAddress.E2_City = "CITY";
			AssertNoErrorContaining(docAddress.E2_CityInfo, MandatoryValidation.MustBeEntered);
			AssertNoWarningContaining(docAddress.E2_CityInfo, "Shipper: City : US Customs only accepts standard English alphabetic characters");
		}

		public void TestValidateE2_PostCode()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			docAddress.E2_Postcode = "";
			docAddress.Validation.ValidateE2_Postcode();
			AssertHasMessageErrorContaining(docAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			docAddress.E2_Postcode = "测试 000888";
			AssertHasWarningContaining(docAddress.E2_PostcodeInfo, "Shipper: Postcode : US Customs only accepts standard English alphabetic characters");
			AssertNoMessageErrorContaining(docAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			docAddress.E2_Postcode = "000888";
			AssertNoWarningContaining(docAddress.E2_PostcodeInfo, "Shipper: Postcode : US Customs only accepts standard English alphabetic characters");
			AssertNoMessageErrorContaining(docAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);

			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			docAddress.E2_Postcode = ZString.Empty;
			AssertNoMessageErrorContaining(docAddress.E2_PostcodeInfo, ZipCodeValidation.ZIPCanNotBeEmpty);
		}

		public void TestValidateOrganization()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST";
			org.OH_FullName = "测试 COMPANY";
			org.MainAddress.OA_Address1 = "测试 ADDRESS 1";
			org.MainAddress.OA_Address2 = "测试 ADDRESS 2";
			org.MainAddress.OA_City = "测试 CITY";
			org.MainAddress.OA_PostCode = "测试 000888";

			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.E2_AddressOverride = false;
			docAddress.Validation.ValidateOrganisationPK();
			docAddress.OrganisationPK = org.PK;

			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, GetEnglishCharactersValidationMessage(docAddress.E2_CompanyNameInfo));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, GetEnglishCharactersValidationMessage(docAddress.E2_Address1Info));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, GetEnglishCharactersValidationMessage(docAddress.E2_Address2Info));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, GetEnglishCharactersValidationMessage(docAddress.E2_CityInfo));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, GetEnglishCharactersValidationMessage(docAddress.E2_PostcodeInfo));
		}

		public void TestGetRegistrationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SYDOCESYD";
			orgHeader.OH_FullName = "SYDNEY OCEAN BOND";
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "888888888", Core.Constants.CountryCodes.UnitedStates);
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "999999999", Core.Constants.CountryCodes.UnitedStates);
			docAddress.E2_AddressType = DocAddressTypes.Codes.Shipper;
			docAddress.OrganisationPK = orgHeader.PK;

			AssertNotNull(docAddress.Requirement.GetRegistrationNumberResult);

			fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			AssertEquals(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, docAddress.E2_GovRegNumType);
			AssertEquals("888888888", docAddress.E2_GovRegNum);

			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertEquals(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, docAddress.E2_GovRegNumType);
			AssertEquals("999999999", docAddress.E2_GovRegNum);

			orgHeader.MainAddress.CustomsCodes.DeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals("", docAddress.E2_GovRegNumType);
			AssertEquals("", docAddress.E2_GovRegNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fda = Factory.New<ACEFDA>();

			docAddresses = fda.DocAddresses;
			if (!(docAddresses is ACEFDAJobDocAddressDependentCollection))
			{
				throw new DeveloperNotificationException("ACEFDA.DocAddresses should be of type ACEFDAJobDocAddressDependentCollection.");
			}

			docAddress = docAddresses.AddNew();
			if (!(docAddress.Requirement is ACEFDAJobDocAddressRequirement))
			{
				throw new DeveloperNotificationException("ACEFDAJobDocAddress should have a requirement of type ACEFDAJobDocAddressRequirement.");
			}
		}

		ACEFDA fda;
		ACEFDAJobDocAddress docAddress;
		ACEFDAJobDocAddressDependentCollection docAddresses;
		string GetEnglishCharactersValidationMessage(ZPropertyInfo info) => EnglishAddressCharactersValidation.GetNotificationMessage(info);
	}
}
