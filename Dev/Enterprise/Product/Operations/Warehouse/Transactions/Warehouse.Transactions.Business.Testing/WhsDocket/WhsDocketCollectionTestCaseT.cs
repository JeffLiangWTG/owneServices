using System.ComponentModel;
using System.Linq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketCollectionTestCase<T> : WhsActiveBusinessObjectCollectionTestCaseWithHelper<T> where T : WhsDocketCollection
	{
		#region TestSetAllowNew

		public void TestSetAllowNew()
		{
			var collection = (WhsDocketCollection)GetCollectionToTest();

			AssertEquals("Collection.AllowNewCore should return " + AllowNew.ToString() + " if SetAllowNew(bool) was not called.",
				AllowNew, ((IBindingList)collection).AllowNew);

			collection.SetAllowNew(true);
			AssertEquals(true, ((IBindingList)collection).AllowNew);

			collection.SetAllowNew(false);
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		protected virtual bool AllowNew
		{
			get { return true; }
		}

		#endregion

		#region RelationshipFilter

		public void TestRelationshipFilter()
		{
			TestRelationshipFilterCore();
		}

		protected virtual void TestRelationshipFilterCore()
		{
			var collection = (WhsDocketCollection)GetCollectionToTest();
			var whs = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient();

			Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsTransfer(client, whs);
			Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsAdjustment(client, whs);
			Helper.CreateWhsWorkOrder(client, whs);

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = client.PK;
			dynamicWorkOrder.WD_WW_Whs = whs.PK;

			Assert("Collection should only allow dockets of the same type to be added.",
				collection.All(d => DocketTypesForTest.Contains(d.WD_DocketType.ToString())));
		}

		protected abstract string[] DocketTypesForTest { get; }

		#endregion
	}
}
