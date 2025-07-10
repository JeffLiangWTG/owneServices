using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdHocServiceJobDocumentSupporter))]
	internal class WhsAdHocServiceJobsDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsAdHocServiceJob, DocSupporter.BusinessContext);
		}

		public override void TestGetDocBusinessObjects()
		{
			//remove this test when you want to implement printing of WhsAdHocServiceJob Documents.
			Assert(true);
		}

		public override void TestGetContactOrganisation()
		{
			var serviceJob = (WhsAdHocServiceJob)BusinessObject;
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			serviceJob.WSJ_OH_Client = client.PK;
			serviceJob.WSJ_WW_Whs = whs.PK;
			Factory.Save();

			var contact = serviceJob.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", serviceJob.Client.PK, contact.OrgHeader.PK);
		}

		#region TestShowReasonForNotPrinting

		protected override bool GetExpectedShowReasonForNotPrinting()
		{
			return false;
		}

		public void TestGetEDocsProviderSupporter()
		{
			var order = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), order.GetEDocsProviderSupporter().GetType());
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsAdHocServiceJob)));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsAdHocServiceJob>();
		}

		protected override Constants.DataContext DataContext
		{
			get { return Constants.DataContext.WhsAdHocServiceJob; }
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsAdHocServiceJobCustomizeDocuments;

		#endregion
	}
}
