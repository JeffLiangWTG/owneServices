using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
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
using Enterprise.Warehouse.Environment.Business;
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
	public class WaveCreationEndToEndTests : TestCase
	{
		public void TestWaveCreation_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order, product.PK, 10m);
			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			Assert("Order is picked", !orderInNewFactory.WD_WP.IsEmpty);
			AssertEquals("Pick created", 1, newFactory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order, product.PK, 9m);

			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var createdPick = newFactory.Load<WhsPick>(new ZQuery()).Single();

			AssertEquals("Order1 is picked", order1InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Auto allocation has been performed", 9m, createdPick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_Multiple_Orders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 15m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 8m);

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order1.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002, W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var createdPick = newFactory.Load<WhsPick>(new ZQuery()).Single();

			AssertEquals("Order1 is picked", order1InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Order2 is picked", order2InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Auto allocation has been performed", 23m, createdPick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_Same_Pick_In_Short()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			order1.WD_PickPriority = 2;
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product.PK, 10m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			order2.WD_PickPriority = 1;
			var orderLine2 = Helper.CreateWhsOrderLine(order2, product.PK, 10m);

			Factory.Save();

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002, W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var createdPick = newFactory.Load<WhsPick>(new ZQuery()).Single();

			AssertEquals("Order1 is picked", order1InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Order2 is picked", order2InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Order Line 1 has 0 pick line count", 0, orderLine1InNewFactory.PickLines.Count);
			AssertEquals("Order Line 2 has 1 pick line count", 1, orderLine2InNewFactory.PickLines.Count);
			AssertEquals("Auto allocation has been performed", 10m, createdPick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_Multiple_Picks_In_Short()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, product.PK, 10m);

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product.PK, 10m);

			Factory.Save();

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 2", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
Warning|No Stock was allocated to pick.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Created Pick P00000002 with Order(s): W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);

			Assert("Order1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			Assert("Order2 is picked", !order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("Order Line 1 has 0 pick line count", 0, orderLine1InNewFactory.PickLines.Count);
			AssertEquals("Order Line 2 has 1 pick line count", 1, orderLine2InNewFactory.PickLines.Count);
			AssertEquals("Auto allocation has been performed for Order2", 10m, order2InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has not been performed for Order1", 0m, order1InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Same_ConsigneeAddress_Same_Pick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orgAddress = Helper.CreateClient("ZZZ");
			var address = orgAddress.MainAddress;
			order1.ConsigneeDocAddress.E2_AddressOverride = false;
			order1.ConsigneeDocAddress.E2_OA_Address = address.PK;
			address.OA_Address1 = "20 Maxwell Street";
			address.OA_Address2 = "";
			address.OA_City = "PERTH";
			address.OA_PostCode = "6162";
			address.OA_State = "WA";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_RL_NKRelatedPortCode = "AUBYW";

			order2.ConsigneeDocAddress.E2_AddressOverride = false;
			order2.ConsigneeDocAddress.E2_OA_Address = address.PK;

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", "PERTH", order1.ConsigneeDocAddress.E2_City);
			AssertEquals("Precondition", "PERTH", order2.ConsigneeDocAddress.E2_City);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1), groupByProperties: "ConsigneeAddress");

			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002, W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var createdPick = newFactory.Load<WhsPick>(new ZQuery()).Single();

			AssertEquals("Order1 is picked", order1InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Order2 is picked", order2InNewFactory.WD_WP, createdPick.PK);
			AssertEquals("Order Line 1 has 1 pick line count", 1, orderLine1InNewFactory.PickLines.Count);
			AssertEquals("Order Line 2 has 1 pick line count", 1, orderLine2InNewFactory.PickLines.Count);

			AssertEquals("Auto allocation has been performed", 20m, createdPick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has been performed on pick", 20m, createdPick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Different_ConsigneeAddress_Different_Pick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product.PK, 5m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, product.PK, 2m);

			order1.ConsigneeDocAddress.E2_AddressOverride = true;
			order1.ConsigneeDocAddress.E2_CompanyName = "XYZ";
			order1.ConsigneeDocAddress.E2_City = "MELBOURNE";
			order1.ConsigneeDocAddress.E2_State = "VIC";
			order1.ConsigneeDocAddress.E2_Postcode = "3000";
			order1.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_CompanyName = "XYZ";
			order2.ConsigneeDocAddress.E2_City = "MELBOURNE";
			order2.ConsigneeDocAddress.E2_State = "VIC";
			order2.ConsigneeDocAddress.E2_Postcode = "3060";
			order2.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Factory.Save();

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 2, groupByProperties: "ConsigneeAddress");
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var createdPicks = newFactory.Load<WhsPick>(new ZQuery());

			Assert("Order1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			Assert("Order2 is picked", !order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("2 picks have been created", 2, createdPicks.Length);

			var createdPick1 = createdPicks.FirstOrDefault(p => p.PK == order1InNewFactory.WD_WP);
			var createdPick2 = createdPicks.FirstOrDefault(p => p.PK == order2InNewFactory.WD_WP);

			AssertEquals("Auto allocation has been performed on pick 1", 5m, createdPick1.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has been performed on pick 2", 2m, createdPick2.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			AssertEquals("Should have logged service tasks.",
	$@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
Information|Created Pick P00000001 with Order(s): {createdPicks.Single(p => p.WP_PickNo == "P00000001").Orders.Cast<WhsOrder>().Select(o => o.WD_DocketID).Single()}.
Information|Created Pick P00000002 with Order(s): {createdPicks.Single(p => p.WP_PickNo == "P00000002").Orders.Cast<WhsOrder>().Select(o => o.WD_DocketID).Single()}.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_In_Short_Requested_More_Than_Available_Units_In_One_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, product.PK, 100m);

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product.PK, 10m);

			Factory.Save();

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 2", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
Warning|No Stock was allocated to pick.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Created Pick P00000002 with Order(s): W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);

			Assert("Order1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			Assert("Order2 is picked", !order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("Order Line 1 has 0 pick line count", 0, orderLine1InNewFactory.PickLines.Count);
			AssertEquals("Order Line 2 has 1 pick line count", 1, orderLine2InNewFactory.PickLines.Count);
			AssertEquals("Auto allocation has been performed for Order2", 10m, order2InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has not been performed for Order1", 0m, order1InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_In_Short_More_Than_Available_Units_In_Multiple_orders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, product.PK, 8m);

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, product.PK, 8m);

			Factory.Save();

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 2", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Created Pick P00000002 with Order(s): W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);

			Assert("Order1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			Assert("Order2 is picked", !order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("Order Line 1 has 0 pick line count", 1, orderLine1InNewFactory.PickLines.Count);
			AssertEquals("Order Line 2 has 1 pick line count", 1, orderLine2InNewFactory.PickLines.Count);
			AssertEquals("Auto allocation has been performed for Order2", 8m, order2InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has been performed for Order1 partially", 2m, order1InNewFactory.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_Auto_Allocation_Multiple_Picks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 5m);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 2m);

			Assert("Precondition", order1.WD_WP.IsEmpty);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Created Pick P00000002 with Order(s): W00000003.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var createdPicks = newFactory.Load<WhsPick>(new ZQuery());

			Assert("Order1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			Assert("Order2 is picked", !order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("2 picks have been created", 2, createdPicks.Length);

			var createdPick1 = createdPicks.FirstOrDefault(p => p.PK == order1InNewFactory.WD_WP);
			var createdPick2 = createdPicks.FirstOrDefault(p => p.PK == order2InNewFactory.WD_WP);

			AssertEquals("Auto allocation has been performed on pick 1", 5m, createdPick1.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Auto allocation has been performed on pick 2", 2m, createdPick2.OrderedInventories[0].PickLineQuantity);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_MultipleOrdersInPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert(!order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert(!order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals(1, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_MultipleScheduledRules()
		{
			TestWaveCreation_EndToEnd_MultipleScheduledRules(singleRuleCreatesBothPicks: false);
		}

		public void TestWaveCreation_EndToEnd_SingleRuleCreatesMultiplePicks()
		{
			TestWaveCreation_EndToEnd_MultipleScheduledRules(singleRuleCreatesBothPicks: true);
		}

		void TestWaveCreation_EndToEnd_MultipleScheduledRules(bool singleRuleCreatesBothPicks)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1, maxPicksPerRun: singleRuleCreatesBothPicks ? 0 : 1, priority: 10);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 2", ZDateTime.Today.AddDays(-1), maxOrderNumber: 1, maxPicksPerRun: singleRuleCreatesBothPicks ? 0 : 1, priority: 20);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert(!order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert(!order2InNewFactory.WD_WP.IsEmpty);
			AssertNotEquals(order1InNewFactory.WD_WP, order2InNewFactory.WD_WP);

			var picks = Factory.Load<WhsPick>(new ZQuery());
			AssertEquals(2, picks.Length);

			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			AssertEquals("Should have logged service tasks.",
				$@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
Information|Created Pick P00000001 with Order(s): {order1InNewFactory.WD_DocketID}.
Information|Created Pick P00000002 with Order(s): {order2InNewFactory.WD_DocketID}.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1, Create Wave 2.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_MultipleScheduledRules_DifferentRuleSets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse1 = data.Whs1;
			var warehouse2 = Helper.CreateWarehouse("WHB", "B");
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse1.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R2", product, 100m);
			var order2 = Helper.CreateWhsOrder(client.PK, warehouse2.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet1 = ProductionRuleHelper.CreateRuleSet("Wave Creation 1", "Wave Creation", true, context: "PWW", warehousePK: warehouse1.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet1, "Create Wave 1", ZDateTime.Today.AddDays(-1));

			var ruleSet2 = ProductionRuleHelper.CreateRuleSet("Wave Creation 2", "Wave Creation", true, context: "PWW", warehousePK: warehouse2.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet2, "Create Wave 2", ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert(!order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert(!order2InNewFactory.WD_WP.IsEmpty);
			AssertNotEquals(order1InNewFactory.WD_WP, order2InNewFactory.WD_WP);

			AssertEquals(2, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation 2 for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 2.
Information|Created Pick P00000001 with Order(s): W00000004.
Information|Succesfully processed and saved RuleSet: Wave Creation 2 for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 2.
Information|Processing RuleSet: Wave Creation 1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
Information|Created Pick P00000002 with Order(s): W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation 1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_MultipleRules_OnlyOneScheduledRule()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);
			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderLineNumber: 1);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 2", ZDateTime.Today.AddDays(-1), scheduleRule: false);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert("Order 1 is picked", !order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert("Order 2 is not picked", order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("Pick created", 1, newFactory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_NoScheduledRule()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order, product.PK, 10m);
			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			Factory.Save();

			AssertEquals("Precondition: Nothing scheduled", 0, Factory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			Assert("Should not have logged anything.", string.IsNullOrEmpty(serviceLogger.ToString()));

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			Assert("Order is not picked", orderInNewFactory.WD_WP.IsEmpty);
			AssertEquals("No picks created", 0, newFactory.Load<WhsPick>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_NoPicksCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order, product.PK, 10m);
			Helper.CreateWhsOrderLine(order, product.PK, 10m);
			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1), maxOrderLineNumber: 1);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|No picks were created from this run.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			Assert("Order is not picked", orderInNewFactory.WD_WP.IsEmpty);
			AssertEquals("Pick is not created", 0, newFactory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_PickAllocationFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert(!order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert(!order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals(1, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Warning|No Stock was allocated to pick.
Information|Created Pick P00000001 with Order(s): W00000001, W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_PickAllocationFailed_AllocationThrewError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			product.OP_Weight = 999999.99;
			product.OP_WeightUQ = "KG";
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));

			var recipient = Factory.NewWithValidTestData<GlbStaff>();
			recipient.GS_EmailAddress = "testingEmail@email.com.au";
			recipient.GS_Code = "ABC";

			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			using (WarehouseDataRegistry.Instance.WaveCreationRulesFailureNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK))
			using (SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertEquals("An email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContainsExactElementsInAnyOrder(new[] { "testingEmail@email.com.au" }, errorEmail1.Recipients.ToStringCollection());
			AssertEquals("Wave Creation Rule Set Failure", errorEmail1.Subject);
			AssertEquals($@"Production Rule Set Wave Creation for Context: ProductWarehouseWaveCreation disabled the following rule(s) due to repeated failure to process:
Create Wave

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


You have received this email because you are a member of the staff group defined at System Registry: Warehouse -> Picking -> Wave Creation Rules Failure Notification Group.", errorEmail1.Body);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert("No pick created.", order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert("No pick created.", order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("No pick created.", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			var ruleInNewFactory = newFactory.Load<ProductionRule>(new ZQuery(ProductionRuleSchema.PRL_Name, "Create Wave")).Single();
			var scheduleTask = newFactory.Load<ProductionRuleScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ParentID, ruleInNewFactory.PK)).Single();
			AssertEquals("Scheduled task is disabled.", false, scheduleTask.S5_IsActive);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Error|Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products.
Information|No picks were created from this run.
Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Error|Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products.
Information|No picks were created from this run.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_PickAllocationFailed_AllocationThrewError_NoEmailRecipient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order1 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order1, product.PK, 10m);
			Assert("Precondition", order1.WD_WP.IsEmpty);

			var order2 = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O2");
			Helper.CreateWhsOrderLine(order2, product.PK, 10m);
			Assert("Precondition", order2.WD_WP.IsEmpty);

			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			Factory.Save();

			product.OP_Weight = 999999.99;
			product.OP_WeightUQ = "KG";
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			using (WarehouseDataRegistry.Instance.WaveCreationRulesFailureNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK))
			using (SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				RunProductionRuleQueueConsumer(serviceLogger);
			}

			AssertEquals("Email should not have been sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			Assert("No pick created.", order1InNewFactory.WD_WP.IsEmpty);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			Assert("No pick created.", order2InNewFactory.WD_WP.IsEmpty);
			AssertEquals("No pick created.", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);

			var ruleInNewFactory = newFactory.Load<ProductionRule>(new ZQuery(ProductionRuleSchema.PRL_Name, "Create Wave")).Single();
			var scheduleTask = newFactory.Load<ProductionRuleScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ParentID, ruleInNewFactory.PK)).Single();
			AssertEquals("Scheduled task is disabled.", false, scheduleTask.S5_IsActive);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Error|Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products.
Information|No picks were created from this run.
Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Error|Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products.
Information|No picks were created from this run.
", serviceLogger.ToString());
		}

		public void TestWaveCreation_EndToEnd_WithUserDefinedProperty_PickCreated()
		{
			TestWaveCreation_EndToEnd_WithUserDefinedPropertyCore(true);
		}

		public void TestWaveCreation_EndToEnd_WithUserDefinedProperty_PickNotCreated()
		{
			TestWaveCreation_EndToEnd_WithUserDefinedPropertyCore(false);
		}

		void TestWaveCreation_EndToEnd_WithUserDefinedPropertyCore(bool isPickCreated)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var product = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m);
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, client.PK, "O1");
			Helper.CreateWhsOrderLine(order, product.PK, isPickCreated ? 10m : 1m);
			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertEquals("Precondition", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			ProductionRuleHelper.CreateUserDefinedProperty("BOO", "CreateWave", factUniqueKey: "ORD");
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: warehouse.PK);
			var rule1 = productionHelper.CreateRule(ruleSet, "Set Create Wave", "Set Create Wave", 1);
			rule1.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""TotalLineUnits"",
      ""operation"": ""greaterThan"",
      ""value"": 5
    },
  ],
  ""action"": {
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""CreateWave"",
    ""value"": true
  }
}".TrimStart();

			var rule2 = ProductionRuleHelper.CreateRule(ruleSet, "Create Wave", "Create Wave", 10);
			rule2.PRL_RuleDefinition = @"
{
	""conditions"": [
		{
      ""fieldPath"": ""CreateWave"",
      ""operation"": ""equals"",
      ""value"": true
		},
	],
	""action"":	{
		""$type"": ""CreateWaveActionState"",
		""SortByCriteria"": [
		],
		""GroupByCriteria"": [
		],
		""MaximumPicksPerRun"": 0,
		""MaximumOrderNumber"": 0,
		""MaximumOrderLineNumber"": 0,
		""MaximumOrderLineUnits"": 0,
		""MaximumWeight"": 0,
		""MaximumWeightUQ"": ""KG"",
		""MaximumVolume"": 0,
		""MaximumVolumeUQ"": ""CC"",
		""MaximumOrderValue"": 0,
		""MaximumOrderValueCurrency"": ""USD"",
		""PickPalletsByLabel"": false,
		""PickCasesByLabel"": false,
		""CartonizeSplitCases"": false,
		""ForcePickByCaseUOMTypeAllocation"": false,
		""ForceSplitCaseUOMTypeAllocation"": false
	}
}".TrimStart();

			var scheduleTask = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ParentID = rule2.PK;
			scheduleTask.S5_ScheduleDescription = rule2.PRL_Name;
			scheduleTask.S5_DayList = "NNYYYYN";
			scheduleTask.S5_ScheduleType = "W";
			scheduleTask.S5_DailyStartTime = ZDateTime.Today;
			scheduleTask.S5_ParentID = rule2.PK;

			ProductionRuleHelper.CreateScheduledRuleQueue(rule2, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			if (isPickCreated)
			{
				AssertEquals("Should have logged service tasks.",
					@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave, Set Create Wave.
Information|Created Pick P00000001 with Order(s): W00000002.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave, Set Create Wave.
", serviceLogger.ToString());
			}
			else
			{
				AssertEquals("Should have logged service tasks.",
					@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave, Set Create Wave.
Information|No picks were created from this run.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave, Set Create Wave.
", serviceLogger.ToString());
			}

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Order picked", !isPickCreated, orderInNewFactory.WD_WP.IsEmpty);
			AssertEquals("Pick created", isPickCreated ? 1 : 0, newFactory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
		}

		public void TestWaveCreation_EndToEnd_SplitingAwaitingReplenishmentPicks_SingleOrderSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order2.WD_WP);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: data.Whs1.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 (awaiting replenishment) with Order(s): W00000004, W00000005.
Information|Created Pick P00000002 by splitting the awaiting replenishment Pick P00000001. This pick contains Order(s): W00000004.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);

			AssertEquals("2 Picks created", 2, newFactory.Load<WhsPick>(new ZQuery()).Length);
			var pick1 = order1InNewFactory.Pick;
			var pick2 = order2InNewFactory.Pick;

			AssertNotEquals(pick1, pick2);

			AssertNotNull("Picks should now be attached.", pick1);
			AssertEquals(false, pick1.WP_IsAwaitingReplenishment);

			AssertNotNull("Picks should now be attached.", pick2);
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
		}

		public void TestWaveCreation_EndToEnd_SplitingAwaitingReplenishmentPicks_MultipleOrdersSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, bulkLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, bulkLocation, pickFaceLocation);
			transferLine1.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, bulkLocation, pickFaceLocation);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "01");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "03");
			Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "04");
			Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order3.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order4.WD_WP);

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: data.Whs1.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave", ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertEquals("Should have logged service tasks.",
				@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
Information|Created Pick P00000001 (awaiting replenishment) with Order(s): W00000004, W00000005, W00000006, W00000007.
Information|Created Pick P00000002 by splitting the awaiting replenishment Pick P00000001. This pick contains Order(s): W00000004, W00000006.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave.
", serviceLogger.ToString());

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Nothing scheduled after running service task", 0, newFactory.Load<ProductionRuleScheduleQueue>(new ZQuery()).Length);
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);

			AssertEquals("4 Picks created", 2, newFactory.Load<WhsPick>(new ZQuery()).Length);

			var pick1 = order1InNewFactory.Pick;
			AssertNotNull("Picks should now be attached.", pick1);
			AssertEquals(false, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Pick1 has correct number of picks", 2, pick1.Orders.Count);
			AssertEquals(pick1.PK, order1InNewFactory.WD_WP);
			AssertEquals(pick1.PK, newFactory.Load<WhsOrder>(order3.PK).WD_WP);

			var pick2 = order2InNewFactory.Pick;
			AssertNotNull("Picks should now be attached.", pick2);
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			AssertEquals("Pick2 has correct number of picks", 2, pick2.Orders.Count);
			AssertEquals(pick2.PK, order2InNewFactory.WD_WP);
			AssertEquals(pick2.PK, newFactory.Load<WhsOrder>(order4.PK).WD_WP);
		}

		public void TestWaveCreation_EndToEnd_SplitingAwaitingReplenishmentPicks_Logging()
		{
			var data = new TestDataSimpleEnvironment(Factory, 300, 1);
			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);
			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_SplitOrdersFromPartiallyReplenishedPicks = false;

			var client3 = Helper.CreateClient("CL3");
			Helper.CreateProductClientRelationShip(client3, data.Part1);
			Helper.CreateProductClientRelationShip(client3, data.Part2);
			var pickParams3 = WhsClientPickingParams.GetClientPickingParams(client3).WarehousePickPackParams.AddNew();
			pickParams3.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams3.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var loops = 15;
			var locations = new List<(WhsLocation, WhsLocation, WhsLocation, WhsLocation, WhsLocation, WhsLocation)>(loops);
			for (var i = 0; i < loops; i++)
			{
				var pickFaceLocation1 = data.Whs1.FindLocation($"A-{i * 6 + 1}");
				var pickFaceLocation2 = data.Whs1.FindLocation($"A-{i * 6 + 2}");
				var pickFaceLocation3 = data.Whs1.FindLocation($"A-{i * 6 + 3}");
				var bulkLocation1 = data.Whs1.FindLocation($"A-{i * 6 + 4}");
				var bulkLocation2 = data.Whs1.FindLocation($"A-{i * 6 + 5}");
				var bulkLocation3 = data.Whs1.FindLocation($"A-{i * 6 + 6}");
				locations.Add((pickFaceLocation1, pickFaceLocation2, pickFaceLocation3, bulkLocation1, bulkLocation2, bulkLocation3));

				Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1);
				Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation1);
				Helper.CreateProductPickFace(data.Part1, client2, pickFaceLocation2);
				Helper.CreateProductPickFace(data.Part2, client2, pickFaceLocation2);
				Helper.CreateProductPickFace(data.Part1, client3, pickFaceLocation3);
				Helper.CreateProductPickFace(data.Part2, client3, pickFaceLocation3);

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1{i}", data.Part1, 10m, bulkLocation1, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2{i}", data.Part2, 10m, bulkLocation1, "");
				Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, $"R3{i}", data.Part1, 10m, bulkLocation2, "");
				Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, $"R4{i}", data.Part2, 10m, bulkLocation2, "");
				Helper.CreateWhsReceiveWithInventory(client3, data.Whs1, $"R5{i}", data.Part1, 10m, bulkLocation3, "");
				Helper.CreateWhsReceiveWithInventory(client3, data.Whs1, $"R6{i}", data.Part2, 10m, bulkLocation3, "");
			}
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "T2");
			var transfer3 = Helper.CreateWhsTransfer(client3, data.Whs1, "T3");
			for (var i = 0; i < loops; i++)
			{
				var (pickFaceLocation1, pickFaceLocation2, pickFaceLocation3, bulkLocation1, bulkLocation2, bulkLocation3) = locations[i];
				var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, bulkLocation1, pickFaceLocation1);
				transferLine1.RunPreSaveValidation();
				transferLine1.FinaliseDocketLine();

				var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation1, pickFaceLocation1);
				transferLine2.RunPreSaveValidation();

				var transferLine3 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, bulkLocation2, pickFaceLocation2);
				transferLine3.RunPreSaveValidation();

				var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, bulkLocation2, pickFaceLocation2);
				transferLine4.RunPreSaveValidation();
				transferLine4.FinaliseDocketLine();

				var transferLine5 = Helper.CreateWhsTransferLine(transfer3, data.Part1, 10m, bulkLocation3, pickFaceLocation3);
				transferLine5.RunPreSaveValidation();

				var transferLine6 = Helper.CreateWhsTransferLine(transfer3, data.Part2, 10m, bulkLocation3, pickFaceLocation3);
				transferLine6.RunPreSaveValidation();
				transferLine6.FinaliseDocketLine();
			}
			Factory.Save();
			AssertEquals("Precondition", false, transfer1.IsFinalised);
			AssertEquals("Precondition", false, transfer2.IsFinalised);
			AssertEquals("Precondition", false, transfer3.IsFinalised);

			var ordersForPicks = new List<(WhsOrder O1, WhsOrder O2, WhsOrder O3, WhsOrder O4, WhsOrder O5, WhsOrder O6)>();
			for (var i = 0; i < loops; i++)
			{
				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O1{i}");
				Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O2{i}");
				Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

				var order3 = Helper.CreateWhsOrder(client2, data.Whs1, $"O3{i}");
				Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

				var order4 = Helper.CreateWhsOrder(client2, data.Whs1, $"O4{i}");
				Helper.CreateWhsOrderLine(order4, data.Part2, 10m);

				var order5 = Helper.CreateWhsOrder(client3, data.Whs1, $"O5{i}");
				Helper.CreateWhsOrderLine(order5, data.Part1, 10m);

				var order6 = Helper.CreateWhsOrder(client3, data.Whs1, $"O6{i}");
				Helper.CreateWhsOrderLine(order6, data.Part2, 10m);
				ordersForPicks.Add((order1, order2, order3, order4, order5, order6));
			}
			Factory.Save();

			var ruleSet = ProductionRuleHelper.CreateRuleSet("Wave Creation", "Wave Creation", true, context: "PWW", warehousePK: data.Whs1.PK);
			CreateProductWarehouseWaveCreationRule(ruleSet, "Create Wave 1", ZDateTime.Today.AddDays(-1), maxOrderNumber: 5);
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			RunProductionRuleQueueConsumer(serviceLogger);

			AssertContains("Should have logged service tasks.",
@"Information|Processing RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.
Information|Created Pick P00000001 (awaiting replenishment) with Order(s): W00000094, W00000095, W00000096, W00000097, W00000098.
Information|Created Pick P00000002 by splitting the awaiting replenishment Pick P00000001. This pick contains Order(s): W00000094.
Information|Created Pick P00000003 (awaiting replenishment) with Order(s): W00000099, W00000100, W00000101, W00000102, W00000103.
Information|Created Pick P00000004 by splitting the awaiting replenishment Pick P00000003. This pick contains Order(s): W00000099, W00000100.
Information|Created Pick P00000005 (awaiting replenishment) with Order(s): W00000104, W00000105, W00000106, W00000107, W00000108.
Information|Created Pick P00000006 by splitting the awaiting replenishment Pick P00000005. This pick contains Order(s): W00000105, W00000106.
Information|Created Pick P00000007 (awaiting replenishment) with Order(s): W00000109, W00000110, W00000111, W00000112, W00000113.
Information|Created Pick P00000008 by splitting the awaiting replenishment Pick P00000007. This pick contains Order(s): W00000111, W00000112.
Information|Created Pick P00000009 (awaiting replenishment) with Order(s): W00000114, W00000115, W00000116, W00000117, W00000118.
Information|Created Pick P00000010 by splitting the awaiting replenishment Pick P00000009. This pick contains Order(s): W00000117, W00000118.
Information|Created Pick P00000011 (awaiting replenishment) with Order(s): W00000119, W00000120, W00000121, W00000122, W00000123.
Information|Created Pick P00000012 by splitting the awaiting replenishment Pick P00000011. This pick contains Order(s): W00000123.
Information|Created Pick P00000013 (awaiting replenishment) with Order(s): W00000124, W00000125, W00000126, W00000127, W00000128.
Information|Created Pick P00000014 by splitting the awaiting replenishment Pick P00000013. This pick contains Order(s): W00000124.
Information|Created Pick P00000015 (awaiting replenishment) with Order(s): W00000129, W00000130, W00000131, W00000132, W00000133.
Information|Created Pick P00000016 by splitting the awaiting replenishment Pick P00000015. This pick contains Order(s): W00000129, W00000130.
Information|Created Pick P00000017 (awaiting replenishment) with Order(s): W00000134, W00000135, W00000136, W00000137, W00000138.
Information|Created Pick P00000018 by splitting the awaiting replenishment Pick P00000017. This pick contains Order(s): W00000135, W00000136.
Information|Created Pick P00000019 (awaiting replenishment) with Order(s): W00000139, W00000140, W00000141, W00000142, W00000143.
Information|Created Pick P00000020 by splitting the awaiting replenishment Pick P00000019. This pick contains Order(s): W00000141, W00000142.
Information|Created Pick P00000021 (awaiting replenishment) with Order(s): W00000144, W00000145, W00000146, W00000147, W00000148.
Information|Created Pick P00000022 by splitting the awaiting replenishment Pick P00000021. This pick contains Order(s): W00000147, W00000148.
Information|Created Pick P00000023 (awaiting replenishment) with Order(s): W00000149, W00000150, W00000151, W00000152, W00000153.
Information|Created Pick P00000024 by splitting the awaiting replenishment Pick P00000023. This pick contains Order(s): W00000153.
Information|Created Pick P00000025 (awaiting replenishment) with Order(s): W00000154, W00000155, W00000156, W00000157, W00000158.
Information|Created Pick P00000026 by splitting the awaiting replenishment Pick P00000025. This pick contains Order(s): W00000154.
Information|Created Pick P00000027 (awaiting replenishment) with Order(s): W00000159, W00000160, W00000161, W00000162, W00000163.
Information|Created Pick P00000028 by splitting the awaiting replenishment Pick P00000027. This pick contains Order(s): W00000159, W00000160.
Information|Created Pick P00000029 (awaiting replenishment) with Order(s): W00000164, W00000165, W00000166, W00000167, W00000168.
Information|Created Pick P00000030 by splitting the awaiting replenishment Pick P00000029. This pick contains Order(s): W00000165, W00000166.
Information|Created Pick P00000031 (awaiting replenishment) with Order(s): W00000169, W00000170, W00000171, W00000172, W00000173.
Information|Created Pick P00000032 by splitting the awaiting replenishment Pick P00000031. This pick contains Order(s): W00000171, W00000172.
Information|Created Pick P00000033 (awaiting replenishment) with Order(s): W00000174, W00000175, W00000176, W00000177, W00000178.
Information|Created Pick P00000034 by splitting the awaiting replenishment Pick P00000033. This pick contains Order(s): W00000177, W00000178.
Information|Created Pick P00000035 (awaiting replenishment) with Order(s): W00000179, W00000180, W00000181, W00000182, W00000183.
Information|Created Pick P00000036 by splitting the awaiting replenishment Pick P00000035. This pick contains Order(s): W00000183.
Information|Succesfully processed and saved RuleSet: Wave Creation for Context: ProductWarehouseWaveCreation, Scheduled Rules: Create Wave 1.", serviceLogger.ToString());
			AssertEquals("36 Picks created", 36, new BusinessObjectFactory().Load<WhsPick>(new ZQuery()).Length);
		}

		#region Implementation

		void RunProductionRuleQueueConsumer(TestServiceLogger serviceLogger)
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("SPC", canRunInAnyBranch: true))
			{
				var queueConsumer = ObjectFactory.Get<IScheduledProductionRuleQueueConsumer>();
				queueConsumer.ProcessQueue(serviceLogger.GetTaskNotificationSubscriber(), It.IsAny<CancellationToken>());
			}
		}

		void CreateProductWarehouseWaveCreationRule(
			ProductionRuleSet ruleSet,
			string ruleName,
			ZDateTime queueCreateTime,
			int maxOrderNumber = 0,
			int maxOrderLineNumber = 0,
			int maxPicksPerRun = 0,
			bool scheduleRule = true,
			short priority = 1,
			string groupByProperties = null)
		{
			var rule = ProductionRuleHelper.CreateRule(ruleSet, ruleName, ruleName, priority);
			var groupBy = groupByProperties != null ? $"\"{groupByProperties}\"" : "";
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""CreateWaveActionState"",
		""SortByCriteria"": [
			{{""propertyPath"":""DocketID"",""direction"":""ascending""}}
		],
		""GroupByCriteria"": [
			{groupBy}
		],
		""MaximumPicksPerRun"": {maxPicksPerRun},
		""MaximumOrderNumber"": {maxOrderNumber},
		""MaximumOrderLineNumber"": {maxOrderLineNumber},
		""MaximumOrderLineUnits"": 0,
		""MaximumWeight"": 0,
		""MaximumWeightUQ"": ""KG"",
		""MaximumVolume"": 0,
		""MaximumVolumeUQ"": ""CC"",
		""MaximumOrderValue"": 0,
		""MaximumOrderValueCurrency"": ""USD"",
		""PickPalletsByLabel"": false,
		""PickCasesByLabel"": false,
		""CartonizeSplitCases"": false,
		""ForcePickByCaseUOMTypeAllocation"": false,
		""ForceSplitCaseUOMTypeAllocation"": false
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
