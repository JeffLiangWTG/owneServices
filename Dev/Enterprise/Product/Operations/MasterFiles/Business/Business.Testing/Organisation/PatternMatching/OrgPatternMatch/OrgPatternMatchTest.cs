using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatch))]
	sealed class OrgPatternMatchTest : EnterpriseBusinessObjectTestCase
	{
		#region Notes Behaviour

		public void TestSupportsNotes()
		{
			AssertEquals(false, Factory.New<OrgPatternMatch>().SupportsNotes);
		}

		#endregion

		#region Encoding Tests

		public void TestEncodePort()
		{
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Port", "AUSYD", OrgMatch.OS_UNLOCO);

			org.OH_RL_NKClosestPort = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Port should be empty", String.Empty, OrgMatch.OS_UNLOCO);

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Blah";
			address2.OA_Address2 = "Blah again";
			address2.OA_City = "City core values";
			address2.OA_RL_NKRelatedPortCode = "HKHKG";

			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), address2, String.Empty);
			AssertEquals("Port should be HK", "HKHKG", OrgMatch.OS_UNLOCO);

			address2.OA_RL_NKRelatedPortCode = String.Empty;

			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), address2, String.Empty);
			AssertEquals("Port should be empty", String.Empty, OrgMatch.OS_UNLOCO);
		}

		public void TestEncodingSetsOrgAndAddressReferences()
		{
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("OrgPK should be set", org.PK, OrgMatch.OS_OH);
			AssertEquals("AddressPK should be set", org.MainAddress.PK, OrgMatch.OS_OA);
		}

		public void TestCompanyNameEncode()
		{
			AssertNameEncoding("TEST ORGANISATION", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("test organisation", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST PTY ORGANISATION", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST ORGANISATION CO", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST, ORGANISATION. CO.", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST,ORGANISATION. CO.", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST ORGANISATION (SECONDARY) CO.", "TEST", "ORGANISATION", "SECONDARY", String.Empty);
			AssertNameEncoding("TEST ORGANISATION (AUSTRALIA) CO.", "TEST", "ORGANISATION", "AUSTRALIA", String.Empty);
			AssertNameEncoding("TEST -ORGANISATION- CO.", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("1ST NATIONAL REAL ESTATE PTY LTD", "1ST", "NATIONAL", "REAL", "ESTATE");
			AssertNameEncoding("TEST ORGANISATION PTY LTD", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("TEST ORGANISATION PTY.LTD.", "TEST", "ORGANISATION", String.Empty, String.Empty);
			AssertNameEncoding("THE DAILY JUICE CO", "DAILY", "JUICE", String.Empty, String.Empty);
			AssertNameEncoding("BOB & SON", "BOB", "SON", String.Empty, String.Empty);
			AssertNameEncoding(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
			AssertNameEncoding("TEST ORGANISATION WITH MORE THAN FOUR WORDS", "TEST", "ORGANISATION", "WITH", "MORE");
		}

		public void TestCompanyNameEncodeWithUNLOCO()
		{
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertNameEncoding(Constants.Languages.EnglishAmerican, "TEST ORGANISATION (AUSTRALIA) KIEV", "TEST", "ORGANISATION", "AUSTRALIA", "KIEV");

			org.OH_RL_NKClosestPort = "UAIEV";
			AssertNameEncoding(Constants.Languages.EnglishAmerican, "TEST ORGANISATION (AUSTRALIA) KIEV", "TEST", "ORGANISATION", "AUSTRALIA", String.Empty);

			org.OH_RL_NKClosestPort = "GBLON";
			AssertNameEncoding(Constants.Languages.EnglishAmerican, "TEST ORGANISATION (AUSTRALIA) KIEV", "TEST", "ORGANISATION", "AUSTRALIA", "KIEV");
		}

		public void TestCompanyNameEncodeGerman()
		{
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "muller l�denscheidt", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER GMBH L�DENSCHEIDT", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT CO", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER, L�DENSCHEIDT. CO.", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER,L�DENSCHEIDT. CO.", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT (SECONDARY) CO.", "MULLER", "L�DENSCHEIDT", "SECONDARY", String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT (DEUTSCHE) CO.", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER -L�DENSCHEIDT- CO.", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "1ST NATIONAL REAL ESTATE GMBH KG", "1ST", "NATIONAL", "REAL", "ESTATE");
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT GMBH KG", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT GMBH.KG.", "MULLER", "L�DENSCHEIDT", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "DIE DAILY JUICE CO", "DAILY", "JUICE", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "BOB & SON", "BOB", "SON", String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
			AssertNameEncoding(Constants.Languages.German, "MULLER L�DENSCHEIDT WITH MORE THAN FOUR WORDS", "MULLER", "L�DENSCHEIDT", "WITH", "MORE");
		}

		public void TestFullCompanyNameEncode()
		{
			AssertFullCompanyName("ACME FIREWORKS", "ACME FIREWORKS", "Full name should be encoded");
			AssertFullCompanyName("ACME,FIREWORKS", "ACME FIREWORKS", "Full name should be encoded");
			AssertFullCompanyName("BOB & SON", "BOB SON", "& should be removed, and spaces compressed");
			AssertFullCompanyName("BOB INC SON", "BOB SON", "INC should be removed, and spaces compressed");
			AssertFullCompanyName("ACME FIREWORKS CO", "ACME FIREWORKS", "Ignored words should be removed");
			AssertFullCompanyName("ACME FIREWORKS PTY.LTD.", "ACME FIREWORKS", "Ignored words should be removed");
			AssertFullCompanyName("acme fireworks", "ACME FIREWORKS", "Case should be ignored");
			AssertFullCompanyName(String.Empty, String.Empty, "Empty should be encoded as empty");
			AssertFullCompanyName("  ", String.Empty, "Empty should be encoded as empty");
		}

		public void TestAddressEncode()
		{
			AssertAddressEncoding(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
			AssertAddressEncoding("58 WESTON DRIVE", "ALEXANDRIA", "WESTON", "ALEXANDRIA", String.Empty, String.Empty);
			AssertAddressEncoding("58 WESTON DRIVE ALEXANDRIA", String.Empty, "WESTON", "ALEXANDRIA", String.Empty, String.Empty);
			AssertAddressEncoding("58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("PO BOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("PO BOX NO 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("PO BOX NO. 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("P.O. BOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("P.O.BOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("POBOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("G.P.O.BOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");
			AssertAddressEncoding("GPOBOX 58 western green bay drive", "alexandria", "WESTERN", "GREEN", "BAY", "ALEXANDRIA");

			foreach (var ignoredAddressWord in OrgPatternMatchGenerationHelper.Get(org.MainAddress.OA_Language, org.MainAddress.PortName, org.MainAddress.CountryName).IgnoredAddressWords)
			{
				CheckAddressWithIgnoredWords(org, OrgMatch, ignoredAddressWord);
			}
		}

		public void TestPOBox()
		{
			AssertPOBox("PO 1234", true);
			AssertPOBox("GPO 1234", true);
			AssertPOBox("PO Box 1234", true);
			AssertPOBox("POBox 1234", true);
			AssertPOBox("P.O.Box 1234", true);
			AssertPOBox("GPOBox 1234", true);
			AssertPOBox("G.P.O.Box 1234", true);
			AssertPOBox("GPO Box 1234", true);
			AssertPOBox("P.O. 1234", true);
			AssertPOBox("P.O. Box 1234", true);
			AssertPOBox("G.P.O. Box 1234", true);
			AssertPOBox("P.O 1234", true);
			AssertPOBox("P.O Box 1234", true);
			AssertPOBox("G.P.O Box 1234", true);
			AssertPOBox("Post office Box 1234", true);
			AssertPOBox("Post-office Box 1234", true);
			AssertPOBox("Postoffice Box 1234", true);

			AssertPOBox("1234 Post street", false);
			AssertPOBox("1234 Post-office road", false);
		}

		public void TestStreetNumber()
		{
			EncodeAddress("158 COPPERHEAD ROAD", String.Empty);
			AssertEquals("Street number should be extracted", "158", OrgMatch.OS_StreetNumber);

			EncodeAddress("58/29 TELEGRAPH ROAD", String.Empty);
			AssertEquals("Street number with slash should be detected", "58/29", OrgMatch.OS_StreetNumber);

			EncodeAddress("58A TEST ROAD", String.Empty);
			AssertEquals("Street number with letter should be detected", "58A", OrgMatch.OS_StreetNumber);

			// Build up a street number that will be longer than the street number field
			org.MainAddress.OA_Address1 = "58";
			while (org.MainAddress.OA_Address1.Length <= OrgMatch.OS_StreetNumberInfo.MaxLength)
			{
				org.MainAddress.OA_Address1 += "VERYLONGADDRESS";
			}

			var expectedStreetNumber = org.MainAddress.OA_Address1.Substring(0, OrgMatch.OS_StreetNumberInfo.MaxLength);
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Street number with letter should be detected", expectedStreetNumber, OrgMatch.OS_StreetNumber);

			EncodeAddress("NO STREET NUMBER HERE", String.Empty);
			AssertEquals("Street number should be empty", String.Empty, OrgMatch.OS_StreetNumber);

			EncodeAddress(" 158 test drive", String.Empty);
			EncodeAddress(" ", String.Empty);
			AssertEquals("Street number should be empty", String.Empty, OrgMatch.OS_StreetNumber);

			EncodeAddress(" UNIT 5/7", String.Empty);
			AssertEquals("Street number should be recognised after an ignored word", "5/7", OrgMatch.OS_StreetNumber);

			EncodeAddress(" 58 test drive", String.Empty);
			AssertEquals("Street number should be recognised after a leading space", "58", OrgMatch.OS_StreetNumber);
		}

		public void TestEncodePhoneAndFaxNumber()
		{
			org.MainAddress.OA_Phone = "(02) 1234 BOB";
			org.MainAddress.OA_Fax = org.MainAddress.OA_Phone;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("All non numeric characters should be stripped from the phone number", "021234", OrgMatch.OS_Phone);
			AssertEquals("All non numeric characters should be stripped from the fax number", "021234", OrgMatch.OS_FaxNum);

			org.MainAddress.OA_Phone = "(12) 3456+7(8)*9 BOB";
			org.MainAddress.OA_Fax = org.MainAddress.OA_Phone;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Phone number should be the last seven digits", "3456789", OrgMatch.OS_Phone);
			AssertEquals("Fax number should be the last seven digits", "3456789", OrgMatch.OS_FaxNum);

			org.MainAddress.OA_Phone = String.Empty;
			org.MainAddress.OA_Fax = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Phone should be empty", String.Empty, OrgMatch.OS_Phone);
			AssertEquals("Fax should be empty", String.Empty, OrgMatch.OS_FaxNum);
		}

		public void TestEncodeEmailAndDomain()
		{
			AssertEmailAndDomain("testuser@testdomain.com", "TESTUSER", "TESTDOMAIN.COM");
			AssertEmailAndDomain("testuser", "TESTUSER", String.Empty);
			AssertEmailAndDomain("@domain.com", String.Empty, "DOMAIN.COM");
			AssertEmailAndDomain("user@dom.edi", "USER", "DOM.EDI");
		}

		public void TestPOBoxNumber()
		{
			AssertPOBoxNumber("PO BOX 1234", "1234");
			AssertPOBoxNumber("PO BOX NO 1234", "1234");
			AssertPOBoxNumber("PO BOX NO. 1234", "1234");
			AssertPOBoxNumber("PO BOX NO.1234", "1234");
			AssertPOBoxNumber("PO BOX NO1234", "1234");
			AssertPOBoxNumber("GPO 1234", "1234");
			AssertPOBoxNumber("PO Box 1234", "1234");
			AssertPOBoxNumber("GPO Box 1234", "1234");
			AssertPOBoxNumber("P.O. 1234", "1234");
			AssertPOBoxNumber("P.O. Box 1234", "1234");
			AssertPOBoxNumber("G.P.O. Box 1234", "1234");
			AssertPOBoxNumber("P.O 1234", "1234");
			AssertPOBoxNumber("P.O Box 1234", "1234");
			AssertPOBoxNumber("G.P.O Box 1234", "1234");
			AssertPOBoxNumber("Post office Box 1234", "1234");
			AssertPOBoxNumber("Post-office Box 1234", "1234");
			AssertPOBoxNumber("Postoffice Box 1234", "1234");

			AssertPOBoxNumber("P.O., Box 1234", "1234");
			AssertPOBoxNumber("P.O, Box 1234", "1234");
			AssertPOBoxNumber("P.O,Box 1234", "1234");
			AssertPOBoxNumber("P.O,Box, 1234", "1234");
			AssertPOBoxNumber("P.O,Box,1234", "1234");
			AssertPOBoxNumber("POBox 1234", "1234");
			AssertPOBoxNumber("GPOBox 1234", "1234");
			AssertPOBoxNumber("P.O.Box 1234", "1234");
			AssertPOBoxNumber("G.P.O.Box 1234", "1234");
			AssertPOBoxNumber("PO    Box 1234", "1234");
			AssertPOBoxNumber("PO  Box   1234", "1234");

			AssertPOBoxNumber("1234 Post street", String.Empty);
			AssertPOBoxNumber("1234 Post-office road", String.Empty);
			AssertPOBoxNumber("PO", String.Empty);
			AssertPOBoxNumber("PO BOX", String.Empty);
			AssertPOBoxNumber("PO BOX ", String.Empty);
			AssertPOBoxNumber("PO BOX  ", String.Empty);
			AssertPOBoxNumber("PO BOX ALEXANDRIA", String.Empty);
			AssertPOBoxNumber(String.Empty, String.Empty);

			var address = "PO BOX " + String.Empty.PadRight(OrgMatch.OS_POBoxNumberInfo.MaxLength + 1, '1');
			var expectedPOBoxNumber = String.Empty.PadRight(OrgMatch.OS_POBoxNumberInfo.MaxLength, '1');
			AssertPOBoxNumber(address, expectedPOBoxNumber);
		}

		public void TestIsCompany()
		{
			AssertIsCorporation("DAILY JUICE CO", true, "Should be a corporation");
			AssertIsCorporation("DAILY JUICE CO.", true, "Should be a corporation");
			AssertIsCorporation("DAILY JUICE INC", true, "Should be a corporation");
			AssertIsCorporation("DAILY JUICE LTD", true, "Should be a corporation");
			AssertIsCorporation("DAILY JUICE", false, "Should not be a corporation");
		}

		public void TestBusinessRegNo()
		{
			AssertBusinessRegNo("123 456 789", "123456789", "Spaces should be removed");
			AssertBusinessRegNo("123 BOB 789", "123BOB789", "Letters should remain");
			AssertBusinessRegNo(" ", String.Empty, "All spaces should be encoded to empty");
			AssertBusinessRegNo("    ", String.Empty, "All spaces should be encoded to empty");
		}

		public void TestPostCode()
		{
			AssertPostCode("1234", "1234", "Postcode should be encoded");
			AssertPostCode("1 234", "1234", "Spaces should be removed");
			AssertPostCode("1234 BOB", "1234BOB", "Letters should remain");
			AssertPostCode(" ", String.Empty, "All spaces should be encoded to empty");
			AssertPostCode("    ", String.Empty, "All spaces should be encoded to empty");
		}

		public void TestCity()
		{
			AssertCity("SYDNEY", "SYDNEY", "City should be encoded");
			AssertCity("sydney", "SYDNEY", "Case should be irrelevant");
			AssertCity("LOS ANGELES", "LOS ANGELES", "City should be encoded");
			AssertCity("L.A.", "LA", "Full stops should be removed");
			AssertCity("A, B", "A B", "Commas should be removed");
			AssertCity(String.Empty, String.Empty, "Empty should be left empty");
			AssertCity(" ", String.Empty, "All spaces should be encoded to empty");
			AssertCity("    ", String.Empty, "All spaces should be encoded to empty");
		}

		public void TestState()
		{
			AssertCity("NSW", "NSW", "State should be encoded");
			AssertCity("nsw", "NSW", "Case should be irrelevant");
			AssertCity("NEW SOUTH WALES", "NEW SOUTH WALES", "State should be encoded");
			AssertCity("N.S.W.", "NSW", "Full stops should be removed");
			AssertCity("NSW, AUSTRALIA", "NSW AUSTRALIA", "Commas should be removed");
			AssertCity(String.Empty, String.Empty, "Empty should be left empty");
			AssertCity(" ", String.Empty, "All spaces should be encoded to empty");
			AssertCity("    ", String.Empty, "All spaces should be encoded to empty");
		}

		#endregion

		#region Filter Tests

		public void TestGetFilter()
		{
			Assert("Filter should be empty", OrgMatch.GetFilter().IsEmpty);

			OrgMatch.OS_Address1 = "1";
			OrgMatch.OS_Address2 = "2";
			OrgMatch.OS_Address3 = "3";
			OrgMatch.OS_Address4 = "4";
			var filter = OrgMatch.GetFilter();
			var expectedFilter = "OS_Address1 = '1' or OS_Address2 = '2' or OS_Address3 = '3' or OS_Address4 = '4'";
			AssertFilter("Filter should have addresses", expectedFilter);

			OrgMatch.OS_Address1 = String.Empty;
			OrgMatch.OS_Address2 = String.Empty;
			OrgMatch.OS_Address3 = String.Empty;
			OrgMatch.OS_Address4 = String.Empty;
			OrgMatch.OS_UNLOCO = String.Empty;
			filter = OrgMatch.GetFilter();
			expectedFilter = String.Empty;
			AssertFilter("ExcludeOrgsNotInSameCountry should return nothing", expectedFilter);

			OrgMatch.OS_UNLOCO = "AUSYD";
			filter = OrgMatch.GetFilter();
			expectedFilter = "OS_UNLOCO = 'AUSYD' or OS_UNLOCO = '' or OS_UNLOCO like 'AU%'";
			AssertFilter("Port should be in the filter, and ExcludeOrgsNotInSameCountry should include Country and blank check", expectedFilter);

			OrgMatch.OS_Address1 = "Test";
			OrgMatch.OS_Address2 = "A123";
			filter = OrgMatch.GetFilter();
			expectedFilter = "(OS_Address1 = 'Test' or OS_Address2 = 'A123') and (OS_UNLOCO = 'AUSYD' or OS_UNLOCO = '' or OS_UNLOCO like 'AU%')";
			AssertFilter("Filter should contain port and address", expectedFilter);
		}

		public void TestCompanyNameFilter()
		{
			OrgMatch.OS_CompanyName1 = "A111";
			OrgMatch.OS_CompanyName2 = "A222";
			OrgMatch.OS_CompanyName3 = "A333";
			OrgMatch.OS_CompanyName4 = "A444";

			var expectedFilter = "OS_CompanyName1 = 'A111' or OS_CompanyName2 = 'A222' or OS_CompanyName3 = 'A333' or OS_CompanyName4 = 'A444'";
			var filter = OrgMatch.GetFilter();

			AssertFilter("Filter should contain all four company name values", expectedFilter);

			OrgMatch.OS_CompanyName3 = String.Empty;
			OrgMatch.OS_CompanyName4 = String.Empty;
			expectedFilter = "OS_CompanyName1 = 'A111' or OS_CompanyName2 = 'A222'";
			filter = OrgMatch.GetFilter();

			AssertFilter("Filter should contain only first two company name fields", expectedFilter);

			OrgMatch.OS_CompanyName1 = String.Empty;
			OrgMatch.OS_CompanyName2 = String.Empty;
			filter = OrgMatch.GetFilter();

			AssertFilter("No filter should be present with no company information", String.Empty);
		}

		public void TestCompanyAndAddressFilter()
		{
			var generator = new SimilarOrgsParameterGenerator(new Hashtable());
			OrgMatch.OS_CompanyName1 = "A111";
			OrgMatch.OS_CompanyName2 = "A222";
			OrgMatch.OS_CompanyName3 = "A333";
			OrgMatch.OS_CompanyName4 = "A444";

			OrgMatch.OS_Address1 = "1";
			OrgMatch.OS_Address2 = "2";
			OrgMatch.OS_Address3 = "3";
			OrgMatch.OS_Address4 = "4";

			// Allow More matches
			var filter = OrgMatch.GetFilter(true);
			var expectedFilter = "(OS_CompanyName1 = 'A111' or OS_CompanyName2 = 'A222' or OS_CompanyName3 = 'A333' or OS_CompanyName4 = 'A444') or (OS_Address1 = '1' or OS_Address2 = '2' or OS_Address3 = '3' or OS_Address4 = '4')";
			AssertFilter("Filter should have OR between Company and Address", expectedFilter);

			// Allow Fewer matches
			filter = OrgMatch.GetFilter(false);
			expectedFilter = "(OS_CompanyName1 = 'A111' or OS_CompanyName2 = 'A222' or OS_CompanyName3 = 'A333' or OS_CompanyName4 = 'A444') and (OS_Address1 = '1' or OS_Address2 = '2' or OS_Address3 = '3' or OS_Address4 = '4')";
			AssertEquals("Filter should have AND between Company and Address", expectedFilter, filter.LiteralTextADO);
		}

		public void TestFullCompanyNameFilter()
		{
			var generator = new SimilarOrgsParameterGenerator(new Hashtable());
			OrgMatch.OS_CompanyName1 = "A111";
			OrgMatch.OS_CompanyName2 = "A222";
			OrgMatch.OS_CompanyName3 = "A333";
			OrgMatch.OS_CompanyName4 = "A444";
			OrgMatch.OS_FullCompanyName = "Spacely Sprockets";

			OrgMatch.OS_Address1 = "1";
			OrgMatch.OS_Address2 = "2";
			OrgMatch.OS_Address3 = "3";
			OrgMatch.OS_Address4 = "4";

			// Allow More matches
			var filter = OrgMatch.GetFilter(true);
			var m1 = Factory.New<OrgPatternMatch>();
			m1.OS_CompanyName1 = OrgMatch.OS_CompanyName1;
			AssertEquals("Matches because OR filter", true, m1.MatchesFilter(filter));

			// Allow Fewer matches
			filter = OrgMatch.GetFilter(false);
			AssertEquals("Doesn't match because AND filter", false, m1.MatchesFilter(filter));
		}

		public void TestStreetNumberFilter()
		{
			OrgMatch.OS_StreetNumber = "58";
			AssertFilter("Street number not be used as filter", String.Empty);

			OrgMatch.OS_StreetNumber = String.Empty;
			AssertFilter("Street number not be used as filter", String.Empty);
		}

		public void TestPhoneAndFaxFilter()
		{
			OrgMatch.OS_Phone = "1234";
			OrgMatch.OS_FaxNum = String.Empty;
			var filter = OrgMatch.GetFilter();
			AssertFilter("Filter should contain phone number", "OS_Phone = '1234' or OS_FaxNum = '1234'");

			OrgMatch.OS_Phone = String.Empty;
			OrgMatch.OS_FaxNum = "12345";
			filter = OrgMatch.GetFilter();
			AssertFilter("Filter should contain fax number", "OS_FaxNum = '12345' or OS_Phone = '12345'");

			OrgMatch.OS_Phone = String.Empty;
			OrgMatch.OS_FaxNum = String.Empty;
			filter = OrgMatch.GetFilter();
			AssertFilter("Filter should be empty", String.Empty);

			OrgMatch.OS_Phone = "54321";
			OrgMatch.OS_FaxNum = "12345";
			filter = OrgMatch.GetFilter();
			AssertFilter("Filter should contain phone number", "OS_Phone = '54321' or OS_FaxNum = '54321' or OS_FaxNum = '12345' or OS_Phone = '12345'");

			OrgMatch.OS_Phone = "12345";
			OrgMatch.OS_FaxNum = "12345";
			filter = OrgMatch.GetFilter();
			AssertFilter("Filter should contain phone number", "OS_Phone = '12345' or OS_FaxNum = '12345'");
		}

		public void TestEmailAndDomainFilter()
		{
			OrgMatch.OS_Email = "testuser";
			OrgMatch.OS_Domain = "testdomain";
			var filter = OrgMatch.GetFilter();
			AssertFilter("Filter should contain email but not domain", "OS_Email = 'testuser'");

			OrgMatch.OS_Email = String.Empty;
			OrgMatch.OS_Domain = String.Empty;
			filter = OrgMatch.GetFilter();
			AssertFilter("Filter should be empty", String.Empty);

			OrgMatch.OS_Email = "test";
			OrgMatch.OS_Domain = String.Empty;
			filter = OrgMatch.GetFilter();
			AssertFilter("Email should be filtered", "OS_Email = 'test'");

			OrgMatch.OS_Email = String.Empty;
			OrgMatch.OS_Domain = "test";
			filter = OrgMatch.GetFilter();
			AssertFilter("Domain should be empty", String.Empty);
		}

		public void TestPOBoxFilter()
		{
			OrgMatch.OS_POBoxNumber = "1234";
			OrgMatch.OS_IsPOBox = true;
			var filter = OrgMatch.GetFilter();
			AssertFilter("POBoxNumber should not be used as filter", String.Empty);

			OrgMatch.OS_IsPOBox = true;
			OrgMatch.OS_POBoxNumber = String.Empty;
			filter = OrgMatch.GetFilter();
			AssertFilter("POBoxNumber should not be used as filter", String.Empty);

			OrgMatch.OS_IsPOBox = false;
			filter = OrgMatch.GetFilter();
			AssertFilter("POBoxNumber should not be used as filter", String.Empty);
		}

		public void TestIsCorporationFilter()
		{
			// IsCorporation should not be filtered on, because it would result in pulling a lot of rows from the DB
			// simply because the organisations were or weren't a corporation.
			OrgMatch.OS_IsCorporation = true;
			var filter = OrgMatch.GetFilter();
			AssertFilter("IsCorporation should not have a filter", String.Empty);

			OrgMatch.OS_IsCorporation = false;
			filter = OrgMatch.GetFilter();
			AssertFilter("IsCorporation should not have a filter", String.Empty);
		}

		public void TestBusinessRegNoFilter()
		{
			OrgMatch.OS_BusinessRegNo = "1234";
			AssertFilter("BusinessRegNo should be filtered", "OS_BusinessRegNo = '1234' or OS_BusinessRegNo = ''");

			OrgMatch.OS_BusinessRegNo = String.Empty;
			AssertFilter("BusinessRegNo should not be filtered", String.Empty);

			OrgMatch.OS_BusinessRegNo = " ";
			AssertFilter("BusinessRegNo should not be filtered", String.Empty);
		}

		public void TestPostCodeFilter()
		{
			OrgMatch.OS_PostCode = "1234";
			AssertFilter("Postcode should not be used as filter", String.Empty);

			OrgMatch.OS_PostCode = String.Empty;
			AssertFilter("Postcode should not be used as filter", String.Empty);

			OrgMatch.OS_PostCode = " ";
			AssertFilter("Postcode should not be used as filter", String.Empty);
		}

		public void TestCityFilter()
		{
			OrgMatch.OS_City = "A123";
			AssertFilter("City should not be used as filter", String.Empty);

			OrgMatch.OS_City = String.Empty;
			AssertFilter("City should not be used as filter", String.Empty);

			OrgMatch.OS_City = " ";
			AssertFilter("City should not be used as filter", String.Empty);
		}

		public void TestStateFilter()
		{
			OrgMatch.OS_State = "NSW";
			AssertFilter("State should not be used as filter", String.Empty);

			OrgMatch.OS_State = String.Empty;
			AssertFilter("State should not be used as filter", String.Empty);

			OrgMatch.OS_State = " ";
			AssertFilter("State should not be used as filter", String.Empty);
		}

		public void TestExcludeOrgsWithDifferentBusinessRegNoFilter()
		{
			OrgMatch.OS_BusinessRegNo = "123 123 123 12";
			var expectedResult = "OS_BusinessRegNo = '123 123 123 12' or OS_BusinessRegNo = ''";
			AssertFilter("ExcludeOrgsWithDifferentBusinessRegNo should check where BusinessRegNo is 123 123 123 12, OR is blank", expectedResult);

			OrgMatch.OS_BusinessRegNo = String.Empty;
			expectedResult = String.Empty;
			AssertFilter("ExcludeOrgsWithDifferentBusinessRegNo should return blank", expectedResult);
		}
		#endregion

		#region Score Tests

		public void TestStreetAddressWithBackSlash()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org2.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			org1.OH_FullName = "GEORGE";
			org2.OH_FullName = "GEORGE PTY LTD";

			org1.MainAddress.OA_Address1 = "UNIT 7\\45";
			org2.MainAddress.OA_Address1 = "NO 7/45";

			org1.MainAddress.OA_Address2 = "GEORGE STR";
			org2.MainAddress.OA_Address2 = "GEORGE STREET";

			org1.MainAddress.OA_City = "GEORGE TOWN";
			org2.MainAddress.OA_City = "GEORGETOWN";

			org1.MainAddress.OA_State = "NSW";
			org2.MainAddress.OA_State = "NEW SOUTH WALES";

			org1.MainAddress.OA_PostCode = "2211";
			org2.MainAddress.OA_PostCode = "2210";

			org1.MainAddress.OA_Phone = "02 66669999";
			org2.MainAddress.OA_Phone = "(02) 6666 9999";

			var match1 = org1.PatternMatchesForThisOrg.AddNew();
			var match2 = org2.PatternMatchesForThisOrg.AddNew();
			match1.EncodeOrganisation(org1, new StringWithLanguage(org1.OH_FullName, org1.OH_Language), org1.MainAddress, String.Empty);
			match2.EncodeOrganisation(org2, new StringWithLanguage(org2.OH_FullName, org2.OH_Language), org2.MainAddress, String.Empty);

			match1.SetMatchScoreForOrg(org2.PatternMatchesForThisOrg);
			AssertEquals("should match on street number making score of 175", 175, match1.OS_Score);
		}

		public void TestNameScore()
		{
			org.OH_FullName = "THE ROYAL ACME MEGA FIREWORKS COMPANY PTY LTD";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertNameScore("NIKE SHOE MAKERS", 0, "No words should match");
			AssertNameScore("ROYAL SHOES", firstNameWordSoundexScore, "First word only should match");
			AssertNameScore("ROYAL AKNE SHOES", firstNameWordSoundexScore + nameWordSoundexScore, "Two words should match");
			AssertNameScore("ROYAL AKNE SHOES AND FAREWORKS", firstNameWordSoundexScore + (2 * nameWordSoundexScore), "Three words should match");
			AssertNameScore("ROYAL AKNE MUGA FAREWORKS", firstNameWordSoundexScore + (3 * nameWordSoundexScore), "Four words should match");
			AssertNameScore("ROYAL ACME MEGA FIREWORKS INCORPORATED", nameExactScore, "Organisation name should be an exact match");
			AssertNameScore("royal acme mega fireworks incorporated", nameExactScore, "Organisation name should be an exact match");

			org.OH_FullName = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertNameScore(String.Empty, 0, "Exact match should not be found because no valid name exists");
		}

		public void TestNameScoreGerman()
		{
			org.OH_FullName = "Die M�llar Staiger MEGA Tscherny INSTITUT GMBH CO";
			org.OH_Language = Constants.Languages.German;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertNameScore("NIKE Schloss MAKERS", Constants.Languages.German, 0, "No words should match");
			AssertNameScore("M�ller Schloss", Constants.Languages.German, firstNameWordSoundexScore, "First word only should match");
			AssertNameScore("Mueller Steigr Schloss", Constants.Languages.German, firstNameWordSoundexScore + nameWordSoundexScore, "Two words should match");
			AssertNameScore("M�ller Steigr Schloss und Tscherny", Constants.Languages.German, firstNameWordSoundexScore + (2 * nameWordSoundexScore), "Three words should match");
			AssertNameScore("Muller Staigr MUGA Tschernie", Constants.Languages.German, firstNameWordSoundexScore + (3 * nameWordSoundexScore), "Four words should match");
			AssertNameScore("M�LLAR STAIGER MEGA TSCHERNY GMBH", Constants.Languages.German, nameExactScore, "Organisation name should be an exact match");
			AssertNameScore("m�llar staiger mega tscherny gmbh", Constants.Languages.German, nameExactScore, "Organisation name should be an exact match");

			org.OH_FullName = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertNameScore(String.Empty, 0, "Exact match should not be found because no valid name exists");
		}

		public void TestAddressScore()
		{
			EncodeAddress("158 GREENTREE FOREST PARADE", String.Empty);
			AssertAddressScore("58 BLACK STUMP DRIVE", String.Empty, 0, "No words should match");
			AssertAddressScore("158 BLACK STUMP DRIVE", String.Empty, streetNumberScore, "Street number should match");
			AssertAddressScore("58 GRENTREY STUMP DRIVE", String.Empty, firstAddressWordSoundexScore, "First word should match");

			AssertAddressScore("158 GRENTREY STUMP DRIVE", String.Empty, firstAddressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "First word, plus street number, plus street number with first address word bonus");
			AssertAddressScore("158 GRENTRE FORREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is misspelled, still should not be an exact soundex match as not enough address parts");
			AssertAddressScore("158 APPLE FOREST DRIVE", String.Empty, streetNumberScore + addressWordSoundexScore, "Street number and first address word bonus should not be given because the first address word does not match");

			AssertAddressScore("158 GREENTREE FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is not an exact match as not enough address parts");
			AssertAddressScore("158, GREENTREE FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is not an exact match as not enough address parts");
			AssertAddressScore("158. GREENTREE FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is not an exact match as not enough address parts");
			AssertAddressScore("158., GREENTREE FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is not an exact match as not enough address parts");
			AssertAddressScore("158 GREENTREE, FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "Address is not an exact match as not enough address parts");
			AssertAddressScore("58 GREENTREE FOREST DRIVE", String.Empty, firstAddressWordSoundexScore + addressWordSoundexScore, "Only street name matches, street number does not match, so this should not be an exact match");

			EncodeAddress("158 GREENTREE PARADE CITY CENTRE", "PARRAMATTA");
			AssertAddressScore("58 GREENTREE DRIVE", "CITY", firstAddressWordSoundexScore + addressWordSoundexScore, "Two words should match");
			AssertAddressScore("58 GREENTREE DRIVE", "CITY CENTRE", firstAddressWordSoundexScore + (2 * addressWordSoundexScore), "Three words should match");
			AssertAddressScore("58 GREENTREE DRIVE", "CITY CENTRE PARRAMATTA", firstAddressWordSoundexScore + (3 * addressWordSoundexScore), "Four words should match");
			AssertAddressScore("158 Greentree parade city", "centre parramatta", addressExactScore, "Address is an exact match");

			EncodeAddress("PO BOX 58 GREENTREE PARADE CITY CENTRE", String.Empty);
			AssertAddressScore("GREENTREE PARADE CITY CENTRE", String.Empty, firstAddressWordSoundexScore + (2 * addressWordSoundexScore), "PO Box info should be removed, but should not be an exact match without the PO information");
			AssertAddressScore("GREENTREE", String.Empty, firstAddressWordSoundexScore, "PO Box info should be removed");

			EncodeAddress("GREENTREE PARADE CITY CENTRE", String.Empty);
			AssertAddressScore("PO BOX 58 GREENTREE PARADE CITY CENTRE", String.Empty, firstAddressWordSoundexScore + (2 * addressWordSoundexScore), "PO Box info should be removed, but should not be an exact match without the PO information");
			AssertAddressScore("PO BOX 58 GREENTREE", String.Empty, firstAddressWordSoundexScore, "PO Box info should be removed");

			EncodeAddress(" ", " ");
			AssertAddressScore(" PARADE ", " DRIVE ", 0, "No valid address exists, there should be no score");

			EncodeAddress("Strade Hornbach Nr. 1, Domnesti/Jud", " ");
			AssertAddressScore("Str. Bucegi Nr. 3 ", " ", 0, "Address should not match");
		}

		public void TestAddressExactMatch()
		{
			var adr1 = "kharkovskoe shosse 5/1, ap 47";
			var postcode = "02090";
			var city = "Kiev";
			var state = "Kievskiy";

			compareOrg.MainAddress.OA_Address1 = adr1;
			compareOrg.MainAddress.OA_PostCode = postcode;
			compareOrg.MainAddress.OA_City = city;
			compareOrg.MainAddress.OA_State = state;

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();
			org.MainAddress.OA_Address1 = adr1;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore, "first address line match");

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();
			org.MainAddress.OA_Address1 = adr1;
			org.MainAddress.OA_State = state;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore + stateSoundexScore, "first address line match, state match");

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();
			org.MainAddress.OA_Address1 = adr1;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postcode;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore + stateSoundexScore + postCodeScore, "first and second address line match, postcode match");

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();
			org.MainAddress.OA_Address1 = adr1;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postcode;
			org.MainAddress.OA_City = city;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(addressExactScore, "exact match - four parts supplied");

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();
			org.MainAddress.OA_Address1 = adr1;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postcode;
			org.MainAddress.OA_City = "zzz";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(firstAddressWordSoundexScore + addressWordSoundexScore + streetNumberScore + streetNumberFirstAddressWordScore + stateSoundexScore + postCodeScore, "first and second address line match, postcode match");
		}

		public void TestAddressWithCompanyName()
		{
			// Work Item WI00180203 - different addresses with same company name

			compareOrg.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;
			org.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			compareOrg.MainAddress.OA_Address1 = "Troensevej 16";
			org.MainAddress.OA_Address1 = "Luetticher Strasse 12";

			compareOrg.MainAddress.OA_City = "Aalborg";
			org.MainAddress.OA_City = "Triosdorf";

			compareOrg.MainAddress.OA_State = "";
			org.MainAddress.OA_State = "NW";

			compareOrg.MainAddress.OA_PostCode = "9220";
			org.MainAddress.OA_PostCode = "53842";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertScore(0, "addresses should not match, therefore score should be 0 at this stage");

			compareOrg.OH_FullName = "DSV Air & Sea A/S";
			org.OH_FullName = "DSV Air & Sea GMBH";

			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(OrgPatternMatch.MatchScores.NameExact, "When company name is added, this results in exact match - See WI00180203");
		}

		public void TestPortScore()
		{
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertPortScore(org.OH_RL_NKClosestPort, portScore, "Port should match");
			AssertPortScore(String.Empty, 0, "Port should not match");

			org.OH_RL_NKClosestPort = String.Empty;
			AssertPortScore(String.Empty, 0, "Two empties do not make a match!");
		}

		public void TestEmailScore()
		{
			org.MainAddress.OA_Email = "testuser@test.domain.com.au";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEmailScore("testuser@bigpond.com", emailNameScore, "User name should match");
			AssertEmailScore("testuser@", emailNameScore, "User name should match");
			AssertEmailScore("testuser", emailNameScore, "User name should match");

			AssertEmailScore("incorrect@test.domain.com.au", 0, "Domain name should match");
			AssertEmailScore("incorrect@test.domain.org", 0, "Domain name should not match");
			AssertEmailScore("@test.domain.com.au", 0, "Domain name should match");

			AssertEmailScore("testuser@test.domain.com.au", emailExactScore, "Exact match");

			AssertEmailScore("incorrect@incorrect.com.nz", 0, "No match");
			AssertEmailScore("incorrect@", 0, "No match");
			AssertEmailScore("@incorrect.com.nz", 0, "No match");
			AssertEmailScore("@", 0, "No match");
			AssertEmailScore(String.Empty, 0, "No match");
		}

		public void TestPhoneNumberScore()
		{
			var phoneNumber = "+61 (2) 9125 1100";
			org.MainAddress.OA_Phone = phoneNumber;
			org.MainAddress.OA_Fax = org.MainAddress.OA_Phone;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertPhoneSevenDigitsScore(phoneNumber, "91251100", "Last seven digits should match");
			AssertPhoneSevenDigitsScore(phoneNumber, "9 1 2 5 1 1 0 0", "Last seven digits should match");
			AssertPhoneSevenDigitsScore(phoneNumber, "+(9)1[2]5 TEST 1100", "Last seven digits should match");
			AssertPhoneSevenDigitsScore(phoneNumber, "71251100", "Last seven digits should match");
			AssertPhoneSevenDigitsScore(phoneNumber, "1251100", "Last seven digits should match");
			AssertPhoneScore(phoneNumber, "1100", 0, 0, "Not enough digits to make the match");

			AssertPhoneScore(phoneNumber, "9125 1192", 0, 0, "Phone number does not match");
			AssertPhoneScore(phoneNumber, String.Empty, 0, 0, "Phone number does not match");

			AssertPhoneScore(String.Empty, String.Empty, 0, 0, "Two empties do not make a match!");
			AssertPhoneScore(String.Empty, " ", 0, 0, "Two empties do not make a match!");
			AssertPhoneScore(String.Empty, " +()[test", 0, 0, "Two empties do not make a match!");
		}

		public void TestPOBoxScore()
		{
			EncodeAddress("PO BOX 58", String.Empty);
			AssertPOBoxScore("PO BOX 58", String.Empty, postOfficeBoxNumberScore, "Exact match");
			AssertPOBoxScore("PO 58", String.Empty, postOfficeBoxNumberScore, "PO Box number match");
			AssertPOBoxScore("GPO BOX 58", String.Empty, postOfficeBoxNumberScore, "PO Box number match");
			AssertPOBoxScore("POBOX 58", String.Empty, postOfficeBoxNumberScore, "PO Box number match");

			AssertPOBoxScore("PO BOX 158", String.Empty, postOfficeBoxScore, "PO Box number does not match, but both addresses are PO boxes");
		}

		public void TestCorporationScore()
		{
			org.OH_FullName = "ACME FIREWORKS CO";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertCorporationScore("DOG FOOD INC", corporationScore, "Only IsCorporation should match");
			AssertCorporationScore("DOG FOOD INC.", corporationScore, "Only IsCorporation should match");
			AssertCorporationScore("DOG FOOD CO AUSTRALIA ", corporationScore, "Only IsCorporation should match");

			AssertCorporationScore("ACME PTY LTD", corporationScore + firstNameWordSoundexScore, "One name word, plus IsCorporation should match");
			AssertCorporationScore("ACME FIREWORKS INC.", nameExactScore, "Should be an exact match");
			AssertCorporationScore(" THE", 0, "Corporation match should have no effect with an empty organisation name");

			org.OH_FullName = "ACME FIREWORKS KOMPANEE";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertCorporationScore("ACME FIREWORKS CO.", firstNameWordSoundexScore + nameWordSoundexScore, "Should not be an exact match because IsCorporation does not match");
			AssertCorporationScore("DOG FOOD CO.", 0, "Name and IsCorporation do not match.");
			AssertCorporationScore("ACME FIREWORKS KOMPANEE", nameExactScore, "Name and IsCorporation match.");
			AssertCorporationScore("ACME FIREWORKS", firstNameWordSoundexScore + nameWordSoundexScore, "Two name words match, but no IsCorporation score should be given when both organisations are not corporations.");

			AssertCorporationScore(String.Empty, 0, "Empty should not match");
			AssertCorporationScore(" ", 0, "Empty should not match");
		}

		public void TestPostCodeScore()
		{
			org.MainAddress.OA_PostCode = "1234";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertPostCodeScore("1234", postCodeScore, "Postcode should be an exact match");
			AssertPostCodeScore(" 1234", postCodeScore, "Postcode should be an exact match");
			AssertPostCodeScore("1234 ", postCodeScore, "Postcode should be an exact match");
			AssertPostCodeScore("1 2 3 4", postCodeScore, "Postcode should be an exact match");

			AssertPostCodeScore("1234 BOB", 0, "PostCode should not match");
			AssertPostCodeScore("12345", 0, "PostCode should not match");
			AssertPostCodeScore("123", 0, "PostCode should not match");
			AssertPostCodeScore(String.Empty, 0, "PostCode should not match");

			org.MainAddress.OA_PostCode = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertPostCodeScore(String.Empty, 0, "Two empties do not make a match");
			AssertPostCodeScore(" ", 0, "Two empties do not make a match");
		}

		public void TestBusinessRegNoScore()
		{
			org.PrimaryRegistrationNumber.Number = "123 456 789";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, org.PrimaryRegistrationNumber.Number);

			AssertBusinessRegNoScore("123456789", businessRegNoScore, "Should be an exact match");
			AssertBusinessRegNoScore("1234 5 6 7 8 9", businessRegNoScore, "Should be an exact match");
			AssertBusinessRegNoScore(" 123 456 789 ", businessRegNoScore, "Should be an exact match");

			AssertBusinessRegNoScore("123 456 789 BOB", 0, "Should not be a match");

			org.PrimaryRegistrationNumber.Number = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertBusinessRegNoScore(String.Empty, 0, "Two empties do not make a match");
			AssertBusinessRegNoScore(" ", 0, "Two empties do not make a match");
		}

		public void TestCityScore()
		{
			org.MainAddress.OA_City = "SYDNEY";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertCityScore("sydney", citySoundexScore, "Should be an exact match");
			AssertCityScore("syd,ney", citySoundexScore, "Should be an exact match");
			AssertCityScore("syd.ney", citySoundexScore, "Should be an exact match");

			org.MainAddress.OA_City = "LOS ANGELES";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertCityScore("los angeles", citySoundexScore, "Should be an exact match");
			AssertCityScore("sydney", 0, "Should not match");

			org.MainAddress.OA_City = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertCityScore(" ", 0, "Two empties do not make a match");
			AssertCityScore(String.Empty, 0, "Two empties do not make a match");
		}

		public void TestStateScore()
		{
			org.MainAddress.OA_State = "NSW";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertStateScore("nsw", stateSoundexScore, "Should be an exact match");
			AssertStateScore("N.S.W.", stateSoundexScore, "Should be an exact match");
			AssertStateScore("N,S,W,", stateSoundexScore, "Should be an exact match");

			org.MainAddress.OA_State = "New South Wales";
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertStateScore("NEW SOUTH WALES", stateSoundexScore, "Should be an exact match");
			AssertStateScore("victoria", 0, "Should not match");

			org.MainAddress.OA_State = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertStateScore(" ", 0, "Two empties do not make a match");
			AssertStateScore(String.Empty, 0, "Two empties do not make a match");
		}

		#endregion

		#region Org Header Properties

		public void TestOrgHeaderAddressTypesSummary()
		{
			var factory = new BusinessObjectFactory();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAHCOD";

			var address = org.MainAddress;
			address.AddAddressType(OrgAddressType.Office);
			address.AddAddressType(OrgAddressType.PickupAndDelivery);

			factory.Save();

			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, org.PrimaryRegistrationNumber.Number);

			AssertEquals("We should get a comma seperated list of the addresses capabilities.", "OFC, PAD", OrgMatch.AddressCapabilityCodes);
		}

		public void TestQuickViewCard()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAHCOD";
			org.OH_FullName = "Mah Company";

			var address = org.MainAddress;
			address.OA_Address1 = "69 Druitt St.";
			address.OA_Address2 = "";
			address.OA_City = "Mt Druitt";

			address.OA_RL_NKRelatedPortCode = "AUSYD";

			address.AddAddressType(OrgAddressType.Office);
			address.AddAddressType(OrgAddressType.PickupAndDelivery);

			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), address, org.PrimaryRegistrationNumber.Number);

			AssertEquals(
@"Name: Mah Company
Port: AUSYD
Address 1: 69 Druitt St.
Address 2: 
City: Mt Druitt
Capabilities:
 - Office Address
 - Consignment Pickup and Delivery Address", OrgMatch.QuickViewCard);

			address.AddressCapability.DisableAllCapabilities();

			AssertEquals(
@"Name: Mah Company
Port: AUSYD
Address 1: 69 Druitt St.
Address 2: 
City: Mt Druitt
Capabilities:
 - None", OrgMatch.QuickViewCard);
		}

		public void TestSavingCityProblem()
		{
			var newFactory = new BusinessObjectFactory();
			var testOrg = newFactory.New<OrgHeader>();
			testOrg.OH_FullName = "test city code org";
			testOrg.OH_RL_NKClosestPort = "AUSYD";

			testOrg.MainAddress.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			testOrg.MainAddress.OA_Address1 = OrgHeaderUnicodeTestConstants.ChineseAddress1;
			testOrg.MainAddress.OA_Address2 = OrgHeaderUnicodeTestConstants.ChineseAddress2;
			testOrg.MainAddress.OA_City = OrgHeaderUnicodeTestConstants.ChineseAddress2; // a long city name to cause a failure

			newFactory.Save();

			Assert("should be saved", testOrg.IsInDatabase);
		}

		public void TestOrgHeaderProperties()
		{
			AssertEquals("Code should be empty with no org set", String.Empty, OrgMatch.OH_Code);
			AssertEquals("Name should be empty with no org set", String.Empty, OrgMatch.OH_FullName);
			AssertEquals("Address1 should be empty with no org set", String.Empty, OrgMatch.OH_Calc_Address1);
			AssertEquals("Address2 should be empty with no org set", String.Empty, OrgMatch.OH_Calc_Address2);
			AssertEquals("City should be empty with no org set", String.Empty, OrgMatch.OH_Calc_City);
			AssertEquals("State should be empty with no org set", String.Empty, OrgMatch.OH_Calc_State);
			AssertEquals("Email should be empty with no org set", String.Empty, OrgMatch.OH_Calc_Email);
			AssertEquals("Phone should be empty with no org set", String.Empty, OrgMatch.OH_Calc_Phone);
			AssertEquals("Fax should be empty with no org set", String.Empty, OrgMatch.OH_Calc_Fax);
			AssertEquals("PostCode should be empty with no org set", String.Empty, OrgMatch.OH_Calc_PostCode);
			AssertEquals("BusinessRegNo should be empty with no org set", String.Empty, OrgMatch.LocalBusinessNumber);

			// OrgHeader testOrg = Factory.New<OrgHeader>();
			var testOrg = Factory.New<OrgHeader>();

			// ensure that all fields have values, as some could be empty
			testOrg.OH_Code = "code";
			testOrg.OH_FullName = "name";
			testOrg.OH_RL_NKClosestPort = "AUSYD";
			testOrg.PrimaryRegistrationNumber.Number = "businessregno";
			testOrg.MainAddress.OA_Address1 = "address1";
			testOrg.MainAddress.OA_Address2 = "address2";
			testOrg.MainAddress.OA_City = "city";
			testOrg.MainAddress.OA_State = "state";
			testOrg.MainAddress.OA_Email = "email";
			testOrg.MainAddress.OA_Phone = "phone";
			testOrg.MainAddress.OA_Fax = "fax";
			testOrg.MainAddress.OA_PostCode = "1234";
			OrgMatch.EncodeOrganisation(testOrg, new StringWithLanguage(testOrg.OH_FullName, testOrg.OH_Language), testOrg.MainAddress, testOrg.PrimaryRegistrationNumber.Number);
			AssertCompanyDetailsSet(testOrg, testOrg.MainAddress, OrgMatch);

			var address = testOrg.Addresses.AddNew();
			address.OA_Address1 = "alternative1";
			address.OA_Address2 = "alternatve2";
			address.OA_City = "altcity";
			address.OA_State = "altstate";
			address.OA_Email = "altemail";
			address.OA_Phone = "altphone";
			address.OA_Fax = "altfax";
			address.OA_PostCode = "4321";
			OrgMatch.EncodeOrganisation(testOrg, new StringWithLanguage(testOrg.OH_FullName, testOrg.OH_Language), address, testOrg.PrimaryRegistrationNumber.Number);
			AssertCompanyDetailsSet(testOrg, address, OrgMatch);
		}

		void AssertCompanyDetailsSet(OrgHeader testHeader, OrgAddress addressToCheck, OrgPatternMatch matchToCheck)
		{
			AssertEquals("Code should match loaded organisation", testHeader.OH_Code, matchToCheck.OH_Code);
			AssertEquals("Name should match loaded organisation", testHeader.OH_FullName, matchToCheck.OH_FullName);
			AssertEquals("Address1 should match loaded organisation", addressToCheck.OA_Address1, matchToCheck.OH_Calc_Address1);
			AssertEquals("Address2 should match loaded organisation", addressToCheck.OA_Address2, matchToCheck.OH_Calc_Address2);
			AssertEquals("City should match loaded organisation", addressToCheck.OA_City, matchToCheck.OH_Calc_City);
			AssertEquals("State should match loaded organisation", addressToCheck.OA_State, matchToCheck.OH_Calc_State);
			AssertEquals("Email should match loaded organisation", addressToCheck.OA_Email, matchToCheck.OH_Calc_Email);
			AssertEquals("Phone should match loaded organisation", addressToCheck.OA_Phone, matchToCheck.OH_Calc_Phone);
			AssertEquals("Fax should match loaded organisation", addressToCheck.OA_Fax, matchToCheck.OH_Calc_Fax);
			AssertEquals("BusinessRegNo should match loaded organisation", testHeader.PrimaryRegistrationNumber.Number, matchToCheck.LocalBusinessNumber);
			AssertEquals("PostCode should match loaded organisation", addressToCheck.OA_PostCode, matchToCheck.OH_Calc_PostCode);
		}

		public void TestMatchLikelihood()
		{
			var testMatch = Factory.New<OrgPatternMatch>();

			// Test Low Threshold boundaries
			testMatch.OS_Score = 0;
			AssertEquals("Should be Low", OrgMatchThresholds.Descriptions.Low, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.LowMatchThreshold - 1;
			AssertEquals("Should be Low", OrgMatchThresholds.Descriptions.Low, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.LowMatchThreshold;
			AssertEquals("Should be Low", OrgMatchThresholds.Descriptions.Low, testMatch.MatchLikelihood);

			// Test Meduim Threshold boundaries
			testMatch.OS_Score = OrgPatternMatchCollection.LowMatchThreshold + 1;
			AssertEquals("Should be Low", OrgMatchThresholds.Descriptions.Low, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.MediumMatchThreshold - 1;
			AssertEquals("Should be Low", OrgMatchThresholds.Descriptions.Low, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.MediumMatchThreshold;
			AssertEquals("Should be Medium", OrgMatchThresholds.Descriptions.Medium, testMatch.MatchLikelihood);

			// Test High Threshold boundaries
			testMatch.OS_Score = OrgPatternMatchCollection.MediumMatchThreshold + 1;
			AssertEquals("Should be Medium", OrgMatchThresholds.Descriptions.Medium, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.HighMatchThreshold - 1;
			AssertEquals("Should be Medium", OrgMatchThresholds.Descriptions.Medium, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.HighMatchThreshold;
			AssertEquals("Should be High", OrgMatchThresholds.Descriptions.High, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.HighMatchThreshold + 1;
			AssertEquals("Should be High", OrgMatchThresholds.Descriptions.High, testMatch.MatchLikelihood);

			// Test High Threshold boundaries
			testMatch.OS_Score = OrgPatternMatchCollection.HighMatchThreshold + 1;
			AssertEquals("Should be High", OrgMatchThresholds.Descriptions.High, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.ExtremeMatchThreshold - 1;
			AssertEquals("Should be High", OrgMatchThresholds.Descriptions.High, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.ExtremeMatchThreshold;
			AssertEquals("Should be Extreme", OrgMatchThresholds.Descriptions.Extreme, testMatch.MatchLikelihood);
			testMatch.OS_Score = OrgPatternMatchCollection.ExtremeMatchThreshold + 1;
			AssertEquals("Should be Extreme", OrgMatchThresholds.Descriptions.Extreme, testMatch.MatchLikelihood);
		}

		#endregion

		#region Cloning Pattern Match

		public void TestClone()
		{
			var match = Factory.New<OrgPatternMatch>();
			match.OS_FullCompanyName = "Test Org";
			match.OS_UNLOCO = "AUSYD";
			match.OS_Email = "test@test";

			var clone = (OrgPatternMatch)match.Clone();
			foreach (ZPropertyInfo property in match.ZPropertyInfoHash)
			{
				AssertEquals("Property " + property.Name, match[property.Name], clone[property.Name]);
			}
		}

		#endregion

		#region AddressLanguage Test

		public void TestOS_AddressLanguage_DefaultsToEN()
		{
			var patternMatch = Factory.New<OrgPatternMatch>();

			AssertEquals("Default should be English", "EN", patternMatch.OS_AddressLanguage);

			patternMatch.OS_AddressLanguage = "KO";

			AssertEquals("KO", patternMatch.OS_AddressLanguage);
		}

		#endregion

		#region Implementation

		OrgPatternMatch orgMatch, compareOrgMatch;
		OrgHeader org, compareOrg;

		#region Match Scores

		readonly int postOfficeBoxScore = OrgPatternMatch.MatchScores.POBox;
		readonly int corporationScore = OrgPatternMatch.MatchScores.Corporation;
		readonly int portScore = OrgPatternMatch.MatchScores.Port;
		readonly int streetNumberScore = OrgPatternMatch.MatchScores.StreetNumber;
		readonly int postOfficeBoxNumberScore = OrgPatternMatch.MatchScores.POBoxNumber;
		readonly int postCodeScore = OrgPatternMatch.MatchScores.PostCode;
		readonly int citySoundexScore = OrgPatternMatch.MatchScores.CitySoundex;
		readonly int stateSoundexScore = OrgPatternMatch.MatchScores.StateSoundex;
		readonly int emailNameScore = OrgPatternMatch.MatchScores.EmailName;
		readonly int nameWordSoundexScore = OrgPatternMatch.MatchScores.NameWordSoundex;
		readonly int firstNameWordSoundexScore = OrgPatternMatch.MatchScores.FirstNameWordSoundex;
		readonly int addressWordSoundexScore = OrgPatternMatch.MatchScores.AddressWordSoundex;
		readonly int firstAddressWordSoundexScore = OrgPatternMatch.MatchScores.FirstAddressWordSoundex;
		readonly int streetNumberFirstAddressWordScore = OrgPatternMatch.MatchScores.StreetNumberFirstAddressWord;

		readonly int phoneLastSevenDigitsScore = OrgPatternMatch.MatchScores.PhoneLastSevenDigits;
		readonly int faxLastSevenDigitsScore = OrgPatternMatch.MatchScores.FaxLastSevenDigits;

		readonly int nameExactScore = OrgPatternMatch.MatchScores.NameExact;
		readonly int businessRegNoScore = OrgPatternMatch.MatchScores.BusinessRegNo;
		readonly int addressExactScore = OrgPatternMatch.MatchScores.AddressExact;
		readonly int emailExactScore = OrgPatternMatch.MatchScores.EmailExact;

		#endregion

		OrgPatternMatch OrgMatch
		{
			get { return orgMatch; }
			set { orgMatch = value; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			org = Factory.New<OrgHeader>();
			OrgMatch = org.PatternMatchesForThisOrg.AddNew();

			compareOrg = Factory.New<OrgHeader>();
			compareOrgMatch = compareOrg.PatternMatchesForThisOrg.AddNew();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#region Encoding Test Utilities

		void EncodeAddress(ZString address1, ZString address2)
		{
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_Address2 = address2;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
		}

		void AssertIsCorporation(ZString organisationName, bool isCorporation, string message)
		{
			org.OH_FullName = organisationName;

			// IsCorporation defaults to Y. This ensures that the pattern match object starts in a state which is incorrect.
			// Otherwise it is possible for the default value to mask a failing test.
			OrgMatch.OS_IsCorporation = !isCorporation;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals(message + " (" + organisationName + ")", isCorporation, OrgMatch.OS_IsCorporation);
		}

		void AssertAddressEncoding(ZString address1, ZString address2, ZString expectedAddress1, ZString expectedAddress2, ZString expectedAddress3, ZString expectedAddress4)
		{
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_Address2 = address2;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals("Checking address1 for: " + address1 + " " + address2, OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex(expectedAddress1), OrgMatch.OS_Address1);
			AssertEquals("Checking address2 for: " + address1 + " " + address2, OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex(expectedAddress2), OrgMatch.OS_Address2);
			AssertEquals("Checking address3 for: " + address1 + " " + address2, OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex(expectedAddress3), OrgMatch.OS_Address3);
			AssertEquals("Checking address4 for: " + address1 + " " + address2, OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex(expectedAddress4), OrgMatch.OS_Address4);
		}

		void AssertNameEncoding(ZString organisationName, ZString expectedName1, ZString expectedName2, ZString expectedName3, ZString expectedName4)
		{
			AssertNameEncoding(Constants.Languages.EnglishAmerican, organisationName, expectedName1, expectedName2, expectedName3, expectedName4);
		}

		void AssertNameEncoding(string languageCode, ZString organisationName, ZString expectedName1, ZString expectedName2, ZString expectedName3, ZString expectedName4)
		{
			org.OH_FullName = organisationName;
			org.OH_Language = languageCode;
			org.MainAddress.OA_Language = languageCode;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, languageCode), org.MainAddress, String.Empty);

			AssertEquals("Name1 (" + organisationName + ")", OrgPatternLanguageSetting.Get(languageCode, org.PortName, org.CountryName).GetSoundex(expectedName1), OrgMatch.OS_CompanyName1);
			AssertEquals("Name2 (" + organisationName + ")", OrgPatternLanguageSetting.Get(languageCode, org.PortName, org.CountryName).GetSoundex(expectedName2), OrgMatch.OS_CompanyName2);
			AssertEquals("Name3 (" + organisationName + ")", OrgPatternLanguageSetting.Get(languageCode, org.PortName, org.CountryName).GetSoundex(expectedName3), OrgMatch.OS_CompanyName3);
			AssertEquals("Name4 (" + organisationName + ")", OrgPatternLanguageSetting.Get(languageCode, org.PortName, org.CountryName).GetSoundex(expectedName4), OrgMatch.OS_CompanyName4);
		}

		void AssertFullCompanyName(string companyName, string expectedName, string message)
		{
			org.OH_FullName = companyName;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals(message + "(" + companyName + ")", expectedName, OrgMatch.OS_FullCompanyName);
		}

		void AssertEmailAndDomain(ZString email, ZString expectedEmail, ZString expectedDomain)
		{
			org.MainAddress.OA_Email = email;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertEquals("Email should be set", expectedEmail, OrgMatch.OS_Email);
			AssertEquals("Domain should be set", expectedDomain, OrgMatch.OS_Domain);
		}

		void AssertPOBox(string address, bool isPOBox)
		{
			org.MainAddress.OA_Address1 = address;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			var message = "Address " + (isPOBox ? "is" : "isn't") + " a PO box (" + address + ")";
			AssertEquals(message, isPOBox, OrgMatch.OS_IsPOBox);
		}

		void AssertPOBoxNumber(string address, string expectedPOBoxNumber)
		{
			org.MainAddress.OA_Address1 = address;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals("PO Box Number (" + address + ")", expectedPOBoxNumber, OrgMatch.OS_POBoxNumber);
		}

		void CheckAddressWithIgnoredWords(OrgHeader org, OrgPatternMatch orgMatch, string ignoredPhrase)
		{
			org.MainAddress.OA_Address1 = "ADDRESSONE " + ignoredPhrase;
			org.MainAddress.OA_Address2 = "ADDRESSTWO " + ignoredPhrase;
			orgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			var assertPrefix = "Testing ignoring of " + ignoredPhrase;

			AssertEquals(assertPrefix + " first word", OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex("ADDRESSONE"), orgMatch.OS_Address1);
			AssertEquals(assertPrefix + " second word", OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex("ADDRESSTWO"), orgMatch.OS_Address2);
			AssertEquals(assertPrefix + " third word should be ignored", String.Empty, orgMatch.OS_Address3);
			AssertEquals(assertPrefix + " fourth word should be ignored", String.Empty, orgMatch.OS_Address4);
		}

		void AssertBusinessRegNo(string businessRegNo, string expectedBusinessRegNo, string message)
		{
			org.PrimaryRegistrationNumber.Number = businessRegNo;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, businessRegNo);

			AssertEquals(message + " (" + businessRegNo + ")", expectedBusinessRegNo, OrgMatch.OS_BusinessRegNo);
		}

		void AssertPostCode(string postCode, string expectedPostCode, string message)
		{
			org.MainAddress.OA_PostCode = postCode;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals(message + " (" + postCode + ")", expectedPostCode, OrgMatch.OS_PostCode);
		}

		void AssertCity(string city, string expectedCity, string message)
		{
			org.MainAddress.OA_City = city;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			AssertEquals(message + "(" + city + ")", OrgPatternLanguageSetting.Get(Constants.Languages.EnglishAmerican).GetSoundex(expectedCity), OrgMatch.OS_City);
		}

		#endregion

		#region Score Test Utilities

		void AssertScore(int expectedScore, string message)
		{
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(compareOrg.OH_FullName, compareOrg.OH_Language), compareOrg.MainAddress, String.Empty);
			OrgMatch.SetMatchScoreForOrg(compareOrg.PatternMatchesForThisOrg);
			AssertEquals(message, expectedScore, OrgMatch.OS_Score);
		}

		void AssertPhoneSevenDigitsScore(ZString actualPhoneNumber, ZString comparePhoneNumber, string message)
		{
			AssertPhoneScore(actualPhoneNumber, comparePhoneNumber, phoneLastSevenDigitsScore, faxLastSevenDigitsScore, message);
		}

		void AssertPhoneScore(ZString actualPhoneNumber, ZString comparePhoneNumber, int expectedPhoneScore, int expectedFaxScore, string message)
		{
			org.MainAddress.OA_Phone = actualPhoneNumber;
			org.MainAddress.OA_Fax = String.Empty;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			compareOrg.MainAddress.OA_Phone = comparePhoneNumber;
			compareOrg.MainAddress.OA_Fax = String.Empty;
			AssertScore(expectedPhoneScore, "Phone (" + comparePhoneNumber + "): " + message);

			compareOrg.MainAddress.OA_Phone = String.Empty;
			compareOrg.MainAddress.OA_Fax = comparePhoneNumber;
			AssertScore(expectedPhoneScore, "Fax (" + comparePhoneNumber + "): " + message);

			org.MainAddress.OA_Phone = String.Empty;
			org.MainAddress.OA_Fax = actualPhoneNumber;
			OrgMatch.EncodeOrganisation(org, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);

			compareOrg.MainAddress.OA_Phone = comparePhoneNumber;
			compareOrg.MainAddress.OA_Fax = String.Empty;
			AssertScore(expectedFaxScore, "Phone (" + comparePhoneNumber + "): " + message);

			compareOrg.MainAddress.OA_Phone = String.Empty;
			compareOrg.MainAddress.OA_Fax = comparePhoneNumber;
			AssertScore(expectedFaxScore, "Fax (" + comparePhoneNumber + "): " + message);
		}

		void AssertEmailScore(ZString email, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_Email = email;
			AssertScore(expectedScore, message);
		}

		void AssertPortScore(ZString port, int expectedScore, string message)
		{
			compareOrg.OH_RL_NKClosestPort = port;
			compareOrg.MainAddress.OA_State = String.Empty;
			AssertScore(expectedScore, message);
		}

		void AssertNameScore(string name, int expectedScore, string message)
		{
			AssertNameScore(name, Constants.Languages.EnglishAmerican, expectedScore, message);
		}

		void AssertNameScore(string name, string language, int expectedScore, string message)
		{
			compareOrg.OH_FullName = name;
			compareOrg.OH_Language = language;
			AssertScore(expectedScore, message + " (" + name + ")");
		}

		void AssertAddressScore(string address1, string address2, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_Address1 = address1;
			compareOrg.MainAddress.OA_Address2 = address2;
			AssertScore(expectedScore, message);
		}

		void AssertPOBoxScore(string address1, string address2, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_Address1 = address1;
			compareOrg.MainAddress.OA_Address2 = address2;
			AssertScore(expectedScore, message);
		}

		void AssertCorporationScore(string name, int expectedScore, string message)
		{
			compareOrg.OH_FullName = name;
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(compareOrg.OH_FullName, compareOrg.OH_Language), compareOrg.MainAddress, String.Empty);
			AssertScore(expectedScore, message + "(" + name + ")");
		}

		void AssertPostCodeScore(string postCode, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_PostCode = postCode;
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(expectedScore, message + "(" + postCode + ")");
		}

		void AssertBusinessRegNoScore(string businessRegNo, int expectedScore, string message)
		{
			compareOrg.PrimaryRegistrationNumber.Number = businessRegNo;
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, businessRegNo);
			OrgMatch.SetMatchScoreForOrg(compareOrg.PatternMatchesForThisOrg);
			AssertEquals(message, expectedScore, OrgMatch.OS_Score);
		}

		void AssertCityScore(string city, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_City = city;
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(expectedScore, message + "(" + city + ")");
		}

		void AssertStateScore(string state, int expectedScore, string message)
		{
			compareOrg.MainAddress.OA_State = state;
			compareOrgMatch.EncodeOrganisation(compareOrg, new StringWithLanguage(org.OH_FullName, org.OH_Language), org.MainAddress, String.Empty);
			AssertScore(expectedScore, message + "(" + state + ")");
		}

		#endregion

		#region Filter Test Utilities

		void AssertFilter(string message, string expectedFilter)
		{
			var filter = OrgMatch.GetFilter();
			AssertEquals(message, expectedFilter, filter.LiteralTextADO);
		}

		#endregion

		#endregion

		public void TestHumanReadableName_WhenOrganizationIsDeleted_ShouldReturnSensibleValue()
		{
			// Arrange.

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "FOO";

			var patternMatch = Factory.New<OrgPatternMatch>();
			patternMatch.OS_OH = organization.PK;

			organization.Delete();

			// Act & Assert.

			AssertEquals("Deleted Organization", patternMatch.HumanReadableName);
		}
	}
}
