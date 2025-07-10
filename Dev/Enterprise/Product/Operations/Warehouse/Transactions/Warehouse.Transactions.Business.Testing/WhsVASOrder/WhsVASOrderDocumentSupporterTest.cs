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
	[TestedType(typeof(WhsVASOrderDocumentSupporter))]
	internal class WhsVASOrderDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsVASOrder, DocSupporter.BusinessContext);
		}

		public override void TestGetDocBusinessObjects()
		{
			//remove this test when you want to implement printing of WhsVASOrder Documents.
			Assert(true);
		}

		public override void TestGetContactOrganisation()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Factory.Save();

			var contact = vasOrder.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", vasOrder.Client.PK, contact.OrgHeader.PK);
		}

		#region TestShowReasonForNotPrinting

		protected override bool GetExpectedShowReasonForNotPrinting() => false;

		public void TestGetEDocsProviderSupporter()
		{
			var vasOrder = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), vasOrder.GetEDocsProviderSupporter().GetType());
			AssertEquals("DocManagerInfo should be of type WVO", "WVO", vasOrder.DocManagerInfo.DocManagerCode);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.WhsVASOrder)));
		}

		#endregion

		#region Implementation

		protected override Constants.DataContext DataContext => Constants.DataContext.WhsVASOrder;

		protected override BusinessObject GetNewBusinessObject() => Factory.New<WhsVASOrder>();

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsVASOrderCustomizeDocuments;

		#endregion
	}
}
