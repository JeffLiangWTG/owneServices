using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReportingEntryLineDutyDataProviderTest : TestCaseWithFactory
	{
		public void TestCalculateException()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			ReportingDeclarationDutyDataProvider declarationProvider = new ReportingDeclarationDutyDataProvider(declaration, false);
			ReportingEntryLineDutyDataProvider entryLineProvider = new ReportingEntryLineDutyDataProvider(entryLine, declarationProvider);

			Assert(((IEntryLineOrInvoiceLineDutyData)entryLineProvider).CalculateException.IsEmpty);

			((IEntryLineOrInvoiceLineDutyData)entryLineProvider).CalculateException = "Test error";
			AssertEquals("Test error", invoiceLine.TariffCalculateExceptionMessage);
		}
	}
}
