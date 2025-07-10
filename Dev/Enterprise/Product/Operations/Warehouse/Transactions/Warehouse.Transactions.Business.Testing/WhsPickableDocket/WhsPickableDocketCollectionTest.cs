using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickableDocketCollection))]
	public sealed class WhsPickableDocketCollectionTest : WhsPickableDocketCollectionTest<WhsPickableDocketCollection>
	{
		protected override string[] DocketTypesForTest => new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder };

		protected override WhsPickableDocketCollection GetCollectionToTest()
		{
			return new WhsPickableDocketCollection(Factory);
		}
	}
}
