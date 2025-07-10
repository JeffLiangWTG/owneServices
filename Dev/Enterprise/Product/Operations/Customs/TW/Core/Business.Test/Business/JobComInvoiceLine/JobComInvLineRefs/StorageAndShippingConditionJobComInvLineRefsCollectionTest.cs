using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(StorageAndShippingConditionJobComInvLineRefsCollection))]
	sealed class StorageAndShippingConditionJobComInvLineRefsCollectionTest : JobComInvLineRefsCollectionTest<StorageAndShippingConditionJobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<StorageAndShippingConditionJobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new StorageAndShippingConditionJobComInvLineRefsCollection(jobComInvoice);
		}
	}
}
