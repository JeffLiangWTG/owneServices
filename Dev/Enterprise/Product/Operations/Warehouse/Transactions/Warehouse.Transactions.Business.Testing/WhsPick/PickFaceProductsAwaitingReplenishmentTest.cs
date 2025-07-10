using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(PickFaceProductsAwaitingReplenishment))]
	public class PickFaceProductsAwaitingReplenishmentTest : WhsNonPersistentBusinessObjectTestCase
	{
		public void TestPickFacesAwaitingReplenishmentWithSameWhsClientProduct()
		{
			var data = new PickFaceViewTestData(Factory, warehouses: 2, clients: 1, locationsPerWarehouse: 2);
			var locationsWhs1 = data.Locations[data.Warehouses[0]];
			var locationsWhs2 = data.Locations[data.Warehouses[1]];
			var normalLocation = Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locationsWhs1[0].WLV_WLT_LocationType = normalLocation.PK;
			locationsWhs2[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locationsWhs1[1], replenishMin++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locationsWhs2[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locationsWhs1[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);

			var receive2PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[1].PK, "R2", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive2PK, data.Parts[0].PK, 100m, locationsWhs2[0].PK);
			transactionHelper.FinaliseDocket(receive2PK);
			Factory.Save();

			var transfer1 = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Helper.Notify);
			var transferLine1 = transactionHelper.CreateWhsTransferLine(transfer1, data.Parts[0].PK, 100m, locationsWhs1[0].PK, locationsWhs1[1].PK);
			var transferLineBizO1 = Factory.GetBizOsForPK(transferLine1.ToGuid());
			transferLineBizO1[0].RunPreSaveValidation();

			var transfer2 = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[1].PK, "T2", Helper.Notify);
			var transferLine2 = transactionHelper.CreateWhsTransferLine(transfer2, data.Parts[0].PK, 100m, locationsWhs2[0].PK, locationsWhs2[1].PK);
			var transferLineBizO2 = Factory.GetBizOsForPK(transferLine2.ToGuid());
			transferLineBizO2[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var pick1 = (WhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick2 = (WhsPick)transactionHelper.CreatePickNew(false, false, order2.PK);
			pick2.PickPriority = 1;
			var order3 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[1].PK, data.Clients[0].PK, "order3");
			transactionHelper.CreateWhsOrderLine(order3.PK, data.Parts[0].PK, 30m);
			var pick3 = (WhsPick)transactionHelper.CreatePickNew(false, false, order3.PK);
			pick3.PickPriority = 2;
			Factory.Save();

			var pickFaceProductsAwaitingReplenishment1 = pick1.DistinctClientProductPickFacesInWhsAwaitingReplenishment[0];
			var pickFaceProductsAwaitingReplenishment2 = pick3.DistinctClientProductPickFacesInWhsAwaitingReplenishment[0];

			var pickFaceView1Collection = pickFaceProductsAwaitingReplenishment1.PickFacesAwaitingReplenishmentWithSameWhsClientProduct;
			var pickFaceView2Collection = pickFaceProductsAwaitingReplenishment2.PickFacesAwaitingReplenishmentWithSameWhsClientProduct;

			AssertEquals(2, pickFaceView1Collection.Count);
			AssertEquals(1, pickFaceView2Collection.Count);
			AssertEquals(pick2.PK, pickFaceView1Collection[0].WWP_WP);
			AssertEquals(pick1.PK, pickFaceView1Collection[1].WWP_WP);
		}

		public void TestPickFaceProductsAwaitingReplenishmentWithEmptyClientProduct()
		{
			var pickFaceProductsAwaitingReplenishment = new PickFaceProductsAwaitingReplenishment(ZGuid.Empty, ZGuid.Empty, Factory);

			AssertEquals("", pickFaceProductsAwaitingReplenishment.ClientCode);
			AssertEquals("", pickFaceProductsAwaitingReplenishment.ProductCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var client = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			return new PickFaceProductsAwaitingReplenishment(client.PK, product.PK, Factory);
		}
	}
}
