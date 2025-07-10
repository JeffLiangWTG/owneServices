using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.ProductionRules.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[GuiTest]
	[UseSnapshotProtection]
	public class CycleCountTaskCreationEndToEndTests : TestCase
	{
		public void TestCycleCountAutomation_NoScheduledRules()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A", scheduleRule: false);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(string.Empty, serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_NothingCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: warehouse.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "LOL");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|No cycle count tasks were created in this run.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_SingleLocation_PWA() => TestCycleCountAutomation_SingleLocation(1, "PWA");
		public void TestCycleCountAutomation_SingleLocation_PWP() => TestCycleCountAutomation_SingleLocation(3, "PWP");

		void TestCycleCountAutomation_SingleLocation(int priority, string granularity)
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var rowB = Helper.CreateRowAndGenerateLocations(whs, "B", 1, 1);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A", priority: priority, granularity: granularity);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", rowA.Locations.Single().PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", priority, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", granularity, cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_SingleLocation_Void()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			location.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|No cycle count tasks were created in this run.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_SingleLocation_OpenTask()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");
			Assert("Precondition: Cycle Count is not finalised.", cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|No cycle count tasks were created in this run.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_SingleLocation_FinalisedTask()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Assert("Precondition: Cycle Count is finalised.", !cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_SingleLocation_OpenVariance()
		{
			var client1 = Helper.CreateClient("A");
			var product1 = Helper.CreateProduct(client1, "A");

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 5, expectedQty: 0, client: client1, part: product1);

			AssertEquals("Cycle Count is finalised", false, cycleCount.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatus.Codes.Open, locationVariance.WCC_Status);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|No cycle count tasks were created in this run.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_SingleLocation_FinalisedVariance_Approved()
			=> TestCycleCountAutomation_SingleLocation_FinalisedVariance(CycleCountVarianceStatus.Codes.Approved);

		public void TestCycleCountAutomation_SingleLocation_FinalisedVariance_Rejected()
			=> TestCycleCountAutomation_SingleLocation_FinalisedVariance(CycleCountVarianceStatus.Codes.Rejected);

		void TestCycleCountAutomation_SingleLocation_FinalisedVariance(string varianceStatus)
		{
			var client1 = Helper.CreateClient("A");
			var product1 = Helper.CreateProduct(client1, "A");

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, varianceStatus,
				varianceQty: 0, expectedQty: 10, client: client1, part: product1);

			AssertEquals("Cycle Count is finalised", false, cycleCount.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should not be open", varianceStatus, locationVariance.WCC_Status);

			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_SingleLocation_MultipleClients()
		{
			var client1 = Helper.CreateClient("A");
			var client2 = Helper.CreateClient("B");

			var product = Helper.CreateProduct(client1, "A");
			Helper.CreateProductClientRelationShip(client2, product);

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", product, 100m);
			Helper.CreateWhsReceiveWithInventory(client2, whs, "R2", product, 100m);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", rowA.Locations.Single().PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_ClientFilter()
		{
			var client1 = Helper.CreateClient("A");
			var client2 = Helper.CreateClient("B");

			var product = Helper.CreateProduct(client1, "A");
			Helper.CreateProductClientRelationShip(client2, product);

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var location1 = whs.FindLocation("A-1-1");
			var location2 = whs.FindLocation("A-1-2");
			var location3 = whs.FindLocation("A-2-1");
			var location4 = whs.FindLocation("A-2-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", product, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(client1, whs, "R2", product, 100m, location2, "");
			Helper.CreateWhsReceiveWithInventory(client2, whs, "R3", product, 100m, location4, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client1, whs, product, 100m);
			AssertNotNull("Precondition: Reserved.", order.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0]));

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition: Finalised pick.", true, pick.IsFinalised);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule = CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			rule.PRL_RuleDefinition = rule.PRL_RuleDefinition.Replace("RowName", "Client.Code");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", location2.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_SingleLocation_MultipleProducts()
		{
			var client1 = Helper.CreateClient("A");
			var product1 = Helper.CreateProduct(client1, "A");
			var product2 = Helper.CreateProduct(client1, "B");

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", product1, 100m);
			Helper.CreateWhsReceiveWithInventory(client1, whs, "R2", product2, 100m);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", rowA.Locations.Single().PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_ProductFilter()
		{
			var client1 = Helper.CreateClient("A");
			var product1 = Helper.CreateProduct(client1, "A");
			var product2 = Helper.CreateProduct(client1, "B");

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var location1 = whs.FindLocation("A-1-1");
			var location2 = whs.FindLocation("A-1-2");
			var location3 = whs.FindLocation("A-2-1");
			var location4 = whs.FindLocation("A-2-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", product1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(client1, whs, "R2", product1, 100m, location2, "");
			Helper.CreateWhsReceiveWithInventory(client1, whs, "R3", product2, 100m, location4, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client1, whs, product1, 100m);
			AssertNotNull("Precondition: Reserved.", order.Lines[0].ReserveStockIfAbleTo(receive1.Inventory[0]));

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition: Finalised pick.", true, pick.IsFinalised);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule = CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			rule.PRL_RuleDefinition = rule.PRL_RuleDefinition.Replace("RowName", "Product.Code");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery()).Single();
			AssertEquals("Should have correct location.", location2.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_MultipleLocations()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A-1-1.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Created Cycle Count Task for Location: A-1-3.
Information|Created Cycle Count Task for Location: A-2-1.
Information|Created Cycle Count Task for Location: A-2-2.
Information|Created Cycle Count Task for Location: A-2-3.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 6 tasks.", 6, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_MultipleLocations_MaxTasks()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A", maximumTasksPerRun: 4);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A-1-1.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Created Cycle Count Task for Location: A-1-3.
Information|Created Cycle Count Task for Location: A-2-1.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 4 tasks.", 4, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_MultipleRules()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 3, 3);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			CreateRule(ruleSet, "Create Cycle Count Task 1", ZDateTime.Today.AddDays(-1), rowName: "A", maximumTasksPerRun: 3);
			CreateRule(ruleSet, "Create Cycle Count Task 2", ZDateTime.Today.AddDays(-1), rowName: "A", maximumTasksPerRun: 3, rulePriority: 2);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task 1, Create Cycle Count Task 2.
Information|Created Cycle Count Task for Location: A-1-1.
Information|Created Cycle Count Task for Location: A-1-2.
Information|Created Cycle Count Task for Location: A-1-3.
Information|Created Cycle Count Task for Location: A-2-1.
Information|Created Cycle Count Task for Location: A-2-2.
Information|Created Cycle Count Task for Location: A-2-3.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task 1, Create Cycle Count Task 2.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 6 tasks.", 6, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_MultipleScheduledRules_MultipleRuleSets()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs1RowA = Helper.CreateRowAndGenerateLocations(whs1, "A", 1, 1);

			var whs2 = Helper.CreateWarehouse("WH2");
			var whs2RowA = Helper.CreateRowAndGenerateLocations(whs2, "A", 1, 1);
			Factory.Save();

			var ruleSet1 = ProductionRuleHelper.CreateRuleSet("Cycle Counting 1", "Cycle Counting", true, context: "PWC", warehousePK: whs1.PK);
			CreateRule(ruleSet1, "Create Cycle Count Task", ZDateTime.Today.AddDays(-2), rowName: "A", priority: 1, granularity: "PWA");

			var ruleSet2 = ProductionRuleHelper.CreateRuleSet("Cycle Counting 2", "Cycle Counting", true, context: "PWC", warehousePK: whs2.PK);
			CreateRule(ruleSet2, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A", priority: 2, granularity: "PWP");

			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting 1 for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting 1 for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Processing RuleSet: Cycle Counting 2 for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting 2 for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should have created 2 tasks.", 2, cycleCountTasks.Length);

			var whs1Task = cycleCountTasks.Single(cct => cct.WCL_WL_Location == whs1RowA.Locations.Single().PK);
			AssertEquals("Should have correct priority.", (byte)1, whs1Task.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", whs1Task.WCL_Granularity);

			var whs2Task = cycleCountTasks.Single(cct => cct.WCL_WL_Location == whs2RowA.Locations.Single().PK);
			AssertEquals("Should have correct priority.", (byte)2, whs2Task.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWP", whs2Task.WCL_Granularity);
		}

		public void TestCycleCountAutomation_WithUserDefinedProperties()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Assert("Precondition: Cycle Count is finalised.", !cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			ProductionRuleHelper.CreateUserDefinedProperty("BOO", "CreateTask", factUniqueKey: "CCL");
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule1 = productionHelper.CreateRule(ruleSet, "Set Property", "Set Property", 1);
			rule1.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""RowName"",
      ""operation"": ""equals"",
      ""value"": ""A""
    },
  ],
  ""action"": {
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""CreateTask"",
    ""value"": true
  }
}"
			.TrimStart();

			var rule2 = productionHelper.CreateRule(ruleSet, "Create Cycle Count Task", "Create Cycle Count Task", 10);
			rule2.PRL_RuleDefinition = $@"
{{
	""conditions"": [
        {{
            ""fieldPath"":""CreateTask"",
            ""operation"":""equals"",
            ""value"":true
        }}
	],
	""action"":	{{
		""$type"": ""CreateCycleCountTaskActionState"",
		""sortByCriteria"": [
			{{""propertyPath"":""LocationStringSortIndex"",""direction"":""ascending""}}
		],
		""priority"": 0,
		""granularity"": ""PWA"",
		""maximumTasksPerRun"": 0
	}}
}}".TrimStart();
			ProductionRuleHelper.CreateScheduledRuleQueue(rule2, ZDateTime.Now.AddDays(-2));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task, Set Property.
Information|Created Cycle Count Task for Location: A.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task, Set Property.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTask = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK)).Single();
			AssertEquals("Should have correct location.", location.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Should have correct priority.", ZByte.Zero, cycleCountTask.WCL_Priority);
			AssertEquals("Should have correct granularity.", "PWA", cycleCountTask.WCL_Granularity);
		}

		public void TestCycleCountAutomation_Error()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule = CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var recipient = Factory.NewWithValidTestData<GlbStaff>();
			recipient.GS_EmailAddress = "testingEmail@email.com.au";
			recipient.GS_Code = "ABC";

			Factory.Save();

			var creatorMock = new Mock<IWhsCycleCountLocationCreator>();
			creatorMock
				.Setup(cc => cc.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsCycleCountLocationInfo>>()))
				.Throws(new Exception("ERROR!"));

			var serviceLogger = new TestServiceLogger();
			using (ObjectFactory.Substitute(creatorMock.Object))
			using (WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK))
			using (SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertEquals("An email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContainsExactElementsInAnyOrder(new[] { "testingEmail@email.com.au" }, errorEmail1.Recipients.ToStringCollection());
			AssertEquals("Cycle Counting Rule Set Failure", errorEmail1.Subject);
			AssertEquals($@"Production Rule Set Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation disabled the following rule(s) due to repeated failure to process:
Create Cycle Count Task

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


You have received this email because you are a member of the staff group defined at System Registry: Warehouse -> Inventory Accuracy Management -> Cycle Count Task Creation Failure Notification Group.", errorEmail1.Body);

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("Should not have created any cycle counting tasks.", 0, cycleCountTasks.Length);

			var scheduleTask = newFactory.Load<ProductionRuleScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ParentID, rule.PK)).Single();
			AssertEquals("Scheduled task is disabled.", false, scheduleTask.S5_IsActive);

			AssertEquals("Should have logged service tasks.",
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Error|Unexpected error occurred: ERROR!
Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Error|Unexpected error occurred: ERROR!
", serviceLogger.ToString());

			ErrorReporter.Clear();
		}

		public void TestCycleCountAutomation_TaskCreatedConcurrently()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule = CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var concreteCreator = new WhsCycleCountLocationCreator();

			WhsCycleCountLocation cycleCount = null;
			var creatorMock = new Mock<IWhsCycleCountLocationCreator>();
			creatorMock
				.Setup(cc => cc.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsCycleCountLocationInfo>>()))
				.Returns<BusinessObjectFactory, IEnumerable<WhsCycleCountLocationInfo>>(
					(f, infos) =>
					{
						var newHelper = new WhsTestHelperFunctions(f);
						cycleCount = newHelper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");
						Assert("Precondition: Cycle Count is not finalised.", cycleCount.WCL_EndTime.IsEmpty);
						f.Save();

						return concreteCreator.CreateCycleCountLocations(f, infos);
					});

			var serviceLogger = new TestServiceLogger();
			using (ObjectFactory.Substitute(creatorMock.Object))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertNotNull("Precondition: Delegate hit.", cycleCount);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Warning|Skipped creating Cycle Count Task for Location: A as one already exists.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		public void TestCycleCountAutomation_TaskCreatedConcurrently_Variance()
		{
			var client1 = Helper.CreateClient("A");
			var product1 = Helper.CreateProduct(client1, "A");

			var whs = Helper.CreateWarehouse("WHS");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = rowA.Locations.Single();
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Cycle Counting", "Cycle Counting", true, context: "PWC", warehousePK: whs.PK);
			var rule = CreateRule(ruleSet, "Create Cycle Count Task", ZDateTime.Today.AddDays(-1), rowName: "A");
			Factory.Save();

			var concreteCreator = new WhsCycleCountLocationCreator();

			WhsCycleCountLocation cycleCount = null;
			var creatorMock = new Mock<IWhsCycleCountLocationCreator>();
			creatorMock
				.Setup(cc => cc.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsCycleCountLocationInfo>>()))
				.Returns<BusinessObjectFactory, IEnumerable<WhsCycleCountLocationInfo>>(
					(f, infos) =>
					{
						var newHelper = new WhsTestHelperFunctions(f);
						var now = DateTimeOffset.Now;
						cycleCount = newHelper.CreateWhsCycleCountLocation(location,
							CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

						var locationVariance = newHelper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
							varianceQty: 5, expectedQty: 0, client: client1, part: product1);

						AssertEquals("Cycle Count is finalised", false, cycleCount.WCL_EndTime.IsEmpty);
						AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatus.Codes.Open, locationVariance.WCC_Status);
						f.Save();

						return concreteCreator.CreateCycleCountLocations(f, infos);
					});

			var serviceLogger = new TestServiceLogger();
			using (ObjectFactory.Substitute(creatorMock.Object))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertNotNull("Precondition: Delegate hit.", cycleCount);

			AssertEquals(
@"Information|Processing RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
Warning|Skipped creating Cycle Count Task for Location: A as one already exists.
Information|Succesfully processed and saved RuleSet: Cycle Counting for Context: ProductWarehouseCycleCountTaskCreation, Scheduled Rules: Create Cycle Count Task.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var cycleCountTasks = newFactory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.PK, SQLComparisonOperator.NotEqual, cycleCount.PK));
			AssertEquals("Should have created no tasks.", 0, cycleCountTasks.Length);
		}

		#region Implementation

		void RunProductionRuleQueueConsumer(TestServiceLogger serviceLogger)
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SPC", canRunInAnyBranch: true))
			{
				var queueConsumer = ObjectFactory.Get<IScheduledProductionRuleQueueConsumer>();
				queueConsumer.ProcessQueue(serviceLogger.GetTaskNotificationSubscriber(), It.IsAny<CancellationToken>());
			}
		}

		ProductionRule CreateRule(
			ProductionRuleSet ruleSet,
			string ruleName,
			ZDateTime queueCreateTime,
			string rowName,
			int priority = 0,
			string granularity = "PWA",
			int maximumTasksPerRun = 0,
			bool scheduleRule = true,
			short rulePriority = 1)
		{
			var rule = ProductionRuleHelper.CreateRule(ruleSet, ruleName, ruleName, rulePriority);
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
        {{
            ""fieldPath"":""RowName"",
            ""operation"":""equals"",
            ""value"":""{rowName}""
        }}
	],
	""action"":	{{
		""$type"": ""CreateCycleCountTaskActionState"",
		""sortByCriteria"": [
			{{""propertyPath"":""LocationStringSortIndex"",""direction"":""ascending""}}
		],
		""priority"": {priority},
		""granularity"": ""{granularity}"",
		""maximumTasksPerRun"": {maximumTasksPerRun}
	}}
}}".TrimStart();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule.PK;
			scheduleTask.S5_ScheduleDescription = rule.PRL_Name;
			scheduleTask.S5_DayList = "NNYYYYN";
			scheduleTask.S5_ScheduleType = "W";
			scheduleTask.S5_DailyStartTime = ZDateTime.Today;
			scheduleTask.S5_ParentID = rule.PK;

			if (scheduleRule)
			{
				ProductionRuleHelper.CreateScheduledRuleQueue(rule, queueCreateTime);
			}

			return rule;
		}

		Helper ProductionRuleHelper => productionHelper ?? (productionHelper = new Helper(Factory));
		Helper productionHelper;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection));
		BusinessObjectFactory factory;

		DbConnection testCaseDbConnection;

		protected override void SetUp()
		{
			testCaseDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion
	}
}
