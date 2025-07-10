using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceDocumentSupporter))]
	public class CusUSLVClearanceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.INVALID, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.USLVClearanceCustomiseDocuments, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(0, supporter.ListOfSupportedDataContexts.Count);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusUSLVClearance>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			supporter = GetDocumentSupportableBusinessObject().DocumentSupporter;
		}

		DocumentSupporter supporter;

		#endregion
	}
}
