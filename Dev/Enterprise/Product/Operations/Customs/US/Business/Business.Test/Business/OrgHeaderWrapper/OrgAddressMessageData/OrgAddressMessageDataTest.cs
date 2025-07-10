using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgAddressMessageData))]
	internal class OrgAddressMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasPermissionToSendImporterBondNumber()
		{
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			var addressData1 = new OrgAddressMessageData(Wrapper);
			Assert(addressData1.HasPermissionToSendImporterBondNumber);
			addressData1.US_ImporterNumber = "123-12-1234";
			Assert(addressData1.HasPermissionToSendImporterBondNumber);
			addressData1.US_ImporterNumber = ZString.Empty;
			var relatedBusiness1 = addressData1.RelatedBusinessItems.AddNew();
			relatedBusiness1.US_Number = "123-12-1234";
			Assert(addressData1.HasPermissionToSendImporterBondNumber);
			relatedBusiness1.US_Number = ZString.Empty;
			var personIdentify1 = addressData1.PIIs.AddNew();
			personIdentify1.US_SSN = "123-12-1234";
			Assert(addressData1.HasPermissionToSendImporterBondNumber);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			var addressData2 = new OrgAddressMessageData(Wrapper);
			Assert(addressData2.HasPermissionToSendImporterBondNumber);
			addressData2.US_ImporterNumber = "123-12-1234";
			Assert(!addressData2.HasPermissionToSendImporterBondNumber);
			addressData2.US_ImporterNumber = ZString.Empty;
			var relatedBusiness2 = addressData2.RelatedBusinessItems.AddNew();
			relatedBusiness2.US_Number = "123-12-1234";
			Assert(!addressData2.HasPermissionToSendImporterBondNumber);
			relatedBusiness2.US_Number = ZString.Empty;
			var personIdentify2 = addressData2.PIIs.AddNew();
			personIdentify2.US_SSN = "123-12-1234";
			Assert(!addressData2.HasPermissionToSendImporterBondNumber);
		}

		public void TestNotDefaultPostCodeWhenNonNAFTACountry()
		{
			var usAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(Enterprise.ZArchitecture.Schema.OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, "US"));
			usAddress.OA_PostCode = "US1";
			var mxAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(Enterprise.ZArchitecture.Schema.OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, "MX"));
			mxAddress.OA_PostCode = "MX1";
			var caAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(Enterprise.ZArchitecture.Schema.OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, "CA"));
			caAddress.OA_PostCode = "CA1";
			var auAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(Enterprise.ZArchitecture.Schema.OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, "AU"));
			auAddress.OA_PostCode = "AU1";

			AddressData.US_OA_Address1 = usAddress.PK;
			AssertEquals("Default Post code for US", usAddress.OA_PostCode, AddressData.US_ZipAddress1);

			AddressData.US_OA_Address1 = mxAddress.PK;
			AssertEquals("Default Post code for Mexico", ZString.Empty, AddressData.US_ZipAddress1);

			AddressData.US_OA_Address1 = caAddress.PK;
			AssertEquals("Default Post code for Canada", caAddress.OA_PostCode, AddressData.US_ZipAddress1);

			AddressData.US_OA_Address1 = auAddress.PK;
			AssertEquals("Not Default Post code for non-NAFTA", ZString.Empty, AddressData.US_ZipAddress1);

			AddressData.US_OA_Address2 = usAddress.PK;
			AssertEquals("Default Post code for US", usAddress.OA_PostCode, AddressData.US_ZipAddress2);

			AddressData.US_OA_Address2 = mxAddress.PK;
			AssertEquals("Default Post code for Mexico", ZString.Empty, AddressData.US_ZipAddress2);

			AddressData.US_OA_Address2 = caAddress.PK;
			AssertEquals("Default Post code for Canada", caAddress.OA_PostCode, AddressData.US_ZipAddress2);

			AddressData.US_OA_Address2 = auAddress.PK;
			AssertEquals("Not Default Post code for non-NAFTA", ZString.Empty, AddressData.US_ZipAddress2);
		}

		public void TestDefaultImporterNumberForImporterConsigneeCreateUpdateWhenActionCodeSelected()
		{
			Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "7892897");

			var addressData = new OrgAddressMessageData(Wrapper);
			AssertEquals("Importer Number should not have been defaulted", "", addressData.US_ImporterNumber);

			Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "7892897");
			addressData.US_ActionCode = ImporterADDActionCodeList.Codes.RequestCBPNumber;
			AssertEquals("Importer Number should have been cleared for the selected action", "", addressData.US_ImporterNumber);

			Organisation.CustomsCodes.RemoveAndDeleteAll();
			Organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "7892897", Core.Constants.CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			addressData.US_ActionCode = ImporterADDActionCodeList.Codes.ChangeImporter;
			AssertEquals("Importer Number", "7892897", addressData.US_ImporterNumber);
		}

		public void TestReadOnly()
		{
			AssertEquals("UtlOtherDescription readonly", true, AddressData.US_UtlOtherDescription_ReadOnly);
			AddressData.US_UtlOtherIndicator = true;
			AssertEquals("UtlOtherDescription readonly", false, AddressData.US_UtlOtherDescription_ReadOnly);
		}

		#region TestSetDefaultState

		public void TestSetDefaultState()
		{
			addressData = SetupForDefaultStateTest(MXRelatedPortCode, "AGU", MXRelatedPortCode, "MEX");
			AssertEquals("AG", addressData.US_StateAddress1);
			AssertEquals(ZString.Empty, addressData.US_ZipAddress1);
		}

		const string MXRelatedPortCode = "MXZUM";
		const string PRRelatedPortCode = "PRSJU";

		OrgAddressMessageData SetupForDefaultStateTest(string portCode1, string state1, string portCode2, string state2, string country = "US")
		{
			AssertNotEquals("Precondition: state1 not equals to state2", state1, state2);
			Organisation.OH_FullName = "CargoWise edi (previously known as EDI)";

			SetAddressForTestWithSpecifiedPortCode(Organisation.MainAddress, portCode2, state2, country);
			var address = Organisation.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			SetAddressForTestWithSpecifiedPortCode(address, portCode1, state1, country);

			var orgHeaderWrapper = OrgHeaderWrapper.New(Organisation);
			orgHeaderWrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;
			return new OrgAddressMessageData(orgHeaderWrapper);
		}

		public void TestSetDefaultStateAndCountryForPR()
		{
			addressData = SetupForDefaultStateTest(PRRelatedPortCode, "PR", PRRelatedPortCode, "");
			AssertEquals("PR", addressData.US_StateAddress1);
			AssertEquals("US", addressData.US_RN_NKCountry1);

			addressData = SetupForDefaultStateTest(PRRelatedPortCode, "PR", PRRelatedPortCode, "", "PR");
			AssertEquals("PR", addressData.US_StateAddress1);
			AssertEquals("US", addressData.US_RN_NKCountry1);

			addressData.US_OA_Address2 = addressData.US_OA_Address1;
			AssertEquals("PR", addressData.US_StateAddress2);
			AssertEquals("US", addressData.US_RN_NKCountry2);

			addressData = SetupForDefaultStateTest(PRRelatedPortCode, "", PRRelatedPortCode, "PR", "PR");
			AssertEquals("PR", addressData.US_StateAddress1);
			AssertEquals("US", addressData.US_RN_NKCountry1);
		}

		#endregion

		void SetAddressForTestWithSpecifiedPortCode(OrgAddress address, string relatedPortCode, string stateForTest, string country = "US")
		{
			address.OA_Address1 = "3A, 72 O'Riordan St";
			address.OA_Address2 = "(Back of Eat Me restaurant)";
			address.OA_City = "Alexandria";
			address.OA_PostCode = "2015";
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
			address.OA_CompanyNameOverride = "CargoWise edi pty Ltd";
			address.OA_State = stateForTest;
			address.OA_RN_NKCountryCode = country;
		}

		public void TestSetDefaultValues()
		{
			Organisation.OH_FullName = "CargoWise edi (previously known as EDI)";
			SetAddressForTestWithSpecifiedPortCode(Organisation.MainAddress, "AUSYD", "NSW");
			Organisation.MainAddress.OA_Code = "Office Address";

			var customsAddress = AddAddress(OrgAddressType.CustomsAddressOfRecord, "Test Customs Address", "Alexandria1", "Customs Address", "NSW", "2020", "AUSYD");
			var postalAddress2 = AddAddress(OrgAddressType.Postal, "P.O. Box 3425", "Alexandria", "Postal Address for EDI", "NSW", "2015", "AUSYD");

			Wrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;
			Organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12121212", Core.Constants.CountryCodes.UnitedStates);
			Organisation.MainAddress.OA_Phone = "(61) 1234 5678";
			Organisation.MainAddress.OA_Email = "TEST@ABC.COM";
			Organisation.MainAddress.OA_Fax = "(61) 9876 5432";
			Organisation.MainWebURL.PU_URL = "http://www.wisetechglobal.com";
			Organisation.MiscServ.OM_CMEstablishedDate = new ZDateTime(2019, 11, 12);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			addressData = new OrgAddressMessageData(Wrapper);
			AssertEquals("Importer Number", "12121212", addressData.US_ImporterNumber);
			AssertEquals("Importer Name", "CargoWise edi (previously known as EDI)", addressData.US_ImporterName);
			AssertEquals("Importer Type", ImporterTypeList.Codes.Corporation, addressData.US_ImporterType);
			AssertEquals("Action Code", ImporterADDActionCodeList.Codes.AddImporterNumber, addressData.US_ActionCode);
			AssertEquals("Importer Phone Number", "6112345678", addressData.US_ImporterPhoneNumber);
			AssertEquals("Importer Email", "TEST@ABC.COM", addressData.US_ImporterEmail);
			AssertEquals("Importer Fax Number", "6198765432", addressData.US_ImporterFaxNumber);
			AssertEquals("Importer Website", "http://www.wisetechglobal.com", addressData.US_ImporterWebsite);
			AssertEquals("Year Established", "2019", addressData.US_YearEstablished);
		}

		OrgAddress AddAddress(OrgAddressType addressType, ZString addressLine1, ZString city, ZString code, ZString state, ZString postCode, ZString port)
		{
			var address = Organisation.Addresses.AddNew(addressType, true);
			address.OA_Address1 = addressLine1;
			address.OA_City = city;
			address.OA_Code = code;
			address.OA_State = state;
			address.OA_PostCode = postCode;
			address.OA_RL_NKRelatedPortCode = port;
			return address;
		}

		public void TestAddresses()
		{
			AddressData.US_OA_Address1 = Organisation.MainAddress.PK;
			AssertEquals(Organisation.MainAddress, AddressData.FirstAddress);

			var address = Organisation.Addresses.AddNew();
			AddressData.US_OA_Address2 = address.PK;
			AssertEquals(address, AddressData.SecondAddress);

			AssertEquals(typeof(OrgAddressMessageDataLookups), AddressData.Lookups.GetType());
		}

		public void TestDefaultAddressDetailsWhenFirstAddressIsSet()
		{
			SetAddressForTestWithSpecifiedPortCode(Organisation.MainAddress, "AUSYD", "NSW");
			Organisation.MainAddress.OA_Code = "Office Address";

			var postalAddress = AddAddress(OrgAddressType.Postal, "P.O. Box 3426", "Mascot", "Postal Address", "AHK", "2115", "NZAKL");
			var customsAddress = AddAddress(OrgAddressType.CustomsAddressOfRecord, "Test Customs Address", "Alexandria1", "Customs Address", "NSW", "2020", "NZAKL");

			AddressData.US_OA_Address1 = Organisation.MainAddress.PK;

			AssertEquals("3A, 72 O'Riordan St", AddressData.US_LineOneAddress1);
			AssertEquals("for foreign-based importers, the second line should not be defaulted", "", AddressData.US_LineTwoAddress1);
			AssertEquals("Alexandria", AddressData.US_CityAddress1);
			AssertEquals(ZString.Empty, AddressData.US_ZipAddress1);
			AssertEquals("AU", AddressData.US_RN_NKCountry1);

			AddressData.US_OA_Address1 = postalAddress.PK;

			AssertEquals("P.O. Box 3426", AddressData.US_LineOneAddress1);
			AssertEquals("", AddressData.US_LineTwoAddress1);
			AssertEquals("Mascot", AddressData.US_CityAddress1);
			AssertEquals(ZString.Empty, AddressData.US_ZipAddress1);
			AssertEquals("NZ", AddressData.US_RN_NKCountry1);

			AddressData.US_OA_Address1 = customsAddress.PK;

			AssertEquals("Test Customs Address", AddressData.US_LineOneAddress1);
			AssertEquals("", AddressData.US_LineTwoAddress1);
			AssertEquals("Alexandria1", AddressData.US_CityAddress1);
			AssertEquals(ZString.Empty, AddressData.US_ZipAddress1);
			AssertEquals("NZ", AddressData.US_RN_NKCountry1);
		}

		public void TestDefaultAddressDetailsWhenSecondAddressIsSet()
		{
			SetAddressForTestWithSpecifiedPortCode(Organisation.MainAddress, "AUSYD", "NSW");
			Organisation.MainAddress.OA_Code = "Office Address";

			var postalAddress = AddAddress(OrgAddressType.Postal, "P.O. Box 3426", "Mascot", "Postal Address", "AHK", "2115", "NZAKL");

			AddressData.US_OA_Address2 = postalAddress.PK;

			AssertEquals("P.O. Box 3426", AddressData.US_LineOneAddress2);
			AssertEquals("Mascot", AddressData.US_CityAddress2);
			AssertEquals(ZString.Empty, AddressData.US_ZipAddress2);
			AssertEquals("NZ", AddressData.US_RN_NKCountry2);

			AddressData.US_OA_Address2 = Organisation.MainAddress.PK;
			AssertEquals("3A, 72 O'Riordan St", AddressData.US_LineOneAddress2);
			AssertEquals("for foreign-based importers, the second line should not be defaulted", "", AddressData.US_LineTwoAddress2);
			AssertEquals("Alexandria", AddressData.US_CityAddress2);
			AssertEquals(ZString.Empty, AddressData.US_ZipAddress2);
			AssertEquals("AU", AddressData.US_RN_NKCountry2);
		}

		public void TestIsImporter()
		{
			AssertEquals("IsUSImporter", true, AddressData.IsUSImporter(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("IsUSImporter", true, AddressData.IsUSImporter(Core.Constants.CountryCodes.PuertoRico));
			AssertEquals("IsCAImporter", false, AddressData.IsCAImporter(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("IsMXImporter", false, AddressData.IsMXImporter(Core.Constants.CountryCodes.UnitedStates));

			AssertEquals("IsUSImporter", false, AddressData.IsUSImporter(Core.Constants.CountryCodes.Canada));
			AssertEquals("IsCAImporter", true, AddressData.IsCAImporter(Core.Constants.CountryCodes.Canada));
			AssertEquals("IsMXImporter", false, AddressData.IsMXImporter(Core.Constants.CountryCodes.Canada));

			AssertEquals("IsUSImporter", false, AddressData.IsUSImporter(Core.Constants.CountryCodes.Mexico));
			AssertEquals("IsCAImporter", false, AddressData.IsCAImporter(Core.Constants.CountryCodes.Mexico));
			AssertEquals("IsMXImporter", true, AddressData.IsMXImporter(Core.Constants.CountryCodes.Mexico));
		}

		public void TestDefaultZipCode()
		{
			SetAddressForTestWithSpecifiedPortCode(Organisation.MainAddress, "AUSYD", "NSW");
			Organisation.MainAddress.OA_Code = "Office Address";

			var postalAddress = AddAddress(OrgAddressType.Postal, "P.O. Box 3426", "Mascot", "Postal Address", "IL", "60010-1575", "USCHI");

			AddressData.US_OA_Address1 = postalAddress.PK;
			AddressData.US_OA_Address2 = postalAddress.PK;

			AssertEquals("600101575", AddressData.US_ZipAddress1);
			AssertEquals("600101575", AddressData.US_ZipAddress2);

			postalAddress = AddAddress(OrgAddressType.Postal, "P.O. Box 3426", "MANITOBA", "Postal Address", "BC", "R0A 2A0", "CAWEL");

			AddressData.US_OA_Address1 = postalAddress.PK;
			AddressData.US_OA_Address2 = postalAddress.PK;

			AssertEquals("IT should not contain space", "R0A2A0", AddressData.US_ZipAddress1);
			AssertEquals("IT should not contain space", "R0A2A0", AddressData.US_ZipAddress2);
		}

		public void TestUS_AddressExplaination()
		{
			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._01;
			AssertEquals(ImporterAddressTypesList.Descriptions._01, AddressData.US_AddressExplanation1);
			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._02;
			AssertEquals(ImporterAddressTypesList.Descriptions._02, AddressData.US_AddressExplanation2);

			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._08;
			AddressData.US_AddressExplanation1 = "AAAAA";
			AssertEquals("AAAAA", AddressData.US_AddressExplanation1);
			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._08;
			AddressData.US_AddressExplanation2 = "BBBBB";
			AssertEquals("BBBBB", AddressData.US_AddressExplanation2);

			AddressData.US_AddressType1 = ImporterAddressTypesList.Codes._03;
			AssertEquals(ImporterAddressTypesList.Descriptions._03, AddressData.US_AddressExplanation1);
			AddressData.US_AddressType2 = ImporterAddressTypesList.Codes._04;
			AssertEquals(ImporterAddressTypesList.Descriptions._04, AddressData.US_AddressExplanation2);
		}

		public void TestDefaultCertifyingIndividualFromContact()
		{
			var contact = Organisation.Contacts.AddNew();
			contact.OC_ContactName = "IANaaaaaaabbbbbbbbbbccccccccccddddddddddeeeee THEaaaaaaabbbbbbbbbb BUILDERaaabbbbbbbbbbccccccccccddddddddddeeeee";
			contact.OC_Phone = "+610041096252";
			contact.OC_Title = "PRESIDENT";
			contact.OC_Email = "IAN@TEST.COM";

			AddressData.US_OC_CertifyIndividual = contact.PK;
			AssertEquals("BUILDERaaabbbbbbbbbbccccccccccdddddddddd, IANaaaaaaabbbbbbbbbbccccccccccdddddddddd, T", AddressData.US_IndividualName);
			AssertEquals("PRESIDENT", AddressData.US_IndividualTitle);
			AssertEquals("0041096252", AddressData.US_IndividualPhone);
		}

		public void TestDefaultBrokerFromGlbStaff()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "TEST BROKER";
			broker.GS_WorkPhone = "+610041096252";

			AddressData.US_GS_Broker = broker.GS_Code;
			AssertEquals("TEST BROKER", AddressData.US_BrokerName);
			AssertEquals("0041096252", AddressData.US_BrokerPhone);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AddressData;
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
