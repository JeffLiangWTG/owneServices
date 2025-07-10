using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderLineCollection))]
	public class WhsVASOrderLineCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsVASOrderLineCollection>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var collection = new WhsVASOrderLineCollection(vasOrder);
			AssertEquals(true, ((IBindingList)collection).AllowNew);

			vasOrder.ReadOnly = true;
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.ReadOnly = false;
			vasOrder.WVO_OH_Client = ZGuid.Empty;
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.WVO_OH_Client = ZGuid.Invalid;
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.WVO_OH_Client = data.Org1.PK;
			AssertEquals(true, ((IBindingList)collection).AllowNew);

			vasOrder.WarehousePK = ZGuid.Empty;
			AssertEquals(false, vasOrder.WVO_WA_ServiceArea.IsValid);
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.WarehousePK = ZGuid.Invalid;
			AssertEquals(false, vasOrder.WVO_WA_ServiceArea.IsValid);
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.WarehousePK = data.Whs1.PK;
			AssertEquals(false, vasOrder.WVO_WA_ServiceArea.IsValid);
			AssertEquals(false, ((IBindingList)collection).AllowNew);

			vasOrder.WVO_WA_ServiceArea = data.Whs1.Areas[0].PK;
			AssertEquals(true, ((IBindingList)collection).AllowNew);
		}

		#endregion

		#region TestSetDefaultsForNewElement

		public void TestSetDefaultsForNewElement()
		{
			var order = Factory.New<WhsVASOrder>();
			var line1 = order.Lines.AddNew();
			AssertEquals(1, line1.WVL_LineNumber);

			var line2 = order.Lines.AddNew();
			AssertEquals(1, line1.WVL_LineNumber);
			AssertEquals(2, line2.WVL_LineNumber);

			line1.Delete();
			var line3 = order.Lines.AddNew();
			AssertEquals(2, line2.WVL_LineNumber);
			AssertEquals(3, line3.WVL_LineNumber);

			line3.Delete();
			var line4 = order.Lines.AddNew();
			AssertEquals(2, line2.WVL_LineNumber);
			AssertEquals(3, line4.WVL_LineNumber);
		}

		#endregion

		#region TestCollection

		public void TestCollection()
		{
			var collection = GetCollectionToTest();
			var line = collection.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { line }, collection);
			AssertEquals(collection.Relationship.Master.PK, line.WVL_WVO_VASOrder);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		protected override WhsVASOrderLineCollection GetCollectionToTest()
		{
			return new WhsVASOrderLineCollection(Factory.New<WhsVASOrder>());
		}

		#endregion
	}
}
