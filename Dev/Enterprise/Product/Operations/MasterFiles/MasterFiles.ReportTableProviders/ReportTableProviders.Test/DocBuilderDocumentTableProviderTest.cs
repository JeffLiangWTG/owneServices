using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.ReportTableProviders.Testing
{
	internal class DocBuilderDocumentTableProviderTest : TestCaseWithFactory
	{
		public void TestDocBuilderDocumentTableProviderReturnsMenuPath()
		{
			var docbuilderDocuments = Provider.GetDataTable("", "", TestReport, false);
			CombineAssertions("", () =>
			 {
				 foreach (DataRow row in docbuilderDocuments.Rows)
				 {
					 if (!SuppressedBusinessContext.Contains(row["BusinessContext"].ToString()))
					 {
						 AssertNotEquals(@"Document business context should have .
You will get this failure due to either there is no ZController was set for the document supported BizO or no SecurityCheckPoint was set for the ZController's related ZModule.
Consider implementing IDocumentBusinessContext to the ZModule or adding this business context to the suppressed list.", row["SecurityPath"].ToString(), row["BusinessContext"].ToString());
					 }
				 }
			 });
		}

		//Not all documents have menus in GUI
		readonly string[] SuppressedBusinessContext = new string[]
		{
			"ForwardingContainer",
			"CFSAirCargoOutturn",
			"CnsgmentJobService",
			"ConsolAgent",
			"ContainerRelease",
			"DeliveryAgent",
			"JobCartageRunSheet",
			"JobService",
			"Package",
			"PackageHeader",
			"ParticipantStmnt",
			"Protest",
			"ReconDeclaration",
			"StatementSummary",
			"SupplierBookingLine",
			"HVLVConsignment",
			"HVLVOuterPackage",
			"TransitRecTranspUnt",
			"TransitReceiveASN",
			"LTConsignment",
			"TransitDispTranspUnt",
			"TransitDspConsignmnt",
			"TransitRcvConsignmnt",
			"Statement",
			"DetentionInvoice",
			"GateTransportCYDet",
			"GateTransportCFSDet",
			"ApplicantRatingTest"
		};

		Report TestReport;
		DocBuilderDocumentTableProvider Provider;
		protected override void SetUp()
		{
			base.SetUp();
			TestReport = new Report(new DocumentPack(Factory.New<StmMenuItem>()), null);
			Provider = new DocBuilderDocumentTableProvider();
		}
	}
}
