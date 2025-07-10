using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderDocumentSupporter))]
	internal class WhsWorkOrdersDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public void TestGetEDocsProviderSupporter()
		{
			IEDocsProvider order = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), order.GetEDocsProviderSupporter().GetType());
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsWorkOrder, DocSupporter.BusinessContext);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.WhsWorkOrder)));
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.WhsPickableDocket)));
		}

		public override void TestGetContactOrganisation()
		{
			var docket = (WhsWorkOrder)BusinessObject;
			var client = Factory.NewWithValidTestData<OrgHeader>();
			docket.WD_OH_Client = client.PK;
			var contact = docket.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", docket.Client.PK, contact.OrgHeader.PK);
		}

		public void TestGetDocumentWrappers()
		{
			var docket = (WhsWorkOrder)BusinessObject;
			DocumentWrapper[] orderWrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsWorkOrder, null);
			AssertEquals("DocWhsWorkOrder", orderWrappers[0].GetType().Name);
			AssertEquals(docket, orderWrappers[0].WrappedObject);

			DocumentWrapper[] pickableDocketWrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPickableDocket, null);
			AssertEquals("DocWhsPickableDocket", pickableDocketWrappers[0].GetType().Name);
			AssertEquals(docket, pickableDocketWrappers[0].WrappedObject);
		}

		#region TestShowReasonForNotPrinting

		protected override bool GetExpectedShowReasonForNotPrinting()
		{
			return false;
		}

		#endregion

		#region Implementation

		protected override Core.Constants.DataContext DataContext
		{
			get { return Enterprise.Core.Constants.DataContext.WhsPickableDocket; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsWorkOrder>();
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsWorkOrderCustomiseDocuments;

		#endregion
	}
}
