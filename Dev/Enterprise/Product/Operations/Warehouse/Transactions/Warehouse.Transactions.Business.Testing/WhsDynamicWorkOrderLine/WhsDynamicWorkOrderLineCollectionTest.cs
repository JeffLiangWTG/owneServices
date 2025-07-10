using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderLineCollection))]
	class WhsDynamicWorkOrderLineCollectionTest : WhsComponentOrderLineCollectionTest<WhsDynamicWorkOrderLineCollection>
	{
		#region Implementation

		protected override WhsDynamicWorkOrderLineCollection GetCollectionToTest()
		{
			return new WhsDynamicWorkOrderLineCollection(Factory.New<WhsDynamicWorkOrder>());
		}

		#endregion
	}
}
