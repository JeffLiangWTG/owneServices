using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEOceanManifestJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestValidateABICharactersWhenNotOverrideAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Â/*A";
			org.MainAddress.OA_Address1 = "Â/*A";
			org.MainAddress.OA_City = "Â*A";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Â/*A";

			docAddress.OrganisationPK = org.PK;
			docAddress.ContactPK = contact.PK;

			AssertHasWarningContaining(docAddress.OrganisationPKInfo, "Unspecified: Contact Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			AssertHasWarningContaining(docAddress.OrganisationPKInfo, "Unspecified: City : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			AssertHasWarningContaining(docAddress.OrganisationPKInfo, "Unspecified: Company Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			AssertHasWarningContaining(docAddress.OrganisationPKInfo, "Unspecified: Address Line 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
		}

		public void TestCheckE2_CompanyName()
		{
			var message = @"Unspecified: Company Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "NAME*";
			AssertHasWarningContaining(docAddress.E2_CompanyNameInfo, message);
			docAddress.E2_CompanyName = "NA/E";
			AssertNoWarningContaining(docAddress.E2_CompanyNameInfo, message);
			docAddress.E2_CompanyName = "NÂMË";
			AssertHasWarning(docAddress.E2_CompanyNameInfo, message);

			docAddress.E2_CompanyName = "测试NAMË";
			AssertHasMessageErrorContaining(docAddress.E2_CompanyNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CompanyNameInfo));
		}

		public void TestCheckE2_Address1()
		{
			var message = @"Unspecified: Address Line 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "ADDRESS1*";
			AssertHasWarningContaining(docAddress.E2_Address1Info, message);
			docAddress.E2_Address1 = "ADDRESS1";
			AssertNoWarningContaining(docAddress.E2_Address1Info, message);
			docAddress.E2_Address1 = "àddrêss1";
			AssertHasWarning(docAddress.E2_Address1Info, message);

			docAddress.E2_Address1 = "测试ADDRESS1";
			AssertHasMessageErrorContaining(docAddress.E2_Address1Info, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address1Info));
		}

		public void TestCheckE2_Address2()
		{
			var message = @"Unspecified: Address Line 2 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address2 = "ADDRESS2*";
			AssertHasWarningContaining(docAddress.E2_Address2Info, message);
			docAddress.E2_Address2 = "ADDRESS2";
			AssertNoWarningContaining(docAddress.E2_Address2Info, message);
			docAddress.E2_Address2 = "äddréss2";
			AssertHasWarning(docAddress.E2_Address2Info, message);

			docAddress.E2_Address2 = "测试ADDRESS2";
			AssertHasMessageErrorContaining(docAddress.E2_Address2Info, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address2Info));
		}

		public void TestCheckE2_Contact()
		{
			var message = @"Unspecified: Contact Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "CONTACT*";
			AssertHasWarningContaining(docAddress.E2_ContactInfo, message);
			docAddress.E2_Contact = "CONTACT";
			AssertNoWarningContaining(docAddress.E2_ContactInfo, message);
			docAddress.E2_Contact = "CONTÀCT";
			AssertHasWarning(docAddress.E2_ContactInfo, message);

			docAddress.E2_Contact = "测试CONTACT";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_ContactInfo));
		}

		public void TestCheckE2_PostCode()
		{
			var message = @"Unspecified: Postcode : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Postcode = "102*";
			AssertHasWarningContaining(docAddress.E2_PostcodeInfo, message);
			docAddress.E2_Postcode = "102";
			AssertNoWarningContaining(docAddress.E2_PostcodeInfo, message);
			docAddress.E2_Postcode = "102É";
			AssertHasWarning(docAddress.E2_PostcodeInfo, message);

			docAddress.E2_Postcode = "测试102";
			AssertHasMessageErrorContaining(docAddress.E2_PostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_PostcodeInfo));
		}

		public void TestCheckE2_City()
		{
			var message = @"Unspecified: City : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_City = "MEL*";
			AssertHasWarningContaining(docAddress.E2_CityInfo, message);
			docAddress.E2_City = "MEL";
			AssertNoWarningContaining(docAddress.E2_CityInfo, message);
			docAddress.E2_City = "MÉL";
			AssertHasWarning(docAddress.E2_CityInfo, message);

			docAddress.E2_City = "测试MEL";
			AssertHasMessageErrorContaining(docAddress.E2_CityInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CityInfo));
		}

		public void TestCheckE2_Phone()
		{
			var message = @"Unspecified: Telephone Number : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Phone = "1É1*";
			AssertHasWarningContaining(docAddress.E2_PhoneInfo, message);
			docAddress.E2_Phone = "111";
			AssertNoWarningContaining(docAddress.E2_PhoneInfo, message);
		}

		public void TestCheckE2_Fax()
		{
			var message = @"Unspecified: Fax Number : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Fax = "222*";
			AssertHasWarningContaining(docAddress.E2_FaxInfo, message);
			docAddress.E2_Fax = "222";
			AssertNoWarningContaining(docAddress.E2_FaxInfo, message);
			docAddress.E2_Fax = "2É22";
			AssertHasWarning(docAddress.E2_FaxInfo, message);
		}

		public void TestCheckE2_Email()
		{
			var message = @"Unspecified: Email Address : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Email = "t*v@gmail.com";
			AssertHasWarningContaining(docAddress.E2_EmailInfo, message);
			docAddress.E2_Email = "tv@gmail.com";
			AssertNoWarningContaining(docAddress.E2_EmailInfo, message);
			docAddress.E2_Email = "tÉv@gmail.com";
			AssertHasWarning(docAddress.E2_EmailInfo, message);
		}

		JobDocAddress docAddress;
		protected override void SetUp()
		{
			base.SetUp();
			var collection = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			docAddress = collection.AddNew();
			docAddress.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(docAddress);
		}
	}
}
