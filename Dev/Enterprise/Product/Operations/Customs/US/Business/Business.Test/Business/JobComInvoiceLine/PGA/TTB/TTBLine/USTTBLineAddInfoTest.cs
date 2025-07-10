using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USTTBLineAddInfo))]
	public class USTTBLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("TTBLine.Data.HumanReadableName", "TTB Data", TTBLine.Data.HumanReadableName);
		}

		public void TestValidationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			AssertEquals(typeof(USImportTTBLineAddInfoValidation), ttbLine.AddInfoValidation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(USExportTTBLineAddInfoValidation), ttbLine.AddInfoValidation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USTTBLineAddInfo(TTBLine.B7_AddInfoDataInfo);
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

		TTBLine TTBLine
		{
			get { return ttbLine ?? (ttbLine = InvoiceLine.TTBLines.AddNew()); }
		}

		TTBLine ttbLine;

		#endregion
	}
}
