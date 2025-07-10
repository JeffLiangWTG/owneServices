using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeaderDocumentSupporter))]
	public class HVLVBookingHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.HVLVBookingHeader, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.HVLVBookingHeaderCustomiseDocuments, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(0, supporter.ListOfSupportedDataContexts.Count);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<HVLVBookingHeader>();
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
