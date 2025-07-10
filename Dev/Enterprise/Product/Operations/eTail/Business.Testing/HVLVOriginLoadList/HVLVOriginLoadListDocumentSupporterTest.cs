using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadListDocumentSupporter))]
	class HVLVOriginLoadListDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.HVLVOriginLoadList, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.HVLVBookingHeaderCustomiseDocuments, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(1, supporter.ListOfSupportedDataContexts.Count);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<HVLVOriginLoadList>();
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
