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
	[TestedType(typeof(WhsTransferDocumentSupporter))]
	internal class WhsTransferDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestGetContactOrganisation()
		{
			var transfer = (WhsTransfer)BusinessObject;
			var client = Factory.New<OrgHeader>();
			transfer.WD_OH_Client = client.PK;
			var contact = transfer.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertEquals("The contact should be the client", transfer.Client.PK, contact.OrgHeader.PK);
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsTransfer, DocSupporter.BusinessContext);
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
			get { return Enterprise.Core.Constants.DataContext.WhsTransfer; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsTransfer>();
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsTransferCustomiseDocuments;

		#endregion
	}
}
