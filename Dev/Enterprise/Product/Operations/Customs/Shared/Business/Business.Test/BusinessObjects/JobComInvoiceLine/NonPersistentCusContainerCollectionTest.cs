using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(NonPersistentCusContainerCollection))]
	sealed class NonPersistentCusContainerCollectionTest : NonPersistentCusContainerCollectionTest<NonPersistentCusContainerCollection>
	{
		protected override NonPersistentCusContainerCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			return new NonPersistentCusContainerCollection(invoiceLine);
		}
	}
}
