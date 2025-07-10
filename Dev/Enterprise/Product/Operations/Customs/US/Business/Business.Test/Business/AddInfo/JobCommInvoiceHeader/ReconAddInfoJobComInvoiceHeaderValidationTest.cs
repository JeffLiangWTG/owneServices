using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconAddInfoJobComInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckUS_CH_ReconEntry()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			invoice.US_CH_ReconEntry = ZGuid.Empty;
			AssertHasMessageError(invoice.US_CH_ReconEntryInfo, ReconAddInfoJobComInvoiceHeaderValidation.ReconEntryIsMandatory);
			invoice.US_CH_ReconEntry = ZGuid.NewZGuid();
			AssertHasMessageError(invoice.US_CH_ReconEntryInfo, ReconAddInfoJobComInvoiceHeaderValidation.ReconEntryIsMandatory);
			invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries.AddNew().CH_PK;
			AssertNoMessageError(invoice.US_CH_ReconEntryInfo, ReconAddInfoJobComInvoiceHeaderValidation.ReconEntryIsMandatory);
		}
	}
}
