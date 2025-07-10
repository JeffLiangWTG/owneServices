using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PGAIndicatorsTest : TestCaseWithFactory
	{
		public void TestHasInvoiceLinesWithAMSRequireFDADate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EnableENS = true;
			declaration1.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration1.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = "C";
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO7;
			Assert(declaration1.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;
			Assert(!declaration1.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			invoiceLine.US_AMSInd = "D";
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			Assert(declaration1.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			amsLine.US_Program = AMSProgramList.Codes.MO2;
			Assert(declaration1.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);

			amsLine.US_Program = AMSProgramList.Codes.MO3;
			Assert(!declaration1.PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2);
		}

		public void TestHasInvoiceLinesWith()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = ZString.Empty;
			invoiceLine.FDAs.AddNew();
			invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);

			invoiceLine.FDAs.RemoveAndDeleteAll();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithFDADisclaim);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithFDADisclaim);

			var fwsheader = invoiceLine.FWSHeaders.AddNew();
			fwsheader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithFWSProcessingCodeWithEDS);
		}

		public void TestHasPGADetailsRequiringDeclarationDate()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			Assert(!dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);

			invoiceLine.US_VNEInd = "D";
			Assert(dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);

			invoiceLine.US_VNEInd = "";
			invoiceLine.US_NHTSAIndicator = "D";
			Assert(dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);

			invoiceLine.US_NHTSAIndicator = "";
			invoiceLine.US_PSTIndicator = "D";
			Assert(dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);

			invoiceLine.US_PSTIndicator = "";
			invoiceLine.US_FSISInd = "D";
			Assert(dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);

			invoiceLine.US_FSISInd = "";
			invoiceLine.US_LaceyIndicator = "D";
			Assert(dec.PGAFlags.HasPGADetailsRequiringDeclarationDate);
		}

		public void TestHasInvoiceLinesWithFWS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithFWS);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithFWS);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			Assert(declaration.PGAFlags.HasInvoiceLinesWithFWS);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithFWS);
			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			Assert(declaration.PGAFlags.HasInvoiceLinesWithFWS);
		}

		public void TestHasInvoiceLinesWithSection301Or232()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceline1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceline1.US_SupTariff = "99011001";
			var invoiceline2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceline2.US_SupTariff = "99011002";
			var invoiceline3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceline3.US_SupTariff = "99011003";
			var invoiceline4 = invoice2.JobComInvoiceLines.AddNew();
			invoiceline4.US_SupTariff = "99011004";
			AssertEquals("No invoice line with section 301 or 232 exists", false, declaration.PGAFlags.HasInvoiceLinesWithSection301Or232);

			invoiceline1.US_SupTariff = "99031001";
			AssertEquals("One invoice line with section 301 or 232 exists", true, declaration.PGAFlags.HasInvoiceLinesWithSection301Or232);
		}

		public void TestHasInvoiceLinesWithADCVDCaseReported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals("No AD/CVD case reported", false, declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported);

			invoiceLine1.US_ADDCaseNo = "A462105011";
			AssertEquals("Has AD/CVD case reported", true, declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported);

			invoiceLine1.US_ADDCaseNo = ZString.Empty;
			AssertEquals("No AD/CVD case reported", false, declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported);

			invoiceLine2.US_CVDCaseNo = "C462105011";
			AssertEquals("Has AD/CVD case reported", true, declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported);

			invoiceLine2.US_CVDCaseNo = ZString.Empty;
			invoiceLine3.US_ADDCaseNo = "A462105011";
			invoiceLine3.US_CVDCaseNo = "C462105011";
			AssertEquals("Has AD/CVD case reported", true, declaration.PGAFlags.HasInvoiceLinesWithADCVDCaseReported);
		}

		public void TestAllInvoiceLinesAreFromCA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "US";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = "XB";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfOrigin = "XC";
			AssertEquals("Not all invoice lines are from CA", false, declaration.PGAFlags.AllInvoiceLinesAreFromCA);

			invoiceLine1.US_UC_NKCountryOfOrigin = "XA";
			AssertEquals("All invoice lines are from CA", true, declaration.PGAFlags.AllInvoiceLinesAreFromCA);
		}

		public void TestAllInvoiceLinesAreUSGoodsReturned()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			AssertEquals("Not all invoice lines are US Goods Returned", false, declaration.PGAFlags.AllInvoiceLinesAreReturnedGoods);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "01234567";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = JobComInvoiceHeader.ReturnedGoodsTariffPrefix + "10";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = JobComInvoiceHeader.ReturnedGoodsTariffPrefix + "10";
			AssertEquals("Not all invoice lines are US Goods Returned", false, declaration.PGAFlags.AllInvoiceLinesAreReturnedGoods);

			invoiceLine1.JI_Tariff = JobComInvoiceHeader.ReturnedGoodsTariffPrefix + "10";
			AssertEquals("All invoice lines are US Goods Returned", true, declaration.PGAFlags.AllInvoiceLinesAreReturnedGoods);
		}

		public void TestHasInvoiceLinesWithPGARequireIOR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithNHTSARequireIOR);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var nhtsa = invoiceLine.NHTSALines.AddNew();
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithNHTSARequireIOR);

			var document = nhtsa.NHTSADocuments.AddNew();
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;
			Assert(declaration.PGAFlags.HasInvoiceLinesWithNHTSARequireIOR);
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.FabricatingManufacturer;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithNHTSARequireIOR);
		}

		public void TestHasInvoiceLinesWithAPHIS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithAPHIS);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithAPHIS);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			Assert(declaration.PGAFlags.HasInvoiceLinesWithAPHIS);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!declaration.PGAFlags.HasInvoiceLinesWithAPHIS);
			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			Assert(declaration.PGAFlags.HasInvoiceLinesWithAPHIS);
		}

		public void TestHasInvoiceLinesWithOGAFDAPGAFDA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = ZString.Empty;
			invoiceLine.FDAs.AddNew();
			invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);

			invoiceLine.FDAs.RemoveAndDeleteAll();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithFDADisclaim);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithOGAFDA);
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithPGAFDA);
			AssertEquals(false, dec.PGAFlags.HasInvoiceLinesWithFDADisclaim);

			var fwsheader = invoiceLine.FWSHeaders.AddNew();
			fwsheader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			AssertEquals(true, dec.PGAFlags.HasInvoiceLinesWithFWSProcessingCodeWithEDS);
		}
	}
}
