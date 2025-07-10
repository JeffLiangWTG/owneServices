using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask))]
	class AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTaskTest : ServiceTaskTestCase<AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information|Did not find any picks with Waiting Replenishment status.".Trim(), logger.ToString());
		}

		public void TestServiceTask_EndToEnd()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var helper = new WhsTestHelperFunctions(Factory);
				var warehouse = helper.CreateWarehouse("WH1", "A", 2, 1);
				Factory.Save();

				var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
				AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
				AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

				var client = helper.CreateClient("CL1");
				var product = helper.CreateProduct("PROD1", client);
				helper.CreateProductUnit(product, Constants.PkgUnit.Pallet, 3m); // Part1 : 1PLT = 3 UNT

				var pickFaceLocationWhs = warehouse.FindLocation("A-1");
				var bulkLocationWhs = warehouse.FindLocation("A-2");
				helper.CreateProductPickFace(product, client, pickFaceLocationWhs);
				helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", ZDateTimeOffset.Today, product, 40m, bulkLocationWhs, "");
				helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", ZDateTimeOffset.Today, product, 15m, pickFaceLocationWhs, "");

				var order = helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 16m, WhsPickOption.Codes.Manual);
				var pick = helper.CreatePickNew(order);
				pick.WP_IsAwaitingReplenishment = true;
				Factory.Save();
				AssertEquals(true, pick.WP_IsAwaitingReplenishment);

				var transfer = helper.CreateWhsTransfer(client, warehouse, "T1", new TestNotificationBuffer());
				helper.CreateWhsTransferLine(transfer, product, 16m, bulkLocationWhs, pickFaceLocationWhs);
				transfer.FinaliseDocket();
				AssertEquals(true, transfer.IsFinalised);
				Factory.Save();

				var logger = RunServiceTask();

				var logs = logger.ToString();
				Assert("Allocation is successful.", logs.Contains("Pick No: P00000001 has successfully allocated stock."));
				Assert("There are no errors.", !logs.Contains("Error"));
			}
		}

		[TestDate(2022, 2, 2)]
		public void TestQueueProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var orderSequence = 1;
			var itemAgeDays = 1;
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Code);
			AssertNotNull("Should have valid queue provider.", queueProvider);
			AssertEquals("Precondition", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			var expectedItemAge = DateTime.UtcNow - TestDateAttribute.Date;
			CreateWaitingReplenishmentJob();
			AssertEquals("After creating WaitingReplenishment pick, Should have one in queue.", 1, queueProvider.QueueResult.QueueSize);
			NUnit.Framework.Assert.That((int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedItemAge.TotalSeconds).Within(60));

			CreateWaitingReplenishmentJob();
			AssertEquals("After creating WaitingReplenishment pick, Should have two in queue.", 2, queueProvider.QueueResult.QueueSize);
			NUnit.Framework.Assert.That((int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedItemAge.TotalSeconds).Within(60));

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				RunServiceTask();
			}

			AssertEquals("Should process the queue after running service task.", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			void CreateWaitingReplenishmentJob()
			{
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{orderSequence}", data.Part1, 1m);
				var pick = helper.CreatePickNew(order);
				pick.WP_IsAwaitingReplenishment = true;
				pick.WP_SystemLastEditTimeUtc = DateTime.UtcNow.AddDays(-itemAgeDays);
				Factory.Save();
				orderSequence++;
			}
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask.Code);
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						WhsDocketSchema.Constants.TableName,
						null,
						WhsDocketSchema.Constants.WD_IsPickFaceReplenishment + "=1",
						WhsDocketSchema.Constants.WD_FinalisedDate + " IS NOT NULL"),
				};
			}
		}

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new AllocateWaitingReplenishmentPicksWithExistingInventoryServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);

			using (EnvProxy.Instance.TemporaryServiceTaskContext("APW", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
