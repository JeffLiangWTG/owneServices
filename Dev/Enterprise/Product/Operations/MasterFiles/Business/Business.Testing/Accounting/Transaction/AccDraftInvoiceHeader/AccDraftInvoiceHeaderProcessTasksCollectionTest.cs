using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceHeaderProcessTaskCollection))]
	public class AccDraftInvoiceHeaderProcessTasksCollectionTest : ProcessTaskCollectionTest<AccDraftInvoiceHeaderProcessTaskCollection>
	{
		protected override AccDraftInvoiceHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var invoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			return new AccDraftInvoiceHeaderProcessTaskCollection(invoice);
		}
	}
}
