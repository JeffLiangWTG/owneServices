using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(ABCAnalysisServiceTask))]
	class ABCAnalysisServiceTaskTest : ServiceTaskTestCase<ABCAnalysisServiceTask>
	{
		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
None
".Trim(), logger.ToString());
		}

		[TestDate(2023, 10, 5, 1, 0, 0)]
		public void TestServiceTask_EndToEnd()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WHS", "Big Warehouse");
			warehouse1.WW_ABCAnalysisEnabled = true;
			var warehouse2 = helper.CreateWarehouse("ABC", "ABC Warehouse");
			warehouse2.WW_ABCAnalysisEnabled = true;
			var warehouse3 = helper.CreateWarehouse("DEF", "ABC Warehouse");
			warehouse3.WW_ABCAnalysisEnabled = true;
			warehouse3.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse3.WW_GB_RelatedCompanyBranch = warehouse2.WW_GB_RelatedCompanyBranch;

			var client = helper.CreateClient("TRANS", "TRANSLOGIC");
			client.OH_IsWarehouseClient = true;
			client.MiscServ.OM_WhsABCAnalysisEnabled = true;
			client.MiscServ.OM_WhsABCAnalysisMethod = "QTC";
			client.MiscServ.OM_WhsABCAnalysisPeriod = "QLY";

			var product1 = helper.CreateProduct(client, "P1");
			var product2 = helper.CreateProduct(client, "P2");

			// Data
			helper.CreateWhsReceiveWithInventory(client, warehouse1, "R01", product1, 1000m);
			helper.CreateWhsReceiveWithInventory(client, warehouse1, "R02", product2, 1000m);

			helper.CreateProductParamsByWhsAndClient(product2, client, warehouse1, 0);

			// Orders
			var order1 = helper.CreateWhsOrder(client, warehouse1, "O1");
			helper.CreateWhsOrderLine(order1, product1, 2m);
			helper.CreatePickNew(finaliseOrders: true, finalisePick: false, order1);
			order1.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-16);

			var order2 = helper.CreateWhsOrder(client, warehouse1, "O2");
			helper.CreateWhsOrderLine(order2, product1, 2m);
			helper.CreateWhsOrderLine(order2, product2, 2m);
			helper.CreateWhsOrderLine(order2, product1, 4m);
			helper.CreatePickNew(finaliseOrders: true, finalisePick: false, order2);
			order2.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-16);
			Factory.Save();

			var logger = RunServiceTask();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var warehouse2Categories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouse2.PK);
			AssertEquals(0, warehouse2Categories.Count());

			var warehouse3Categories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouse3.PK);
			AssertEquals(0, warehouse3Categories.Count());
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(ABCAnalysisServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == "ABC");
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new ABCAnalysisServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);

			using (EnvProxy.Instance.TemporaryServiceTaskContext("ABC", canRunInAnyBranch: true))
			{	
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
