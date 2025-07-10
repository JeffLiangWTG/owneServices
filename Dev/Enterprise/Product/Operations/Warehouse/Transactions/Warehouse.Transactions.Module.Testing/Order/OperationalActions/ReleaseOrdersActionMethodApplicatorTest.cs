using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReleaseOrdersActionMethodApplicator))]
	public class ReleaseOrdersActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			PrepareData();
			var orders = new[] { Order1, Order2 };

			ApplyApplicator(orders, @"
ERROR: Order [HL W00000002] has a status of Entered (Saved) and cannot be released until it is finalized.
ERROR: Order [HL W00000003] has a status of Entered (Saved) and cannot be released until it is finalized.
			".Trim());

			AssertEquals("order1 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order1));
			AssertEquals("order2 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order2));

			var pick1 = Helper.CreatePickNew(Order1);
			var pick2 = Helper.CreatePickNew(Order2);

			Factory.Save();

			ApplyApplicator(orders, @"
ERROR: Order [HL W00000002] has a status of Attached To Pick and cannot be released until it is finalized.
ERROR: Order [HL W00000003] has a status of Attached To Pick and cannot be released until it is finalized.
			".Trim());

			AssertEquals("order1 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order1));
			AssertEquals("order2 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order2));

			pick1.FinaliseAllOrders();
			pick2.FinaliseAllOrders();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Order1);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Order2);

			Factory.Save();

			ApplyApplicator(orders, "");

			AssertEquals("order1 Released milestone should have date", true, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order1));
			AssertEquals("order2 Released milestone should have date", true, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order2));

			ApplyApplicator(orders, @"
ERROR: Order [HL W00000002] has already been released.
ERROR: Order [HL W00000003] has already been released.
			".Trim());
		}

		[TestDate(2013, 9, 26, 5, 10, 0)]
		public void TestReleaseMilestoneHasCorrectActualTime_WithFinaliseEventInThePast()
		{
			AssertLatestLogTime_WithMostRecentLog("Order1 Released milestone should have current date time.", -3, ZDateTime.Now);
		}

		[TestDate(2013, 9, 26, 5, 10, 0)]
		public void TestReleaseMilestoneHasCorrectActualTime_WithFinaliseEventInFuture()
		{
			AssertLatestLogTime_WithMostRecentLog("Order1 Released milestone should have current date time.", 1, ZDateTime.Now);
		}

		[TestDate(2013, 9, 26, 5, 10, 0)]
		public void TestReleaseMilestoneHasCorrectActualTime_WithFinaliseEventAtCurrentTime()
		{
			AssertLatestLogTime_WithMostRecentLog("Order1 Released milestone should have current date time.", 0, ZDateTime.Now);
		}

		void AssertLatestLogTime_WithMostRecentLog(string message, int timeOffset, ZDateTime expectedDateTime)
		{
			PrepareData();
			var pick = Helper.CreatePickNew(Order1);

			Factory.Save();
			AssertEquals("Precondition: order1 WHE milestone should have a date.", true, CompletedMilestoneExistsForThisEvent(AutoEvents.WarehouseJobEnteredCode, Order1));
			AssertEquals("Precondition: order1 WHI milestone should have a date.", true, CompletedMilestoneExistsForThisEvent(AutoEvents.WarehouseOrderPickingCode, Order1));

			pick.FinaliseAllOrders();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Order1);

			Factory.Save();
			AssertEquals("Precondition: order1 Finalized milestone should have a date.", true, CompletedMilestoneExistsForThisEvent(AutoEvents.ItemDocumentJobFinalisedCode, Order1));

			// Apply time offset to all event logs
			foreach (var log in Order1.Logs.Find(new ZQuery()))
			{
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_EventTime = log.SL_EventTime.AddMinutes(timeOffset);
				}
			}

			ApplyApplicator(new[] { Order1 }, "");
			AssertEquals("Precondition: order1 Released milestone should have a date.", true, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUpCode, Order1));
			AssertEquals(message, expectedDateTime, GetEventDateTime(AutoEvents.PickedUpCode, Order1));
		}

		#endregion

		#region TestDBHits_ReleaseOrdersActionMethodApplicator

		public void TestDBHits_ReleaseOrdersActionMethodApplicator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var numberOfOrders = 10;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, numberOfOrders * 10m);
			Factory.Save();

			for (int i = 0; i < numberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, i.ToString());
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ordersInNewFactory = new WhsOrderCollection(newFactory);
			AssertEquals("Precondition: Number of orders created.", numberOfOrders, ordersInNewFactory.Count);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobHeaderSchema.Constants.TableName, 10 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ StmALogSchema.Constants.TableName, 11 }, // Cannot reduce hits due to propagation as query has Order by
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				ApplyApplicator(ordersInNewFactory.Cast<WhsOrder>().ToArray(), "");
			}

			foreach (WhsOrder order in ordersInNewFactory)
			{
				AssertEquals("Order Released milestone should have a date.", true, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, order));
			}
		}

		#endregion

		#region Implementation

		protected void PrepareData()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var part = Helper.CreateProduct(org, "P1");

			Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 100m);

			Order1 = Helper.CreateWhsOrder(org, whs, "O1");
			Order2 = Helper.CreateWhsOrder(org, whs, "O2");

			Helper.CreateWhsOrderLine(Order1, part, 10m);
			Helper.CreateWhsOrderLine(Order2, part, 10m);

			Factory.Save();
			AssertEquals("Precondition - order1 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order1));
			AssertEquals("Precondition - order2 Released milestone should have no date.", false, CompletedMilestoneExistsForThisEvent(AutoEvents.PickedUp.Code, Order2));
		}

		WhsOrder Order1;
		WhsOrder Order2;

		ZDateTime GetEventDateTime(ZString eventCode, WhsOrder order)
		{
			return order.Logs.Find(t => t.SL_SE_NKEvent == eventCode).Select(t => t.SL_EventTime).Single();
		}

		bool CompletedMilestoneExistsForThisEvent(ZString eventCode, WhsOrder order)
		{
			foreach (ProcessTask task in order.WorkflowItems)
			{
				if (task.P9_SE_NKMilestoneEvent == eventCode)
				{
					if (!task.P9_ActualDate.IsEmpty)
					{
						return true;
					}

					return false;
				}
			}

			return false;
		}

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
