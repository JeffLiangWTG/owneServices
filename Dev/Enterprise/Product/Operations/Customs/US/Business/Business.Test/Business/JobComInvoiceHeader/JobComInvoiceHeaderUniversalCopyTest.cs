using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderUniversalCopyTest : BaseAddInfoUniversalCopyTest
	{
		protected override void AssertHasOtherNodes(string[] allNodeNames)
		{
			CombineAssertions(() =>
			{
				AssertCollectionContains("CustomFields", "CustomFields", allNodeNames);
				AssertCollectionContains("RelatedDocuments", "RelatedDocuments", allNodeNames);
				AssertCollectionContains("DocAddresses", "DocAddresses", allNodeNames);
			});
		}

		protected override Customs.Business.IAddInfoManager GetManager()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Invoices.AddNew();
		}
	}
}
