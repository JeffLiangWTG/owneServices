using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsTransferLookupsTest : WhsDocketLookupsTest<WhsTransfer>
	{
		#region TestSubTypes

		protected override void TestSubTypesCore()
		{
			var transfer = GetNewBusinessObject();
			var lookups = (WhsTransferLookups)transfer.Lookups;
			AssertEquals("Precondition", false, transfer.WD_IsPutawayTransfer);
			AssertEquals(3, lookups.SubTypes.Count);
			AssertEquals(true, lookups.SubTypes.ContainsCode(TransferType.Codes.InterWhsSource));
			AssertEquals(true, lookups.SubTypes.ContainsCode(TransferType.Codes.InterWhsDest));
			AssertEquals(true, lookups.SubTypes.ContainsCode(TransferType.Codes.Internal));

			AssertEquals(3, lookups.SubTypesWithPutawayType.Count);
			AssertEquals(true, lookups.SubTypesWithPutawayType.ContainsCode(TransferType.Codes.InterWhsSource));
			AssertEquals(true, lookups.SubTypesWithPutawayType.ContainsCode(TransferType.Codes.InterWhsDest));
			AssertEquals(true, lookups.SubTypesWithPutawayType.ContainsCode(TransferType.Codes.Internal));

			var putawayTransfer = GetNewBusinessObject();
			putawayTransfer.WD_IsPutawayTransfer = true;
			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair(NonPersistentTransferType.Codes.Putaway, NonPersistentTransferType.Descriptions.Putaway) },
				((WhsTransferLookups)putawayTransfer.Lookups).SubTypesWithPutawayType);

			var outboundDockDoorTransfer = GetNewBusinessObject();
			outboundDockDoorTransfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair(NonPersistentTransferType.Codes.OutboundDockDoor, NonPersistentTransferType.Descriptions.OutboundDockDoor) },
				((WhsTransferLookups)outboundDockDoorTransfer.Lookups).SubTypesWithPutawayType);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var autoCreatedReplenishmentTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			autoCreatedReplenishmentTransfer.WD_IsPickFaceReplenishment = true;
			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair(NonPersistentTransferType.Codes.AutoCreatedReplenishment, NonPersistentTransferType.Descriptions.AutoCreatedReplenishment) },
				((WhsTransferLookups)autoCreatedReplenishmentTransfer.Lookups).SubTypesWithPutawayType);
		}

		#endregion

		#region TestPicksForReplenishment

		protected override void TestPicksForReplenishmentCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row1", 2, 2);
			var client2 = Helper.CreateClient("Tst");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 915m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R2", data.Part1, 85m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();

			var order2 = Helper.CreateWhsOrder(data.Org1, warehouse2, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			var pick3 = Factory.New<WhsPick>();
			pick3.AddOrders(new[] { order3 });
			Factory.Save();

			var transfer = GetNewBusinessObject();
			transfer.WD_OH_Client = data.Org1.PK;
			transfer.WD_WW_Whs = data.Whs1.PK;
			var collection = transfer.Lookups.PicksForReplenishment;
			CombineAssertions(() =>
			{
				AssertEquals("Collection count correct", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder("Pick collections correct", new[] { pick1.PK, pick3.PK }, collection.Select(p => p.PK));
				AssertContainsExactElementsInAnyOrder("Pick Order External References correct", new[] { "O1", "O3" }, collection.Select(p => p.Orders[0].WD_ExternalReference));
			});
		}

		public void TestPicksForReplenishment_Defaults()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var transfer = GetNewBusinessObject();
			transfer.WD_OH_Client = data.Org1.PK;
			transfer.WD_WW_Whs = data.Whs1.PK;
			var collection = transfer.Lookups.PicksForReplenishment;
			AssertFilterDefaults(collection, data.Whs1.PK, "Warehouse", "Property");
			AssertFilterDefaults(collection, (ZString)DocketStatus.Codes.New, "Status", "Property");
		}

		void AssertFilterDefaults(WhsPickCollectionForTransfers collection, IZType value, string filterName, string propertyName, bool isRemovable = false)
		{
			var filterDefault = collection.FilterBusinessObjectDefaults[$"{filterName}:{propertyName}"];
			AssertEquals(propertyName, filterDefault.PropertyName);
			AssertEquals(value, filterDefault.Value);
			AssertEquals(isRemovable, filterDefault.IsRemovable);
		}

		#endregion
	}
}
