using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public class PackableItemSplitHelperTest : PackingTestCaseWithFactory
	{
		#region TestSplitItem

		public void TestSplitItem()
		{
			Data.CreatePackingData();
			var packableItemOnLine1 = Data.DummyLine1.PackableItems.Single();
			AssertEquals("Quantity should be 100", 100m, packableItemOnLine1.Quantity);

			var splitedPackableITem = PackableItemSplitHelper.SplitItem(packableItemOnLine1, 10);
			AssertEquals("Quantity should be 90.", 90m, packableItemOnLine1.Quantity);
			AssertEquals("Quantity should be 10.", 10m, splitedPackableITem.Quantity);
		}

		#endregion

		#region TestSplitItem_Quantity_MoreInfoInException

		public void TestSplitItem_Quantity_MoreInfoInException()
		{
			var item = Factory.New<DummyPackableItemWithWrongSpliting>();
			item.ZD1_NumberUnit = Guid.NewGuid();
			item.Quantity = 100m;
			AssertEquals("Quantity should be 100", 100m, item.Quantity);

			AssertExceptionThrown(typeof(InvalidOperationException), $@"Split() Function on IPackableItem should Split the Quantity Correctly.
Packable Item PK: {item.PK}
Table Prefix: ZD1
Packable Item Quantity: 1
Quantity to split: 10
Is Deleted: False
Is In Database: False
This error is seen because Packable Item Qty and Qty to split are different.
This can be because Split has failed, the scales of the two decimals are different because one was rounded to fit a database property scale (i.e. 1.55556 vs 1.556), or other unknown reason.", () => PackableItemSplitHelper.SplitItem(item, 10));

			item.Delete();
			item.InDB = true;

			AssertExceptionThrown(typeof(InvalidOperationException), $@"Split() Function on IPackableItem should Split the Quantity Correctly.
Packable Item PK: {item.PK}
Table Prefix: ZD1
Packable Item Quantity: 1
Quantity to split: 12
Is Deleted: True
Is In Database: True
This error is seen because Packable Item Qty and Qty to split are different.
This can be because Split has failed, the scales of the two decimals are different because one was rounded to fit a database property scale (i.e. 1.55556 vs 1.556), or other unknown reason.", () => PackableItemSplitHelper.SplitItem(item, 12));

			var notBusinessObject = new NotBusinessObject();

			AssertExceptionThrown(typeof(InvalidOperationException), $@"Split() Function on IPackableItem should Split the Quantity Correctly.
Packable Item PK: {notBusinessObject.PK}
Table Prefix: ABC
Packable Item Quantity: 0
Quantity to split: 15
Is Deleted: 
Is In Database: 
This error is seen because Packable Item Qty and Qty to split are different.
This can be because Split has failed, the scales of the two decimals are different because one was rounded to fit a database property scale (i.e. 1.55556 vs 1.556), or other unknown reason.", () => PackableItemSplitHelper.SplitItem(notBusinessObject, 15));
		}

		#endregion

		#region DummyPackableItems

		public class DummyPackableItemWithWrongSpliting : DummyPackableItem, IPackableItem
		{
			public DummyPackableItemWithWrongSpliting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			IPackableItem IPackableItem.Split(ZDecimal qtyToSplit)
			{
				var splitItem = Factory.New<DummyPackableItemWithWrongSpliting>();
				splitItem.ZD1_NumberUnit = Guid.NewGuid();
				splitItem.Quantity = 1;
				return splitItem;
			}

			public bool? InDB { get; set; }

			public override bool IsInDatabase => InDB ?? base.IsInDatabase;
		}

		public class NotBusinessObject : IPackableItem
		{
			public ZGuid PK { get; } = Guid.NewGuid();

			public string TablePrefix => "ABC";

			public bool IsDeleted => throw new NotImplementedException();

			public bool IsInDatabase => false;

			public ZDecimal Quantity => 0;

			public GroupingKey Key => throw new NotImplementedException();

			public void ReMerge()
			{
				throw new NotImplementedException();
			}

			public IPackableItem Split(ZDecimal qtyToSplit)
			{
				return new NotBusinessObject();
			}
		}

		#endregion
	}
}
