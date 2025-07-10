using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentCollection))]
	class WhsAdjustmentCollectionTest : WhsDocketCollectionTestCase<WhsAdjustmentCollection>
	{
		#region Implementation

		protected override WhsAdjustmentCollection GetCollectionToTest()
		{
			return new WhsAdjustmentCollection(Factory);
		}

		protected override string[] DocketTypesForTest
		{
			get { return new[] { DocketType.Codes.Adjustment }; }
		}

		#endregion
	}
}
