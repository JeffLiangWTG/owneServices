using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallDocumentSupporter))]
	internal class OrgSalesCallDocumentSupporterTest : DocumentSupporterTest
	{
		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CommunicationManagerCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.Communication, DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContext()
		{
			Assert("Core.Constants.DataContext.BusinessObject is Supported", DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.BusinessObject)));
			Assert("Core.Constants.DataContext.GenericFreightJob is Supported", DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<OrgSalesCall>();
		}

		OrgSalesCallDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new OrgSalesCallDocumentSupporter(Factory.New<OrgSalesCall>())); }
		}
		OrgSalesCallDocumentSupporter documentSupporter;

		#endregion
	}
}
