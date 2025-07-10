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
	[TestedType(typeof(PickFaceReplenishmentServiceTask))]
	class PickFaceReplenishmentServiceTaskTest : ServiceTaskTestCase<PickFaceReplenishmentServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertEquals("Since there are no locations to create replenishment transfers, warning should be shown.",
				"Information|Did not find any Locations that needed replenishing.", logger.ToString().Trim());
		}

		[TestDate(2023, 1, 1)]
		public void TestServiceTask_EndToEnd()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WH1", "A", 4, 1);
			var warehouse2 = helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = helper.CreateClient("CL1");
			var client2 = helper.CreateClient("CL2");
			var product1 = helper.CreateProduct("PROD1", client1);
			var product2 = helper.CreateProduct("PROD2", client2);
			Factory.Save();

			var bulkLocation1Whs1 = warehouse1.FindLocation("A-1");
			var bulkLocation2Whs1 = warehouse1.FindLocation("A-2");
			var pickFaceLocation1Whs1 = warehouse1.FindLocation("A-3");
			var pickFaceLocation2Whs1 = warehouse1.FindLocation("A-4");
			helper.CreateProductPickFace(product1, client1, pickFaceLocation1Whs1, 1m, 10m);
			helper.CreateProductPickFace(product2, client2, pickFaceLocation2Whs1, 1m, 10m);

			var bulkLocationWhs2 = warehouse2.FindLocation("B-1");
			var pickFaceLocationWhs2 = warehouse2.FindLocation("B-2");
			helper.CreateProductPickFace(product1, client1, pickFaceLocationWhs2, 1m, 10m);
			Factory.Save();

			var receive1 = helper.CreateWhsReceive(client1.PK, warehouse1.PK, "R1", ZDateTimeOffset.Today);
			helper.CreateWhsReceiveInventoryLine(receive1, product1, 10m, bulkLocation1Whs1, "");
			receive1.FinaliseDocket();

			var receive2 = helper.CreateWhsReceive(client2.PK, warehouse1.PK, "R2", ZDateTimeOffset.Today);
			helper.CreateWhsReceiveInventoryLine(receive2, product2, 10m, bulkLocation2Whs1, "");
			receive2.FinaliseDocket();

			var receive3 = helper.CreateWhsReceive(client1.PK, warehouse2.PK, "R2", ZDateTimeOffset.Today);
			helper.CreateWhsReceiveInventoryLine(receive3, product1, 10m, bulkLocationWhs2, "");
			receive3.FinaliseDocket();
			Factory.Save();
			Assert(receive1.IsFinalised);
			Assert(receive2.IsFinalised);
			Assert(receive3.IsFinalised);

			var logger = RunServiceTask();

			var logs = logger.ToString();
			Assert(logs.Contains("Information|Transfer W00000004: was created successfully."));
			Assert(logs.Contains("Information|Transfer W00000005: was created successfully."));
			Assert(logs.Contains("Information|Transfer W00000006: was created successfully."));
			Assert(logs.Contains("Information|Pick Face: A-4 is to be replenished with 10 Product: PROD2 for Client: CL2 from Warehouse: WH1 Location: A-2."));
			Assert(logs.Contains("Information|Pick Face: A-3 is to be replenished with 10 Product: PROD1 for Client: CL1 from Warehouse: WH1 Location: A-1."));
			Assert(logs.Contains("Information|Pick Face: B-2 is to be replenished with 10 Product: PROD1 for Client: CL1 from Warehouse: WH2 Location: B-1."));
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(PickFaceReplenishmentServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == "PFR");
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new PickFaceReplenishmentServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("PFR", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
