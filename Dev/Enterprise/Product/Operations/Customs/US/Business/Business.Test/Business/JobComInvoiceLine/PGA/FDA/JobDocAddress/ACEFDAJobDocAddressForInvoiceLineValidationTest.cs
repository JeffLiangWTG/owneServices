using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	sealed class ACEFDAJobDocAddressForInvoiceLineValidationTest : ACEFDAJobDocAddressValidationTest
	{
		public override void TestCheckLaboratoryDocAddress()
		{
			base.TestCheckLaboratoryDocAddress();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.TOB_INV;
			FDA.LaboratoryDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);
			AssertNoMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.LaboratoryDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);
			AssertNoMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.LaboratoryDocAddress.E2_OA_Address = ZGuid.Empty;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertNoMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.LabRequiredForIVN);

			FDA.LaboratoryDocAddress.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(FDA.LaboratoryDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public override void TestCheckManufacturerDocAddress()
		{
			base.TestCheckManufacturerDocAddress();

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			var manufacturer = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
			manufacturer.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			manufacturer.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			var consolidator = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Consolidator);
			consolidator.E2_OA_Address = USAddress.PK;
			manufacturer.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			manufacturer = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
			manufacturer.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			consolidator.E2_OA_Address = ZGuid.Empty;
			manufacturer.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			manufacturer.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			var growner = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Grower);
			growner.E2_OA_Address = USAddress.PK;
			manufacturer.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			USAddress.OA_RN_NKCountryCode = "PR";
			manufacturer.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "PR";
			FDA.ManufacturerDocAddress.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "US";
			FDA.ManufacturerDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "PR";
			USAddress.OA_RN_NKCountryCode = "CN";
			FDA.ManufacturerDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			FDA.ManufacturerDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public void TestCheckManufacturerDocAddress_Country()
		{
			_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
			FDA.ManufacturerDocAddress.E2_AddressOverride = true;
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "PR";
			FDA.ManufacturerDocAddress.E2_RN_NKCountryCode = "PR";
			AssertHasMessageError(FDA.ManufacturerDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "US";
			FDA.ManufacturerDocAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);

			FDA.US_ProdCountry = "PR";
			FDA.ManufacturerDocAddress.E2_RN_NKCountryCode = "CN";
			AssertNoMessageError(FDA.ManufacturerDocAddress.E2_RN_NKCountryCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShouldBeUSWhenManufacturerIsPR);
		}

		public override void TestCheckDeliverToPartyDocAddress()
		{
			base.TestCheckDeliverToPartyDocAddress();
			FDA.US_DeliverToPartyAddress = ZGuid.Empty;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);
			AssertNoMessageError(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.DeliverToPartyDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeliverToPartyRequired);
			AssertNoMessageError(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			FDA.DeliverToPartyDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.DeliverToPartyDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public override void TestCheckFSVPImporterDocAddress()
		{
			base.TestCheckFSVPImporterDocAddress();
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_FSVPImporterAddress = ZGuid.Empty;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.FSVPImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.FSVPImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAForcePN = false;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			FDA.US_ProductCode = "3223E56";
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.FSX);
			FDA.FSVPImporterDocAddress.E2_OA_Address = USAddress.PK;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.RNE);
			FDA.FSVPImporterDocAddress.E2_OA_Address = USAddress.PK;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_ADD;
			FDA.US_PNC = "";
			FDA.US_PND = true;
			FDA.US_FDAForcePN = true;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FSVPImpShouldNotBeSent);
		}

		public void TestCheckFSVPImporterDocAddress_Contact()
		{
			var fda = FDA;
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_FSVPImporterAddress = USAddress.PK;

			var emailRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email");
			var contactNameRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name");
			var phoneNumberRequired = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone");

			fda.FSVPImporterDocAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(fda.FSVPImporterDocAddress.E2_ContactInfo, emailRequired);
			Assert(!fda.FSVPImporterDocAddress.E2_ContactInfo.HasMessageError(contactNameRequired));
			Assert(!fda.FSVPImporterDocAddress.E2_ContactInfo.HasMessageError(phoneNumberRequired));

			DeclarationTestHelper.AddFSVPContact(USAddress, "DNZ", "WTG", null, "test@test.com", null);
			fda.FSVPImporterDocAddress.OrganisationPK = ZGuid.Empty;
			fda.FSVPImporterDocAddress.OrganisationPK = USAddress.PK;
			AssertNoMessageError(fda.FSVPImporterDocAddress.E2_ContactInfo, emailRequired);
		}

		public void TestCheckFSVPImporterDocAddress_Email_Override()
		{
			var fda = FDA;
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_FSVPImporterAddress = USAddress.PK;
			fda.FSVPImporterDocAddress.E2_AddressOverride = true;
			FDA.FSVPImporterDocAddress.Validation.ValidateE2_Email();
			AssertHasMessageError(FDA.FSVPImporterDocAddress.E2_EmailInfo, "You have not entered an Email.");
			FDA.FSVPImporterDocAddress.E2_Email = "test@test.com";
			AssertNoMessageError(FDA.FSVPImporterDocAddress.E2_EmailInfo, "You have not entered an Email.");
		}

		public override void TestCheckShipperDocAddress()
		{
			base.TestCheckShipperDocAddress();
			FDA.US_OA_ShipperAddress = ZGuid.Empty;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.ShipperDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);
			AssertNoMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			FDA.ShipperDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.ShipperDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public override void TestCheckFDAImporterDocAddress()
		{
			base.TestCheckFDAImporterDocAddress();
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";

			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_FDAImporterAddress = ZGuid.Empty;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.FDAImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.InvoiceLine.Declaration.US_EnableSPN = true;
			FDA.InvoiceLine.Declaration.US_EnableENS = false;
			FDA.InvoiceLine.Declaration.US_EnableCRL = false;
			FDA.FDAImporterDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.FDAImpRequired);

			FDA.InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			FDA.FDAImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public void TestCheckFDAImporterDocAddress_Contact()
		{
			FDA.US_FDAImporterAddress = USAddress.PK;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.FDAImporterDocAddress.E2_OA_Address = USAddress.PK;
			var errorMsg = "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Contact();
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_ContactInfo, errorMsg);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_ContactInfo, errorMsg);

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Contact();
			AssertHasMessageErrorContaining(FDA.FDAImporterDocAddress.E2_ContactInfo, errorMsg);

			FDA.FDAImporterDocAddress.E2_AddressOverride = true;
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(FDA.FDAImporterDocAddress.E2_ContactInfo, errorMsg);
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_ContactInfo, "You have not entered a Name.");
			FDA.FDAImporterDocAddress.E2_Contact = "WTGDNZ";
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_ContactInfo, "You have not entered a Name.");
		}

		public void TestCheckFDAImporterDocAddress_Contact_Override()
		{
			FDA.US_FDAImporterAddress = USAddress.PK;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			FDA.FDAImporterDocAddress.E2_AddressOverride = true;

			FDA.FDAImporterDocAddress.Validation.ValidateE2_Email();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_EmailInfo, "You have not entered an Email/Fax.");
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Fax_Formatted();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_Fax_FormattedInfo, "You have not entered an Email/Fax.");

			FDA.FDAImporterDocAddress.E2_Email = "test@test.com";
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_EmailInfo, "You have not entered an Email/Fax.");
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Fax_Formatted();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_Fax_FormattedInfo, "You have not entered an Email/Fax.");

			FDA.FDAImporterDocAddress.E2_Email = "";
			FDA.FDAImporterDocAddress.E2_Fax_Formatted = "test@test.com";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Email();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_EmailInfo, "You have not entered an Email/Fax.");
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_Fax_FormattedInfo, "You have not entered an Email/Fax.");

			FDA.FDAImporterDocAddress.E2_Email = "test1@test.com";
			FDA.FDAImporterDocAddress.E2_Fax_Formatted = "test2@test.com";
			FDA.FDAImporterDocAddress.Validation.ValidateE2_Email();
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_EmailInfo, "You have not entered an Email/Fax.");
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_Fax_FormattedInfo, "You have not entered an Email/Fax.");

			FDA.FDAImporterDocAddress.Validation.ValidateE2_Phone_Formatted();
			AssertHasMessageError(FDA.FDAImporterDocAddress.E2_Phone_FormattedInfo, "You have not entered a Work Phone.");

			FDA.FDAImporterDocAddress.E2_Phone_Formatted = "10086";
			AssertNoMessageError(FDA.FDAImporterDocAddress.E2_Phone_FormattedInfo, "You have not entered a Work Phone.");
		}

		public override void TestCheckGoodsOwnerDocAddress()
		{
			base.TestCheckGoodsOwnerDocAddress();
			FDA.GoodsOwnerDocAddress.E2_OA_Address = ZGuid.Empty;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
			FDA.GoodsOwnerDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.GoodsOwnerDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.GoodsOwnerDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerRequired);
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerOnlyRequiredForFood);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.GoodsOwnerDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertHasMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.OwnerOnlyRequiredForFood);
		}

		public void TestCheckGoodsOwnerDocAddress_CountryIsMX()
		{
			USAddress.OA_State = ZString.Empty;
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
			FDA.GoodsOwnerDocAddress.E2_OA_Address = USAddress.PK;
			FDA.GoodsOwnerDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, "State should not be empty.");

			USAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			FDA.GoodsOwnerDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.GoodsOwnerDocAddress.E2_OA_AddressInfo, "State should not be empty.");
		}

		public void TestCheckInitialImporterDocAddress()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.InitialImporter);
			FDA.InitialImporterDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);
			AssertNoMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.InitialImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);
			AssertHasMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			FDA.InitialImporterDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertHasMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);

			FDA.InitialImporterDocAddress.E2_OA_Address = USAddress.PK;
			AssertNoMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.DeviceInitialImporterRequired);
			AssertNoMessageError(FDA.InitialImporterDocAddress.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public void TestCheckDocAddressNotRequired_NoExplicitDeclareInFDA()
		{
			var goodsLocation = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsLocation);
			goodsLocation.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(goodsLocation.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			goodsLocation.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(goodsLocation.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			var sponsor = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Sponsor);
			sponsor.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(sponsor.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			sponsor.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(sponsor.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			var thirdPartyLaboratory = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ThirdPartyLaboratory);
			thirdPartyLaboratory.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(thirdPartyLaboratory.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);

			thirdPartyLaboratory.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(thirdPartyLaboratory.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
		}

		public void TestCheckConsolidatorDocAddress()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			var consolidator = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Consolidator);
			consolidator.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			consolidator.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(consolidator.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			consolidator.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			consolidator.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(consolidator.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertHasMessageError(consolidator.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
		}

		public void TestCheckGrowerDocAddress()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			var grower = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Grower);
			grower.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(grower.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(grower.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			grower.E2_OA_Address = USAddress.PK;
			AssertHasMessageError(grower.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(grower.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			grower.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(grower.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertNoMessageError(grower.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);

			grower.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(grower.E2_OA_AddressInfo, ACEFDAJobDocAddressForInvoiceLineValidation.AddressNotAppliedMessage);
			AssertHasMessageError(grower.E2_OA_AddressInfo, USACEFDAAddInfoInvoiceLineValidation.ManufacturerOrAltAddressRequired);
		}
	}
}
