using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickGroupHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetPickGroupDescription

		public void TestGetPickGroupDescription()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup2.Description = (NoResString)"Desc";
			AssertEquals("", PickGroupHelper.GetPickGroupDescription(0, collection));
			AssertEquals("1", PickGroupHelper.GetPickGroupDescription(1, collection));
			AssertEquals("2 - Desc", PickGroupHelper.GetPickGroupDescription(2, collection));
		}

		#endregion

		#region TestGetPickGroupFromString

		public void TestGetPickGroupFromString()
		{
			AssertEquals(new ZShort(0), PickGroupHelper.GetPickGroupFromString(""));
			AssertEquals(new ZShort(1), PickGroupHelper.GetPickGroupFromString("1"));
			AssertEquals(new ZShort(11), PickGroupHelper.GetPickGroupFromString("1.1"));
			AssertEquals(new ZShort(0), PickGroupHelper.GetPickGroupFromString("A"));
			AssertEquals(short.MaxValue, PickGroupHelper.GetPickGroupFromString("111111"));
		}

		#endregion

		#region TestPickGroupForBindingMaxLength

		public void TestPickGroupForBindingMaxLength()
		{
			AssertEquals(5 + 3 + 256, PickGroupHelper.PickGroupForBindingMaxLength);
		}

		#endregion
	}
}
