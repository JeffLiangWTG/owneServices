using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class ImportEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestStopGeneratingTwoCusEntryHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "ABC";

			var invoice = declaration.Invoices.AddNew();
			invoice.US_DateOfExport = new ZDateTime(2015, 1, 2);
			invoice.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.US_DateOfExport = new ZDateTime(2015, 1, 1);
			invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
		}

		public void TestInvoiceLinesWithSupTariffGetMerged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "ABC";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.US_SupTariff = "9801";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.US_SupTariff = "9801";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines.Count);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
		}

		public void TestInvoiceLinesWithTSCAOrODSData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "ABC";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";

			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);

			invoiceLine1.US_TSCAInd = ZString.Empty;
			invoiceLine2.US_TSCAInd = ZString.Empty;

			invoiceLine1.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);

			invoiceLine1.US_TSCAInd = ZString.Empty;
			invoiceLine2.US_TSCAInd = ZString.Empty;
		}
	}
}
