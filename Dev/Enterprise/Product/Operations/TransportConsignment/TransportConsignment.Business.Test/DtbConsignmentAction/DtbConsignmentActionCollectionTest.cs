using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentActionCollection))]
	sealed class DtbConsignmentActionCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentActionCollection>
	{
		public void TestCollectionHasObject()
		{
			var address = Helper.CreateConsignmentAddress();
			var action1 = Helper.CreateConsignmentAction(address);
			var action2 = Helper.CreateConsignmentAction(address);
			var action3 = Helper.CreateConsignmentAction(null);

			var actions = new DtbConsignmentActionCollection(address);
			AssertEquals("Collection should have 2 objects.", 2, actions.Count);
			AssertContainsExactElementsInAnyOrder("Should contain action1 and action2", actions, new[] { action1, action2 });
		}

		#region Implementation

		protected override DtbConsignmentActionCollection GetCollectionToTest()
		{
			return new DtbConsignmentActionCollection(Helper.CreateConsignmentAddress());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

	}
}
