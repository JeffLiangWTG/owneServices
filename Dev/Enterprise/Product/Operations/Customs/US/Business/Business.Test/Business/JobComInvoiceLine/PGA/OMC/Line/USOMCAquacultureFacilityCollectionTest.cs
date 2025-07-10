using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOMCAquacultureFacilityCollection))]
	public class USOMCAquacultureFacilityCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = InvoiceLine.OMCHeaders.AddNew();
			return new USOMCAquacultureFacilityCollection(header);
		}

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

		#endregion
	}
}
