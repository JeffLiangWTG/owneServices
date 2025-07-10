using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFWSHeaderAddInfo))]
	public class USFWSHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals(typeof(USImportFWSHeaderAddInfoValidation), invoiceLine.ExportFWS.AddInfoValidation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(USExportFWSHeaderAddInfoValidation), invoiceLine.ExportFWS.AddInfoValidation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USFWSHeaderAddInfo(InvoiceLine.FWSHeaders.AddNew().B7_AddInfoDataInfo);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
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
