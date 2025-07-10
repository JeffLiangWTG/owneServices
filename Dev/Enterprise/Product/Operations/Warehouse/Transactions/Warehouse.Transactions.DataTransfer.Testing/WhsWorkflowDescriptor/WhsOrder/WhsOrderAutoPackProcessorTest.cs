using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderAutoPackProcessorTest : WhsTestCaseWithFactory
	{
		public void TestPickCompletedEventTriggerAutoPackAction()
		{
			AssertPickCompletedEventTriggerAutoPackAction(isPrinting: true);
		}

		public void TestPickCompletedEventTriggerAutoPackAction_WithoutPrinting()
		{
			AssertPickCompletedEventTriggerAutoPackAction(isPrinting: false);
		}

		void AssertPickCompletedEventTriggerAutoPackAction(bool isPrinting)
		{
			var notify = new TestNotificationBuffer();
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "x", data.Part1, 500, locations[0], "PL1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "y", data.Part2, 500, locations[1], "PL2");
			Helper.CreateProductUnit(data.Part1, "PLT", 4);
			Helper.CreateProductUnit(data.Part2, "PLT", 10);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
				Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m).WE_F3_NKPackType = "PLT";
			Helper.CreateWhsOrderLine(order1, data.Part2, 2m).WE_F3_NKPackType = "PLT";
			Factory.Save();

			var order1Trigger = CreateTriggerForOrder(order1);
			var order1Action = CreateTriggerActionForOrder(order1Trigger);

			var pick = Helper.CreatePickNew(order1);
			pick.AutoAllocateItemsWithMock();

			SetPickedDateInAvailableInventoriesForOrgPart(data.Part1, locations[0], pick);
			SetPickedDateInAvailableInventoriesForOrgPart(data.Part2, locations[1], pick);

			AssertEquals("All locations (100%) should be marked as Picked if Pick was finalised", (ZByte)100,
				pick.WP_PercentageComplete);
			Assert("PickCompleted event should be created in Pick",
				pick.Logs.Find(GetLogFilter(pick.PK, Events.ServiceCompleted.Code)).Length == 1);

			var order1Logs = order1.Logs.Find(GetLogFilter(order1.PK, Events.ServiceCompleted.Code));
			Assert("PickCompleted event should be created in Order1", order1Logs.Length == 1);

			AssertEquals("No Package should be in PackageJob.", 0, order1.PackageJob.Packages.Count);
			AssertEquals("No PrintJob should be created.", 0, Factory.Load<IStmPrintJob>(new ZQuery()).Length);

			var logger = new NotificationsForTesting();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			using (WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, !isPrinting))
			using (new FactorySaveAlerterForTest())
			{
				var queuedLog = new QueuedLogForTesting(order1Logs[0], order1Trigger);
				var processor = workFlowDescriptor.GetWorkflowTriggerAction(order1Action, queuedLog);
				AssertNoExceptionThrown(() => processor.Process(logger));
				AssertMultilineASCIIEquals("No errors/warnings are logged.", string.Empty, logger.ToString());
			}

			AssertEquals("No Package should be in PackageJob.", 0,
				new BusinessObjectFactory().Load<WhsOrder>(order1.PK).PackageJob.Packages.Count);
			AssertEquals("No PrintJob should be created.", 0,
				new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery()).Length);

			Factory.Save();

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order1.PK);
			AssertAutoPackedItems(orderInNewFactory.PackageJob, data);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			if (isPrinting)
			{
				AssertEquals("There should be a print job for all 4 packages.", 4, printJobs.Length);
				AssertPrintJobParentEqualsPackageJob(orderInNewFactory, printJobs);
			}
			else
			{
				AssertEquals("There should be no print jobs when printing is disabled.", 0, printJobs.Length);
			}
		}

		#region SetPickedDateInAvailableInventoriesForOrgPart

		void SetPickedDateInAvailableInventoriesForOrgPart(OrgSupplierPart part, WhsLocation location, WhsPick pick)
		{
			var partOrderdInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(o => o.SupplierPart == part);
			var partAvailableInventories = partOrderdInventory.AvailableInventories.Cast<WhsPickAvailableInventory>();
			partAvailableInventories.Single(o => o.Location == location).AvailableInventoriesSplitByPickedDetails
				.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Today;
		}

		#endregion

		#region CreateTriggerForOrder

		static ProcessTask CreateTriggerForOrder(WhsOrder order)
		{
			var trigger = order.WorkflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceCompletedCode;

			return trigger;
		}

		#endregion

		#region CreateTriggerActionForOrder

		static ProcessTaskNotification CreateTriggerActionForOrder(ProcessTask trigger)
		{
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoPack;

			return action;
		}

		#endregion

		#region AssertPrintJobParentEqualsPackageJob

		void AssertPrintJobParentEqualsPackageJob(WhsOrder order1, IStmPrintJob[] printJobs)
		{
			foreach (StmPrintJob job in printJobs)
			{
				AssertEquals("The job's parent should match the packagejob.", order1.PackageJob.PK, job.SP_ParentGuid);
			}
		}

		#endregion

		#region AssertAutoPackedItems

		void AssertAutoPackedItems(PkgPackageJob packageJob, TestDataSimpleEnvironment data)
		{
			// expect 4x PLT, 2 with 4 items each, 2 with with 10 items each.
			var part1Packages = packageJob.Packages.Where(p =>
				p.KP_F3_NKPackType == "PLT" && p.PackedItemDivots[0].KI_PackedQty == 4);
			var part2Packages = packageJob.Packages.Where(p =>
				p.KP_F3_NKPackType == "PLT" && p.PackedItemDivots[0].KI_PackedQty == 10);

			AssertEquals(4, packageJob.Packages.Count);
			AssertEquals(2, part1Packages.Count());
			AssertEquals(2, part2Packages.Count());

			foreach (var package in part1Packages)
			{
				var divot = package.PackedItemDivots.Single();
				AssertEquals(4m, divot.KI_PackedQty);
			}

			foreach (var package in part2Packages)
			{
				var divot = package.PackedItemDivots.Single();
				AssertEquals(10m, divot.KI_PackedQty);
				Assert(!package.KP_PackageID.IsEmpty);
				Assert(package.IsClosed);
			}
		}

		#endregion

		#region NotificationsForTesting

		class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			readonly List<string> notifications = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}

		#endregion

		#region GetLogFilter

		protected ZQuery GetLogFilter(ZGuid pK, ZString code, string reference)
		{
			ZQuery logFilter = new ZQuery(StmALogSchema.SL_Parent, new ZGuid(pK));
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, code);
			if (reference != null)
			{
				logFilter.AddToFilter(StmALogSchema.SL_Reference, reference);
			}

			return logFilter;
		}

		protected ZQuery GetLogFilter(ZGuid pK, ZString code)
		{
			return GetLogFilter(pK, code, null);
		}

		#endregion
	}
}
