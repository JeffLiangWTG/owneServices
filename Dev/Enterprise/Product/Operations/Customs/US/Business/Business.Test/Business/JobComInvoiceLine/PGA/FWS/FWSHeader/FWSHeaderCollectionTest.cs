using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FWSHeaderCollection))]
	public class FWSHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.FWSHeaders;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.FWSHeaders;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		public void TestSetDefaultsForNewChild()
		{
			var dateToSet = new ZDateTime(2016, 11, 09);
			var stringToSet = new ZString("1234");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InspecDate = dateToSet;
			declaration.US_InspecFirms = stringToSet;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_LinePrice = 200m;
			var collection = invoiceLine.FWSHeaders;
			var fwsHeader = collection.AddNew();
			AssertEquals(stringToSet, fwsHeader.US_FIRMS);
			AssertEquals(300m, fwsHeader.US_InvCurrPGAValue);

			fwsHeader.US_InvCurrPGAValue = 150m;
			var fwsHeader2 = invoiceLine2.FWSHeaders.AddNew();
			AssertEquals(150m, fwsHeader2.US_InvCurrPGAValue);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FWSHeaderCollection(InvoiceLine);
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
