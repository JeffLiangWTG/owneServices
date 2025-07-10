using System.ComponentModel;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderComponentLineCollection))]
	class WhsDynamicWorkOrderComponentLineCollectionTest : WhsComponentOrderLineCollectionTest<WhsDynamicWorkOrderComponentLineCollection>
	{
		#region TestAllowNew_MainXorSecondaryProduct

		public void TestAllowNew_MainXorSecondaryProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			var dynamicWorkOrderLine = dynamicWorkOrder.Lines[0];
			var collection = (IBindingList)new WhsDynamicWorkOrderComponentLineCollection(dynamicWorkOrderLine);

			dynamicWorkOrderLine.IsMainInwardProcessedItem = false;
			dynamicWorkOrderLine.IsSecondaryInwardProcessedItem = false;
			AssertEquals("Should *not* be allowed to add new when no flags are set.", false, collection.AllowNew);

			dynamicWorkOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals("Should be allowed to add new when a single flags is set.", true, collection.AllowNew);

			dynamicWorkOrderLine.IsMainInwardProcessedItem = false;
			dynamicWorkOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertEquals("Should be allowed to add new when a single flags is set.", true, collection.AllowNew);

			dynamicWorkOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals("Should *not* be allowed to add new when both flags are set.", false, collection.AllowNew);
		}

		#endregion

		#region TestOnAdded_SetsFKs

		public void TestOnAdded_SetsFKs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			var dynamicWorkOrderLine = dynamicWorkOrder.Lines[0];

			var collection = new WhsDynamicWorkOrderComponentLineCollection(dynamicWorkOrderLine);
			AssertEquals(0, collection.Count);

			var newLine = Factory.New<WhsDynamicWorkOrderLine>();
			AssertEquals(ZGuid.Empty, newLine.WE_WE_ParentDocketLine);
			AssertEquals(ZGuid.Empty, newLine.WE_WD);
			AssertEquals(ZGuid.Empty, newLine.WE_WE_MatchingLine);

			collection.Add(newLine);
			AssertEquals(dynamicWorkOrderLine.PK, newLine.WE_WE_ParentDocketLine);
			AssertEquals(dynamicWorkOrder.PK, newLine.WE_WD);
			AssertEquals(ZGuid.Empty, newLine.WE_WE_MatchingLine);
		}

		#endregion

		#region TestDocket

		protected override void TestDocketCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			var dynamicWorkOrderLine = dynamicWorkOrder.Lines[0];

			var collection = new WhsDynamicWorkOrderComponentLineCollection(dynamicWorkOrderLine);
			AssertEquals(dynamicWorkOrder, collection.Docket);
		}

		#endregion

		#region Implementation

		protected override WhsDynamicWorkOrderComponentLineCollection GetCollectionToTest()
		{
			var dwoLine = Factory.NewWithValidTestData<WhsDynamicWorkOrderLine>();
			dwoLine.IsMainInwardProcessedItem = true;
			dwoLine.IsSecondaryInwardProcessedItem = false;
			return new WhsDynamicWorkOrderComponentLineCollection(dwoLine);
		}

		#endregion
	}
}
