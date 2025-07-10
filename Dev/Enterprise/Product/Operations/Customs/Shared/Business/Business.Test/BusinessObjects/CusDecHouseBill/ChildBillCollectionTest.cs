using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ChildBillCollection<Bill, BaseJobDeclaration>))]
	sealed class ChildBillCollectionTest : ChildBillCollectionTest<ChildBillCollection<Bill, BaseJobDeclaration>>
	{
		protected override ChildBillCollection<Bill, BaseJobDeclaration> GetCollectionToTest()
		{
			return new ChildBillCollection<Bill, BaseJobDeclaration>(Bill, Bill.Declaration);
		}
	}
}
