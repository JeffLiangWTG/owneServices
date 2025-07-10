using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsPickLineInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsPickLineInfo>
	{
		#region TestConstructor_PickLines

		public void TestConstructor_PickLines()
		{
			var collection1 = new WhsPickLineInfoCollection(Enumerable.Empty<WhsPickLine>(), new WhsPickInfo());
			AssertNotNull(collection1);
			AssertEquals(0, collection1.Count);

			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var line1 = Helper.CreateWhsPickLine(order1.Lines[0], receive.Inventory[0], 10);

			var collection2 = new WhsPickLineInfoCollection(new[] { line1 }, new WhsPickInfo());
			AssertEquals(1, collection2.Count);
			AssertEquals(line1.PK.ToGuid(), collection2[0].PKs[0]);

			var line2 = Helper.CreateWhsPickLine(order1.Lines[0], receive.Inventory[0], 10);

			var collection3 = new WhsPickLineInfoCollection(new[] { line1, line2 }, new WhsPickInfo());
			AssertEquals(1, collection3.Count);
			AssertContainsExactElementsInAnyOrder(collection3[0].PKs, new[] { line1.PK.ToGuid(), line2.PK.ToGuid() });
		}

		#endregion

		#region TestConstructor_PickLinesForTrolleyPicking

		public void TestConstructor_PickLinesForTrolleyPicking()
		{
			var collection1 = new WhsPickLineInfoCollection(new SortedDictionary<WhsPickLine, PickLineAdditionalInfo>(), new WhsPickInfo());
			AssertNotNull(collection1);
			AssertEquals(0, collection1.Count);

			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var line1 = Helper.CreateWhsPickLine(order1.Lines[0], receive.Inventory[0], 10);

			var collection2 = new WhsPickLineInfoCollection(new[] { line1 }, new WhsPickInfo());
			AssertEquals(1, collection2.Count);
			AssertEquals(line1.PK.ToGuid(), collection2[0].PKs[0]);

			var line2 = Helper.CreateWhsPickLine(order1.Lines[0], receive.Inventory[0], 10);

			var collection3 = new WhsPickLineInfoCollection(new[] { line1, line2 }, new WhsPickInfo());
			AssertEquals(1, collection3.Count);
			AssertContainsExactElementsInAnyOrder(collection3[0].PKs, new[] { line1.PK.ToGuid(), line2.PK.ToGuid() });
		}

		#endregion

		#region Implementation

		protected new WhsPickLineInfoCollection Parent
		{
			get
			{
				return (WhsPickLineInfoCollection)base.Parent;
			}
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsPickLineInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsPickLineInfoCollection);
		}

		protected override WhsPickLineInfo GetNewObjectInfo()
		{
			return new WhsPickLineInfo();
		}

		protected override DataObjectInfoCollection<WhsPickLineInfo> GetNewObjectInfoCollection()
		{
			return new WhsPickLineInfoCollection();
		}

		#endregion
	}
}
