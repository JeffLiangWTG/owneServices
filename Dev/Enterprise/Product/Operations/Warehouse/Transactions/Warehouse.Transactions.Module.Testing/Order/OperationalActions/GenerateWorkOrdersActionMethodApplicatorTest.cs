using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateWorkOrdersActionMethodApplicator))]
	public class GenerateWorkOrdersActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			Data.CreateBOMProducts();

			var finalisedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Finalised Order");
			Helper.CreateWhsOrderLine(finalisedOrder, Data.Product1.Parent, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: finalisedOrder);

			var canceledOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Canceled Order");
			canceledOrder.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			var orderWithNoBOMShortfall = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "No BOM Shortfall Order", data.Part1, 10m);
			var orderWithBOMShortfall = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "BOM Shortfall Order", data.BOM.Bike, 10m);

			Factory.Save(); // to generate DocketIDs

			var orders = new WhsOrder[] { finalisedOrder, canceledOrder, orderWithNoBOMShortfall, orderWithBOMShortfall };
			var expectedLogText = string.Format(
				"WARNING: Order [HL {0}] - No Work Orders were created because the Order is already Finalized.\n\r" +
				"WARNING: Order [HL {1}] - No Work Orders were created because the Order is Canceled.\n\r" +
				"INFO: Order [HL {2}] - Order has no BOM Shortfalls and does not require a Work Order.\n\r" +
				"INFO: Order [HL {3}] - Successfully created Work Orders to build all shortfall items. After Saving the Order all Work Orders will be available on the Related Jobs tab.",
				finalisedOrder.WD_DocketID,
				canceledOrder.WD_DocketID,
				orderWithNoBOMShortfall.WD_DocketID,
				orderWithBOMShortfall.WD_DocketID);

			ApplyApplicator(orders, expectedLogText);

			AssertEquals(0, finalisedOrder.CurrentWorkOrders.Count());
			AssertEquals(0, canceledOrder.CurrentWorkOrders.Count());
			AssertEquals(0, orderWithNoBOMShortfall.CurrentWorkOrders.Count());
			AssertEquals("Should have created a WorkOrder to build the Bikes.", 1, orderWithBOMShortfall.CurrentWorkOrders.Count());
			AssertEquals("Should have created two child WorkOrders to build the Engines and Wheels.", 2, orderWithBOMShortfall.CurrentWorkOrders.ElementAt(0).CurrentWorkOrders.Count());
		}

		#endregion

		#region TestAction_DbHits

		public void TestAction_DbHits_SameClientWhsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, data.Part2.OP_StockKeepingUnit);
			var numOfOrders = 30;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, (numOfOrders * 5m));
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var orderGUIDs = new List<ZGuid>();
			for (var i = 0; i < numOfOrders; i++)
			{
				orderGUIDs.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"Order {i}", data.Part1, 5m).PK);
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orderGUIDs));
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 3 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 3 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 30 }, // current implementation of WhsInventoryQuery clears query cache preventing any fetch hints from working.
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(ordersInNewFactory, true);
			}
		}

		public void TestAction_DbHits_SameClientDifferentWhsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var numOfOrders = 30;
			var warehouses = new WhsWarehouse[numOfOrders];
			for (var i = 0; i < numOfOrders; i++)
			{
				warehouses[i] = Helper.CreateWarehouse($"WHS{i}", "B", 1, 1);
				var part = Helper.CreateProduct($"PT{i}", data.Org1);
				Helper.CreateProductBOM(data.Part1, part, 2m, part.OP_StockKeepingUnit);

				var receive = Helper.CreateWhsReceive(data.Org1, warehouses[i], $"R{i}");
				Helper.CreateWhsReceiveInventoryLine(receive, part, 20m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertEquals($"Precondition - ensure Receive {i} is Finalised.", true, receive.IsFinalised);
			}
			Factory.Save();

			var orderGUIDs = new List<ZGuid>();
			for (var j = 0; j < numOfOrders; j++)
			{
				orderGUIDs.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouses[j], $"Order {j}", data.Part1, 5m).PK);
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orderGUIDs));
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgPartBOMSchema.Constants.TableName, 3 },
				{ OrgSupplierPartSchema.Constants.TableName, 3 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 30 }, // current implementation of WhsInventoryQuery clears query cache preventing any fetch hints from working.
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(ordersInNewFactory, true);
			}
		}

		public void TestAction_DbHits_DifferentClientWhsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var numOfOrders = 30;
			var warehouses = new WhsWarehouse[numOfOrders];
			var clients = new OrgHeader[numOfOrders];
			for (var i = 0; i < numOfOrders; i++)
			{
				warehouses[i] = Helper.CreateWarehouse($"WHS{i}", "B", 1, 1);
				clients[i] = Helper.CreateClient($"CLI{i}");
				var part = Helper.CreateProduct($"PT{i}", clients[i]);
				Helper.CreateProductBOM(data.Part1, part, 2m, part.OP_StockKeepingUnit);

				var receive = Helper.CreateWhsReceive(clients[i], warehouses[i], $"R{i}");
				Helper.CreateWhsReceiveInventoryLine(receive, part, 20m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertEquals($"Precondition - ensure Receive {i} is Finalised.", true, receive.IsFinalised);
			}
			Factory.Save();

			var orderGUIDs = new List<ZGuid>();
			for (var j = 0; j < numOfOrders; j++)
			{
				orderGUIDs.Add(Helper.CreateWhsOrderWithOrderLine(clients[j], warehouses[j], $"Order {j}", data.Part1, 5m).PK);
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orderGUIDs));
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgPartBOMSchema.Constants.TableName, 3 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 30 }, // current implementation of WhsInventoryQuery clears query cache preventing any fetch hints from working.
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(ordersInNewFactory, true);
			}
		}

		#endregion

		#region Implementation

		protected TestDataForBOM Data => data ?? (data = new TestDataForBOM(Factory));
		TestDataForBOM data;

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
