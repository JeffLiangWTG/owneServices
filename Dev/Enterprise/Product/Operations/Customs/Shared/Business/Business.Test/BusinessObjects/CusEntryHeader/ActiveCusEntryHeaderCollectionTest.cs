using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	sealed class ActiveCusEntryHeaderCollectionTest : ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			return new ActiveCusEntryHeaderCollection(Declaration);
		}
	}
}
