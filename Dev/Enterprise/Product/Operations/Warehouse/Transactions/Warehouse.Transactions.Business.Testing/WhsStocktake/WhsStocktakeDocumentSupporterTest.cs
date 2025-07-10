using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeDocumentSupporter))]
	internal class WhsStocktakeDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestGetContactOrganisation()
		{
			var stocktake = (WhsStocktake)BusinessObject;
			var client = Factory.New<OrgHeader>();
			stocktake.WS_OH_Client = client.PK;
			var contact = stocktake.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL,
				DocumentDirection.ANY);
			AssertEquals("The contact should be the client", stocktake.Client.PK, contact.OrgHeader.PK);
		}

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider stocktake = (IEDocsProvider)BusinessObject;
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter),
				stocktake.GetEDocsProviderSupporter().GetType());
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsStocktake, DocSupporter.BusinessContext);
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
			get { return Enterprise.Core.Constants.DataContext.WhsStocktake; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsStocktake>();
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsStocktakeCustomiseDocuments;

		#endregion
	}
}
