using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportAddInfoJobComInvoiceHeaderValidationTest : CommonImportAddInfoJobComHeaderValidationTest
	{
		public void TestFlags()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AddInfoJobComInvoiceHeader addInfo = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryValidationMode", false, ((FormalImportAddInfoJobComInvoiceHeaderValidation)addInfo.Validation).IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", false, ((FormalImportAddInfoJobComInvoiceHeaderValidation)addInfo.Validation).IsCargoReleaseValidationMode);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryValidationMode", true, ((FormalImportAddInfoJobComInvoiceHeaderValidation)addInfo.Validation).IsEntrySummaryValidationMode);
			AssertEquals("IsCargoReleaseValidationMode", true, ((FormalImportAddInfoJobComInvoiceHeaderValidation)addInfo.Validation).IsCargoReleaseValidationMode);
		}

		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			var invoice = declaration.Invoices.AddNew();
			var addInfo = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
			var validation = (FormalImportAddInfoJobComInvoiceHeaderValidation)addInfo.Validation;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
		}

		public void TestCheckUS_ZoneStatus()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertUS_ZoneStatus(invoice);
		}

		public void TestCheckUS_DateOfExport()
		{
			declaration.JE_DateOfArrival = ZDateTime.Today;
			invoice.US_DateOfExport = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(invoice.US_DateOfExportInfo, "Date of Arrival can not be before the Export Date");
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			invoice.US_DateOfExport = ZDateTime.Empty;
			invoice.US_DateOfExport = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrors(invoice.US_DateOfExportInfo);
		}

		public void TestCheckUS_FirstSale()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoice.US_FirstSale = "~";
			AssertHasMessageErrorContaining(invoice.US_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_FirstSale = "Y";
			AssertNoMessageErrorContaining(invoice.US_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_FirstSale = " ";
			AssertNoMessageErrorContaining(invoice.US_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoice.US_FirstSale = "~";
			AssertHasMessageErrorContaining(invoice.US_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_FirstSale = "N";
			AssertNoMessageErrorContaining(invoice.US_FirstSaleInfo, ListValidation.InvalidCodeMessageError);
			invoice.US_FirstSale = ZString.Empty;
			AssertNoMessageErrors(invoice.US_FirstSaleInfo);
		}

		public void TestCheckUS_PrivilegedStatusDate()
		{
			invoice.US_PrivilegedStatusDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageError(invoice.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceHeaderValidation.PrivilegedStatusDateCannotBeFuture);
			invoice.US_PrivilegedStatusDate = ZDateTime.Today;
			AssertNoMessageError(invoice.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceHeaderValidation.PrivilegedStatusDateCannotBeFuture);
		}

		public void TestCheckUS_UC_NKCountryOfExport()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertUS_UC_NKCountryOfExport(invoice);
		}

		public void TestCheckUS_SC_NKCountryOfOrigin()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertUS_UC_NKCountryOfOrigin(invoice);
		}

		//CS00080536 - None of tariffs were prior notice required - but user sent optional phone no which was not valid.
		//contact phone no & contact name are optional fields, but if sent, must still be valid.
		//It had a FDA code 'FD2' and FDA contact number which goes into FD04 - still needs to be a valid phone no.
		public void TestCheckUS_FDAContact()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoice.US_FDAContactName = ZString.Empty;
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			invoice.US_FDAContactName = "John";
			invoice.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			invoice.US_FDAContactPhoneNo = "";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "+1 555 555";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "2345678901";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "1234567890";
			AssertHasMessageError("This should give error as well - field is 10 digits Max BUT no US Area Codes begin with 1 (US Country Code value)", invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoice.US_FDAContactName = "";
			AssertHasMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EnableCRL = false;
			invoice.US_FDAContactName = "";
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.Contact);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			invoice.US_FDAContactName = "";
			AssertHasMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactName = "BOB";
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactPhoneNo = "";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "+1 555 555";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "2345678901";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "1234567890";
			AssertHasMessageError("This should give error as well - field is 10 digits Max BUT no US Area Codes begin with 1 (US Country Code value)", invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactEmail = "r";
			AssertHasWarning(invoice.US_FDAContactEmailInfo, "Invalid email format");
			invoice.US_FDAContactEmail = "123456780123456780123456780johan.anderson@tpg.com";
			AssertNoWarning(invoice.US_FDAContactEmailInfo, "Invalid email format");
			invoice.US_FDAContactEmail = "johan.anderson@tpg.com";
			AssertNoWarning(invoice.US_FDAContactEmailInfo, "Invalid email format");
			invoiceLine.US_FSISInd = ZString.Empty;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			nhtsa.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			invoice.US_FDAContactName = ZString.Empty;
			AssertHasMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactName = "INC";
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactPhoneNo = ZString.Empty;
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "2345678901";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
		}

		public void TestCheckUS_FDAContactEmailForFDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			invoiceLine.ACE_FDALines.AddNew();
			invoice.US_FDAContactName = "";
			AssertHasMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactName = "BOB";
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
			invoice.US_FDAContactPhoneNo = "";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "+1 555 555";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			invoice.US_FDAContactPhoneNo = "2345678901";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
		}

		public void TestCheckUS_FDAContacts_FW3()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			invoice.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertBrokerContactDetailsForFW3(invoice);
			invoiceLine.US_FWSInd = "";
			invoice.AddInfoValidation.ValidateUS_FDAContactName();
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
		}

		public void TestCheckUS_FDAContactNameForStandAloneInvoice()
		{
			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateUS_FDAContactName();
			});
		}

		public void TestCheckUS_FDAContactPhoneNoForStandAloneInvoice()
		{
			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateUS_FDAContactPhoneNo();
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			invoice = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		void AssertBrokerContactDetailsForFW3(JobComInvoiceHeader invoice)
		{
			invoice.US_FDAContactName = "";
			AssertHasMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			invoice.US_FDAContactName = "John";
			AssertNoMessageError(invoice.US_FDAContactNameInfo, ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
			invoice.US_FDAContactPhoneNo = "";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			invoice.US_FDAContactPhoneNo = "+1 82 9384";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			invoice.US_FDAContactPhoneNo = "1829384";
			AssertHasMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			invoice.US_FDAContactPhoneNo = "8293845001";
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			AssertNoMessageError(invoice.US_FDAContactPhoneNoInfo, ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
			invoice.US_FDAContactEmail = "";
			AssertHasMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			invoice.US_FDAContactEmail = "r";
			AssertHasWarning(invoice.US_FDAContactEmailInfo, "Invalid email format");
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			invoice.US_FDAContactEmail = "johan.anderson@tpg.com";
			AssertNoWarning(invoice.US_FDAContactEmailInfo, "Invalid email format");
			AssertNoMessageError(invoice.US_FDAContactEmailInfo, ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
		}
	}
}
