using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(VASOrderCreateInitialTransferServiceTask))]
	class VASOrderCreateInitialTransferServiceTaskTest : ServiceTaskTestCase<VASOrderCreateInitialTransferServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertEquals("Since there are no VAS Order to initial transfer, Information should be shown.",
				"Information|Did not find any VAS Order with no initial transfer.", logger.ToString().Trim());
		}

		public void TestServiceTask_EndToEnd()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = helper.CreateClient("CL1");
			var product1 = helper.CreateProduct("PROD1", client1);
			var client2 = helper.CreateClient("CL2");
			var product2 = helper.CreateProduct("PROD2", client2);
			var client3 = helper.CreateClient("CL3");
			var product3 = helper.CreateProduct("PROD3", client3);
			Factory.Save();

			helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", product1, 20m);
			helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R2", product2, 20m);
			helper.CreateWhsReceiveWithInventory(client3, warehouse1, "R3", product3, 20m);
			Factory.Save();

			var serviceArea1 = helper.CreateServiceAreaForVASOrder(warehouse1, 1, 2, 1);
			var serviceArea2 = helper.CreateServiceAreaForVASOrder(warehouse2, 1, 2, 1);

			helper.CreateWhsVASOrderWithLine(serviceArea1, client1, product1, quantity: 10m);
			helper.CreateWhsVASOrderWithLine(serviceArea2, client2, product2, quantity: 10m);
			helper.CreateWhsVASOrderWithLine(serviceArea1, client3, product3, quantity: 10m);
			Factory.Save();

			var logger = RunServiceTask();

			var logs = logger.ToString();
			Assert(logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000001 with Client CL1 Warehouse WH1."));
			Assert(logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000002 with Client CL2 Warehouse WH2."));
			Assert(logs.Contains("Information|Initial transfer was created successfully for VAS Order WV00000003 with Client CL3 Warehouse WH1."));
		}

		[TestDate(2023, 11, 1)]
		public void TestQueueProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50);
			var serviceArea = helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();

			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode(VASOrderCreateInitialTransferServiceTask.Code);
			AssertNotNull("Should have valid queue provider.", queueProvider);
			AssertEquals("Precondition", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals("Precondition", TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			var vasOrder1 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, quantity: 1m);
			vasOrder1.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			Assert("Precondition", vasOrder1.IsCancelled);
			AssertEquals("Cancel vasOrder will not add to Queue.", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			var vasOrder2 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, quantity: 1m);
			var logger = new TestServiceLogger();
			var manager = new VASOrderCreateInitialTransferManager(logger);
			manager.CreateTransfersForVASOrdersWithNoInitialTransfer();
			Factory.Save();
			AssertNotNull("Precondition", vasOrder2.TransferIntoServiceArea);
			AssertEquals("VasOrder with initial transfer does not need to be queued. ", 0, queueProvider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, queueProvider.QueueResult.MaximumItemAge);

			var vasOrder3 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, quantity: 1m);
			Factory.Save();
			AssertNull("Precondition", vasOrder3.TransferIntoServiceArea);
			AssertEquals("VasOrder with no initial transfer need to be queued.", 1, queueProvider.QueueResult.QueueSize);
			NUnit.Framework.Assert.That((int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)(DateTime.UtcNow - TestDateAttribute.Date).TotalSeconds).Within(60));

			var vasOrder4 = helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, quantity: 1m);
			Factory.Save();
			AssertNull("Precondition", vasOrder4.TransferIntoServiceArea);
			AssertEquals("VasOrder with no initial transfer need to be queued.", 2, queueProvider.QueueResult.QueueSize);

			RunServiceTask();

			AssertEquals("Should process the queue after runing service task.", 0, queueProvider.QueueResult.QueueSize);
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(VASOrderCreateInitialTransferServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == VASOrderCreateInitialTransferServiceTask.Code);
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new VASOrderCreateInitialTransferServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("CVT", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
