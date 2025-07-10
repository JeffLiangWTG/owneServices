using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing.AESTIRPartyTest
{
	sealed class AESTIRPartyTest : TestCaseWithFactory
	{
		public void TestIAESTIRPartyMembers()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EXPSTA, "OUT", "Export State Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EXPSTA, "MXBCS", "BN", startDate, endDate, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			string[] codeTypes = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.CodeTypes.DataUniversalNumberingSystem };
			OrgHeader org = Factory.New<OrgHeader>();
			IAESTIRParty party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("", party.PartyID);
			AssertEquals("", party.PartyIDType);
			AssertEquals("", party.PartyName);
			AssertEquals("", party.ContactFirstName);
			AssertEquals("", party.ContactMiddleInitial);
			AssertEquals("", party.ContactLastName);
			AssertEquals("", party.AddressLine1);
			AssertEquals("", party.AddressLine2);
			AssertEquals("", party.ContactPhoneNumber);
			AssertEquals("", party.City);
			AssertEquals("", party.StateCode);
			AssertEquals("", party.CountryCode);
			AssertEquals("", party.PostalCode);

			org.OH_FullName = "ABC 2343 x 34k *) 23 @#4 DSD -=? ";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "AB45-465 3323 =+", GlbCompany.CurrentCompany.Country);
			org.OH_RL_NKClosestPort = "USLAX";

			org.MainAddress.OA_Address1 = "  WHO STREETS THAT IS LONG addresS 1";
			org.MainAddress.OA_Address2 = "	WHO addresS 2 STREETS THAT IS LONG";
			org.MainAddress.OA_City = "-234 AB#@# A is This? 2ho";
			org.MainAddress.OA_PostCode = "-234 kd 2;";

			OrgAddress address2 = org.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "MXCOA";
			address2.OA_Address1 = "		ADDREss 2 THAT IS LONG addresS 1";
			address2.OA_Address2 = "   ADDREss 2 STREETS THAT IS FUNNY";
			address2.OA_City = "CI97 23DR 2342";
			address2.OA_PostCode = "96863 -232";
			address2.OA_State = AESMexicoStates.Descriptions.BC;
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "9282 ASB -LOk-89446", GlbCompany.CurrentCompany.Country);
			address2.OA_CompanyNameOverride = "Overridden Name";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "12BO-2B3 T3H3E -29BUI-ilD3er3";
			contact1.OC_Phone = "+61(28002 4564A12345";
			OrgDocument document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.Consignor.ToString();
			document1.OD_DefaultContact = true;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "(Wed323dy) Des32Troye3r";
			contact2.OC_Phone = "+612865652 A12345";
			OrgDocument document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Consignee.ToString();
			document2.OD_DefaultContact = true;

			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("AB454653323", party.PartyID);
			AssertEquals("E", party.PartyIDType);
			AssertEquals("ABC 2343 X 34K *) 23 @#4 DSD -", party.PartyName);
			AssertEquals("BOB", party.ContactFirstName);
			AssertEquals("T", party.ContactMiddleInitial);
			AssertEquals("BUIILDER", party.ContactLastName);
			AssertEquals("WHO STREETS THAT IS LONG ADDRESS", party.AddressLine1);
			AssertEquals("WHO ADDRESS 2 STREETS THAT IS LO", party.AddressLine2);
			AssertEquals("8002456412345", party.ContactPhoneNumber);
			AssertEquals(" AB A IS THIS HO", party.City);
			AssertEquals("CA", party.StateCode);
			AssertEquals("US", party.CountryCode);
			AssertEquals("234KD2", party.PostalCode);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.USPPIDocAddress.E2_OA_Address = address2.PK;

			party = AESTIRParty.New(invoice.US_USPPI, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals("9282ASBLOK8", party.PartyID);
			AssertEquals("D", party.PartyIDType);
			AssertEquals("OVERRIDDEN NAME", party.PartyName);
			AssertEquals("BOB", party.ContactFirstName);
			AssertEquals("BUIILDER", party.ContactLastName);
			AssertEquals("ADDRESS 2 THAT IS LONG ADDRESS 1", party.AddressLine1);
			AssertEquals("ADDRESS 2 STREETS THAT IS FUNNY", party.AddressLine2);
			AssertEquals("8002456412345", party.ContactPhoneNumber);
			AssertEquals("CI DR ", party.City);
			AssertEquals(AESMexicoStates.Codes.BC, party.StateCode);
			AssertEquals("MX", party.CountryCode);
			AssertEquals("96863232", party.PostalCode);

			var decPickupAddress = invoice.SupplierPickupAddress;
			decPickupAddress.E2_OA_Address = org.MainAddress.PK;
			party = AESTIRParty.New(invoice.US_USPPI, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem }, decPickupAddress);
			AssertEquals("9282ASBLOK8", party.PartyID);
			AssertEquals("D", party.PartyIDType);
			AssertEquals("OVERRIDDEN NAME", party.PartyName);
			AssertEquals("BOB", party.ContactFirstName);
			AssertEquals("BUIILDER", party.ContactLastName);
			AssertEquals("WHO STREETS THAT IS LONG ADDRESS", party.AddressLine1);
			AssertEquals("WHO ADDRESS 2 STREETS THAT IS LO", party.AddressLine2);
			AssertEquals("8002456412345", party.ContactPhoneNumber);
			AssertEquals(" AB A IS THIS HO", party.City);
			AssertEquals("CA", party.StateCode);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, party.CountryCode);
			AssertEquals("234KD2", party.PostalCode);

			org.OH_RL_NKClosestPort = "PRARR";
			party = AESTIRParty.New(org, ContactType.Consignee, Core.Constants.TransportModes.Sea, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals("", party.PartyID);
			AssertEquals("", party.PartyIDType);
			AssertEquals("ABC 2343 X 34K *) 23 @#4 DSD -", party.PartyName);
			AssertEquals("WEDDY", party.ContactFirstName);
			AssertEquals("DESTROYER", party.ContactLastName);
			AssertEquals("WHO STREETS THAT IS LONG ADDRESS", party.AddressLine1);
			AssertEquals("WHO ADDRESS 2 STREETS THAT IS LO", party.AddressLine2);
			AssertEquals("1286565212345", party.ContactPhoneNumber);
			AssertEquals(" AB A IS THIS HO", party.City);
			AssertEquals(Core.Constants.CountryCodes.PuertoRico, party.StateCode);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, party.CountryCode);
			AssertEquals("234KD2", party.PostalCode);

			org.OH_RL_NKClosestPort = "VIAGL";
			party = AESTIRParty.New(org, ContactType.Consignee, Core.Constants.TransportModes.Sea, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals("", party.PartyID);
			AssertEquals("", party.PartyIDType);
			AssertEquals("ABC 2343 X 34K *) 23 @#4 DSD -", party.PartyName);
			AssertEquals("WEDDY", party.ContactFirstName);
			AssertEquals("DESTROYER", party.ContactLastName);
			AssertEquals("WHO STREETS THAT IS LONG ADDRESS", party.AddressLine1);
			AssertEquals("WHO ADDRESS 2 STREETS THAT IS LO", party.AddressLine2);
			AssertEquals("1286565212345", party.ContactPhoneNumber);
			AssertEquals(" AB A IS THIS HO", party.City);
			AssertEquals("", party.StateCode);
			AssertEquals(Core.Constants.CountryCodes.VirginIslands, party.CountryCode);
			AssertEquals("234KD2", party.PostalCode);

			OrgAddress address3 = org.Addresses.AddNew();
			address3.OA_CompanyNameOverride = "NEW COMPANY NAME";
			invoice.USPPIDocAddress.E2_OA_Address = address3.PK;
			party = AESTIRParty.New(invoice.US_USPPI, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals("NEW COMPANY NAME", party.PartyName);

			org.Contacts.RemoveAndDeleteAll();
			var slash = org.Contacts.AddNew();
			slash.OC_ContactName = "Slash";
			slash.OC_Phone = "+612865652 A12345";
			OrgDocument documentslash = slash.Documents.AddNew();
			documentslash.OD_DocumentGroup = ContactType.Consignee.ToString();
			documentslash.OD_DefaultContact = true;
			var partySlash = AESTIRParty.New(org, ContactType.Consignee, Core.Constants.TransportModes.Sea, new string[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
			AssertEquals("", partySlash.ContactFirstName);
			AssertEquals("SLASH", partySlash.ContactLastName);

			var usppi = invoice.USPPIDocAddress;
			usppi.E2_AddressOverride = true;
			usppi.E2_CompanyName = "USPPI OVERRIDE CO.";
			usppi.E2_Contact = "TEST CONTACT NAME";
			usppi.E2_Mobile = "+1 201-659-8745";
			usppi.E2_GovRegNumType = "EIN";
			usppi.E2_GovRegNum = "12-123659700";

			var pickupAddress = invoice.SupplierPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_Address1 = "TEST ADDRESS 1";
			pickupAddress.E2_Address2 = "TEST ADDRESS 2";
			pickupAddress.E2_Postcode = "201100";
			pickupAddress.E2_RN_NKCountryCode = "CA";
			pickupAddress.E2_City = "TEST CITY";

			var aestiParty = AESTIRParty.New(invoice.US_USPPI, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.USACodeTypes.ForeignRegistrationNumber,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem }, invoice.SupplierPickupAddress);

			AssertEquals("12123659700", aestiParty.PartyID);
			AssertEquals("E", aestiParty.PartyIDType);
			AssertEquals("USPPI OVERRIDE CO.", aestiParty.PartyName);
			AssertEquals("TEST", aestiParty.ContactFirstName);
			AssertEquals("C", aestiParty.ContactMiddleInitial);
			AssertEquals("NAME", aestiParty.ContactLastName);
			AssertEquals("TEST ADDRESS 1", aestiParty.AddressLine1);
			AssertEquals("TEST ADDRESS 2", aestiParty.AddressLine2);
			AssertEquals("TEST CITY", aestiParty.City);
			AssertEquals("CA", aestiParty.CountryCode);
			AssertEquals("201100", aestiParty.PostalCode);
		}

		public void TestState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EXPSTA, "OUT", "Export State Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EXPSTA, "MXAGU", "AG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var codeTypes = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber };
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_State = "NSW";

			IAESTIRParty party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("", party.StateCode);

			org.OH_RL_NKClosestPort = "MXPMX";
			org.MainAddress.OA_State = "AGU";

			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("AG", party.StateCode);

			org.MainAddress.OA_State = "XXX";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("XX", party.StateCode);
		}

		public void TestPostCode()
		{
			var codeTypes = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber };
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_PostCode = "22 15-152";

			IAESTIRParty party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("2215152", party.PostalCode);

			org.OH_RL_NKClosestPort = "USLAX";
			org.MainAddress.OA_PostCode = " 66555 ";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("66555", party.PostalCode);

			org.MainAddress.OA_PostCode = "65 55-1234 ";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("US Postcodes strip spaces & hyphens", "65551234", party.PostalCode);
		}

		public void TestGetUnformattedPhoneNumber()
		{
			var codeTypes = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber };
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			org.MainAddress.OA_Phone = "61 22 15-152";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Consignor.ToString();
			IAESTIRParty party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("612215152", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "+61 22 15-152";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("612215152", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "01161 22 15-152";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("612215152", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "+44 1234 555 5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("4412345555555", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "44 1234 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("4412345555555", party.ContactPhoneNumber);

			org.OH_RL_NKClosestPort = "USLAX";
			org.MainAddress.OA_Phone = "+1 (555) 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("5555555555", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "+1(555) 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("5555555555", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "1 555 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("15555555555", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "2 555 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("25555555555", party.ContactPhoneNumber);

			org.MainAddress.OA_Phone = "0111 555 555-5555";
			party = AESTIRParty.New(org, ContactType.Consignor, Core.Constants.TransportModes.Sea, codeTypes);
			AssertEquals("5555555555", party.ContactPhoneNumber);
		}

		public void TestNoExceptionThrownWhenDocAddressIsDeleted()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TSTMPX";
			org.MainAddress.OA_Address1 = "IMP ADDRESS 1";
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.US_USPPI.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;
			invoice.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;
			invoice.US_IntermediateConsignee.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var dec = newFactory.Load<JobDeclaration>(declaration.PK);
			var loadedInv = dec.Invoices[0];
			loadedInv.US_USPPI.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_USPPI.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;

			loadedInv.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;

			loadedInv.US_IntermediateConsignee.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_IntermediateConsignee.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;

			newFactory.Save();

			AssertNoExceptionThrown(() => AESTIRParty.New(invoice.US_USPPI, System.Array.Empty<string>()));
			AssertNoExceptionThrown(() => AESTIRParty.New(invoice.US_ExportUltimateConsignee, System.Array.Empty<string>()));
			AssertNoExceptionThrown(() => AESTIRParty.New(invoice.US_IntermediateConsignee, System.Array.Empty<string>()));
		}
	}
}
