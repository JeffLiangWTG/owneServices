using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNMFSLineAddInfo))]
	public class USNMFSLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationMethod()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var nmfsLine = InvoiceLine.NMFSLines.AddNew();
			AssertEquals(typeof(USImportNMFSLineAddInfoValidation), nmfsLine.AddInfoValidation.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(USExportNMFSAddInfoValidation), nmfsLine.AddInfoValidation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USNMFSLineAddInfo(InvoiceLine.NMFSLines.AddNew().B7_AddInfoDataInfo);
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
