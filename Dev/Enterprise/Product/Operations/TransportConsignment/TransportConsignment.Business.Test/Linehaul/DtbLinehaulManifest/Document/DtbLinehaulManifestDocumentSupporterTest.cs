using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbLinehaulManifestDocumentSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.DtbLinehaulManifest, GetNewDocumentSupporter().BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, GetNewDocumentSupporter().CustomisationSecurityCheckpoint);
		}

		public void TestGetDocumentWrappers()
		{
			var supporter = GetNewDocumentSupporter();
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var wrappersForSomeGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertEquals("We print document for 1 job, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals(true, GetNewDocumentSupporter().IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.GenericFreightJob))));
		}

		#region TestShowReasonForNotPrinting

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", false, GetNewDocumentSupporter().ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		#endregion

		#region Implementation

		DtbLinehaulManifestDocumentSupporter GetNewDocumentSupporter()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			return new DtbLinehaulManifestDocumentSupporter(manifest);
		}

		#endregion
	}
}
