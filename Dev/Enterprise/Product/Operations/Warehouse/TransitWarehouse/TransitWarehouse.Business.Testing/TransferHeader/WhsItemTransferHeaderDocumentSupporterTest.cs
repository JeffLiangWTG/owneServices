using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderDocumentSupporter))]
	class WhsItemTransferHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			var docSupporter = ((IDocumentSupportable)transferHeader).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			var docSupporter = ((IDocumentSupportable)transferHeader).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemTransferHeaderCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WhsItemTransferHeader>();
		}

		#endregion
	}
}
