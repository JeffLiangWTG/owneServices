using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOMCAquacultureFacilityAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizations()
		{
			var aquaculture = Header.AquacultureFacilities.AddNew();
			AssertNotNull(aquaculture.AddInfoLookups.Organizations);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		OMCHeader Header
		{
			get { return header ?? (header = InvoiceLine.OMCHeaders.AddNew()); }
		}
		OMCHeader header;

		#endregion
	}
}
