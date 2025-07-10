using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderCollection))]
	class WhsDynamicWorkOrderCollectionTest : WhsComponentOrderCollectionTest<WhsDynamicWorkOrderCollection>
	{
		protected override string[] DocketTypesForTest => new[] { DocketType.Codes.DynamicWorkOrder };
	}
}
