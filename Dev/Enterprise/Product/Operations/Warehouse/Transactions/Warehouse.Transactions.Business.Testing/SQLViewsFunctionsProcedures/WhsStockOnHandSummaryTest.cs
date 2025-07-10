using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStockOnHandSummaryTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship =
				productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Factory.Save();

			var result1 = LoadView_WithProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WithProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = string.Empty;
			if (categoryPK.IsEmpty)
			{
				sql = @"SELECT * FROM WhsStockOnHandSummaryReport(null)";
				result.Load(sql);
			}
			else
			{
				sql = @"SELECT * FROM WhsStockOnHandSummaryReport(@ProductCategoryPK)";
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
				result.Load(sql, sqlParams);
			}

			return result;
		}

		#endregion

		#region TestView

		[TestDate(2019, 1, 1)]
		public void TestView()
		{
			var whs = new WhsWarehouseCollection(Factory);
			whs.Add(Helper.CreateWarehouse("1", "A", 4, 4));
			whs.Add(Helper.CreateWarehouse("2", "A", 4, 4));

			var clients = new OrgHeaderCollection(Factory);
			clients.Add(Helper.CreateClient("1", "1"));
			clients.Add(Helper.CreateClient("2", "2"));

			var parts = new OrgSupplierPartCollection(Factory);
			parts.Add(Helper.CreateProduct(clients[0], "1", OrgPartRelation.RelationshipTypes.Both));
			parts.Add(Helper.CreateProduct(clients[1], "2"));
			parts.Add(Helper.CreateProduct(clients[0], "3"));
			parts.Add(Helper.CreateProduct(clients[0], "4"));
			parts.Add(Helper.CreateProduct(clients[0], "5"));

			SetupData(clients, parts, whs);

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("ResultSet Count", 8, result.Count);

			AssertRow(result[0], whs[0], clients[0], parts[0], 232m, 0m, 5m, 106m, 0m);
			AssertRow(result[1], whs[0], clients[0], parts[2], 234m, 17m, 7m, 88m, 0m);
			AssertRow(result[2], whs[0], clients[0], parts[3], 123m, 0m, 0m, 0m, 0m);
			AssertRow(result[3], whs[0], clients[0], parts[4], 321m, 0m, 0m, 0m, 0m);
			AssertRow(result[4], whs[0], clients[1], parts[1], 211m, 11m, 0m, 0m, 0m);
			AssertRow(result[5], whs[0], clients[1], parts[2], 212m, 0m, 0m, 212m, 0m);
			AssertRow(result[6], whs[1], clients[1], parts[1], 221m, 0m, 0m, 0m, 0m);
			AssertRow(result[7], whs[1], clients[1], parts[2], 222m, 0m, 0m, 0m, 0m);
		}

		#endregion

		#region TestView_UnallocatedOrders

		#region TestView_UnallocatedOrders_SingleProduct_NoPicks

		public void TestView_UnallocatedOrders_SingleProduct_NoPicks()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", part, 10m, true, true);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Report with a single product received should hold a single row.", 1,
				result.Count);
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 0m, 10m, 0m);

			Helper.CreateWhsOrderWithOrderLine(client, whs, "order1", part, 10m);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Products with Unallocated Orders should appear on report.", 1, result.Count);
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 0m, 10m, 10m);

			Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", part, 15m);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Products with Unallocated Orders should appear on report.", 1, result.Count);
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 0m, 10m, 25m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_NoInventory

		public void TestView_UnallocatedOrders_SingleProduct_NoInventory()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", part, 10m, true, true);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Report with a single product received should hold a single row.", 1,
				result.Count);
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 0m, 10m, 0m);

			var order = Helper.CreateWhsOrder(client, whs, "order");
			var orderline = Helper.CreateWhsOrderLine(order, part, 15m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 10m, 0m, 5m);

			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Report with a single product with no inventory holds no rows.", 0,
				result.Count);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", part, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick2.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Products with Unallocated Orders but no stock should not appear on report.", 0, result.Count);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_SingleOrder

		public void TestView_UnallocatedOrders_SingleProduct_SingleOrder()
		{
			var whs = Helper.CreateWarehouse("warehouse1", "A", 4, 4);
			var client = Helper.CreateClient("client1", "client1");
			var part = Helper.CreateProduct(client, "product1");

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "receive", part, 10m, true, true);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs, "order", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part, 20m);
			var pick = Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderLine1, receive.Inventory[0], 5m);
			Helper.CreateWhsPickLine(orderLine2, receive.Inventory[0], 5m);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick.StatusDesc);
			AssertEquals("Precondition: Commited Quantity.", 5m, orderLine1.PickLineQuantity);
			AssertEquals("Precondition: Commited Quantity.", 5m, orderLine2.PickLineQuantity);
			AssertEquals("Precondition: Commited Quantity.", 0m, orderLine3.PickLineQuantity);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Products with Inventory should appear on report.", 1, result.Count);
			AssertRow(result.Single(), whs, client, part, 10m, 0m, 10m, 0m, 25m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_MultiplePicklines

		public void TestView_UnallocatedOrders_SingleProduct_MultiplePicklines()
		{
			var whs = Helper.CreateWarehouse("warehouse1", "A", 4, 4);
			var client = Helper.CreateClient("client1", "client1");
			var part = Helper.CreateProduct(client, "product1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", ZDateTimeOffset.Today, part, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs, "receive2", ZDateTimeOffset.Today, part, 10m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(client, whs, "receive3", ZDateTimeOffset.Today, part, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, "order3", part, 40m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Products with Inventory should appear on report.", 1, result.Count);
			AssertRow(result.Single(), whs, client, part, 35m, 0m, 35m, 0m, 5m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_MultipleWarehouses

		public void TestView_UnallocatedOrders_SingleProduct_MultipleWarehouses()
		{
			var whs = new WhsWarehouseCollection(Factory);
			whs.Add(Helper.CreateWarehouse("warehouse1", "A", 4, 4));
			whs.Add(Helper.CreateWarehouse("warehouse2", "A", 4, 4));
			var client = Helper.CreateClient("client1", "client1");
			var part = Helper.CreateProduct(client, "product1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, whs[0], "receive1", part, 10m, true, true);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs[1], "receive2", part, 15m, true, true);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Each warehouse should hold a row for each receive.", 2, result.Count);
			AssertRow(result[0], whs[0], client, part, 10m, 0m, 0m, 10m, 0m);
			AssertRow(result[1], whs[1], client, part, 15m, 0m, 0m, 15m, 0m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs[0], "order1", part, 15m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick1.StatusDesc);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs[1], "order2", part, 25m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick2.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Unallocated Orders are unique to each warehouse.", 2, result.Count);
			AssertRow(result[0], whs[0], client, part, 10m, 0m, 10m, 0m, 5m);
			AssertRow(result[1], whs[1], client, part, 15m, 0m, 15m, 0m, 10m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_MultipleClients

		public void TestView_UnallocatedOrders_SingleProduct_MultipleClients()
		{
			var whs = Helper.CreateWarehouse("warehouse1", "A", 4, 4);
			var clients = new OrgHeaderCollection(Factory);
			clients.Add(Helper.CreateClient("client1", "client1"));
			clients.Add(Helper.CreateClient("client2", "client2"));

			var part = Helper.CreateProduct(clients[0], "product1");
			Helper.CreateProductClientRelationShip(clients[1], part);

			var receive1 = Helper.CreateWhsReceiveWithInventory(clients[0], whs, "receive1", part, 10m, true, true);
			var receive2 = Helper.CreateWhsReceiveWithInventory(clients[1], whs, "receive2", part, 15m, true, true);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Each client should hold a row for each receive.", 2, result.Count);
			AssertRow(result[0], whs, clients[0], part, 10m, 0m, 0m, 10m, 0m);
			AssertRow(result[1], whs, clients[1], part, 15m, 0m, 0m, 15m, 0m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(clients[0], whs, "order1", part, 15m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition: Pick status is Created", PickStatus.Descriptions.Created, pick1.StatusDesc);

			var order2 = Helper.CreateWhsOrderWithOrderLine(clients[1], whs, "order2", part, 25m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Pick status is Created", PickStatus.Descriptions.Created, pick2.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Each client should hold a row for each receive.", 2, result.Count);
			AssertRow(result[0], whs, clients[0], part, 10m, 0m, 10m, 0m, 5m);
			AssertRow(result[1], whs, clients[1], part, 15m, 0m, 15m, 0m, 10m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_ShortOrderFinalised

		public void TestView_UnallocatedOrders_SingleProduct_ShortOrderFinalised()
		{
			var whs = Helper.CreateWarehouse("warehouse1", "A", 4, 4);
			var client = Helper.CreateClient("client1", "client1");
			var part = Helper.CreateProduct(client, "product1");

			Helper.CreateWhsReceiveWithInventory(client, whs, "receive", part, 20m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 15m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().First().WZ_Units = 10m;
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick.StatusDesc);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Products with inventory should appear as rows in report.", 1, result.Count);
			AssertRow(result[0], whs, client, part, 20m, 0m, 10m, 10m, 5m);

			pick.FinaliseOrder(order);
			AssertEquals("Precondition: Order status is Finalised.", true, order.IsFinalised);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Multiple products with inventory should appear as rows in report.", 1,
				result.Count);
			AssertRow(result[0], whs, client, part, 20m, 0m, 10m, 10m, 5m);

			pick.FinalisePick();
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Products with inventory should appear as rows in report.", 1, result.Count);
			AssertRow(result[0], whs, client, part, 10m, 0m, 0m, 10m, 0m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_CrossDock

		#region TestView_UnallocatedOrders_SingleProduct_CrossDock_NoPick

		public void TestView_UnallocatedOrders_SingleProduct_CrossDock_NoPick()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveLine(receive, part, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 20m);
			order1.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			AssertEquals("Precondition: Quantity should be reserved for line.", 10m, order1.Lines[0].ReservedQuantity);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Product with inventory should appear on report.", 1, result.Count);
			AssertRow(result[0], whs, client, part, 10m, 10m, 0m, 0m, 10m);
		}

		#endregion

		#region TestView_UnallocatedOrders_SingleProduct_CrossDock_Pick

		public void TestView_UnallocatedOrders_SingleProduct_CrossDock_Pick()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveLine(receive, part, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 20m);
			Helper.CreateReservePickLine(order1.Lines[0], receive.Inventory[0], 10m);
			AssertEquals("Precondition: Quantity should be reserved for line.", 10m, order1.Lines[0].ReservedQuantity);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Product with inventory should appear on report.", 1, result.Count);
			AssertRow(result[0], whs, client, part, 10m, 10m, 0m, 0m, 10m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", part, 15m);
			var pick = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Product with inventory should appear on report.", 1, result.Count);
			AssertRow(result[0], whs, client, part, 10m, 10m, 0m, 0m, 25m);
		}

		#endregion

		#endregion

		#region TestView_UnallocatedOrders_MultipleProducts_Orders

		public void TestView_UnallocatedOrders_MultipleProducts()
		{
			var whs = Helper.CreateWarehouse("warehouse1", "A", 4, 4);
			var client = Helper.CreateClient("client1", "client1");
			var parts = new OrgSupplierPartCollection(Factory);
			parts.Add(Helper.CreateProduct(client, "product1"));
			parts.Add(Helper.CreateProduct(client, "product2"));
			parts.Add(Helper.CreateProduct(client, "product3"));

			var receive = Helper.CreateWhsReceive(client, whs, "receive");
			Helper.CreateWhsReceiveInventoryLine(receive, parts[0], 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, parts[1], 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, parts[2], 100m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Multiple products with inventory should appear as rows in report.", 3,
				result.Count);

			var order1 = Helper.CreateWhsOrder(client, whs, "order1");
			Helper.CreateWhsOrderLine(order1, parts[0], 15m);
			Helper.CreateWhsOrderLine(order1, parts[1], 25m);
			Helper.CreateWhsOrderLine(order1, parts[2], 150m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick1.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: Multiple products with inventory should appear as rows in report.", 3,
				result.Count);
			AssertRow(result[0], whs, client, parts[0], 10m, 0m, 10m, 0m, 5m);
			AssertRow(result[1], whs, client, parts[1], 50m, 0m, 25m, 25m, 0);
			AssertRow(result[2], whs, client, parts[2], 100m, 0m, 100m, 0m, 50m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", parts[1], 50m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: Pick status is Created.", PickStatus.Descriptions.Created, pick2.StatusDesc);
			Factory.Save();

			result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: All products with inventory should appear as rows in report.", 3, result.Count);
			AssertRow(result[0], whs, client, parts[0], 10m, 0m, 10m, 0m, 5m);
			AssertRow(result[1], whs, client, parts[1], 50m, 0m, 50m, 0m, 25m);
			AssertRow(result[2], whs, client, parts[2], 100m, 0m, 100m, 0m, 50m);
		}

		#endregion

		#region TestView_UnallocatedOrders_CancelledOrder

		public void TestView_UnallocatedOrders_CancelledOrder()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", part, 20m, true, true);
			Factory.Save();

			Helper.CreateWhsOrderWithOrderLine(client, whs, "order1", part, 10m);
			var orderToCancel = Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", part, 10m);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Precondition: total unallocated quantity is from the 2 orders.", 1, result.Count);
			AssertEquals("TotalUnallocatedQuantity", 20m, (ZDecimal)result.Single()["TotalUnallocatedQuantity"]);

			orderToCancel.CancelReactivateDocket();
			Factory.Save();

			AssertEquals("Precondition: order is cancelled.", true, orderToCancel.IsCancelled);
			var result2 = Load_Report_WhsStockOnHandSummary();
			AssertEquals("TotalUnallocatedQuantity", 10m, (ZDecimal)result2.Single()["TotalUnallocatedQuantity"]);
		}

		#endregion

		#region TestView_UnallocatedOrders_CancelledPick

		public void TestView_UnallocatedOrders_CancelledPick()
		{
			var whs = Helper.CreateWarehouse("warehouse", "A", 4, 4);
			var client = Helper.CreateClient("client", "client");
			var part = Helper.CreateProduct(client, "product");

			Helper.CreateWhsReceiveWithInventory(client, whs, "receive1", part, 20m, true, true);
			Factory.Save();

			Helper.CreateWhsOrderWithOrderLine(client, whs, "order1", part, 10m);
			var orderWithPickToCancel = Helper.CreateWhsOrderWithOrderLine(client, whs, "order2", part, 10m);
			Factory.Save();

			var result = Load_Report_WhsStockOnHandSummary().Single();
			AssertEquals("Precondition: TotalUnallocatedQuantity", 20m, (ZDecimal)result["TotalUnallocatedQuantity"]);
			AssertEquals("Precondition: AvailableUnits", 20m, (ZDecimal)result["AvailableUnits"]);
			AssertEquals("Precondition: CommittedUnits", 0m, (ZDecimal)result["CommittedUnits"]);

			var pick = Helper.CreatePickNew(orderWithPickToCancel);
			Factory.Save();

			var result2 = Load_Report_WhsStockOnHandSummary().Single();
			AssertEquals("Precondition: TotalUnallocatedQuantity", 10m, (ZDecimal)result2["TotalUnallocatedQuantity"]);
			AssertEquals("Precondition: AvailableUnits", 10m, (ZDecimal)result2["AvailableUnits"]);
			AssertEquals("Precondition: CommittedUnits", 10m, (ZDecimal)result2["CommittedUnits"]);

			pick.CancelPick();
			Factory.Save();

			AssertEquals("Precondition: pick is cancelled.", true, pick.IsCancelled);
			var result3 = Load_Report_WhsStockOnHandSummary().Single();
			AssertEquals("TotalUnallocatedQuantity", 20m, (ZDecimal)result3["TotalUnallocatedQuantity"]);
			AssertEquals("AvailableUnits", 20m, (ZDecimal)result3["AvailableUnits"]);
			AssertEquals("CommittedUnits", 0m, (ZDecimal)result3["CommittedUnits"]);
		}

		#endregion

		#endregion

		#region TestSOHSReport_LastMovementDate

		[TestDate(2013, 12, 30, 5, 5, 0)]
		public void TestSOHSReport_LastMovementDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.AddDays(-1), data.Part1, 100m);
			var receive2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today.AddDays(-2), data.Part1, 150m);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", today, data.Part1, 150m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var result = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should rollup all transactions into 1.", 1, result.Count);
			AssertRow(result[0], data.Whs1, data.Org1, data.Part1, 100m, 0m, 0m, 100m, 0m);
			AssertEquals("LastMovementDate", order.WD_FinalisedDate.ToString("dd-MMM-yyyy hh:mm"), ((ZDateTime)result[0]["LastMovementDate"]).ToString("dd-MMM-yyyy hh:mm"));
		}

		#endregion

		#region TestView_InTransitInventory_Order

		public void TestView_InTransitInventory_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderPick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var results = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should be 1 grouped line for the Receive and In-Transit Inventory.", 1, results.Count);
			AssertRow(results.Single(), data.Whs1, data.Org1, data.Part1, 10m, 0m, 4m, 6m, 0m);
		}

		#endregion

		#region TestView_InTransitInventory_Transfer

		public void TestView_InTransitInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 6m, location1, location2);
			transferLine1.RunPreSaveValidation();
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer line is Picked.", true, transferLine1.IsPicked);
			Factory.Save();

			var results = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should be 1 grouped line for the Receive and In-Transit Inventory.", 1, results.Count);
			AssertRow(results.Single(), data.Whs1, data.Org1, data.Part1, 10m, 0m, 0m, 4m, 0m);
		}

		#endregion

		#region TestView_PuttingAwayInventory_Transfer

		public void TestView_PuttingAwayInventory_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLT1", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals(
				$"InventoryStatus of Precondition: Transfer Line should be {InventoryStatus.Codes.PuttingAway}.",
				InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"InventoryStatus of Precondition: Inventory should be {InventoryStatus.Codes.Received}.",
				InventoryStatus.Codes.Received, receiveLine.WE_CurrentInventoryStatus);

			var results = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should be 1 grouped line for the Receive and Putting-Away Inventory.", 1, results.Count);
			AssertRow(results.Single(), data.Whs1, data.Org1, data.Part1, 10m, 0m, 0m, 0m, 0m);
		}

		#endregion

		#region TestView_StagedInventory

		public void TestView_StagedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should be 1 grouped line for the Receive and Staged Inventory.", 1, results.Count);
			AssertRow(results.Single(), data.Whs1, data.Org1, data.Part1, 20m, 0m, 8m, 12m, 0m);
		}

		#endregion

		#region TestView_ReadyToPackInventory

		public void TestView_ReadyToPackInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = Load_Report_WhsStockOnHandSummary();
			AssertEquals("Should be 1 grouped line for the Receive and Ready To Pack Inventory.", 1, results.Count);
			AssertRow(results.Single(), data.Whs1, data.Org1, data.Part1, 20m, 0m, 8m, 12m, 0m);
		}

		#endregion

		#region Load_Report_WhsStockOnHandSummary

		DynamicBusinessObjectCollection Load_Report_WhsStockOnHandSummary()
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			string sql = @"select * from WhsStockOnHandSummaryReport(null)
						   order by WarehouseName, Client, Product";

			var sqlParams = new ZSqlParameterCollection();
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region SetupData

		void SetupData(OrgHeaderCollection clients, OrgSupplierPartCollection parts, WhsWarehouseCollection whs)
		{
			var category1 = Helper.CreateProductCategory(clients[0], parts[3], "Cat1");
			category1.OPC_CategoryDescription = "Category Description 1";
			var category2 = Helper.CreateProductCategory(clients[1], parts[4], "Cat2");
			category2.OPC_CategoryDescription = "Category Description 2";

			parts[0].OP_StockKeepingUnit = "CTN";
			parts[0].RelatedOrganisations.FindFirstByOrganisationPK(clients[0].PK).OU_ClientUQ = "PLT";

			parts[1].OP_Brand = "PRO BRAND";
			parts[1].OP_Model = "PRO MODEL";
			parts[1].OP_Weight = 11m;
			parts[1].OP_Cubic = 44m;

			var partUnit = parts[0].PartUnits.AddNew();
			partUnit.OF_PackType = "CTN";
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 20;

			parts[0].OP_LastCost = 3m;
			parts[0].OP_RX_NKLastWeightedCostCurr = "AUD";
			parts[1].OP_LastCost = 4m;
			parts[1].OP_RX_NKLastWeightedCostCurr = "USD";
			parts[2].OP_LastCost = 5m;
			parts[2].OP_RX_NKLastWeightedCostCurr = "EUR";

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			parts[0].OP_RH_NKCommodityCode = commodity[0].RH_Code;
			parts[1].OP_RH_NKCommodityCode = commodity[1].RH_Code;
			parts[2].OP_RH_NKCommodityCode = commodity[0].RH_Code;
			// product 3 is owned by both clients
			var relation = parts[2].RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = parts[2].PK;
			relation.OU_OH = clients[1].PK;

			SetClientAttributesType(clients[0]);
			SetClientAttributesType(clients[1]);
			SetProductAttributesUse(clients[0], parts[0]);
			SetProductAttributesUse(clients[1], parts[1]);
			SetProductAttributesUse(clients[0], parts[2]);
			SetProductAttributesUse(clients[1], parts[2]);

			var receive1 = SetupReceive(clients[0], whs[0], "11", parts[0], 111, parts[2], 112, true);
			var receive2 = SetupReceive(clients[0], whs[0], "12", parts[0], 121, parts[2], 122, false,
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var receive3 = SetupReceive(clients[1], whs[0], "21", parts[1], 211, parts[2], 212, true,
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			var receive4 = SetupReceive(clients[1], whs[1], "22", parts[1], 221, parts[2], 222, false);
			var receive5 = SetupReceive(clients[0], whs[0], "13", parts[3], 123, parts[4], 321, false);

			// setup an Allocated Qty for test
			var order = Helper.CreateWhsOrder(clients[1], whs[0], "1");
			var orderLine = Helper.CreateWhsOrderLine(order, parts[1], 11m);
			var reservedPickLine1 = Helper.CreateReservePickLine(orderLine, receive3.Inventory[0], 11m);
			((IBusinessObjectInternals)reservedPickLine1).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 10m;
			AssertEquals(11m, receive3.Inventory[0].WI_CrossDockQuantity);

			var order1 = Helper.CreateWhsOrder(clients[0], whs[0], "1A");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, parts[2], 17m);
			var reservedPickLine2 = Helper.CreateReservePickLine(orderLine1, receive1.Inventory[1], 17m);
			((IBusinessObjectInternals)reservedPickLine2).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 15m;
			AssertEquals(17m, receive1.Inventory[1].WI_CrossDockQuantity);

			Factory.Save();

			var order2 = Helper.CreateWhsOrder(clients[0], whs[0], "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, parts[0], 5m);
			Helper.CreatePickNew(order2);

			var transfer = Helper.CreateWhsTransfer(clients[0], whs[0]);
			var sourceLocation = receive1.Inventory[1].Location;
			var destLocation = whs[0].Rows.Cast<WhsRow>().SelectMany(r => r.Locations)
				.First(l => l.PK != sourceLocation.PK);
			var transferLine = Helper.CreateWhsTransferLine(transfer, parts[2], 7m, sourceLocation, "", destLocation,
				new ZDate(2019, 1, 2), new ZDate(2019, 2, 1), "PA1", "PA2", "PA3");
			transferLine.WE_BondedEntryKey = "BEK";
			transferLine.RunPreSaveValidation(); // commits stock

			Factory.Save();
		}

		void SetClientAttributesType(OrgHeader client)
		{
			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
		}

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part1,
			ZDecimal units1, OrgSupplierPart part2, ZDecimal units2, bool finaliseDocket,
			string inventoryStatus = "PND", string heldCode = "")
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, new ZDate(2019, 1, 2),
				new ZDate(2019, 2, 1), "PA1", "PA2", "PA3", "BEK");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, new ZDate(2019, 1, 2), new ZDate(2019, 2, 1),
				"PA1", "PA2", "PA3", "BEK");
			receiveLine1.OriginalInventoryStatus = inventoryStatus;

			if (!string.IsNullOrEmpty(heldCode))
			{
				receiveLine1.OriginalInventoryHeldCode = heldCode;
			}

			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPutaway", true, receive.IsPuttingAway);

			if (finaliseDocket)
			{
				receive.FinaliseDocket();
				AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
			}

			return receive;
		}

		#endregion

		#region Asserts

		void AssertRow(DynamicBusinessObject dynamicObject, WhsWarehouse warehouse, OrgHeader client,
			OrgSupplierPart part,
			ZDecimal totalUnits, ZDecimal crossDockQuantity, ZDecimal committedUnits, ZDecimal availableUnits,
			ZDecimal totalUnallocatedQuantity)
		{
			var expectedPalletSpaces = (part.OP_StockKeepingUnitPerPallet > 0)
				? Math.Ceiling(totalUnits / part.OP_StockKeepingUnitPerPallet)
				: 0m;
			var expectedCommodityPK = (!part.OP_RH_NKCommodityCode.IsEmpty) ? part.CommodityCode.PK : ZGuid.Empty;
			var partRelation =
				part.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var expectedClientUnits =
				Utilities.Round(
					part.UnitConverter.Convert(totalUnits, part.OP_StockKeepingUnit, partRelation.OU_ClientUQ), 3);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("WarehousePK", warehouse.PK, dynamicObject["WarehousePK"]);
				AssertEquals("WarehouseName", warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
				AssertEquals("ClientPK", client.PK, dynamicObject["ClientPK"]);
				AssertEquals("ClientCode", client.OH_Code, dynamicObject["ClientCode"]);
				AssertEquals("Client", client.OH_FullName, dynamicObject["Client"]);
				AssertEquals("ProductPK", part.PK, dynamicObject["ProductPK"]);
				AssertEquals("Product", part.OP_PartNum, dynamicObject["Product"]);
				AssertEquals("ProductDesc", part.OP_Desc, dynamicObject["ProductDesc"]);
				AssertEquals("ProductBrandName", part.OP_Brand, dynamicObject["ProductBrandName"]);
				AssertEquals("ProductModel", part.OP_Model, dynamicObject["ProductModel"]);
				AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
				AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
					dynamicObject["ProductCategoryDescription"]);
				AssertEquals("WeightUQ", part.OP_WeightUQ, dynamicObject["WeightUQ"]);
				AssertEquals("VolumeUQ", part.OP_CubicUQ, dynamicObject["VolumeUQ"]);
				AssertEquals("ClientUnit", partRelation.OU_ClientUQ, dynamicObject["ClientUnit"]);
				AssertEquals("CommodityPK", expectedCommodityPK, dynamicObject["CommodityPK"]);
				AssertEquals("CommodityCode", part.OP_RH_NKCommodityCode, dynamicObject["CommodityCode"]);
				AssertEquals("StockKeepingUnit", part.OP_StockKeepingUnit, dynamicObject["StockKeepingUnit"]);
				AssertEquals("Weight", part.OP_Weight * totalUnits, dynamicObject["Weight"]);
				AssertEquals("Volume", part.OP_Cubic * totalUnits, dynamicObject["Volume"]);
				AssertEquals("TotalClientUnits", expectedClientUnits, dynamicObject["TotalClientUnits"]);
				AssertEquals("Pallet Spaces", expectedPalletSpaces, dynamicObject["TotalPalletSpaces"]);
				AssertEquals("Allocated Units", crossDockQuantity, dynamicObject["AllocatedUnits"]);
				AssertEquals("Total Units", totalUnits, dynamicObject["TotalUnits"]);
				AssertEquals("Committed Units", committedUnits, dynamicObject["CommittedUnits"]);
				AssertEquals("Available Units", availableUnits, dynamicObject["AvailableUnits"]);
				AssertEquals("Total Value", part.OP_LastCost * totalUnits, dynamicObject["TotalValue"]);
				AssertEquals("Last Cost", part.OP_LastCost, dynamicObject["LastCost"]);
				AssertEquals("Currency", part.OP_RX_NKLastWeightedCostCurr, dynamicObject["Currency"]);
				AssertEquals("Total Unallocated Quantity", totalUnallocatedQuantity,
					dynamicObject["TotalUnallocatedQuantity"]);
			});
		}

		#endregion
	}
}
