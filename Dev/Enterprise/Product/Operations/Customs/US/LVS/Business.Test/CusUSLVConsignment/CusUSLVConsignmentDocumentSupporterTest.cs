using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignmentDocumentSupporter))]
	class CusUSLVConsignmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestBusinessContext()
		{
			var supporter = GetDocumentSupporter();
			AssertEquals(BusinessContext.USLowValueBill, supporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var supporter = GetDocumentSupporter();
			AssertEquals(Env.Security.USLVClearanceCustomiseDocuments, supporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			var supporter = GetDocumentSupporter();
			AssertEquals(0, supporter.ListOfSupportedDataContexts.Count);
		}

		public void TestShowDocumentsInDynamicMenu()
		{
			var supporter = GetDocumentSupporter();
			AssertEquals(false, supporter.ShowDocumentsInDynamicMenu);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusUSLVConsignment>();

		CusUSLVConsignmentDocumentSupporter GetDocumentSupporter() => (CusUSLVConsignmentDocumentSupporter)GetDocumentSupportableBusinessObject().DocumentSupporter;

		#endregion
	}
}
