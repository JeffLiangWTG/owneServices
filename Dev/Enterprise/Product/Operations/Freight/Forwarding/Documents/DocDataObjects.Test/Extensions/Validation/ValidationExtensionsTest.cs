using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ValidationExtensionsTest : TestCaseWithFactory
	{
		#region TestAddCompanyNameValidation

		public void TestAddCompanyNameValidation()
		{
			var context = new CommonContext(Factory);

			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddPartyNameAndAddressValidation("Shipper");
			address.ValidateAll();
			const string errorMessage = "Shipper party name and address information is required.";
			address.CompanyName = "WTG";
			address.Country.Name = "Australia";

			address.AddressLine1 = "ABC";
			address.AddressLine2 = "EDF";
			AssertNoMessageError(address.CompanyNameInfo, errorMessage);

			address.AddressLine1 = "";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.AddressLine1 = "ABC";
			address.AddressLine2 = "";
			AssertNoMessageError(address.CompanyNameInfo, errorMessage);

			address.AddressLine1 = "";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.CompanyName = "";
			address.AddressLine1 = "ABC";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.Country.Name = "";
			address.CompanyName = "WTG";
			address.AddressLine1 = "ABC";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);
		}

		public void TestIsPartyNameAndAddressEmpty()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			bool result = false;

			AssertNoExceptionThrown("Expect no Exception thrown in IsPartyNameAndAddressEmpty method", () => result = ValidationExtensions.IsPartyNameAndAddressEmpty(null));
			AssertEquals(result, true);

			address.Country = null;
			AssertNoExceptionThrown("Expect no Exception thrown in IsPartyNameAndAddressEmpty method", () => result = address.IsPartyNameAndAddressEmpty());
			AssertEquals(result, true);
		}

		#endregion

		#region TestAddContactDetailsValidation

		public void TestAddContactDetailsValidation()
		{
			var context = new CommonContext(Factory);

			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddContactDetailsValidation();

			address.ValidateAll();

			const string errorMessage = "Please enter both contact name and at least one communication: phone, email or fax.";

			AssertNoMessageError(address.ContactInfo, errorMessage);

			address.Contact = "Rafaello";

			AssertHasMessageError(address.ContactInfo, errorMessage);

			address.Phone = "1234567";

			AssertNoMessageError(address.ContactInfo, errorMessage);

			address.Phone = "";
			address.Email = "a@a.com";

			AssertNoMessageError(address.ContactInfo, errorMessage);

			address.Email = "";
			address.Fax = "1234567";

			AssertNoMessageError(address.ContactInfo, errorMessage);
		}

		#endregion

		#region TestAddContactNameLengthValidation

		public void TestAddContactNameLengthValidation()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddContactNameLengthValidation();

			address.ValidateAll();

			const string errorMessage = "Contact Name must not exceed 35 characters.";

			AssertNoWarning(address.ContactInfo, errorMessage);

			address.Contact = "1234567abcdefg1234567abcdefg1234567";
			AssertNoWarning("35 characters should not have the warning", address.ContactInfo, errorMessage);

			address.Contact = "1234567abcdefg1234567abcdefg12345678";
			AssertHasWarning("36 characters should have the warning", address.ContactInfo, errorMessage);
		}

		public void TestAddContactNameLengthValidation_WithPrecondition()
		{
			const string errorMessage = "Contact Name must not exceed 35 characters.";

			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			var precondition = true;
			address.AddContactNameLengthValidation(() => precondition);

			precondition = true;
			address.Contact = new string('1', 36);

			AssertHasWarning(address.ContactInfo, errorMessage);

			precondition = false;
			address.Contact = new string('2', 36);

			AssertNoWarning(address.ContactInfo, errorMessage);
		}

		#endregion

		#region TestAddEmptyCountryCodeValidation

		public void TestAddEmptyCountryCodeValidation()
		{
			var errorMessage = "Country code is required, if Country is entered.";
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddEmptyCountryCodeValidation();

			address.ValidateAll();

			address.Country.Name = "";
			address.Country.Code = "";
			AssertNoMessageError(address.Country.NameInfo, errorMessage);

			address.Country.Name = "CountryName";
			AssertHasMessageError(address.Country.NameInfo, errorMessage);

			address.Country.Code = "CN";
			AssertNoMessageError(address.Country.NameInfo, errorMessage);

			address.Country.Name = "";
			AssertNoMessageError(address.Country.NameInfo, errorMessage);
		}

		public void TestAddEmptyCountryCodeValidation_WithPrecondition()
		{
			const string errorMessage = "Country code is required, if Country is entered.";

			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			var precondition = true;
			address.AddEmptyCountryCodeValidation(() => precondition);

			precondition = true;
			address.Country.Name = "CountryName";
			address.Country.Code = "";

			AssertHasMessageError(address.Country.NameInfo, errorMessage);

			precondition = false;
			address.Country.Name = "AnotherCountryName";
			address.Country.Code = "";

			AssertNoMessageError(address.Country.NameInfo, errorMessage);
		}

		#endregion

		#region TestAddCurrentUserValidation

		public void TestAddCurrentUserValidation()
		{
			var context = new CommonContext(Factory);
			var currentUserAddress = AddressBuilder.Create(context, (OrgAddress)null);
			var signature = new SignatureDetails();

			signature.AddCurrentUserValidation(currentUserAddress);

			signature.ValidateAll();

			const string errorMessage = "Your staff login profile does not specify contact details. At least one communication number (email or phone) is required.";

			AssertHasMessageError(signature.NameInfo, errorMessage);

			currentUserAddress.Email = "a@a.com";

			AssertNoMessageError(signature.NameInfo, errorMessage);
			AssertNoMessageError(signature.NameInfo, errorMessage);
			AssertNoMessageError(signature.NameInfo, errorMessage);

			currentUserAddress.Email = string.Empty;

			AssertHasMessageError(signature.NameInfo, errorMessage);
			AssertHasMessageError(signature.NameInfo, errorMessage);
			AssertHasMessageError(signature.NameInfo, errorMessage);

			currentUserAddress.Phone = "0123456";

			AssertNoMessageError(signature.NameInfo, errorMessage);
			AssertNoMessageError(signature.NameInfo, errorMessage);
			AssertNoMessageError(signature.NameInfo, errorMessage);
		}

		#endregion

		#region TestAddAsciiCharactersValidation

		public void TestAddAsciiCharacterValidation_Address()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddAsciiCharactersValidation();

			address.ValidateAll();

			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			AssertNoMessageError(address.CompanyNameInfo, errorMessage);
			AssertNoMessageError(address.ContactInfo, errorMessage);
			AssertNoMessageError(address.AddressLine1Info, errorMessage);
			AssertNoMessageError(address.AddressLine2Info, errorMessage);
			AssertNoMessageError(address.CityInfo, errorMessage);
			AssertNoMessageError(address.StateInfo, errorMessage);
			AssertNoMessageError(address.PostcodeInfo, errorMessage);

			address.CompanyName = "物語";

			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.Contact = "物語";

			AssertHasMessageError(address.ContactInfo, errorMessage);

			address.AddressLine1 = "物語";

			AssertHasMessageError(address.AddressLine1Info, errorMessage);

			address.AddressLine2 = "物語";

			AssertHasMessageError(address.AddressLine2Info, errorMessage);

			address.City = "物語";

			AssertHasMessageError(address.CityInfo, errorMessage);

			address.State = "物語";

			AssertHasMessageError(address.StateInfo, errorMessage);

			address.Postcode = "物語";

			AssertHasMessageError(address.PostcodeInfo, errorMessage);

			address.CompanyName = "Company";
			address.Contact = "John Smith";
			address.AddressLine1 = "123 Address";
			address.AddressLine2 = "Address Street";
			address.City = "Valhalla";
			address.State = "Asgard";
			address.Postcode = "1234";

			AssertNoMessageError(address.CompanyNameInfo, errorMessage);
			AssertNoMessageError(address.ContactInfo, errorMessage);
			AssertNoMessageError(address.AddressLine1Info, errorMessage);
			AssertNoMessageError(address.AddressLine2Info, errorMessage);
			AssertNoMessageError(address.CityInfo, errorMessage);
			AssertNoMessageError(address.StateInfo, errorMessage);
			AssertNoMessageError(address.PostcodeInfo, errorMessage);
		}

		public void TestAddAsciiCharacterValidation_Address_WithPrecondition()
		{
			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			var precondition = true;
			address.AddAsciiCharactersValidation(() => precondition);

			precondition = true;
			address.CompanyName = "物";
			address.Contact = "物";
			address.AddressLine1 = "物";
			address.AddressLine2 = "物";
			address.City = "物";
			address.State = "物";
			address.Postcode = "物";

			CombineAssertions(() =>
			{
				AssertHasMessageError(address.CompanyNameInfo, errorMessage);
				AssertHasMessageError(address.ContactInfo, errorMessage);
				AssertHasMessageError(address.AddressLine1Info, errorMessage);
				AssertHasMessageError(address.AddressLine2Info, errorMessage);
				AssertHasMessageError(address.CityInfo, errorMessage);
				AssertHasMessageError(address.StateInfo, errorMessage);
				AssertHasMessageError(address.PostcodeInfo, errorMessage);
			});

			precondition = false;
			address.CompanyName = "語";
			address.Contact = "語";
			address.AddressLine1 = "語";
			address.AddressLine2 = "語";
			address.City = "語";
			address.State = "語";
			address.Postcode = "語";

			CombineAssertions(() =>
			{
				AssertNoMessageError(address.CompanyNameInfo, errorMessage);
				AssertNoMessageError(address.ContactInfo, errorMessage);
				AssertNoMessageError(address.AddressLine1Info, errorMessage);
				AssertNoMessageError(address.AddressLine2Info, errorMessage);
				AssertNoMessageError(address.CityInfo, errorMessage);
				AssertNoMessageError(address.StateInfo, errorMessage);
				AssertNoMessageError(address.PostcodeInfo, errorMessage);
			});
		}

		public void TestAddAsciiCharacterValidation_AddressSpecialCharacters()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddAsciiCharactersValidation();

			address.ValidateAll();

			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			AssertNoMessageError(address.CompanyNameInfo, errorMessage);
			AssertNoMessageError(address.ContactInfo, errorMessage);
			AssertNoMessageError(address.AddressLine1Info, errorMessage);
			AssertNoMessageError(address.AddressLine2Info, errorMessage);
			AssertNoMessageError(address.CityInfo, errorMessage);
			AssertNoMessageError(address.StateInfo, errorMessage);
			AssertNoMessageError(address.PostcodeInfo, errorMessage);

			address.CompanyName = "š";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.Contact = "š";
			AssertHasMessageError(address.ContactInfo, errorMessage);

			address.AddressLine1 = "š";
			AssertHasMessageError(address.AddressLine1Info, errorMessage);

			address.AddressLine2 = "š";
			AssertHasMessageError(address.AddressLine2Info, errorMessage);

			address.City = "š";
			AssertHasMessageError(address.CityInfo, errorMessage);

			address.State = "š";
			AssertHasMessageError(address.StateInfo, errorMessage);

			address.Postcode = "š";
			AssertHasMessageError(address.PostcodeInfo, errorMessage);

			address.CompanyName = "Compàny";
			address.Contact = "Contàct";
			address.AddressLine1 = "123 àddress";
			address.AddressLine2 = "àddress Street";
			address.City = "Valhàllà";
			address.State = "àsgàrd";
			address.Postcode = "1234";

			AssertNoMessageError(address.CompanyNameInfo, errorMessage);
			AssertNoMessageError(address.ContactInfo, errorMessage);
			AssertNoMessageError(address.AddressLine1Info, errorMessage);
			AssertNoMessageError(address.AddressLine2Info, errorMessage);
			AssertNoMessageError(address.CityInfo, errorMessage);
			AssertNoMessageError(address.StateInfo, errorMessage);
			AssertNoMessageError(address.PostcodeInfo, errorMessage);
		}

		public void TestAddAsciiCharacterValidation_Unloco()
		{
			var context = new CommonContext(Factory);
			var unloco = Unloco.Create(context, (RefUNLOCO)null);
			unloco.AddAsciiCharactersValidation();

			AssertAsciiCharactersValidation("Unloco.Code", unloco);
		}

		#endregion

		#region TestAddSupportedCharactersValidationForUSCustoms

		public void TestAddSupportedCharactersValidationForUSCustoms_Address()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddSupportedCharactersValidationForUSCustoms();

			address.ValidateAll();

			AssertSupportedCharactersValidationForUSCustoms(address);
		}

		public void TestAddSupportedCharactersValidationForUSCustoms_Unloco()
		{
			var context = new CommonContext(Factory);
			var unloco = Unloco.Create(context, (RefUNLOCO)null);
			unloco.AddSupportedCharactersValidationForUSCustoms();

			AssertSupportedCharactersValidationForUSCustoms(unloco);
		}

		public void TestAddSupportedCharactersValidationForUSCustoms_ZProperty()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.CompanyNameInfo.AddSupportedCharactersValidationForUSCustoms();
			address.ValidateAll();

			const string errorMessage = "This text contains characters not supported by the United States Customs (CBP).\r\n"
				+ "Only characters shown directly on a keyboard with US layout are acceptable for this message, not typed or special characters.";

			address.CompanyName = "마늘빵 어디있어";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);

			address.CompanyName = @"QWERTYUIOP{}|
ASDFGHJKL:
ZCVBNM<>?
qwertyuiop[]\
asdfghjkl; '
zxcvbnm,./
1234567890 -=`
!@#$%^&*()_+~	";
			AssertNoMessageError(address.CompanyNameInfo, errorMessage);

			address.CompanyName = "—éÇƒ";
			AssertHasMessageError(address.CompanyNameInfo, errorMessage);
		}

		#endregion

		#region TestRequiredUnlocoValidation

		public void TestRequiredUnlocoValidation()
		{
			AssertRequiredUnlocoValidation(null);
			AssertRequiredUnlocoValidation("");
			AssertRequiredUnlocoValidation("Test");
		}

		void AssertRequiredUnlocoValidation(string prefix)
		{
			var context = new CommonContext(Factory);

			var unloco = Unloco.Create(context, (RefUNLOCO)null);
			unloco.AddRequiredValidation(prefix);

			unloco.ValidateAll();

			var messagePrefix = string.IsNullOrWhiteSpace(prefix)
				? ""
				: prefix + " ";

			var codeErrorMessage = $"{messagePrefix}Code is required.";
			var nameErrorMessage = $"{messagePrefix}Name is required.";

			AssertHasMessageError(unloco.CodeInfo, codeErrorMessage);
			AssertHasMessageError(unloco.NameInfo, nameErrorMessage);

			unloco.Name = "Sydney";

			AssertHasMessageError(unloco.CodeInfo, codeErrorMessage);
			AssertNoMessageError(unloco.NameInfo, nameErrorMessage);

			unloco.Code = "AUSYD";

			AssertNoMessageError(unloco.CodeInfo, codeErrorMessage);
			AssertNoMessageError(unloco.NameInfo, nameErrorMessage);
		}

		#endregion

		#region TestAddEmptyCodeDescriptionValidation

		public void TestAddEmptyCodeDescriptionValidation()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("AAA", "Description");

			var codeDescription = new CodeDescription(list);
			codeDescription.AddEmptyCodeDescriptionValidation("TestCodeDescription");

			codeDescription.ValidateAll();

			const string errorMessage = "A TestCodeDescription is required.";

			AssertHasMessageError(codeDescription.CodeInfo, errorMessage);

			codeDescription.Code = "AAA";

			AssertNoMessageError(codeDescription.CodeInfo, errorMessage);

			codeDescription.Code = string.Empty;

			AssertHasMessageError(codeDescription.CodeInfo, errorMessage);
		}

		#endregion

		#region AddCompanyNameLengthValidation

		public void TestAddCompanyNameLengthValidation()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddCompanyNameLengthValidation();

			AssertAddressCompanyNameLength("address", address);
		}

		public void TestAddCompanyNameLengthValidation_WithPrecondition()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			var precondition = true;
			address.AddCompanyNameLengthValidation(() => precondition);

			AssertAddressCompanyNameLength("address", address);

			precondition = false;
			address.CompanyName = "Alibaba's name is from a story known across the world named 'Alibaba and the Forty Thieves'.";

			var warning = "Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient.";
			AssertNoWarning(address.CompanyNameInfo, warning);
		}

		#endregion

		#region AddInvalidCodeValidation

		public void TestAddInvalidCodeValidation()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";

			var unloco = Unloco.Create(context, shipment.LoadPort);
			unloco.CodeInfo.AddInvalidCodeValidation();

			unloco.Code = string.Empty;
			AssertNoMessageError(unloco.CodeInfo, "You have not entered a valid code.");

			unloco.Code = "XXXXX";
			AssertHasMessageError(unloco.CodeInfo, "You have not entered a valid code.");

			unloco.Code = "CNSHA";
			AssertNoMessageError(unloco.CodeInfo, "You have not entered a valid code.");

			unloco = Unloco.Create(context, shipment.LoadPort);
			unloco.CodeInfo.AddInvalidCodeValidation("You have not entered a valid unloco.");

			unloco.Code = "XXXXX";
			AssertHasMessageError(unloco.CodeInfo, "You have not entered a valid unloco.");

			unloco.Code = "CNSHA";
			AssertNoMessageError(unloco.CodeInfo, "You have not entered a valid unloco.");
		}

		public void TestAddInvalidCodeValidationWithCodeMapper()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mapping = orgHeader.CreatePatternMatchOverrideForTest();

			mapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping.OO_ForeignCode = "CNXXX";
			mapping.OO_LocalCode = "CNSHA";

			var context = new ContextWithCarrierUnlocoMapping(Factory, orgHeader.PK);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";

			var unloco = Unloco.Create(context, shipment.LoadPort);
			unloco.CodeInfo.AddInvalidCodeValidation();

			unloco.Code = string.Empty;
			AssertNoMessageError(unloco.CodeInfo, "You have not entered a valid code.");

			unloco.Code = "XXXXX";
			AssertHasMessageError("Message error with invlid code", unloco.CodeInfo, "You have not entered a valid code.");

			unloco.Code = "CNSHA";
			AssertNoMessageError("No message error for valid local code", unloco.CodeInfo, "You have not entered a valid code.");

			unloco.Code = "CNXXX";
			AssertNoMessageError("No message error for valid foreign code", unloco.CodeInfo, "You have not entered a valid code.");
		}

		#endregion

		public void TestCityNameLength()
		{
			var context = new CommonContext(Factory);
			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddCityLengthValidation();
			address.ValidateAll();
			const string errorMessage = "City Name must not exceed 50 characters.";
			AssertNoWarning("should not have an error", address.CityInfo, errorMessage);
			address.City = "111111111111111222222222wwwwwwwwwwwwwwwwwwwwwwwwwww";
			AssertHasWarning("51 characters should have an error", address.CityInfo, errorMessage);
		}

		#region Then

		public void TestThen_WhenPreconditionIsNull()
		{
			Func<bool> precondition = null;

			CombineAssertions(() =>
			{
				Assert(precondition.Then(() => true)());
				Assert(!precondition.Then(() => false)());
			});
		}

		public void TestThen_WhenMeetPrecondition()
		{
			Func<bool> precondition = () => true;

			CombineAssertions(() =>
			{
				Assert(precondition.Then(() => true)());
				Assert(!precondition.Then(() => false)());
			});
		}

		public void TestThen_WhenNotMeetPrecondition()
		{
			Func<bool> precondition = () => false;

			CombineAssertions(() =>
			{
				Assert(!precondition.Then(() => true)());
				Assert(!precondition.Then(() => false)());
			});
		}

		#endregion
	}
}
