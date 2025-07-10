using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class PostalAddressFormatterTest : TestCaseWithFactory
	{
		public void TestFromOrgAddress()
		{
			OrgHeader org = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "LEVEL 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				AddressFormatter addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				string actualAddress = addressFormatter.PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);

				expectedAddress = "LEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				actualAddress = addressFormatter.PostalAddressWithoutCompanyName();
				AssertEquals("Wrong address", expectedAddress, actualAddress);

				expectedAddress = "XYZ FORWARDERS LEVEL 69 1234 SMITH STREET NEW FOO CITY ABC 123 XG 27 CANADA";
				addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				actualAddress = addressFormatter.PostalAddressAsASingleLine();
				AssertEquals("Wrong address", expectedAddress, actualAddress);

				expectedAddress = "LEVEL 69 1234 SMITH STREET NEW FOO CITY ABC 123 XG 27 CANADA";
				addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				actualAddress = addressFormatter.PostalAddressAsASingleLineWithoutCompanyName();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestNoExceptionThrownWhenGetPostalAddressWithoutCompanyName()
		{
			var flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				var addressFormatter = new AddressFormatter(Factory, (OrgHeader)null, null, false);
				AssertNoExceptionThrown(() => addressFormatter.PostalAddressWithoutCompanyName());
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestFromOrgAddressWithAndWithoutCompanyName()
		{
			var org = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "LEVEL 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddressWithCompanyName = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				string expectedAddressWithoutCompanyName = "LEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";

				var addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				AssertEquals("Address With Company Name", expectedAddressWithCompanyName, addressFormatter.PostalAddress());
				AssertEquals("Address Without Company Name", expectedAddressWithoutCompanyName, addressFormatter.PostalAddressWithoutCompanyName());
				AssertEquals("Address With Company Name", expectedAddressWithCompanyName, addressFormatter.PostalAddress());
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestFromOrgAddressWithNameAndAddressContainingMultipleSpaces()
		{
			var org = InsertOrgHeader("     XYZ        Forwarders    ", "    XYZFWLLC    ", "LEVEL 69", "    1234 Smith        Street", "New Foo City    ", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddressWithCompanyName = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				string expectedAddressWithoutCompanyName = "LEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";

				var addressFormatter = new AddressFormatter(Factory, org.MainAddress, GlbCompany.CurrentCompany, false);
				AssertEquals("Company Name", "XYZ FORWARDERS", addressFormatter.CompanyName());
				AssertEquals("With Company Name", expectedAddressWithCompanyName, addressFormatter.PostalAddress());
				AssertEquals("Without Company Name", expectedAddressWithoutCompanyName, addressFormatter.PostalAddressWithoutCompanyName());
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestJobDocAndOrgDeleted()
		{
			OrgHeader org = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "LEVEL 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			JobDocAddress docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = org.MainAddress.PK;
			Env.Registry.SetOrgAllowMixedCase(false);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "LEVEL 40";
			docAddress.E2_RN_NKCountryCode = "CA";

			Factory.Save();
			var addressFormatter = new AddressFormatter(Factory, docAddress, GlbCompany.CurrentCompany, false);

			org.Delete();
			docAddress.Delete();
			var actualAddress = addressFormatter.PostalAddress();

			Assert("Address should return empty value", !string.IsNullOrEmpty(actualAddress));
		}

		public void TestFromJobDocAddress()
		{
			OrgHeader org = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "LEVEL 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			JobDocAddress docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = org.MainAddress.PK;
			Factory.Save();

			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				AddressFormatter addressFormatter = new AddressFormatter(Factory, docAddress, GlbCompany.CurrentCompany, false);
				string actualAddress = addressFormatter.PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_Address1 = "LEVEL 40";
				docAddress.E2_RN_NKCountryCode = "CA";
				expectedAddress = "XYZ FORWARDERS\nLEVEL 40\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				addressFormatter = new AddressFormatter(Factory, docAddress, GlbCompany.CurrentCompany, false);
				actualAddress = addressFormatter.PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestLanguage()
		{
			AddressFormatter addressFormatter = new AddressFormatter(Factory, "A", "BUILDING", "1", "2", "CITY", "PROVINCE", "POSTCODE", (NoResString)"COUNTRYNAME", false);
			AssertEquals("Default Language", Core.SharedConstants.Languages.English, addressFormatter.Language);

			var recipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			addressFormatter = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany);
			AssertEquals("French Language", Core.SharedConstants.Languages.French, addressFormatter.Language);

			addressFormatter = new AddressFormatter(Factory, "A", "BUILDING", "1", "2", "CITY", "PROVINCE", "POSTCODE", (NoResString)"COUNTRYNAME", (NoResString)Constants.Languages.Icelandic, false);
			AssertEquals("French Language", Constants.Languages.Icelandic, addressFormatter.Language);
		}

		public void TestCountryIsCorrectWhenShouldSetupCountryFromRelatedCountryIsTrue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = CanadianUNLOCO;
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "Test";
			orgAddress.OA_RN_NKCountryCode = "NZ";
			orgAddress.OA_OH = org.PK;
			Factory.Save();

			var addressFormatter = new AddressFormatterForTest(Factory, orgAddress);

			AssertEquals("Precondition", true, addressFormatter.ShouldSetupCountryFromRelatedCountry);
			AssertEquals("New Zealand", addressFormatter.CountryName);
			AssertEquals("NZ", addressFormatter.CountryCode);
		}

		public void TestCountryIsCorrectWhenShouldSetupCountryFromRelatedCountryIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = CanadianUNLOCO;
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "Test";
			orgAddress.OA_RN_NKCountryCode = "NZ";
			orgAddress.OA_OH = org.PK;
			Factory.Save();

			var addressFormatter = new AddressFormatterForTest(Factory, orgAddress, false);

			AssertEquals("Precondition", false, addressFormatter.ShouldSetupCountryFromRelatedCountry);
			AssertEquals("Canada", addressFormatter.CountryName);
			AssertEquals("CA", addressFormatter.CountryCode);
		}

		public void TestInternationalOneLineAddress()
		{
			var internationalOneLineRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				string actualAddress = NewPostalAddressFormatter(Factory, internationalOneLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\n1234 Smith Street\nNew Foo City ABC 123 XG 27\nCanada";
				actualAddress = NewPostalAddressFormatter(Factory, internationalOneLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestInternationalTwoLineAddress()
		{
			var internationalTwoLineRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				string actualAddress = NewPostalAddressFormatter(Factory, internationalTwoLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\nLevel 69\n1234 Smith Street\nNew Foo City ABC 123 XG 27\nCanada";
				actualAddress = NewPostalAddressFormatter(Factory, internationalTwoLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestInternationalAddressWithAdditionalInfo()
		{
			var internationalRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", CanadianUNLOCO, Constants.Languages.French);
			internationalRecipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "New Foo Building";
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nNEW FOO BUILDING\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27\nCANADA";
				string actualAddress = NewPostalAddressFormatter(Factory, internationalRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\nNew Foo Building\nLevel 69\n1234 Smith Street\nNew Foo City ABC 123 XG 27\nCanada";
				actualAddress = NewPostalAddressFormatter(Factory, internationalRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestDomesticOneLineAddress()
		{
			var domesticOneLineRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "1234 Smith Street", "", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Constants.Languages.EnglishAmerican);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
				string actualAddress = NewPostalAddressFormatter(Factory, domesticOneLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\n1234 Smith Street\nNew Foo City ABC 123 XG 27";
				actualAddress = NewPostalAddressFormatter(Factory, domesticOneLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestDomesticTwoLineAddress()
		{
			var domesticTwoLineRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Constants.Languages.EnglishAmerican);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
				string actualAddress = NewPostalAddressFormatter(Factory, domesticTwoLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\nLevel 69\n1234 Smith Street\nNew Foo City ABC 123 XG 27";
				actualAddress = NewPostalAddressFormatter(Factory, domesticTwoLineRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestDomesticAddressWithAdditionalInfo()
		{
			var domesticRecipient = InsertOrgHeader("XYZ Forwarders", "XYZFWLLC", "Level 69", "1234 Smith Street", "New Foo City", "ABC", "123 XG 27", AustralianUNLOCO, Constants.Languages.EnglishAmerican);
			domesticRecipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "New Foo Building";
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "XYZ FORWARDERS\nNEW FOO BUILDING\nLEVEL 69\n1234 SMITH STREET\nNEW FOO CITY ABC 123 XG 27";
				string actualAddress = NewPostalAddressFormatter(Factory, domesticRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "XYZ Forwarders\nNew Foo Building\nLevel 69\n1234 Smith Street\nNew Foo City ABC 123 XG 27";
				actualAddress = NewPostalAddressFormatter(Factory, domesticRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestFrenchAddress()
		{
			var frenchRecipient = InsertOrgHeader("A", "A", "1", "2", "PARIS", "", "12345", FrenchUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\n1\n2\n12345 PARIS\nFRANCE";
				string actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\n1\n2\n12345 PARIS\nFrance";
				actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestFrenchAddressWithAdditionalInfo()
		{
			var frenchRecipient = InsertOrgHeader("A", "A", "1", "2", "PARIS", "", "12345", FrenchUNLOCO, Constants.Languages.French);
			frenchRecipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\nFAKE BUILDING\n1\n2\n12345 PARIS\nFRANCE";
				string actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\nFake Building\n1\n2\n12345 PARIS\nFrance";
				actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestFrenchAddressWithoutPostCode()
		{
			var frenchRecipient = InsertOrgHeader("A", "A", "1", "2", "PARIS", "", "", FrenchUNLOCO, Constants.Languages.French);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\n1\n2\nPARIS\nFRANCE";
				string actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\n1\n2\nPARIS\nFrance";
				actualAddress = NewPostalAddressFormatter(Factory, frenchRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestSpanishAddress()
		{
			var spanishRecipient = InsertOrgHeader("A", "A", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO, Constants.Languages.Spanish);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nESPAÑA";
				string actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\n1\n2\n1234 ARANJUEZ (MADRID)\nEspaña";
				actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestSpanishAddressWithAdditionalInfo()
		{
			var spanishRecipient = InsertOrgHeader("A", "A", "1", "2", "ARANJUEZ", "MADRID", "1234", SpanishUNLOCO, Constants.Languages.Spanish);
			spanishRecipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\nFAKE BUILDING\n1\n2\n1234 ARANJUEZ (MADRID)\nESPAÑA";
				string actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\nFake Building\n1\n2\n1234 ARANJUEZ (MADRID)\nEspaña";
				actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestSpanishAddressWithoutProvince()
		{
			var spanishRecipient = InsertOrgHeader("A", "A", "1", "2", "ARANJUEZ", "", "1234", SpanishUNLOCO, Constants.Languages.Spanish);
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(false);
				string expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nESPAÑA";
				string actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
				Env.Registry.SetOrgAllowMixedCase(true);
				expectedAddress = "A\n1\n2\n1234 ARANJUEZ\nEspaña";
				actualAddress = NewPostalAddressFormatter(Factory, spanishRecipient, GlbCompany.CurrentCompany).PostalAddress();
				AssertEquals("Wrong address", expectedAddress, actualAddress);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestBritishAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Totternhoe", "Beds", "LU6 1AA", "GBDTE", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nTOTTERNHOE\nLU6 1AA\nUnited Kingdom";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestBritishAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Totternhoe", "Beds", "LU6 1AA", "GBDTE", Constants.Languages.EnglishAmerican);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nStation Approach\nTOTTERNHOE\nLU6 1AA\nUnited Kingdom";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestAussieAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Sydney", "NSW", "1234", "AUSYD", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nSydney NSW 1234";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestAussieAddressWithAdditonalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Sydney", "NSW", "1234", "AUSYD", Constants.Languages.EnglishAmerican);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nStation Approach\nSydney NSW 1234";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestUnitedArabEmiratesAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedArabEmirates));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Dubai", "No state, thanks", "N/A", "AEDXB", Constants.Languages.Arabic);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nDubai\nالامارات العربية المتحدة";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestCanuckAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Canada));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Villeville", "QC", "LU6 1AA", "CAYTO", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nVilleville QC LU6 1AA\nCanada";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestGermanAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Germany));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressPostCodeAndCityInCapitalsThenCountry;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Straße", "Station Approach", "Berlin", "Saxony", "BER1", "DEBER", Constants.Languages.German);
			string expectedAddress = "Daniel\n82 Fake Straße\nStation Approach\nBER1 BERLIN\nDeutschland";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestHongKongAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.HongKong));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Kwan Tai Ping Pong Tower, 900th Floor", "Hong Kong", "No state, thanks", "N/A", "HKHKG", Constants.Languages.ChineseSimplified);
			string expectedAddress = "Daniel\n82 Fake Street\nKwan Tai Ping Pong Tower, 900th Floor\nHong Kong\n香港";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestHongKongAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.HongKong));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityCountryNoPostcodeNoState;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Kwan Tai Ping Pong Tower, 900th Floor", "Hong Kong", "No state, thanks", "N/A", "HKHKG", Constants.Languages.ChineseSimplified);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nKwan Tai Ping Pong Tower, 900th Floor\nHong Kong\n香港";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestHungarianAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Hungary));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityFirstThenAddressPostCodeLast;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "Big happy woman street 69", "Valami", "Little Castle", "N/A", "1234", "HUBUD", Constants.Languages.Hungarian);
			string expectedAddress = "Daniel\nLittle Castle\nBig happy woman street 69\nValami\n1234\nMagyarország";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestHungarianAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Hungary));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityFirstThenAddressPostCodeLast;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "Big happy woman street 69", "Valami", "Little Castle", "N/A", "1234", "HUBUD", Constants.Languages.Hungarian);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nLittle Castle\nFake Building\nBig happy woman street 69\nValami\n1234\nMagyarország";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestJapaneseAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "123 Domo Arigoto Street", "Walkman Town", "Tokyo", "N/A", "1234", "JPTYO", Constants.Languages.Japanese);
			string expectedAddress = "Daniel\n123 Domo Arigoto Street\nWalkman Town, TOKYO\n1234 日本";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestJapaneseAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "123 Domo Arigoto Street", "Walkman Town", "Tokyo", "N/A", "1234", "JPTYO", Constants.Languages.Japanese);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n123 Domo Arigoto Street\nWalkman Town, TOKYO\n1234 日本";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestJapaneseAddressWithoutSecondLIne()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressThenSecondAddressAndCityInCapitalsFinallyPostCodeAndCountryTogether;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "123 Domo Arigoto Street", "", "Tokyo", "N/A", "1234", "JPTYO", Constants.Languages.Japanese);
			string expectedAddress = "Daniel\n123 Domo Arigoto Street\nTOKYO\n1234 日本";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestPostcodeStateCityAddressCountry()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeStateCityAddressCountry;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "123 Domo Arigoto Street", "Walkman Town", "Minato", "Tokyo", "1234", "JPTYO", Constants.Languages.Japanese);
			var expectedAddress = "Daniel\n1234\nTokyo Minato 123 Domo Arigoto Street Walkman Town\n日本";
			var actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestPostcodeStateCityAddressCountryWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Japan));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeStateCityAddressCountry;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "123 Domo Arigoto Street", "Walkman Town", "Minato", "Tokyo", "1234", "JPTYO", Constants.Languages.Japanese);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Building";
			var expectedAddress = "Daniel\n1234\nTokyo Minato 123 Domo Arigoto Street Walkman Town\nBuilding\n日本";
			var actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestMalaysianAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Malaysia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeCityCommaState;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Kuala Lumpur", "Some state", "12345", "MYKUL", Constants.Languages.Malay);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\n12345 KUALA LUMPUR, SOME STATE\nMalaysia";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestMalaysianAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Malaysia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.PostcodeCityCommaState;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Kuala Lumpur", "Some state", "12345", "MYKUL", Constants.Languages.Malay);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nStation Approach\n12345 KUALA LUMPUR, SOME STATE\nMalaysia";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestNuZillandAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.NewZealand));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressCityInCapitalsAndPostCodeThenCountry;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Aukland", "Haka Haka State", "123465", "NZAKL", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nAUKLAND 123465\nNew Zealand";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSingaporeAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Singapore));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.Singapore;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Singapore", "Singapore", "12346", "SGSIN", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nSINGAPORE 12346\nREP. OF SINGAPORE";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSingaporeAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Singapore));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.Singapore;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Singapore", "Singapore", "12346", "SGSIN", Constants.Languages.EnglishAmerican);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nStation Approach\nSINGAPORE 12346\nREP. OF SINGAPORE";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestUSAAddress()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Atlanta", "GA", "12346", "USATL", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nStation Approach\nAtlanta GA 12346\nUnited States";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestUSAAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeAllOnOneLine;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Station Approach", "Atlanta", "GA", "12346", "USATL", Constants.Languages.EnglishAmerican);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nStation Approach\nAtlanta GA 12346\nUnited States";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSouthAfricaAddressWithCity()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.SouthAfrica));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Smallville", "Joburg", "XX", "12346", "ZAJNB", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nSMALLVILLE\nJoburg\n12346\nSouth Africa";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSouthAfricaAddressWithoutCity()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.SouthAfrica));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Smallville", "", "XX", "12346", "ZAJNB", Constants.Languages.EnglishAmerican);
			string expectedAddress = "Daniel\n82 Fake Street\nSMALLVILLE\n12346\nSouth Africa";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestSouthAfricaAddressWithAdditionalInfo()
		{
			Environment.Env.Registry.SetOrgAllowMixedCase(true);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.SouthAfrica));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.AddressSuburbInCaptialsOptionalCityPostcode;
			Factory.Save();
			var recipient = InsertOrgHeader("Daniel", "DAN", "82 Fake Street", "Smallville", "", "XX", "12346", "ZAJNB", Constants.Languages.EnglishAmerican);
			recipient.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "Fake Building";
			string expectedAddress = "Daniel\nFake Building\n82 Fake Street\nSMALLVILLE\n12346\nSouth Africa";
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		public void TestPCS()
		{
			AssertFormattingRule(CountryAddressFormattingRuleList.Codes.PostcodeCityStateCountry, "Kiev Rus\n1 Kreschatik\nHoromy\n123456 Kiev\nKievskiy\nUkraine".ToUpper());
		}

		public void TestCPO()
		{
			AssertFormattingRule(CountryAddressFormattingRuleList.Codes.CityPostcodeCountry, "Kiev Rus\n1 Kreschatik\nHoromy\nKiev\n123456\nUkraine".ToUpper());
		}

		public void TestCSP()
		{
			AssertFormattingRule(CountryAddressFormattingRuleList.Codes.CityStatePostcodeCountry, "Kiev Rus\n1 Kreschatik\nHoromy\nKiev\nKievskiy\n123456\nUkraine".ToUpper());
		}

		public void TestCST()
		{
			AssertFormattingRule(CountryAddressFormattingRuleList.Codes.CityStateAsOneLinePostcodeCountry, "Kiev Rus\n1 Kreschatik\nHoromy\nKiev, Kievskiy\n123456\nUkraine".ToUpper());
		}

		void AssertFormattingRule(string rule, string expectedAddress)
		{
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Ukraine));
			country.RN_AddressFormattingRule = rule;
			RefCountryStates state = Factory.New<RefCountryStates>();
			state.RW_RN_NKCountryCode = country.RN_Code;
			state.RW_Code = "KKK";
			state.RW_Description = "Kievskiy";
			Factory.Save();
			var recipient = InsertOrgHeader("Kiev Rus", "KRS", "1 Kreschatik", "Horomy", "Kiev", "KKK", "123456", "UAIEV", Constants.Languages.EnglishAmerican);
			string actualAddress = NewPostalAddressFormatter(Factory, recipient, GlbCompany.CurrentCompany).PostalAddress();
			AssertEquals("Wrong address", expectedAddress, actualAddress);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			AustralianUNLOCO = GetUNLOCOInCountry(Core.Constants.CountryCodes.Australia);
			CanadianUNLOCO = GetUNLOCOInCountry(Core.Constants.CountryCodes.Canada);
			FrenchUNLOCO = GetUNLOCOInCountry(Core.Constants.CountryCodes.France);
			SpanishUNLOCO = GetUNLOCOInCountry(Core.Constants.CountryCodes.Spain);
		}

		string GetUNLOCOInCountry(string countryCode)
		{
			ZQuery countryCodeFilter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(countryCodeFilter);
			return uNLOCO.RL_Code;
		}

		string AustralianUNLOCO;
		string CanadianUNLOCO;
		string FrenchUNLOCO;
		string SpanishUNLOCO;

		OrgHeader InsertOrgHeader(string name, string code, string address1, string address2, string city, string state, string postCode, string uNLOCO, string language)
		{
			OrgHeader newOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			newOrg.OH_FullName = name;
			newOrg.OH_Code = code;
			newOrg.MainAddress.OA_Address1 = address1;
			newOrg.MainAddress.OA_Address2 = address2;
			newOrg.MainAddress.OA_City = city;
			newOrg.MainAddress.OA_PostCode = postCode;
			newOrg.OH_RL_NKClosestPort = uNLOCO;
			newOrg.MainAddress.OA_State = state;
			newOrg.MainAddress.OA_Language = language;
			Factory.Save();
			return newOrg;
		}

		protected virtual AddressFormatter NewPostalAddressFormatter(BusinessObjectFactory factory, OrgHeader recipientOrganisation, GlbCompany senderCompany)
		{
			return new AddressFormatter(factory, recipientOrganisation, senderCompany, false);
		}

		#endregion
	}
}
