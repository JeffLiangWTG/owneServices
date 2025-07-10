using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OrganisationValidationTest : TestCaseWithFactory
	{
		public void TestShouldCheckStateCodeForPGA()
		{
			Assert(OrganisationValidation.ShouldCheckStateCodeForPGA(Core.Constants.CountryCodes.UnitedStates));
			Assert(OrganisationValidation.ShouldCheckStateCodeForPGA(Core.Constants.CountryCodes.Canada));
			Assert(OrganisationValidation.ShouldCheckStateCodeForPGA(Core.Constants.CountryCodes.Mexico));
			Assert(!OrganisationValidation.ShouldCheckStateCodeForPGA(Core.Constants.CountryCodes.PuertoRico));
		}

		public void TestValidateCharactorsForAddressDescription()
		{
			var addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			var addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_City = "KYIV";
			address.OA_Address1 = "éééÄöß";
			address.OA_Address2 = "Address2Äöß";
			address.OA_Code = "öß";
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableAII = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			line.JI_LinePrice = 300m;
			invoice.JZ_InvoiceNumber = "invoice1";
			line.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;

			invoice.JZ_OA_InvoicerDocAddress = address.PK;
			AssertHasWarning(invoice.JZ_OA_InvoicerDocAddressInfo, addressDescriptionWarning);
			AssertHasWarning(invoice.JZ_OA_InvoicerDocAddressInfo, addressCodeWarning);
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_Code = "Code";
			invoice.JZ_OA_InvoicerDocAddress = address.PK;
			AssertNoWarning(invoice.JZ_OA_InvoicerDocAddressInfo, addressDescriptionWarning);
			AssertNoWarning(invoice.JZ_OA_InvoicerDocAddressInfo, addressCodeWarning);
		}

		public void TestValidateCountryForPGAAddress()
		{
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EnableAII = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			var fda = line.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			fda.US_OA_ShipperAddress = address.PK;

			AssertHasMessageErrorContaining(fda.US_OA_ShipperAddressInfo, USACEFDAAddInfoInvoiceLineValidation.DRU_804_ShouldBeCanadaAddress);
		}

		public void TestValidateStateForPGAAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "K1";
			refCountryStates1.RW_RN_NKCountryCode = "US";
			refCountryStates1.RW_Description = "ST1";
			refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "K2";
			refCountryStates1.RW_RN_NKCountryCode = "PR";
			refCountryStates1.RW_Description = "ST2";
			refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "K3";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "ST3";
			refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "K4";
			refCountryStates1.RW_RN_NKCountryCode = "CA";
			refCountryStates1.RW_Description = "ST4";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_RL_NKRelatedPortCode = "USBLA";
			address.OA_State = "XX";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fda.US_ManufacturerAddressInfo, "The state is not a valid");
			address.OA_State = "ST1";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fda.US_ManufacturerAddressInfo, "The state is not a valid");
			address.OA_State = "";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fda.US_ManufacturerAddressInfo, "State should not be empty");

			address.OA_RL_NKRelatedPortCode = "CABLA";
			address.OA_State = "XX";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fda.US_ManufacturerAddressInfo, "The state is not a valid");
			address.OA_State = "ST4";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fda.US_ManufacturerAddressInfo, "The state is not a valid");
			address.OA_State = "";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fda.US_ManufacturerAddressInfo, "State should not be empty");

			address.OA_RL_NKRelatedPortCode = "AEBLA";
			address.OA_State = "XX";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fda.US_ManufacturerAddressInfo, "The state is not a valid");
			address.OA_State = "";
			fda.US_ManufacturerAddressInfo.Value = address.PK;
			fda.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fda.US_ManufacturerAddressInfo, "State should not be empty");
		}

		public void TestValidateCustomsRegNoForOrganisation()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			JobDeclaration declaration = mockDeclaration.Object;
			mockDeclaration.Protected()
				.Setup<Customs.Business.JobDeclarationValidation>("GetNewValidation")
				.Returns(new TestJobDeclarationValidation(declaration));

			OrgHeader org = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = org.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "LOL lol LOL lol");

			org.CustomsCodes.AddNew("AAA", "A");
			declaration.JE_OH_Importer = org.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "LOL lol LOL lol");
		}

		public void TestCusRegistrationNoOnOrg()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;

			var ior = Factory.New<OrgHeader>();
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "AAAAAAAAAABBBBBBBBBB");
			dec.IOROrgPK = ior.PK;
			AssertHasMessageError(dec.IOROrgPKInfo, "The Importer Number, 'AAAAAAAAAABBBBBBBBBB', should be less than 12");

			dec.IOROrgPKInfo.ClearAllNotifications();
			ior.CustomsCodes.RemoveAndDeleteAll();
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "061122-12345");
			AssertNoMessageErrors(dec.IOROrgPKInfo);
		}

		public void TestCarrierAddressSelection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var carrier = Factory.New<OrgHeader>();

			carrier.OH_FullName = "Carrier";
			var address = carrier.MainAddress;
			address.OA_Address1 = "ADDRESS 1";
			address.OA_City = "SYDNEY";
			address.OA_State = "NSW";
			address.OA_PostCode = "2017";
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var address2 = carrier.AddressesActive.AddNew();
			address2.OA_Address1 = "ADDRESS 1";
			address2.OA_City = "CHICAGO";
			address2.OA_State = "IL";
			address2.OA_PostCode = "60010";
			address2.OA_RL_NKRelatedPortCode = "USCHI";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = "D";
			declaration.JE_OH_ShippingLine = carrier.PK;
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			AssertHasMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, errorMsg);

			DeclarationTestHelper.AddPGAContact(carrier, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			declaration.JE_OH_ShippingLine = carrier.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, errorMsg);
		}

		public void TestValidateEINLikeEncryptedNoForOrganisation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.US_TTBInd = "D";
			invoiceLine.TTBLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			CusEntryLine entryline = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			string expectedError = string.Format(OrganisationValidation.EINLikeAsEntryptedNo, "-23GGFRD234");
			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "-23GGFRD234");
			declaration.IOROrgPK = org.PK;
			AssertHasMessageError(declaration.IOROrgPKInfo, expectedError);

			declaration.IOROrgPKInfo.ClearAllNotifications();
			org.CustomsCodes.RemoveAll();
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "23GGFRD234");
			declaration.Validation.CheckIOROrgPK(declaration.IOROrgPKInfo);
			AssertNoMessageError(declaration.IOROrgPKInfo, expectedError);
		}

		public void TestValidateOrganisationRegisteredInCustoms()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			JobDeclaration declaration = mockDeclaration.Object;
			mockDeclaration.Protected()
				.Setup<Customs.Business.JobDeclarationValidation>("GetNewValidation")
				.Returns(new FormalImportJobDeclarationValidation(declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			declaration.JE_OH_Importer = org.PK;
			string messageError = "Organisation Not On File";
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			OrganisationValidation.ValidateOrganisationRegisteredInCustoms(declaration.JE_OH_ImporterInfo, org, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);

			OrgCusCode cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "3GGFRD");
			declaration.JE_OH_Importer = org.PK;
			AssertHasWarning(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			OrganisationValidation.ValidateOrganisationRegisteredInCustoms(declaration.JE_OH_ImporterInfo, org, messageError);
			AssertHasWarning(declaration.JE_OH_ImporterInfo, messageError);

			OrgHeaderWrapper.New(org).ZO_IsEINNumberVerifiedIndicator = "N";
			Factory.Save();

			declaration.JE_OH_Importer = org.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			OrganisationValidation.ValidateOrganisationRegisteredInCustoms(declaration.JE_OH_ImporterInfo, org, messageError);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);

			OrgHeaderWrapper.New(org).ZO_IsEINNumberVerifiedIndicator = "Y";
			Factory.Save();

			declaration.JE_OH_Importer = org.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			OrganisationValidation.ValidateOrganisationRegisteredInCustoms(declaration.JE_OH_ImporterInfo, org, messageError);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(declaration.JE_OH_ImporterInfo,
				OrgMatchedCustomsRegNoType.EIN, messageError, true, false);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);

			OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(declaration.JE_OH_ImporterInfo,
				OrgMatchedCustomsRegNoType.ECN, messageError, true, false);
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);

			if (ErrorReporter.LastKeyReported == "Validation:JE_OH_Importer")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateEncryptedConsigneeNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			CusEntryLine line = entry.MergedLines.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = line.PK;

			OrgHeader org = Factory.New<OrgHeader>();
			declaration.JE_OA_ConsigneeAddress = org.MainAddress.PK;
			string ultimateConsigneeMessageError = string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired,
				"Ultimate Consignee");
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);

			declaration.US_EnableENS = false;
			OrgCusCode cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-23GGFRD234");
			declaration.JE_OA_ConsigneeAddress = org.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);
		}

		public void TestValidate()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			JobDeclaration declaration = mockDeclaration.Object;

			mockDeclaration.Protected().Setup<Customs.Business.JobDeclarationValidation>("GetNewValidation").Returns(new TestJobDeclarationValidation(declaration));

			OrgHeader org = Factory.New<OrgHeader>();

			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, OrganisationValidation.NoSCACForCarrier);

			declaration.JE_OH_Forwarder = org.PK;
			AssertHasMessageError(declaration.JE_OH_ForwarderInfo, OrganisationValidation.NoSCACForCarrier);

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ASDF");
			declaration.JE_OH_Forwarder = org.PK;
			AssertNoMessageError(declaration.JE_OH_ForwarderInfo, OrganisationValidation.NoSCACForCarrier);
		}

		public void TestValidateManufacturerIDForAddress()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "G4";
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			ManufacturerIDValidator validator = new ManufacturerIDValidator(Factory);
			AssertEquals("IsISOCountryCodeValid", true, validator.IsISOCountryCodeValid("G42312312323232"));

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CATOR";

			var mock = Factory.NewMoq<DummyBusinessObject>();
			DummyBusinessObject dummyObj = mock.Object;
			TestDummyBizoValidation validation = new TestDummyBizoValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			validation.forceEntered = true;
			validation.countryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			validation.countryOfExport = Core.Constants.CountryCodes.Canada;
			string messageError = string.Format(OrganisationValidation.ManufacturerIDMissing, org.MainAddress.OA_Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertHasMessageError("ManufacturerIDMissing", dummyObj.Z0_GuidInfo, messageError);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "123456",
				helper.UnitedStates.Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertHasMessageError("ManufacturerIDMinimumLength", dummyObj.Z0_GuidInfo, ManufacturerIDValidator.Constants.Format);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "123456AB",
				helper.UnitedStates.Code);

			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertNoMessageError("ManufacturerIDMinimumLength", dummyObj.Z0_GuidInfo, ManufacturerIDValidator.Constants.Format);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "CA#$#$#",
				helper.UnitedStates.Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertHasMessageError("ManufacturerIDNotAlphaNumeric", dummyObj.Z0_GuidInfo, ManufacturerIDValidator.Constants.Format);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID,
				"1234567890123456", helper.UnitedStates.Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertNoMessageError("ManufacturerIDMissing", dummyObj.Z0_GuidInfo, messageError);
			AssertHasMessageError("ManufacturerIDMaximumLengthExceeded", dummyObj.Z0_GuidInfo,
				ManufacturerIDValidator.Constants.Format);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID,
				"G32312312323232", helper.UnitedStates.Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertNoMessageError("ManufacturerIDMaximumLengthExceeded", dummyObj.Z0_GuidInfo,
				ManufacturerIDValidator.Constants.Format);
			AssertHasMessageError("ManufacturerIDRightFormat", dummyObj.Z0_GuidInfo, ManufacturerIDValidator.Constants.ISOCode);

			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID,
				"G42312312323232", helper.UnitedStates.Code);
			dummyObj.Z0_Guid = org.MainAddress.PK;
			AssertNoMessageError("ManufacturerIDRightFormat", dummyObj.Z0_GuidInfo, ManufacturerIDValidator.Constants.ISOCode);
		}

		public void TestMIDCountryMatchesOrigin()
		{
			var aUManufacturer = Factory.New<OrgHeader>();
			aUManufacturer.OH_RL_NKClosestPort = "AUSYD";
			aUManufacturer.MainAddress.OA_Address1 = "ADDRESS 1";
			aUManufacturer.MainAddress.OA_Address2 = "ADDRESS 2";
			aUManufacturer.MainAddress.OA_City = "SYDNEY";
			aUManufacturer.MainAddress.OA_State = "NSW";
			aUManufacturer.MainAddress.OA_PostCode = "2200";

			var mock = Factory.NewMoq<DummyBusinessObject>();
			var dummyObj = mock.Object;
			dummyObj.Z0_Guid = aUManufacturer.MainAddress.PK;

			IOrganisationDetails manufacturerDetails = OrganisationDetails.New(dummyObj.Z0_GuidInfo,
				OrgMatchedCustomsRegNoType.MID);
			AssertEquals("MID code matches Country of Origin", true,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "AU"));

			var midCode = aUManufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU12365478");
			//OrgAddressOrganisationDetails.New(AUManufacturer.MainAddress, OrgCusCode.USACodeTypes.ManufacturerID);

			manufacturerDetails = OrganisationDetails.New(dummyObj.Z0_GuidInfo, OrgMatchedCustomsRegNoType.MID);
			AssertEquals("MID code matches Country of Origin", true,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "AU"));
			AssertEquals("MID code does not match Country of Origin", false,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "TW"));

			var cAManufacturer = Factory.New<OrgHeader>();
			cAManufacturer.OH_RL_NKClosestPort = "CAONT";
			cAManufacturer.MainAddress.OA_Address1 = "ADDRESS 1";
			cAManufacturer.MainAddress.OA_Address2 = "ADDRESS 2";
			cAManufacturer.MainAddress.OA_City = "ONTARIO";
			cAManufacturer.MainAddress.OA_State = "ON";
			dummyObj.Z0_Guid = cAManufacturer.MainAddress.PK;

			var cAManufacturerMIDCode = cAManufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID,
				"ON33048859");
			manufacturerDetails = OrganisationDetails.New(dummyObj.Z0_GuidInfo, OrgMatchedCustomsRegNoType.MID);
			AssertEquals("MID code does matches Country of Origin", false,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "AU"));
			AssertEquals("MID code does not match Country of Origin", false,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "TW"));
			AssertEquals("MID code does not match Country of Origin (Canada)", false,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "CA"));
			AssertEquals("MID code matches Country of Origin (Canadian Province)", true,
				OrganisationValidation.MIDCountryMatchesOrigin(manufacturerDetails, "ON"));
		}

		public void TestValidateMIDCountryAgainstCountryOfOriginForCanada()
		{
			var supplier = Factory.New<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			supplierAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XAP12345678");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;

			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoice.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XC;
			AssertNoMessageError("'XA' is canadian province", invoice.US_UC_NKCountryOfOriginInfo,
				ManufacturerIDValidator.Constants.Canadian);

			var customsCode = supplierAddress.CustomsCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.ManufacturerID);
			AssertNotNull(customsCode);

			customsCode.OK_CustomsRegNo = "SUP12345678";
			invoice.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError("'SU' is not canadian province", invoice.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}

		public void TestValidateMIDCountryAgainstCountryOfOriginForTextileTariff()
		{
			var tariff = Factory.NewWithValidTestData<USCTariff>();
			tariff.UE_Tariff = "6501003000";
			tariff.UE_DateFrom = new ZDateTime(2010, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);
			Factory.Save();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			supplierAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XAP12345678");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;

			invoiceLine.JI_Tariff = "6501003000";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XC;
			AssertHasWarning(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);

			var customsCode = supplierAddress.CustomsCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.ManufacturerID);
			AssertNotNull(customsCode);

			customsCode.OK_CustomsRegNo = "SUP12345678";
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextile);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoWarning(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.InvalidMIDForTextileCanadianProvinces);
		}

		public void TestValidatePGAAddress()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProductCode = "74D--TF";
			fda.US_ProdCountry = "US";
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();

			fda.US_FSVPImporterAddress = address.PK;

			fda.AddInfoValidation.ValidateUS_FSVPImporterAddress();

			AssertHasMessageError(fda.US_FSVPImporterAddressInfo, OrganisationValidation.CityCanNotBeEmpty);
			AssertHasMessageErrorContaining(fda.US_FSVPImporterAddressInfo, "can not be empty, Please press F3, go to the Address tab and enter the");

			address.CompanyName = "CargoWise";
			address.City = "NJ";
			address.State = "CO";

			fda.AddInfoValidation.ValidateUS_FSVPImporterAddress();

			AssertNoMessageError(fda.US_FSVPImporterAddressInfo, OrganisationValidation.CityCanNotBeEmpty);
			AssertNoMessageErrorContaining(fda.US_FSVPImporterAddressInfo, "can not be empty, Please press F3, go to the Address tab and enter the");
		}

		public void TestValidatePGAContact()
		{
			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			var emailRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email/Fax");
			var contactNameRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name");
			var phoneNumberRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone");

			dummyObj.Z0_Code = "A";
			AssertHasMessageErrorContaining(dummyObj.Z0_CodeInfo, emailRequired);
			AssertHasMessageErrorContaining(dummyObj.Z0_CodeInfo, contactNameRequired);
			AssertHasMessageErrorContaining(dummyObj.Z0_CodeInfo, phoneNumberRequired);

			dummyObj.DummyStringValue = "DDD";
			dummyObj.Z0_Code = "B";
			AssertNoMessageErrorContaining(dummyObj.Z0_CodeInfo, emailRequired);
			AssertNoMessageErrorContaining(dummyObj.Z0_CodeInfo, contactNameRequired);
			AssertNoMessageErrorContaining(dummyObj.Z0_CodeInfo, phoneNumberRequired);
		}

		public void TestValidatePGAEmail()
		{
			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			var emailRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email");
			var emailNotEntered = MandatoryValidation.YouHaveNotEnteredMessage("Email");

			dummyObj.Z0_Number = 1;
			AssertHasMessageErrorContaining(dummyObj.Z0_NumberInfo, emailNotEntered);

			dummyObj.DummyStringValue = "DDD";
			dummyObj.Z0_Number = 2;
			AssertNoMessageErrorContaining(dummyObj.Z0_NumberInfo, emailNotEntered);

			dummyObj.HasRealAddress = true;
			dummyObj.Z0_Number = 3;
			AssertNoMessageErrorContaining(dummyObj.Z0_NumberInfo, emailRequired);
			AssertNoMessageErrorContaining(dummyObj.Z0_NumberInfo, emailNotEntered);

			dummyObj.DummyStringValue = "";
			dummyObj.Z0_Number = 4;
			AssertHasMessageErrorContaining(dummyObj.Z0_NumberInfo, emailRequired);
		}

		public void TestValidateCountryForPGAAddress_JobDocAddress()
		{
			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			validation.ErrorMessageTextIfInvalidCountry = "Invalid Country";

			dummyObj.Z0_AnotherNumber = 1;
			AssertHasMessageError(dummyObj.Z0_AnotherNumberInfo, "Invalid Country");

			dummyObj.DummyStringValue = "US";
			dummyObj.Z0_AnotherNumber = 2;
			AssertNoMessageError(dummyObj.Z0_AnotherNumberInfo, "Invalid Country");
		}

		public void ValidateStateForPGAAddress_WithFactory()
		{
			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			var stateShouldNotBeEmpty = "State should not be empty.";
			var stateIsInvalid = "The state is not a valid US state.";

			dummyObj.Z0_NVarChar = "1";
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateIsInvalid);

			validation.CountryCode = "US";
			dummyObj.Z0_NVarChar = "2";
			AssertHasMessageError(dummyObj.Z0_NVarCharInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateIsInvalid);

			validation.State = "NY";
			dummyObj.Z0_NVarChar = "3";
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateIsInvalid);

			validation.State = "ZZ";
			dummyObj.Z0_NVarChar = "4";
			AssertNoMessageError(dummyObj.Z0_NVarCharInfo, stateShouldNotBeEmpty);
			AssertHasMessageError(dummyObj.Z0_NVarCharInfo, stateIsInvalid);

			validation.CountryCode = "CN";
			dummyObj.Z0_NVarChar = "5";
			AssertNoMessageErrors(dummyObj.Z0_NVarCharInfo);
		}

		public void ValidateStateForPGAAddress_JobDocAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			var mock = Factory.NewMoq<DummyBusinessObjectJobDocAddress>();
			var dummyObj = mock.Object;
			var validation = new TestDummyBizoJobDocAddressValidation(dummyObj);
			mock.Protected().Setup<DummyBizoValidation>("GetNewValidation").Returns(validation);
			var stateShouldNotBeEmpty = "State should not be empty.";
			var stateIsInvalid = "The state is not a valid US state.";

			dummyObj.Z0_Decimal = 1;
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateIsInvalid);

			address.OA_RN_NKCountryCode = "US";
			dummyObj.Z0_Decimal = 2;
			AssertHasMessageError(dummyObj.Z0_DecimalInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateIsInvalid);

			address.State = "NY";
			dummyObj.Z0_Decimal = 3;
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateShouldNotBeEmpty);
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateIsInvalid);

			address.State = "ZZ";
			dummyObj.Z0_Decimal = 4;
			AssertNoMessageError(dummyObj.Z0_DecimalInfo, stateShouldNotBeEmpty);
			AssertHasMessageError(dummyObj.Z0_DecimalInfo, stateIsInvalid);

			address.OA_RN_NKCountryCode = "CN";
			dummyObj.Z0_Decimal = 5;
			AssertNoMessageErrors(dummyObj.Z0_DecimalInfo);
		}

		class TestDummyBizoValidation : DummyBizoValidation
		{
			public TestDummyBizoValidation(DummyBusinessObject parent)
			: base(parent)
			{
			}
			public bool forceEntered;
			public ZString countryOfOrigin;
			public ZString countryOfExport;
			protected override void CheckZ0_Guid()
			{
				base.CheckZ0_Guid();
				OrganisationValidation.ValidateManufacturerIDForAddress(Parent.Z0_GuidInfo, forceEntered, NotificationType.MessageError);
			}
		}

		class TestDummyBizoJobDocAddressValidation : DummyBusinessObjectJobDocAddressValidation
		{
			public TestDummyBizoJobDocAddressValidation(DummyBusinessObjectJobDocAddress parent)
				: base(parent)
			{
			}

			public ZString ErrorMessageTextIfInvalidCountry;

			public ZString CountryCode;

			public ZString State;

			public OrgAddress Address;

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				OrganisationValidation.ValidatePGAContact(ParentDocAddress.Z0_CodeInfo, ParentDocAddress);
			}

			protected override void CheckZ0_Number()
			{
				base.CheckZ0_Number();
				OrganisationValidation.ValidatePGAEmail(ParentDocAddress.Z0_NumberInfo, ParentDocAddress, ParentDocAddress.HasRealAddress);
			}

			protected override void CheckZ0_AnotherNumber()
			{
				base.CheckZ0_AnotherNumber();
				OrganisationValidation.ValidateCountryForPGAAddress(ParentDocAddress.Z0_AnotherNumberInfo, ParentDocAddress, (x) => x == "US", ErrorMessageTextIfInvalidCountry);
			}

			protected override void CheckZ0_NVarChar()
			{
				base.CheckZ0_NVarChar();
				OrganisationValidation.ValidateStateForPGAAddress(ParentDocAddress.Factory, ParentDocAddress.Z0_NVarCharInfo, CountryCode, State);
			}

			protected override void CheckZ0_Decimal()
			{
				base.CheckZ0_Decimal();
				OrganisationValidation.ValidateStateForPGAAddress(ParentDocAddress.Z0_DecimalInfo, Address);
			}
		}

		public void TestValidatePowerOfAttorneyDocumentDates()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importer = Factory.New<OrgHeader>();
			var poaDocument = importer.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			var wrapper = OrgHeaderWrapper.New(importer);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			declaration.IOROrgPK = importer.PK;

			var a2av = new AuthorityToActValidator();
			var receivedDateRequired = AuthorityToActValidator.GetReceivedDateRequiredString(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)");
			var expiryDateRequiredForPeriodicDocument = AuthorityToActValidator.GetExpiryDateRequiredForPeriodicDocumentString(a2av.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)");
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, receivedDateRequired);
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, expiryDateRequiredForPeriodicDocument);

			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, receivedDateRequired);
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, expiryDateRequiredForPeriodicDocument);

			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today;
			poaDocument.EQ_ValidToDate = ZDateTime.Today;
			var pOAWillExpireSoon = AuthorityToActValidator.GetPOAWillExpireSoonString(ZDate.Today.ToString(), "organization (eDocs > Document Tracking)", a2av.CountrySpecificNameForPOA);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, receivedDateRequired);
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, expiryDateRequiredForPeriodicDocument);
			AssertHasWarningContaining(declaration.IOROrgPKInfo, pOAWillExpireSoon);

			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(60);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoWarningContaining(declaration.IOROrgPKInfo, pOAWillExpireSoon);

			var pOAExpired = AuthorityToActValidator.GetPOAExpiredString(ZDate.Today.AddDays(-1).ToString(), "organization (eDocs > Document Tracking)", a2av.CountrySpecificNameForPOA);
			poaDocument.EQ_ValidToDate = ZDateTime.Today;
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertNoMessageErrorContaining(declaration.IOROrgPKInfo, pOAExpired);

			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddDays(-1);
			declaration.JE_OA_DeclarantAddress_ZAddress.ValidateOrgPK();
			AssertHasMessageErrorContaining(declaration.IOROrgPKInfo, pOAExpired);
		}
	}
}
