using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISHeaderCollection))]
	public class APHISHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TST";
			Declaration.IOROrgPK = header.PK;

			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			AssertEquals(aphisHeader.ApplicantOrgPK, header.PK);
			AssertEquals("Inspection Details always required, we should add a row to Inspections grid when adding new APHIS header", 1, aphisHeader.Inspections.Count);
		}

		public void TestSuppressAddNewInspectionLineDuringDataImport()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			AssertEquals("Inspection line is added when import is not in progress", 1, aphisHeader.Inspections.Count);

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				aphisHeader = InvoiceLine.APHISHeaders.AddNew();
				AssertEquals("Inspection line is not added when import is in progress", 0, aphisHeader.Inspections.Count);
			}
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.APHISHeaders;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.APHISHeaders;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
			invoiceLine.SetReadOnlyIncludingChildren(true);
			AssertEquals(false, collection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISHeaderCollection(InvoiceLine);
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
