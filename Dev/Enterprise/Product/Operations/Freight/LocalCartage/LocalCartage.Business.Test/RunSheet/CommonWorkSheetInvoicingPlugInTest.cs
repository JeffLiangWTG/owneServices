using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonWorkSheetInvoicingPlugInTest : TestCaseWithFactory
	{
		public void TestIJobNumber_JobNumber()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var invoicingPlugIn = (IJobNumber)new CommonWorkSheetInvoicingPlugIn(workSheet, cartage);
			AssertEquals("Precondition: Job Number", ZString.Empty, invoicingPlugIn.JobNumber);
			cartage.JJ_ConsignmentID = "T01";
			AssertEquals("Expecting correct job number", "T01", invoicingPlugIn.JobNumber);
		}

		public void TestIJobHeaderParentCore_Precondition()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var invoicingPlugIn = (IJobHeaderParentCore)new CommonWorkSheetInvoicingPlugIn(workSheet, cartage);
			AssertEquals("PK", cartage.PK, invoicingPlugIn.PK);
			AssertEquals("Table Name", "JobCartage", invoicingPlugIn.TableName);
			AssertEquals("Is in database", cartage.IsInDatabase, invoicingPlugIn.IsInDatabase);
			AssertEquals("Factory", cartage.Factory, invoicingPlugIn.Factory);
		}

		public void TestIJobInvoicingPlugIn_Precondition()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var cartage = Factory.New<CommonCartage>();
			var invoicingPlugIn = (IJobInvoicingPlugIn)new CommonWorkSheetInvoicingPlugIn(workSheet, cartage);
			AssertNotNull(invoicingPlugIn.InvoicingSupporter);
			AssertType<CommonWorkSheetInvoicingSupporter>(invoicingPlugIn.InvoicingSupporter);
		}
	}
}
