using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AutoAllocateItemsActionMethodApplicator))]
	public class AutoAllocateItemsActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestApplyApplicator_WarningsIfNotBuildStatus_SystemCreatedTimeSort()
		{
			var pick1 = Helper.CreatePickNew(); // to be cancelled
			pick1.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-5);
			var pick2 = Helper.CreatePickNew();
			pick2.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-4);
			pick2.WP_PickStatus = PickStatus.Codes.Created;
			var pick3 = Helper.CreatePickNew();
			pick3.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-3);
			pick3.WP_PickStatus = PickStatus.Codes.Finalised;
			pick3.WP_FinalizedDateUtc = ZDateTime.UtcNow;
			var pick4 = Helper.CreatePickNew();
			pick4.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-2);
			pick4.WP_PickStatus = PickStatus.Codes.PickSlip;
			var pick5 = Helper.CreatePickNew();
			pick5.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			pick5.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			pick1.CancelPick();

			AssertEquals("Precondition:", pick1.WP_PickStatus, PickStatus.Codes.Cancelled);
			AssertEquals("Precondition:", pick2.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick3.WP_PickStatus, PickStatus.Codes.Finalised);
			AssertEquals("Precondition:", pick4.WP_PickStatus, PickStatus.Codes.PickSlip);
			AssertEquals("Precondition:", pick5.WP_IsAwaitingReplenishment, true);

			var picks = new[] { pick5, pick4, pick3, pick2, pick1 }; // expected sort order: 1,2,3,4,5

			ApplyApplicator(picks, $@"
WARNING: Pick {pick1.WP_PickNo} [HL {pick1.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick2.WP_PickNo} [HL {pick2.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick3.WP_PickNo} [HL {pick3.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick4.WP_PickNo} [HL {pick4.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick5.WP_PickNo} [HL {pick5.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
");
		}

		public void TestApplyApplicator_SortPriorityDateRequiredDateCreated()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// setting earliest required date:
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 1);
			order1.WD_RequiredDate = DateTime.Now.AddDays(-6);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 1);
			order2.WD_RequiredDate = DateTime.Now.AddDays(-1);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 1);
			order3.WD_RequiredDate = DateTime.Now.AddDays(-1);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "4", data.Part1, 1);
			order4.WD_RequiredDate = DateTime.Now.AddDays(-100);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "5", data.Part1, 1);
			order5.WD_RequiredDate = DateTime.Now.AddDays(-20);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "6", data.Part1, 1);
			order6.WD_RequiredDate = DateTime.Now.AddDays(-20);
			Factory.Save();

			// 1,2,3 tie on 1st priority, 2,3 tie on earliest required date
			var pick1 = Helper.CreatePickNew(order1);
			pick1.PickPriority = 1;
			pick1.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.PickPriority = 1;
			pick2.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-5);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.PickPriority = 1;
			pick3.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-4);

			// 4: oldest required date but last allowed except 0 priority
			var pick4 = Helper.CreatePickNew(order4);
			pick4.PickPriority = 20;
			pick4.WP_SystemCreateTimeUtc = DateTime.Now;

			// 5,6 = 0 priority, tied required date
			var pick5 = Helper.CreatePickNew(order5);
			pick5.PickPriority = 0;
			pick5.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-20);
			var pick6 = Helper.CreatePickNew(order6);
			pick6.PickPriority = 0;
			pick6.WP_SystemCreateTimeUtc = DateTime.Now.AddDays(-10);
			Factory.Save();

			AssertEquals("Precondition:", pick1.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick2.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick3.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick4.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick5.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick6.WP_PickStatus, PickStatus.Codes.Created);

			var picks = new[] { pick6, pick5, pick4, pick3, pick2, pick1 };

			// expected sort order: 1,2,3,4,5,6

			ApplyApplicator(picks, $@"
WARNING: Pick {pick1.WP_PickNo} [HL {pick1.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick2.WP_PickNo} [HL {pick2.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick3.WP_PickNo} [HL {pick3.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick4.WP_PickNo} [HL {pick4.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick5.WP_PickNo} [HL {pick5.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick6.WP_PickNo} [HL {pick6.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
");
		}

		public void TestApplyApplicator_SortPickNo()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var time = DateTime.Now;
			// setting earliest required date all same:
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 1);
			order1.WD_RequiredDate = time;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 1);
			order2.WD_RequiredDate = time;
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 1);
			order3.WD_RequiredDate = time;
			Factory.Save();

			// Setting pick priority and System created time all same:
			var pick1 = Helper.CreatePickNew(order1);
			pick1.PickPriority = 1;
			pick1.WP_SystemCreateTimeUtc = time;
			var pick2 = Helper.CreatePickNew(order2);
			pick2.PickPriority = 1;
			pick2.WP_SystemCreateTimeUtc = time;
			var pick3 = Helper.CreatePickNew(order3);
			pick3.PickPriority = 1;
			pick3.WP_SystemCreateTimeUtc = time;

			// Set Pick No.
			pick1.WP_PickNo = "100";
			pick2.WP_PickNo = "200";
			pick3.WP_PickNo = "300";
			Factory.Save();

			AssertEquals("Precondition:", pick1.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick2.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick3.WP_PickStatus, PickStatus.Codes.Created);
			AssertEquals("Precondition:", pick1.WP_PickNo, "100");
			AssertEquals("Precondition:", pick2.WP_PickNo, "200");
			AssertEquals("Precondition:", pick3.WP_PickNo, "300");

			var picks = new[] { pick3, pick2, pick1 };

			// expected sort order: 1,2,3 Alphabetical by WP_PickNo

			ApplyApplicator(picks, $@"
WARNING: Pick {pick1.WP_PickNo} [HL {pick1.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick2.WP_PickNo} [HL {pick2.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
WARNING: Pick {pick3.WP_PickNo} [HL {pick3.WP_PickNo}] - cannot be processed because it does not have status {PickStatus.Descriptions.Building}.
");
		}

		public void TestApplyApplicator_DisplayReturnedMessagesAndInventoryAllocated()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // Part1 has 100 quantity.

			// order of part1, which will be fully filled
			// expect no message back from AutoAllocateItems(), meaning we should see the success message from this operational method.
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 20m);
			order1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			// order of part1, to be partially filled
			// expect no message back from AutoAllocateItems(), meaning we should see the success message from this operational method.
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 100m);
			order2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			// order of part1, all part1 will have been allocated by the time this is processed.
			// should get back: NoStockAllocatedWarningMessage
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 40m);
			order3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			Factory.Save();

			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			pick1.PickPriority = 1;
			pick1.ClearAllocatedItems();// reset for test.

			var pick2 = Helper.CreatePickByAttachingOrders(order2);
			pick2.PickPriority = 2;
			pick2.ClearAllocatedItems();// reset for test.

			var pick3 = Helper.CreatePickByAttachingOrders(order3);
			pick3.PickPriority = 3;
			pick3.ClearAllocatedItems();// reset for test.

			Factory.Save();

			AssertEquals("PreCondition", PickStatus.Codes.Building, pick1.WP_PickStatus);
			AssertEquals("PreCondition", PickStatus.Codes.Building, pick2.WP_PickStatus);
			AssertEquals("PreCondition", PickStatus.Codes.Building, pick3.WP_PickStatus);

			ApplyApplicator(new[] { pick3, pick2, pick1 }, $@"
INFO: Pick {pick1.WP_PickNo} [HL {pick1.WP_PickNo}] - successfully Auto Allocated Items.
INFO: Pick {pick2.WP_PickNo} [HL {pick2.WP_PickNo}] - successfully Auto Allocated Items.
WARNING: Pick {pick3.WP_PickNo} [HL {pick3.WP_PickNo}] - had the following message while trying to Auto Allocate Items:
No stock could be found for this pick either because there is no available stock in the warehouse or there was no stock which meets configured allocation rules.

");
			AssertEquals("pick1 quantity not met", 0m, pick1.Orders[0].Lines[0].QuantityNotMet);
			AssertEquals("pick2 quantity not met", 20m, pick2.Orders[0].Lines[0].QuantityNotMet);
			AssertEquals("pick3 quantity not met", 40m, pick3.Orders[0].Lines[0].QuantityNotMet);
		}

		public void TestApplyApplicator_DisplayNotificationMessage()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // Part1 has 100 quantity.

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 20m);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.PickPriority = 1;
			pick.ClearAllocatedItems();
			Factory.Save();

			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWA");
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var allocationRuleSet = Factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);
			allocationRuleSet.PRS_IsLive = false;
			Factory.Save();

			var expectedLogText = $@"
INFO: Pick {pick.WP_PickNo} [HL {pick.WP_PickNo}] - successfully Auto Allocated Items.
WARNING: Pick {pick.WP_PickNo}: Issue occurred during allocation: No rules found for context: 'PWA'";
			ApplyApplicator(new[] { pick }, expectedLogText);
		}

		public void TestSimulateRun_DBHits()
		{
			var numberOfPicks = 5;
			var numberOfOrdersPerPick = 4;
			var numberOfOrderLinesPerOrder = 5;
			var data = new TestDataSimpleEnvironment(Factory, 10, 10);
			for (int pickIndex = 0; pickIndex < numberOfPicks; pickIndex++)
			{
				var orderList = new List<WhsOrder>();
				for (int ordersPerPickIndex = 0; ordersPerPickIndex < numberOfOrdersPerPick; ordersPerPickIndex++)
				{
					var suffix = pickIndex.ToString() + ordersPerPickIndex.ToString();
					var clientCode = "C" + ordersPerPickIndex.ToString();
					var client = OrgHeader.LoadFromCode(Factory, clientCode) ?? Helper.CreateClient(clientCode);
					var order = Helper.CreateWhsOrder(client, data.Whs1, "O" + suffix);
					order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
					for (int orderLinesPerOrderIndex = 0; orderLinesPerOrderIndex < numberOfOrderLinesPerOrder; orderLinesPerOrderIndex++)
					{
						var suffix2 = suffix + orderLinesPerOrderIndex.ToString();
						var product = Helper.CreateProduct(client, "P" + suffix2);
						var location = Helper
							.CreateRowAndGenerateLocations(data.Whs1, "R" + suffix2, 1, 1).Locations
							.Single();
						Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R" + suffix2,
							ZDateTimeOffset.Today.AddDays(-1), product, 2, location, "", false);

						Helper.CreateWhsOrderLine(order, product, 1m);
					}

					orderList.Add(order);
				}

				var pick = Helper.CreatePickNew(orderList.ToArray());
				pick.ClearAllocatedItems();
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 2 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 4 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 2 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				// These following 2 tables have complex dbonly query and subquery, the tables will be hit once each for every pick.
				// see WhsPick.GetInventoriesQueryForNonFinalisedPick method for the origin of both these.
				{ WhsInventoryViewSchema.Constants.TableName, 5 },
				{ WhsLocationViewSchema.Constants.TableName, 5 },
			};

			var newFactory = new BusinessObjectFactory();
			var picksInNewFactory = newFactory.Load<WhsPick>(new ZQuery());

			using (RowFactory.SetCachedTables())
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				SimulateRun(picksInNewFactory, saveOnSuccess: false);
			}
		}
		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
