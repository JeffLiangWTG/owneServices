using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderProcessTaskCollection))]
	class WhsVASOrderProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsVASOrderProcessTaskCollection>
	{
		#region Implementation

		protected override WhsVASOrderProcessTaskCollection GetCollectionToTestCore()
		{
			return new WhsVASOrderProcessTaskCollection(Factory.NewWithValidTestData<WhsVASOrder>());
		}

		#endregion
	}
}
