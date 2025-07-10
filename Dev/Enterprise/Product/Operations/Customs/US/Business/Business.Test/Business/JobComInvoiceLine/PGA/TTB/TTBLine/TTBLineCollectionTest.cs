using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBLineCollection))]
	public class TTBLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.TTBLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.TTBLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TTBLineCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<TTBLine>();
			result.B7_ParentID = InvoiceLine.PK;
			result.B7_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
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
