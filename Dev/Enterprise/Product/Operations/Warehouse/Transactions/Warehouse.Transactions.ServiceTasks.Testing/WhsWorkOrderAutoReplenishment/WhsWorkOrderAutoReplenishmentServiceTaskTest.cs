using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(WhsWorkOrderAutoReplenishmentServiceTask))]
	class WhsWorkOrderAutoReplenishmentServiceTaskTest : ServiceTaskTestCase<WhsWorkOrderAutoReplenishmentServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = RunServiceTask();
			AssertEquals("Information|Did not find any Products that needed replenishing.", logger.ToString().Trim());
		}

		public void TestServiceTask_EndToEnd()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = helper.CreateClient("CL1");
			var part1 = CreateBOM(client1, "PROD1");
			var client2 = helper.CreateClient("CL2");
			var part2 = CreateBOM(client2, "PROD2");
			var client3 = helper.CreateClient("CL3");
			var part3 = CreateBOM(client3, "PROD3");
			Factory.Save();

			var product1 = WhsProduct.GetWhsProduct(part1);
			helper.CreateProductParamsByWhsAndClient(part1, client1, warehouse1, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product1.ParamsByWhsAndClient.Count);

			var product2 = WhsProduct.GetWhsProduct(part2);
			helper.CreateProductParamsByWhsAndClient(part2, client2, warehouse2, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product2.ParamsByWhsAndClient.Count);

			var product3 = WhsProduct.GetWhsProduct(part3);
			helper.CreateProductParamsByWhsAndClient(part3, client3, warehouse1, 5m, 50m, 1m, "");
			AssertEquals("Product has Param", 1, product3.ParamsByWhsAndClient.Count);
			Factory.Save();

			RunServiceTask();

			var workOrder1 = GetWorkOrder(client1, warehouse1);
			AssertWorkOrder(workOrder1.Lines.Single(), client1, warehouse1, part1, 50m);

			var workOrder2 = GetWorkOrder(client2, warehouse2);
			AssertWorkOrder(workOrder2.Lines.Single(), client2, warehouse2, part2, 50m);

			var workOrder3 = GetWorkOrder(client3, warehouse1);
			AssertWorkOrder(workOrder3.Lines.Single(), client3, warehouse1, part3, 50m);

			OrgSupplierPart CreateBOM(OrgHeader client, string code)
			{
				var bom = helper.CreateProduct(client, code);
				bom.OP_KitIsAutoReplenished = true;

				var subBOM = helper.CreateProduct(client, "Sub" + code);
				helper.CreateProductBOM(bom, subBOM);

				return bom;
			}

			void AssertWorkOrder(WhsDocketLine workOrderLine, OrgHeader expectedClient, WhsWarehouse expectedWarehouse, OrgSupplierPart expectedProduct, decimal expectedQuantity)
			{
				AssertNotNull(workOrderLine);
				AssertEquals(expectedClient, workOrderLine.Docket.Client);
				AssertEquals(expectedWarehouse, workOrderLine.Docket.Warehouse);
				AssertEquals(expectedProduct, workOrderLine.Product.Parent);
				AssertEquals(expectedQuantity, workOrderLine.WE_TransactionQuantity);
			}

			WhsWorkOrder GetWorkOrder(OrgHeader client, WhsWarehouse warehouse, WhsWorkOrder excludeWorkOrderPK = null, OrgSupplierPart part = null)
			{
				var workOrderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				workOrderQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, client.PK);
				workOrderQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);
				workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);
				workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, WorkOrderType.Codes.Assemble);

				if (excludeWorkOrderPK != null)
				{
					workOrderQuery.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, excludeWorkOrderPK.PK);
				}
				if (part != null)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
					subQuery.AddToFilter(WhsDocketLineSchema.WE_OP, part.PK);

					workOrderQuery.AddSubQuery(subQuery, JoinCondition.And);
				}

				var workOrders = Factory.Load<WhsWorkOrder>(workOrderQuery);
				AssertEquals(true, workOrders.Length <= 1);

				return workOrders.SingleOrDefault();
			}
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(WhsWorkOrderAutoReplenishmentServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == "RWO");
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new WhsWorkOrderAutoReplenishmentServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("RWO", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
