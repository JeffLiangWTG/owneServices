using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OverrideWhsOrderFulfillmentRuleActionMethodApplicator))]
	public class OverrideWhsOrderFulfillmentRuleActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestAction()
		{
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA1", product, 5m);
			order1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA2", product, 3m);
			order2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA3", product, 10m);
			order3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order4 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA4", product, 1m);
			order4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Helper.CreatePickNew(new WhsOrder[] { order1, order2, order3, order4 });

			Factory.Save();

			var expectedLogText = string.Format(
				"INFO: Warehouse Order {0} [HL {0}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {1} [HL {1}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {2} [HL {2}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {3} [HL {3}] - fulfillment rule was successfully overridden.\r\n",
				order1.WD_DocketID,
				order2.WD_DocketID,
				order3.WD_DocketID,
				order4.WD_DocketID
			);

			var orders = new[]
			{
				order1,
				order2,
				order3,
				order4
			};

			Applicator.OverrideReason = "Testing Override Reason";
			ApplyApplicator(orders, expectedLogText);

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No new picks created", 1, numberOfPicks);

			var logReference = "Fulfillment Rule Overridden - Ref: Testing Override Reason";
			AssertEquals("Order1 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order1.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order1 should have correct EditedARecord log with Ref -> 'Fulfillment Rule Overridden - Ref: Testing Override Reason'", 1, Helper.FindLogs(order1.Logs, Events.EditedARecord, logReference).Length);

			AssertEquals("Order2 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order2.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order2 should have correct EditedARecord log with Ref -> 'Fulfillment Rule Overridden - Ref: Testing Override Reason'", 1, Helper.FindLogs(order2.Logs, Events.EditedARecord, logReference).Length);

			AssertEquals("Order3 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order3.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order3 should have correct EditedARecord log with Ref -> 'Fulfillment Rule Overridden - Ref: Testing Override Reason'", 1, Helper.FindLogs(order3.Logs, Events.EditedARecord, logReference).Length);

			AssertEquals("Order4 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order4.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order4 should have correct EditedARecord log with Ref -> 'Fulfillment Rule Overridden - Ref: Testing Override Reason'", 1, Helper.FindLogs(order4.Logs, Events.EditedARecord, logReference).Length);
		}

		public void TestAction_LogSkippedOrders_FinalisedOrdersAndNonFulfillmentRuleOrders()
		{
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");

			var finalisedOrder = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA5", product, 1m); // Finalised Order
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: finalisedOrder);
			finalisedOrder.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var ruleAlreadyNoneOrder = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA2", product, 3m);
			ruleAlreadyNoneOrder.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			var notYetPickedOrder = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA3", product, 10m);
			notYetPickedOrder.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Helper.CreatePickNew(new WhsOrder[] { ruleAlreadyNoneOrder });

			Factory.Save();

			string expectedLogText = string.Format(
				"WARNING: Order [HL {0}] - is already finalized so cannot override.\r\n" +
				"WARNING: Order [HL {1}] - fulfillment rule is already NONE.\r\n" +
				"WARNING: Order [HL {2}] - is not picked so cannot override.\r\n",
				finalisedOrder.WD_DocketID,
				ruleAlreadyNoneOrder.WD_DocketID,
				notYetPickedOrder.WD_DocketID
			);

			var orders = new[]
			{
				finalisedOrder,
				ruleAlreadyNoneOrder,
				notYetPickedOrder,
			};

			ApplyApplicator(orders, expectedLogText);

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No new picks created", 2, numberOfPicks);

			AssertEquals("Order should not be overridden", WhsOrderFulfillmentRuleList.Codes.All, finalisedOrder.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, ruleAlreadyNoneOrder.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order should not be overridden", WhsOrderFulfillmentRuleList.Codes.All, notYetPickedOrder.WD_WhsOrderFulfillmentRule);
		}

		public void TestAction_PickStatusUpdated()
		{
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA1", product, 5m);
			order1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA2", product, 3m);
			order2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA3", product, 10m);
			order3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var order4 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA4", product, 1m);
			order4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var pick1 = Helper.CreatePickNew(new WhsOrder[] { order1, order2 });
			pick1.WP_PickStatus = PickStatus.Codes.Building;
			var pick2 = Helper.CreatePickNew(new WhsOrder[] { order3, order4 });
			pick2.WP_PickStatus = PickStatus.Codes.Building;
			Factory.Save();

			var expectedLogText = string.Format(
				"INFO: Warehouse Order {0} [HL {0}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {1} [HL {1}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {2} [HL {2}] - fulfillment rule was successfully overridden.\r\n" +
				"INFO: Warehouse Order {3} [HL {3}] - fulfillment rule was successfully overridden.\r\n",
				order1.WD_DocketID,
				order2.WD_DocketID,
				order3.WD_DocketID,
				order4.WD_DocketID
			);

			var orders = new[]
			{
				order1,
				order2,
				order3,
				order4
			};

			ApplyApplicator(orders, expectedLogText);

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No new picks created", 2, numberOfPicks);

			AssertEquals("Order1 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order1.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order2 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order2.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order3 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order3.WD_WhsOrderFulfillmentRule);
			AssertEquals("Order4 should have fulfillment rule set to NONE", WhsOrderFulfillmentRuleList.Codes.None, order4.WD_WhsOrderFulfillmentRule);
			AssertEquals("Pick1 WP_PickStatus is correct", PickStatus.Codes.Created, pick1.WP_PickStatus);
			AssertEquals("Pick2 WP_PickStatus is correct", PickStatus.Codes.Created, pick2.WP_PickStatus);
		}

		public void TestAction_DbHits()
		{
			const int numberOfOrders = 50;
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");

			var orders = new List<WhsOrder>();
			var ordersForPick = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA" + i, product, 5m);
				order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				orders.Add(order);
				ordersForPick.Add(order);

				if ((i != 0 && i % 10 == 0) || i == numberOfOrders - 1)
				{
					Helper.CreatePickNew(ordersForPick.ToArray());
					ordersForPick.Clear();
				}
			}
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 6 }, // Batched Hits
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = newFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, orders.Select(o => o.PK)));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				_ = SimulateRun(ordersInNewFactory, true).MessagesString();
			}
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;

		new OverrideWhsOrderFulfillmentRuleActionMethodApplicator Applicator => (OverrideWhsOrderFulfillmentRuleActionMethodApplicator)base.Applicator;
	}
}
