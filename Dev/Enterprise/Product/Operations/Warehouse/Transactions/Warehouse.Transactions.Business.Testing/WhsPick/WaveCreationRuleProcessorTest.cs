using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseWaveCreation;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Core.Facts;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WaveCreationRuleProcessorTest : ScheduledRuleProcessorTest
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<WaveCreationRuleProcessor>(ObjectFactory.Get<IScheduledRuleProcessor>(nameof(WaveCreationRuleProcessor)));
		}

		#region TestLoadInputFacts

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2022, 11, 10, 1, 00, 00)]
		public void TestLoadInputFacts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.UNDGs.AddNew();
			data.Part1.OP_Cubic = 0.1m;
			data.Part1.OP_CubicUQ = "M3";
			data.Part1.OP_Weight = 1m;
			data.Part1.OP_WeightUQ = "KG";
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var dateTimeNow = DateTime.Now;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickPriority = 1;
			order.WD_CustomerReference = "ABCDEF";
			order.WD_RequiredDate = dateTimeNow;
			order.WD_PackingAfterPickingRequired = true;
			order.WD_IsAuthorisedToLeave = true;
			order.WD_QualityAuditRequired = true;
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			var serviceLevel = order.Lookups.ServiceLevels.AddNew();
			serviceLevel.RS_Code = "XXX";
			order.WD_RS_NKServiceLevel = "XXX";

			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "SCH";
			salesChannel.WSH_Description = "SCH";
			order.WD_WSH_SalesChannel = salesChannel.PK;
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().Single();
			CombineAssertions(() =>
			{
				AssertEquals("PK", order.PK, orderFact.PK);
				AssertEquals("DocketID", order.WD_DocketID, orderFact.DocketID);
				AssertEquals("OrderNumber", "O1", orderFact.OrderNumber);
				AssertEquals("OrderType", "ORD", orderFact.OrderType);
				AssertEquals("PickPriority", 1, orderFact.PickPriority);
				AssertEquals("SalesChannelCode", "SCH", orderFact.SalesChannelCode);
				AssertEquals("CustomerReference", "ABCDEF", orderFact.CustomerReference);
				AssertEquals("ServiceLevel", "XXX", orderFact.ServiceLevel);
				AssertEquals("PackingRequired", true, orderFact.PackingRequired);
				AssertEquals("AuthorisedToLeave", true, orderFact.AuthorisedToLeave);
				AssertEquals("QualityAuditRequired", true, orderFact.QualityAuditRequired);
				AssertEquals("HasDangerousGoods", true, orderFact.HasDangerousGoods);
				AssertEquals("RequiredDate", dateTimeNow.Date, orderFact.RequiredDate);
				AssertEquals("CreatedDate", new ZDate(2022, 11, 10).ToDateTime(), orderFact.CreatedDate);
				AssertEquals("TotalLineUnits", 10m, orderFact.TotalLineUnits);
				AssertEquals("TotalOrderLines", 1, orderFact.TotalOrderLines);
				AssertEquals("TotalWeight", new MeasureFact(10m, "KG"), orderFact.TotalWeight);
				AssertEquals("TotalVolume", new MeasureFact(1m, "M3"), orderFact.TotalVolume);
			});
		}

		public void TestLoadInputFacts_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", order1.WD_OH_Client, order2.WD_OH_Client);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());
			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			AssertEquals(order1Fact.Client.Fact.PK, order2Fact.Client.Fact.PK);
		}

		public void TestLoadInputFacts_NoOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Factory.Save();

			AssertEquals("Precondition", false, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_NoOrderLineUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 0m);
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_PickedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", true, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_NoRequiredDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals("Precondition", true, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_HeldOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.IsOrderHeld = true;
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertEquals("Precondition", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_CancelledOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.CancelReactivateDocket();
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertEquals("Precondition", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_PickOptionManual()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Manual, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_OrderOnDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			var warehouse = Helper.CreateWarehouse("ZZZ");
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, warehouse, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals("Precondition", false, order.WD_RequiredDate.IsEmpty);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", warehouse.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_MultipleOrders_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			var org2 = Helper.CreateClient("ZZZ");
			var product2 = Helper.CreateProduct("ZPROD2", org2);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", product2, 10m);
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, product2, 10m);
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());
			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.DocketID == order1.WD_DocketID);
			var client1Fact = order1Fact.Client.Fact;
			AssertNotNull(client1Fact);
			AssertEquals(data.Org1.PK, client1Fact.PK);
			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.DocketID == order2.WD_DocketID);
			var client2Fact = order2Fact.Client.Fact;
			AssertNotNull(client2Fact);
			AssertEquals(org2.PK, client2Fact.PK);
		}

		public void TestLoadInputFacts_NoOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";

			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			AssertEquals("Precondition", false, Factory.Load<WhsOrder>(new ZQuery()).Length > 0);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(false, inputFacts.Any());
		}

		public void TestLoadInputFacts_WithCarrierBookingAgent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var carrierBookingAgent = Helper.CreateClient("ZZZ");
			var address = carrierBookingAgent.MainAddress;
			order1.CarrierBookingAgentDocAddress.E2_OA_Address = address.PK;
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = address.PK;
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", carrierBookingAgent.PK, order1.CarrierBookingAgentDocAddress.OrganisationPK);
			AssertEquals("Precondition", carrierBookingAgent.PK, order1.CarrierBookingAgent.PK);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", carrierBookingAgent.PK, order2.CarrierBookingAgentDocAddress.OrganisationPK);
			AssertEquals("Precondition", carrierBookingAgent.PK, order2.CarrierBookingAgent.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var carrierBookingAgent1Fact = order1Fact.CarrierBookingAgent.Fact;
			AssertNotNull(carrierBookingAgent1Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var carrierBookingAgent2Fact = order1Fact.CarrierBookingAgent.Fact;
			AssertNotNull(carrierBookingAgent2Fact);

			AssertEquals(carrierBookingAgent.PK, carrierBookingAgent1Fact.PK);
			AssertEquals(carrierBookingAgent1Fact.PK, carrierBookingAgent2Fact.PK);
		}

		public void TestLoadInputFacts_WithTransportCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var transportCompany = Helper.CreateClient("ZZZ");
			order1.TransportCoPK = transportCompany.PK;
			order1.WD_PL_NKCarrierServiceLevel = "XXX";
			order2.TransportCoPK = transportCompany.PK;
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", transportCompany.PK, order1.TransportCoDocAddress.OrganisationPK);
			AssertEquals("Precondition", transportCompany.PK, order1.TransportCo.PK);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", transportCompany.PK, order2.TransportCoDocAddress.OrganisationPK);
			AssertEquals("Precondition", transportCompany.PK, order2.TransportCo.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var transportCompany1Fact = order1Fact.TransportCompany.Fact;
			AssertNotNull(transportCompany1Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var transportCompany2Fact = order2Fact.TransportCompany.Fact;
			AssertNotNull(transportCompany2Fact);

			AssertEquals(transportCompany.PK, transportCompany1Fact.PK);
			AssertEquals(transportCompany1Fact.PK, transportCompany2Fact.PK);
		}

		public void TestLoadInputFacts_WithTransportCompany_TransportRelatedInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			var transportCompany = Helper.CreateClient("ZZZ");
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", transportCompany);
			var address = transportCompany.MainAddress;
			address.OA_Address1 = "20 Maxwell Street";
			address.OA_Address2 = "";
			address.OA_City = "PERTH";
			address.OA_PostCode = "6162";
			address.OA_State = "WA";
			address.OA_RL_NKRelatedPortCode = "AUBYW";

			var rateTransportZone = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZone, "AU", "6162");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, transportCompany.PK, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var service = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			service.PL_Code = "XXX";

			order.TransportCoPK = transportCompany.PK;
			order.WD_PL_NKCarrierServiceLevel = "XXX";
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);
			AssertEquals("Precondition", transportCompany.PK, order.TransportCoDocAddress.OrganisationPK);
			AssertEquals("Precondition", transportCompany.PK, order.TransportCo.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().Single();
			var transportCompanyFact = orderFact.TransportCompany.Fact;
			AssertNotNull(transportCompanyFact);
			AssertEquals("Perth", orderFact.TransportZone);
			AssertEquals("XXX", orderFact.CarrierServiceLevel);
			AssertEquals(transportCompany.PK, transportCompanyFact.PK);
		}

		public void TestLoadInputFacts_WithConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var consignee = Helper.CreateClient("ZZZ");
			order1.ConsigneePK = consignee.PK;
			order2.ConsigneePK = consignee.PK;
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", consignee.PK, order1.ConsigneeDocAddress.OrganisationPK);
			AssertEquals("Precondition", consignee.PK, order1.Consignee.PK);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", consignee.PK, order2.ConsigneeDocAddress.OrganisationPK);
			AssertEquals("Precondition", consignee.PK, order2.Consignee.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var consignee1Fact = order1Fact.Consignee.Fact;
			AssertNotNull(consignee1Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var consignee2Fact = order2Fact.Consignee.Fact;
			AssertNotNull(consignee2Fact);

			AssertEquals(consignee.PK, consignee1Fact.PK);
			AssertEquals(consignee1Fact.PK, consignee2Fact.PK);
		}

		public void TestLoadInputFacts_WithConsigneeAndDeliveryRoute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var consignee = Helper.CreateClient("ZZZ");
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddress.OA_DeliveryRoute = "ABC";
			order.ConsigneeAddress.OA_DeliveryRouteSequence = 1;
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);
			AssertEquals("Precondition", consignee.PK, order.ConsigneeDocAddress.OrganisationPK);
			AssertEquals("Precondition", consignee.PK, order.Consignee.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().Single();
			AssertEquals(order.WD_DocketID, orderFact.DocketID);
			var consigneeFact = orderFact.Consignee.Fact;
			AssertNotNull(consigneeFact);
			AssertEquals(consignee.PK, consigneeFact.PK);
			AssertEquals("ABC", orderFact.ConsigneeDeliveryRoute);
		}

		public void TestLoadInputFacts_InvalidConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var consignee = Helper.CreateClient("ZZZ");
			order.ConsigneePK = consignee.PK;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "";
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);
			AssertEquals("Precondition", "", order.ConsigneeDocAddress.E2_City);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().SingleOrDefault();
			AssertNull(orderFact);

			order = Factory.Load<WhsOrder>(order.PK);
			AssertOrderHasErrorEvent(order, Constants.EventReferenceParameterReasons.NotPickable);
		}

		public void TestLoadInputFacts_OverriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var orgHeader = Helper.CreateClient("ZZZ");
			var address = orgHeader.MainAddress;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "MELBOURNE";
			order.ConsigneeDocAddress.E2_State = "VIC";
			order.ConsigneeDocAddress.E2_Postcode = "3000";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);
			AssertEquals("Precondition", "MELBOURNE", order.ConsigneeDocAddress.E2_City);
			AssertEquals("Precondition", "ABCD", order.ConsigneeDocAddress.E2_CompanyName);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().SingleOrDefault();
			AssertNotNull(orderFact);

			var consigneeAddressFact = orderFact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressFact);
			AssertEquals(consigneeAddressFact.PK, order.ConsigneeDocAddress.PK);
			AssertEquals("ABCD", consigneeAddressFact.CompanyName);
			AssertEquals("MELBOURNE", consigneeAddressFact.City);
			AssertEquals("VIC", consigneeAddressFact.State);
			AssertEquals("AU", consigneeAddressFact.CountryCode);
			AssertEquals("3000", consigneeAddressFact.Postcode);
		}

		public void TestLoadInputFacts_NonOverriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var orgHeader = Helper.CreateClient("ZZZ");
			var address = orgHeader.MainAddress;
			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeDocAddress.E2_OA_Address = address.PK;
			address.OA_Address1 = "20 Maxwell Street";
			address.OA_Address2 = "";
			address.OA_City = "PERTH";
			address.OA_PostCode = "6162";
			address.OA_State = "WA";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_RL_NKRelatedPortCode = "AUBYW";

			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);
			AssertEquals("Precondition", "PERTH", order.ConsigneeDocAddress.Address.OA_City);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().Single();
			AssertNotNull(orderFact);
			var consigneeAddressFact = orderFact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressFact);
			AssertEquals(consigneeAddressFact.PK, order.ConsigneeAddressPK);
			AssertEquals("PERTH", consigneeAddressFact.City);
			AssertEquals("6162", consigneeAddressFact.Postcode);
			AssertEquals("WA", consigneeAddressFact.State);
			AssertEquals("AU", consigneeAddressFact.CountryCode);
		}

		public void TestLoadInputFacts_MultipleOrders_DifferentOverriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			var org2 = Helper.CreateClient("ZZZ");

			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			order1.ConsigneeDocAddress.E2_AddressOverride = true;
			order1.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order1.ConsigneeDocAddress.E2_City = "MELBOURNE";
			order1.ConsigneeDocAddress.E2_State = "VIC";
			order1.ConsigneeDocAddress.E2_Postcode = "3000";
			order1.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order2.ConsigneeDocAddress.E2_City = "MELBOURNE";
			order2.ConsigneeDocAddress.E2_State = "VIC";
			order2.ConsigneeDocAddress.E2_Postcode = "3000";
			order2.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", "MELBOURNE", order1.ConsigneeDocAddress.E2_City);
			AssertEquals("Precondition", "ABCD", order1.ConsigneeDocAddress.E2_CompanyName);

			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", "MELBOURNE", order2.ConsigneeDocAddress.E2_City);
			AssertEquals("Precondition", "ABCD", order2.ConsigneeDocAddress.E2_CompanyName);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());
			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);    //order => order.DocketID == order1.WD_DocketID
			AssertNotNull(order1Fact);
			var consigneeAddressOrder1Fact = order1Fact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressOrder1Fact);
			AssertEquals(consigneeAddressOrder1Fact.PK, order1.ConsigneeDocAddress.PK);
			AssertEquals("ABCD", consigneeAddressOrder1Fact.CompanyName);
			AssertEquals("MELBOURNE", consigneeAddressOrder1Fact.City);
			AssertEquals("VIC", consigneeAddressOrder1Fact.State);
			AssertEquals("AU", consigneeAddressOrder1Fact.CountryCode);
			AssertEquals("3000", consigneeAddressOrder1Fact.Postcode);
			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			AssertNotNull(order2Fact);
			var consigneeAddressOrder2Fact = order2Fact.ConsigneeAddress.Fact;
			AssertEquals(consigneeAddressOrder2Fact.PK, order2.ConsigneeDocAddress.PK);
			AssertNotNull(consigneeAddressOrder2Fact);
			AssertEquals("ABCD", consigneeAddressOrder2Fact.CompanyName);
			AssertEquals("MELBOURNE", consigneeAddressOrder2Fact.City);
			AssertEquals("VIC", consigneeAddressOrder2Fact.State);
			AssertEquals("AU", consigneeAddressOrder2Fact.CountryCode);
			AssertEquals("3000", consigneeAddressOrder2Fact.Postcode);
		}

		public void TestLoadInputFacts_MultipleOrders_DifferentNonOverriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			var org2 = Helper.CreateClient("ZZZ");
			var product2 = Helper.CreateProduct("ZPROD2", org2);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", product2, 10m);
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, product2, 10m);

			var orgHeader1 = Helper.CreateClient("ZZZ");
			var address1 = orgHeader1.MainAddress;
			order1.ConsigneeDocAddress.E2_AddressOverride = false;
			order1.ConsigneeDocAddress.E2_OA_Address = address1.PK;
			address1.OA_Address1 = "580 ST KILDA ROAD";
			address1.OA_Address2 = "";
			address1.OA_City = "MELBOURNE";
			address1.OA_PostCode = "3004";
			address1.OA_State = "VIC";
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_RL_NKRelatedPortCode = "AUMEL";

			var orgHeader2 = Helper.CreateClient("ZZZ");
			var address2 = orgHeader2.MainAddress;
			order2.ConsigneeDocAddress.E2_AddressOverride = false;
			order2.ConsigneeDocAddress.E2_OA_Address = address2.PK;
			address2.OA_Address1 = "3 PARRA HILLS";
			address2.OA_Address2 = "";
			address2.OA_City = "ADELAIDE";
			address2.OA_PostCode = "5096";
			address2.OA_State = "SA";
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_RL_NKRelatedPortCode = "AUADL";

			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", "MELBOURNE", order1.ConsigneeDocAddress.Address.OA_City);

			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", "ADELAIDE", order2.ConsigneeDocAddress.Address.OA_City);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());
			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			AssertNotNull(order1Fact);
			var consigneeAddressOrder1Fact = order1Fact.ConsigneeAddress.Fact;

			AssertNotNull(consigneeAddressOrder1Fact);
			AssertEquals(consigneeAddressOrder1Fact.PK, order1.ConsigneeAddressPK);
			AssertEquals("MELBOURNE", consigneeAddressOrder1Fact.City);
			AssertEquals("VIC", consigneeAddressOrder1Fact.State);
			AssertEquals("AU", consigneeAddressOrder1Fact.CountryCode);
			AssertEquals("3004", consigneeAddressOrder1Fact.Postcode);
			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			AssertNotNull(order2Fact);
			var consigneeAddressOrder2Fact = order2Fact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressOrder2Fact);
			AssertEquals(consigneeAddressOrder2Fact.PK, order2.ConsigneeAddressPK);
			AssertEquals("ADELAIDE", consigneeAddressOrder2Fact.City);
			AssertEquals("SA", consigneeAddressOrder2Fact.State);
			AssertEquals("AU", consigneeAddressOrder2Fact.CountryCode);
			AssertEquals("5096", consigneeAddressOrder2Fact.Postcode);
		}

		public void TestLoadInputFacts_MultipleOrders_SameNonOverriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			var org2 = Helper.CreateClient("ZZZ");
			var product2 = Helper.CreateProduct("ZPROD2", org2);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", product2, 10m);
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, product2, 10m);

			var orgHeader1 = Helper.CreateClient("ZZZ");
			var address1 = orgHeader1.MainAddress;
			order1.ConsigneeDocAddress.E2_AddressOverride = false;
			order1.ConsigneeDocAddress.E2_OA_Address = address1.PK;
			address1.OA_Address1 = "20 Maxwell Street";
			address1.OA_Address2 = "";
			address1.OA_City = "PERTH";
			address1.OA_PostCode = "6162";
			address1.OA_State = "WA";
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_RL_NKRelatedPortCode = "AUBYW";

			order2.ConsigneeDocAddress.E2_AddressOverride = false;
			order2.ConsigneeDocAddress.E2_OA_Address = address1.PK;

			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", "PERTH", order1.ConsigneeDocAddress.Address.OA_City);

			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", "PERTH", order2.ConsigneeDocAddress.Address.OA_City);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());
			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.DocketID == order1.WD_DocketID);
			AssertNotNull(order1Fact);
			var consigneeAddressOrder1Fact = order1Fact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressOrder1Fact);
			AssertEquals(consigneeAddressOrder1Fact.PK, order1.ConsigneeAddressPK);
			AssertEquals("PERTH", consigneeAddressOrder1Fact.City);
			AssertEquals("WA", consigneeAddressOrder1Fact.State);
			AssertEquals("AU", consigneeAddressOrder1Fact.CountryCode);
			AssertEquals("6162", consigneeAddressOrder1Fact.Postcode);
			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.DocketID == order2.WD_DocketID);
			AssertNotNull(order2Fact);
			var consigneeAddressOrder2Fact = order2Fact.ConsigneeAddress.Fact;
			AssertNotNull(consigneeAddressOrder2Fact);
			AssertEquals(consigneeAddressOrder2Fact.PK, order2.ConsigneeAddressPK);
			AssertEquals("PERTH", consigneeAddressOrder2Fact.City);
			AssertEquals("WA", consigneeAddressOrder2Fact.State);
			AssertEquals("AU", consigneeAddressOrder2Fact.CountryCode);
			AssertEquals("6162", consigneeAddressOrder2Fact.Postcode);
			AssertEquals(consigneeAddressOrder1Fact, consigneeAddressOrder2Fact);
		}

		public void TestLoadInputFacts_WithDistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var distributionCentre = Helper.CreateClient("DC1");
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			order1.WD_PL_NKCarrierServiceLevel = "XXX";
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", distributionCentre.PK, order1.DistributionCentreDocAddress.OrganisationPK);
			AssertEquals("Precondition", distributionCentre.PK, order1.DistributionCentreDocAddress.Organisation.PK);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", distributionCentre.PK, order2.DistributionCentreDocAddress.OrganisationPK);
			AssertEquals("Precondition", distributionCentre.PK, order2.DistributionCentreDocAddress.Organisation.PK);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var distributionCentre1Fact = order1Fact.DistributionCentreAddress.Fact;
			AssertNotNull(distributionCentre1Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var distributionCentre2Fact = order2Fact.DistributionCentreAddress.Fact;
			AssertNotNull(distributionCentre2Fact);

			AssertEquals(order1.DistributionCentreDocAddress.E2_OA_Address, distributionCentre1Fact.PK);
			AssertEquals(distributionCentre1Fact.PK, distributionCentre2Fact.PK);
		}

		public void TestLoadInputFacts_WithOverrideDistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var distributionCentre = Helper.CreateClient("DC1");
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			order1.DistributionCentreDocAddress.E2_AddressOverride = true;
			order1.WD_PL_NKCarrierServiceLevel = "XXX";
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			order2.DistributionCentreDocAddress.E2_AddressOverride = true;
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", false, order1.DistributionCentreDocAddress.OrganisationPK.IsValid);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", false, order2.DistributionCentreDocAddress.OrganisationPK.IsValid);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var distributionCentre1Fact = order1Fact.DistributionCentreAddress.Fact;
			AssertNotNull(distributionCentre1Fact);
			AssertNull(distributionCentre1Fact.Organisation.Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var distributionCentre2Fact = order2Fact.DistributionCentreAddress.Fact;
			AssertNotNull(distributionCentre2Fact);
			AssertNull(distributionCentre2Fact.Organisation.Fact);

			AssertEquals(order1.DistributionCentreDocAddress.PK, distributionCentre1Fact.PK);
			AssertEquals(order2.DistributionCentreDocAddress.PK, distributionCentre2Fact.PK);
		}

		public void TestLoadInputFacts_WithEmptyDistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var distributionCentre = Helper.CreateClient("DC1");
			order1.WD_PL_NKCarrierServiceLevel = "XXX";
			Factory.Save();

			AssertEquals("Precondition", true, order1.Lines.Count > 0);
			AssertEquals("Precondition", false, order1.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order1.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order1.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order1.WD_WW_Whs);
			AssertEquals("Precondition", false, order1.DistributionCentreDocAddress.OrganisationPK.IsValid);
			AssertEquals("Precondition", true, order2.Lines.Count > 0);
			AssertEquals("Precondition", false, order2.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order2.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order2.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order2.WD_WW_Whs);
			AssertEquals("Precondition", false, order2.DistributionCentreDocAddress.OrganisationPK.IsValid);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			AssertEquals(2, inputFacts.Count());

			var order1Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order1.PK);
			var distributionCentre1Fact = order1Fact.DistributionCentreAddress.Fact;
			AssertNull(distributionCentre1Fact);

			var order2Fact = inputFacts.OfType<IOrderFact>().Single(order => order.PK == order2.PK);
			var distributionCentre2Fact = order2Fact.DistributionCentreAddress.Fact;
			AssertNull(distributionCentre2Fact);
		}

		public void TestLoadInputFacts_CurrencyConverterTest()
		{
			GlbCompany.CurrentCompany.SetCurrency("AUD");
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			line.WE_ExtendedLinePrice = 10m;

			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 0.5m, usCurrency);
			var nzCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, nzCurrency);
			Factory.Save();

			AssertEquals("Precondition", true, order.Lines.Count > 0);
			AssertEquals("Precondition", false, order.IsAttachedToPick);
			AssertNotEquals("Precondition", "HEL", order.WD_DocketStatus);
			AssertEquals("Precondition", WhsPickOption.Codes.Auto, order.WD_PickOption);
			AssertEquals("Precondition", data.Whs1.PK, order.WD_WW_Whs);

			var processor = GetProcessor();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), ruleSet, new CancellationToken());
			var orderFact = inputFacts.OfType<IOrderFact>().Single();

			AssertEquals(5m, orderFact.GetTotalOrderValue("USD"));
			AssertEquals(20m, orderFact.GetTotalOrderValue("NZD"));

			void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
			{
				var exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_GC = company.PK;
				exchangeRate.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRate.RE_StartDate = startDate;
				exchangeRate.RE_ExpiryDate = endDate;
				exchangeRate.RE_SellRate = rate;
				exchangeRate.RE_ExRateType = rateType;
			}
		}

		public void TestLoadInputFacts_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			var orders = new List<WhsOrder>();
			for (int orderCount = 0; orderCount < 30; orderCount++)
			{
				var postfix = orderCount.ToString();
				var client = Helper.CreateClient("C" + postfix);
				var consignee = Helper.CreateClient("D" + postfix);

				var transportCo = Helper.CreateClient("T" + postfix);
				transportCo.OH_FullName = "TransportCo" + postfix;
				transportCo.Addresses.AddNewMainAddress();
				transportCo.OH_IsShippingProvider = true;

				var carrierBookingAgent = Helper.CreateClient("Z" + postfix);
				var carrierBookingAgentAddress = carrierBookingAgent.MainAddress;

				var order = Helper.CreateWhsOrder(client.PK, data.Whs1.PK, consignee.PK, "O" + postfix, ZDateTimeOffset.Now, null);
				order.TransportCoPK = transportCo.PK;
				order.CarrierBookingAgentDocAddress.E2_OA_Address = carrierBookingAgentAddress.PK;
				order.ConsigneeAddress.OA_DeliveryRoute = "A" + postfix;
				order.ConsigneeAddress.OA_DeliveryRouteSequence = 1;

				var product1 = Helper.CreateProduct(client, "P" + postfix + orderCount.ToString());
				Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R" + postfix + orderCount.ToString(), ZDateTimeOffset.Today, product1, 10m);
				Helper.CreateWhsOrderLine(order, product1, 5m);

				var product2 = Helper.CreateProduct(client, "Q" + postfix + orderCount.ToString());
				Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "S" + postfix + orderCount.ToString(), ZDateTimeOffset.Today, product2, 10m);
				Helper.CreateWhsOrderLine(order, product2, 5m);
				orders.Add(order);
			}

			Factory.Save();

			Assert("Precondition", orders.All(order => order.Lines.Count > 0));
			Assert("Precondition", orders.All(order => !order.IsAttachedToPick));
			Assert("Precondition", orders.All(order => order.WD_DocketStatus != DocketStatus.Codes.Held));
			Assert("Precondition", orders.All(order => order.WD_PickOption == WhsPickOption.Codes.Auto));
			Assert("Precondition", orders.All(order => order.WD_WW_Whs == data.Whs1.PK));

			var newFactory = new ReadOnlyBusinessObjectFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 4 },
				{ GlbCompanySchema.Constants.TableName, 4 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 5 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 2 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var processor = GetProcessor();
				var inputFacts = processor.LoadInputFacts(newFactory, ruleSet, new CancellationToken());
				var orderFacts = inputFacts.OfType<IOrderFact>();
				AssertEquals(30, orderFacts.Count());
				Assert(orderFacts.All(orderFact => orderFact.Client.Fact != null));
				Assert(orderFacts.All(orderFact => orderFact.Consignee.Fact != null));
				Assert(orderFacts.All(orderFact => orderFact.TransportCompany.Fact != null));
				Assert(orderFacts.All(orderFact => orderFact.CarrierBookingAgent.Fact != null));
				Assert(orderFacts.All(orderFact => !string.IsNullOrEmpty(orderFact.ConsigneeDeliveryRoute)));
			}
		}

		public void TestLoadInputFacts_CancellationToken()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWW";
			ruleSet.PRS_Name = "TEST";
			ruleSet.PRS_Description = "TEST";
			ruleSet.PRS_WW_Warehouse = data.Whs1.PK;

			var consignee = Helper.CreateClient("CON");
			var transportCo = Helper.CreateClient("TRN");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			AssertIsFinalisedPrecondition(receive1);

			var orders = new List<WhsOrder>();
			for (int orderCount = 0; orderCount < 10; orderCount++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + orderCount);
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);

				order.ConsigneePK = consignee.PK;
				order.TransportCoPK = transportCo.PK;
				orders.Add(order);
			}

			Factory.Save();

			Assert("Precondition", orders.All(order => order.Lines.Count > 0));
			Assert("Precondition", orders.All(order => !order.IsAttachedToPick));
			Assert("Precondition", orders.All(order => order.WD_DocketStatus != DocketStatus.Codes.Held));
			Assert("Precondition", orders.All(order => order.WD_PickOption == WhsPickOption.Codes.Auto));
			Assert("Precondition", orders.All(order => order.WD_WW_Whs == data.Whs1.PK));

			var factory = new ReadOnlyBusinessObjectFactory();
			var cancellationTokenSource = new CancellationTokenSource();
			var processor = GetProcessor();

			factory.RowsLoaded += (s, e) =>
			{
				cancellationTokenSource.Cancel();
			};

			AssertExceptionThrown<OperationCanceledException>("Operation is cancelled.",
				() => processor.LoadInputFacts(factory, ruleSet, cancellationTokenSource.Token));
		}

		#endregion

		#region TestProcessResults

		public void TestProcessResults_UnWavedOrderFacts()
		{
			var whs = Helper.CreateWarehouse("Wh1");
			var client = Helper.CreateClient("C1");
			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");

			Factory.Save();

			var processor = GetProcessor();
			var orderFacts = new[]
			{
				MockOrderFact(ord1, Guid.Empty, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, Guid.Empty, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);

			AssertEquals("Pick should not be created for unwaved facts", Guid.Empty, ord1.WD_WP);
			AssertEquals("Pick should not be created for unwaved facts", Guid.Empty, ord2.WD_WP);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message.StartsWith("Created"))), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_AllocationError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product3, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 10m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var waveFact3 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				waveFact3,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact3.PK, client.PK.ToGuid()).Object,
			};

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddError("Allocation Error Occurred.");
				});

			var notificationsMock = new Mock<INotifications>();

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyError("Allocation Error Occurred.", notificationsMock);
			AssertUnsavedPickCreated(ord1);

			AssertNull("Wave Processing short circuited.", ord2.Pick);
			AssertNull("Wave Processing short circuited.", ord3.Pick);

			Assert("Order has no changes", !ord2.HasChanges);
			Assert("Order has no changes", !ord3.HasChanges);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message.StartsWith("Created"))), Times.Never);
		}

		public void TestProcessResults_WavedOrderFacts_AllocationWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product3, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 10m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var waveFact3 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				waveFact3,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact3.PK, client.PK.ToGuid()).Object,
			};

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddWarning("Allocation Warning Occurred.");
				});

			var notificationsMock = new Mock<INotifications>();

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyWarning("Allocation Warning Occurred.", notificationsMock);

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_NoStockAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 10m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var waveFact3 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				waveFact3,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact3.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No Stock was allocated to pick.")), Times.Exactly(2));

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);

			AssertEquals(0m, ord1.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals(10m, ord2.Pick.OrderedInventories[0].PickLineQuantity);
			AssertEquals(0m, ord3.Pick.OrderedInventories[0].PickLineQuantity);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message.StartsWith("Created"))), Times.Exactly(3));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_PickPickabilityFails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product3, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 10m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var waveFact3 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				waveFact3,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact3.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					var otherPick = Factory.New<WhsPick>();
					otherPick.WP_PickType = PickType.Codes.Order;
					ord2.WD_WP = otherPick.PK;

					var allocation = new AllocateFIFOLegacyMock();
					allocation.Allocate(p, n, ps, poi);
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyError("This Order is attached to another Pick.", notificationsMock);

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);

			Assert("Order has no changes", !ord3.HasChanges);
			AssertNull("Pick was never created due to cancelled operation.", ord3.Pick);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Once);
		}

		public void TestProcessResults_WavedOrderFacts_ProcessCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product3, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 10m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var waveFact3 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				waveFact3,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact3.PK, client.PK.ToGuid()).Object,
			};

			var cancellationTokenSource = new CancellationTokenSource();
			var allocateInvocation = 0;

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					allocateInvocation++;
					if (allocateInvocation == 2)
					{
						cancellationTokenSource.Cancel();
					}
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				AssertExceptionThrown<OperationCanceledException>("Operation is cancelled.", () => processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), Helper.Notify, cancellationTokenSource.Token));
			}

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);

			Assert("Order has no changes", !ord3.HasChanges);
			AssertNull("Pick was never created due to cancelled operation.", ord3.Pick);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking() => TestProcessResults_WavedOrderFacts_AutoPicking(isShort: false);

		public void TestProcessResults_WavedOrderFacts_AutoPicking_Short() => TestProcessResults_WavedOrderFacts_AutoPicking(isShort: true);

		public void TestProcessResults_WavedOrderFacts_AutoPicking(bool isShort)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, isShort ? product1 : product2, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord1, waveFact2.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			var pick1 = ord1.Pick;
			var pick2 = ord2.Pick;

			AssertNotEquals(pick1, pick2);

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);

			AssertNotNull("Picks should now be attached.", pick1);
			AssertPickIsConfiguredCorrectly(pick1);

			if (isShort)
			{
				AssertEquals(0, orderLine1.PickLines.Count);
			}
			else
			{
				AssertEquals(1, orderLine1.PickLines.Count);
				AssertEquals(false, orderLine1.PickLines[0].IsPicked);
				AssertEquals(10m, pick1.OrderedInventories[0].PickLineQuantity);
			}

			AssertNotNull("Picks should now be attached.", pick2);
			AssertPickIsConfiguredCorrectly(pick2);
			AssertEquals(1, orderLine2.PickLines.Count);
			AssertEquals(false, orderLine2.PickLines[0].IsPicked);
			AssertEquals(10m, pick2.OrderedInventories[0].PickLineQuantity);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);

			Factory.Save(); // Create pick ID
			Factory.Save(); // Ensure we unhook logging
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick1.WP_PickNo} with Order(s): {ord1.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick2.WP_PickNo} with Order(s): {ord2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_SaveFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 20m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 20m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			var pick1 = ord1.Pick;
			var pick2 = ord2.Pick;

			AssertNotEquals(pick1, pick2);

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);

			AssertNotNull("Picks should now be attached.", pick1);
			AssertPickIsConfiguredCorrectly(pick1);
			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(false, orderLine1.PickLines[0].IsPicked);
			AssertEquals(10m, pick1.OrderedInventories[0].PickLineQuantity);

			AssertNotNull("Picks should now be attached.", pick2);
			AssertPickIsConfiguredCorrectly(pick2);
			AssertEquals(1, orderLine2.PickLines.Count);
			AssertEquals(false, orderLine2.PickLines[0].IsPicked);
			AssertEquals(20m, pick2.OrderedInventories[0].PickLineQuantity);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);

			ord1.WD_DocketStatus = "LOL";

			AssertExceptionThrown<ZSaveException>(() => Factory.Save()); // Create pick ID
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick1.HumanReadableName} with Order(s): {ord1.WD_DocketID}.")), Times.Never); // Should not log if the save failed
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_PickPalletsByLabel()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(pickPalletsByLabel: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_PickCasesByLabel()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(pickCasesByLabel: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_CartonizeSplitCases()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(cartonizeSplitCases: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_CartonizeSplitCases_PickPalletsByLabel()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(cartonizeSplitCases: true, pickPalletsByLabel: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_CartonizeSplitCases_PickCasesByLabel()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(cartonizeSplitCases: true, pickCasesByLabel: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_CartonizeSplitCases_PickCasesAndPalletsByLabel()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(cartonizeSplitCases: true, pickCasesByLabel: true, pickPalletsByLabel: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_ForcePickByCaseUOMTypeAllocation()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(forcePickByCaseUOMTypeAllocation: true);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_ForceSplitCaseUOMTypeAllocation()
		{
			TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(forceSplitCaseUOMTypeAllocation: true);
		}

		void TestProcessResults_WavedOrderFacts_AllocatePackageLabelsCore(bool cartonizeSplitCases = false, bool pickCasesByLabel = false, bool pickPalletsByLabel = false, bool forcePickByCaseUOMTypeAllocation = false, bool forceSplitCaseUOMTypeAllocation = false)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);

			Factory.Save();

			var ord = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine = Helper.CreateWhsOrderLine(ord, product1, 10m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord.WD_WP);

			var processor = GetProcessor();
			var waveFact = PrepareWaveFact(cartonizeSplitCases: cartonizeSplitCases, pickCasesByLabel: pickCasesByLabel, pickPalletsByLabel: pickPalletsByLabel, forcePickByCaseUOMTypeAllocation: forcePickByCaseUOMTypeAllocation, forceSplitCaseUOMTypeAllocation: forceSplitCaseUOMTypeAllocation);

			var orderFacts = new IFact[]
			{
				waveFact,
				MockOrderFact(ord, waveFact.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			strategyMock.Setup(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), false)).Returns(true);

			if (cartonizeSplitCases)
			{
				strategyMock.Setup(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false)).Returns(CartonisationResult.Cartonised);
			}

			if (pickPalletsByLabel)
			{
				strategyMock.Setup(s => s.PickPalletsByLabel(It.IsAny<WhsPick>())).Returns(true);
			}

			if (pickCasesByLabel)
			{
				strategyMock.Setup(s => s.PickCasesByLabel(It.IsAny<WhsPick>())).Returns(true);
			}

			var assertionRun = false;

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					p.SetAllocatePackageLabelsStrategyForTest(strategyMock.Object);
					AssertUnsavedPickCreated(ord);
					assertionRun = true;

					var allocation = new AllocateFIFOLegacyMock();
					allocation.Allocate(p, n, ps, poi);
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);
			Assert(assertionRun);

			var pick = ord.Pick;
			AssertNotNull("Picks should now be attached.", pick);
			AssertEquals(1, orderLine.PickLines.Count);
			AssertEquals(10m, pick.OrderedInventories[0].PickLineQuantity);
			AssertUnsavedPickCreated(ord);

			var expectsCartonisation = cartonizeSplitCases || pickPalletsByLabel || pickCasesByLabel;
			AssertEquals(expectsCartonisation, pick.WP_IsCartonised);

			strategyMock.Verify(s => s.RunChecksPriorToCartonisingOrPickingByLabel(pick, false), expectsCartonisation ? Times.Once() : Times.Never());
			strategyMock.Verify(s => s.CartoniseSplitCases(pick, false), cartonizeSplitCases ? Times.Once() : Times.Never());
			strategyMock.Verify(s => s.PickPalletsByLabel(pick), pickPalletsByLabel ? Times.Once() : Times.Never());
			strategyMock.Verify(s => s.PickCasesByLabel(pick), pickCasesByLabel ? Times.Once() : Times.Never());

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_DoesNotCallAllocatePackageLabels_WhenAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, bulkLocation, pickFaceLocation);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			SetDetachWavedOrdersWithBlockingShortfallValue(data.Org1, data.Whs1.PK, false);
			Factory.Save();

			var pick = TestProcessResults_WavedOrderFacts_AutoPicking_DoesNotCallAllocatePackageLabelsCore(data.Org1, order);
			AssertEquals(true, pick.WP_IsAwaitingReplenishment);
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_DoesNotCallAllocatePackageLabels_WhenPickIsBuilding()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Auto);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Helper.CreateWhsOrderLine(order, data.Part1, 1000m);

			SetDetachWavedOrdersWithBlockingShortfallValue(data.Org1, data.Whs1.PK, false);
			Factory.Save();

			var pick = TestProcessResults_WavedOrderFacts_AutoPicking_DoesNotCallAllocatePackageLabelsCore(data.Org1, order);
			AssertEquals(PickStatus.Codes.Building, pick.WP_PickStatus);
		}

		void SetDetachWavedOrdersWithBlockingShortfallValue(OrgHeader client, ZGuid whsPk, bool value)
		{
			var pickParams = WhsClientPickingParams.GetClientPickingParams(client).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = whsPk;
			pickParams.WPP_DetachWavedOrdersWithBlockingShortfall = value;
		}

		WhsPick TestProcessResults_WavedOrderFacts_AutoPicking_DoesNotCallAllocatePackageLabelsCore(OrgHeader client, WhsOrder order)
		{
			var processor = GetProcessor();
			var waveFact = PrepareWaveFact(cartonizeSplitCases: true, pickCasesByLabel: true, pickPalletsByLabel: true, forcePickByCaseUOMTypeAllocation: false, forceSplitCaseUOMTypeAllocation: false);

			var orderFacts = new IFact[]
			{
				waveFact,
				MockOrderFact(order, waveFact.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			strategyMock.Setup(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns(true);
			strategyMock.Setup(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false)).Returns(CartonisationResult.Cartonised);
			strategyMock.Setup(s => s.PickPalletsByLabel(It.IsAny<WhsPick>())).Returns(true);
			strategyMock.Setup(s => s.PickCasesByLabel(It.IsAny<WhsPick>())).Returns(true);

			var assertionRun = false;

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					p.SetAllocatePackageLabelsStrategyForTest(strategyMock.Object);
					assertionRun = true;

					var allocation = new AllocateFIFOLegacyMock();
					allocation.Allocate(p, n, ps, poi);
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);
			Assert(assertionRun);

			var pick = order.Pick;
			AssertNotNull("Picks should now be attached.", pick);
			AssertEquals(pick.WP_PickOption, WhsPickOption.Codes.Auto);
			AssertUnsavedPickCreated(order);

			strategyMock.Verify(s => s.RunChecksPriorToCartonisingOrPickingByLabel(pick, It.IsAny<bool>()), Times.Never());
			strategyMock.Verify(s => s.CartoniseSplitCases(pick, false), Times.Never());
			strategyMock.Verify(s => s.PickPalletsByLabel(pick), Times.Never());
			strategyMock.Verify(s => s.PickCasesByLabel(pick), Times.Never());

			return pick;
		}

		public void TestProcessResults_WavedOrderFacts_AutoPicking_SharedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;
			var product3 = Helper.CreateProduct(data.Org1, "P3");

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product2, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R4", product3, 30m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 20m);
			var ord3 = Helper.CreateWhsOrder(client, whs, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(ord3, product3, 30m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord3.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact2.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			var pick1 = ord1.Pick;
			var pick2 = ord2.Pick;
			var pick3 = ord3.Pick;

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);

			AssertEquals("Orders should both be attached to same pick.", pick1, pick2);
			AssertNotEquals("Orders should not be attached to same pick.", pick1, pick3);

			AssertPickIsConfiguredCorrectly(pick1);
			AssertPickIsConfiguredCorrectly(pick3);

			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(false, orderLine1.PickLines[0].IsPicked);
			AssertEquals(10m, pick1.OrderedInventories[0].PickLineQuantity);

			AssertEquals(2, orderLine2.PickLines.Count);
			AssertEquals(false, orderLine2.PickLines[0].IsPicked);
			AssertEquals(false, orderLine2.PickLines[1].IsPicked);
			AssertEquals(20m, pick2.OrderedInventories[1].PickLineQuantity);

			AssertEquals(1, orderLine3.PickLines.Count);
			AssertEquals(false, orderLine3.PickLines[0].IsPicked);
			AssertEquals(30m, pick3.OrderedInventories[0].PickLineQuantity);

			Factory.Save(); // Create pick ID
			Factory.Save(); // Ensure we unhook logging
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick1.HumanReadableName} with Order(s): {ord1.WD_DocketID}, {ord2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick3.HumanReadableName} with Order(s): {ord3.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_AutoPickingDoesNotPrintPickSlip()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 20m);

			whs.WW_AutoPrintPickingSlip = true;

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 10m);
			var ord2 = Helper.CreateWhsOrder(client, whs, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(ord2, product2, 20m);

			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, ord2.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();

			var orderFacts = new IFact[]
			{
				waveFact1,
				waveFact2,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact2.PK, client.PK.ToGuid()).Object,
			};

			WhsDocumentPrinter.LastPrintedDocumentName = "";

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			AssertEquals("Pick slip should not be printed.", "", WhsDocumentPrinter.LastPrintedDocumentName);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client1 = data.Org1;
			var client2 = Helper.CreateClient("C2");
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateProductClientRelationShip(client2, product1);
			Helper.CreateProductClientRelationShip(client2, product2);

			for (var i = 0; i < 5; i++)
			{
				Helper.CreateWhsReceiveWithInventory(client1, whs, $"RP1_{i}", product1, 10m);
				Helper.CreateWhsReceiveWithInventory(client1, whs, $"RP2_{i}", product2, 10m);
				Helper.CreateWhsReceiveWithInventory(client2, whs, $"RP3_{i}", product1, 10m);
				Helper.CreateWhsReceiveWithInventory(client2, whs, $"RP4_{i}", product2, 10m);
			}

			Factory.Save();

			var allWaveCreationFacts = new List<IFact>();
			for (var i = 0; i < 5; i++)
			{
				var ord1 = Helper.CreateWhsOrder(client1, whs, $"OP1_{i}");
				Helper.CreateWhsOrderLine(ord1, product1, 10m);
				var ord2 = Helper.CreateWhsOrder(client1, whs, $"OP2_{i}");
				Helper.CreateWhsOrderLine(ord2, product2, 10m);
				var ord3 = Helper.CreateWhsOrder(client2, whs, $"OP3_{i}");
				Helper.CreateWhsOrderLine(ord3, product1, 10m);
				var ord4 = Helper.CreateWhsOrder(client2, whs, $"OP4_{i}");
				Helper.CreateWhsOrderLine(ord4, product2, 10m);

				Factory.Save();

				var waveFact1 = PrepareWaveFact();
				var waveFact2 = PrepareWaveFact(cartonizeSplitCases: true, pickPalletsByLabel: true, pickCasesByLabel: true);
				allWaveCreationFacts.Add(waveFact1);
				allWaveCreationFacts.Add(waveFact2);
				allWaveCreationFacts.Add(MockOrderFact(ord1, waveFact1.PK, client1.PK.ToGuid()).Object);
				allWaveCreationFacts.Add(MockOrderFact(ord2, waveFact1.PK, client1.PK.ToGuid()).Object);
				allWaveCreationFacts.Add(MockOrderFact(ord3, waveFact2.PK, client2.PK.ToGuid()).Object);
				allWaveCreationFacts.Add(MockOrderFact(ord4, waveFact2.PK, client2.PK.ToGuid()).Object);
			}

			var notificationsMock = new Mock<INotifications>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			var allocation = new AllocateFIFOLegacyMock();
			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					p.SetAllocatePackageLabelsStrategyForTest(strategyMock.Object);
					allocation.Allocate(p, n, ps, poi);
				});

			strategyMock.Setup(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), false)).Returns(true);
			strategyMock.Setup(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false)).Returns(CartonisationResult.Cartonised);
			strategyMock.Setup(s => s.PickPalletsByLabel(It.IsAny<WhsPick>())).Returns(true);
			strategyMock.Setup(s => s.PickCasesByLabel(It.IsAny<WhsPick>())).Returns(true);

			var testFactory = new BusinessObjectFactory();
			var processor = GetProcessor();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 }, // 1 initial load of orders, 2 by AllocateFIFOLegacyMock
				{ WhsDocketLineSchema.Constants.TableName, 13 }, // 1 with fetch hint, 10 from SynchroniseCachedBusinessObjectsWithDB per pick, 2 by AllocateFIFOLegacyMock
				{ WhsInventoryViewSchema.Constants.TableName, 10 }, // Once per pick, loading available inventories for attached orders
				{ WhsPickLineSchema.Constants.TableName, 3 }, // 1 initial load, 2 by AllocateFIFOLegacyMock
			};

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, testFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				processor.ProcessResults(testFactory, new ProductionRulesEngineResult(allWaveCreationFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);
			strategyMock.Verify(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), false), Times.Exactly(5));
			strategyMock.Verify(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false), Times.Exactly(5));
			strategyMock.Verify(s => s.PickPalletsByLabel(It.IsAny<WhsPick>()), Times.Exactly(5));
			strategyMock.Verify(s => s.PickCasesByLabel(It.IsAny<WhsPick>()), Times.Exactly(5));
		}

		void AssertPickIsConfiguredCorrectly(WhsPick pick, bool pickPalletsByLabel = false, bool pickCasesByLabel = false, bool cartonizeSplitCases = false, bool forcePickByCaseUOMTypeAllocation = false, bool forceSplitCaseUOMTypeAllocation = false)
		{
			AssertEquals(pickPalletsByLabel, pick.WP_PickPalletsByLabel);
			AssertEquals(pickCasesByLabel, pick.WP_PickCasesByLabel);
			AssertEquals(cartonizeSplitCases, pick.WP_CartoniseSplitCases);
			AssertEquals(forcePickByCaseUOMTypeAllocation, pick.WP_ForcePickByCaseUOMTypeAllocation);
			AssertEquals(forceSplitCaseUOMTypeAllocation, pick.WP_ForceSplitCaseUOMTypeAllocation);
			AssertEquals("WaveCreation PickType should be Order.", PickType.Codes.Order, pick.WP_PickType);
		}

		void AssertUnsavedPickCreated(WhsOrder order)
		{
			Assert("Order has unsaved changes.", order.HasChanges);
			AssertNotNull("Pick was created for order.", order.Pick);
			AssertNotNull("Pick was not saved for order.", order.Pick.IsInDatabase);
			AssertEquals("WaveCreation PickType should be Order.", PickType.Codes.Order, order.Pick.WP_PickType);
		}

		void AssertOrderHasErrorEvent(WhsOrder order, string reason)
		{
			var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("Should have error event on detached order.", $"Error Report: {Constants.EventReferenceParameterTypes.WaveCreation}, {reason}", logs[0].DisplayEventReference);
		}

		void AssertOrderHasNoErrorEvent(WhsOrder order)
		{
			var logs = order.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ErrorReport.Code));
			AssertEquals("Should not have error event on not detached order.", 0, logs.Length);
		}

		public void TestProcessResults_WavedOrderFacts_SplitingAwaitingReplenishmentPicks_SinglePick()
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

			var processor = GetProcessor();
			var waveFact = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact,
				MockOrderFact(order2, waveFact.PK, data.Org1.PK.ToGuid()).Object,
				MockOrderFact(order1, waveFact.PK, data.Org1.PK.ToGuid()).Object,
			};

			WhsPick initialPick = null;
			WhsPick pickFromSplit = null;
			var splitter = new Mock<IPartiallyReplenishedPickSplitter>();
			splitter
				.Setup(m => m.SplitOrdersFromPartiallyReplenishedPick(It.IsAny<WhsPick>()))
				.Callback((WhsPick ps) =>
				{
					initialPick = ps;
					pickFromSplit = Factory.New<WhsPick>();
					pickFromSplit.WP_WW_Whs = initialPick.WP_WW_Whs;
					order1.WD_WP = pickFromSplit.PK;
				})
				.Returns(pickFromSplit);

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(splitter.Object))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			var pick1 = order1.Pick;
			var pick2 = order2.Pick;
			pick1.Orders.Load();
			pick2.Orders.Load();

			AssertNotEquals(pick1, pick2);

			AssertUnsavedPickCreated(order1);
			AssertUnsavedPickCreated(order2);

			AssertNotNull("Picks should now be attached.", pick1);
			AssertPickIsConfiguredCorrectly(pick1);
			AssertEquals(false, pick1.WP_IsAwaitingReplenishment);

			AssertNotNull("Picks should now be attached.", pick2);
			AssertPickIsConfiguredCorrectly(pick2);

			AssertEquals(10m, pick2.OrderedInventories[0].PickLineQuantity);
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);
			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(false, orderLine1.PickLines[0].IsPicked);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);

			Factory.Save(); // Create pick ID
			Factory.Save(); // Ensure we unhook logging

			AssertEquals("Pick1 has correct number of orders", 1, pick1.Orders.Count);
			AssertEquals("Pick2 has correct number of orders", 1, pick2.Orders.Count);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick1.WP_PickNo} by splitting the awaiting replenishment {pick2.HumanReadableName}. This pick contains Order(s): {order1.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick2.WP_PickNo} (awaiting replenishment) with Order(s): {order1.WD_DocketID}, {order2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_SplitingAwaitingReplenishmentPicks_MultiplePicks()
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
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "02");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "03");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "04");
			Helper.CreateWhsOrderLine(order4, data.Part2, 10m);
			Factory.Save();

			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order1.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order2.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order3.WD_WP);
			AssertEquals("Precondition: No attached Picks.", Guid.Empty, order4.WD_WP);

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(order2, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
				MockOrderFact(order1, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
				waveFact2,
				MockOrderFact(order3, waveFact2.PK, data.Org1.PK.ToGuid()).Object,
				MockOrderFact(order4, waveFact2.PK, data.Org1.PK.ToGuid()).Object,
			};

			WhsPick initialPick1 = null;
			WhsPick pickFromSplit1 = null;
			WhsPick initialPick2 = null;
			WhsPick pickFromSplit2 = null;
			var splitter = new Mock<IPartiallyReplenishedPickSplitter>();
			splitter
				.Setup(m => m.SplitOrdersFromPartiallyReplenishedPick(It.Is<WhsPick>(p => p.Orders.Contains(order1))))
				.Callback((WhsPick ps) =>
				{
					initialPick1 = ps;
					pickFromSplit1 = Factory.New<WhsPick>();
					pickFromSplit1.WP_WW_Whs = initialPick1.WP_WW_Whs;
					order1.WD_WP = pickFromSplit1.PK;
				})
				.Returns(pickFromSplit1);

			splitter
				.Setup(m => m.SplitOrdersFromPartiallyReplenishedPick(It.Is<WhsPick>(p => p.Orders.Contains(order3))))
				.Callback((WhsPick ps) =>
				{
					initialPick2 = ps;
					pickFromSplit2 = Factory.New<WhsPick>();
					pickFromSplit2.WP_WW_Whs = initialPick2.WP_WW_Whs;
					order3.WD_WP = pickFromSplit2.PK;
				})
				.Returns(pickFromSplit2);

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(splitter.Object))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			VerifyNoErrors(notificationsMock);

			var pick1 = order1.Pick;
			var pick2 = order2.Pick;
			var pick3 = order3.Pick;
			var pick4 = order4.Pick;
			pick1.Orders.Load();
			pick2.Orders.Load();
			pick3.Orders.Load();
			pick4.Orders.Load();

			AssertNotEquals(pick1, pick2);
			AssertNotEquals(pick2, pick3);
			AssertNotEquals(pick3, pick4);
			AssertNotEquals(pick4, pick1);

			AssertUnsavedPickCreated(order1);
			AssertUnsavedPickCreated(order2);
			AssertUnsavedPickCreated(order3);
			AssertUnsavedPickCreated(order4);

			AssertNotNull("Picks should now be attached.", pick2);
			AssertPickIsConfiguredCorrectly(pick2);

			AssertEquals(1, orderLine1.PickLines.Count);
			AssertEquals(false, orderLine1.PickLines[0].IsPicked);
			AssertEquals(10m, pick2.OrderedInventories[0].PickLineQuantity);
			AssertEquals(true, pick2.WP_IsAwaitingReplenishment);

			AssertNotNull("Picks should now be attached.", pick1);
			AssertPickIsConfiguredCorrectly(pick1);
			AssertEquals(false, pick1.WP_IsAwaitingReplenishment);

			AssertNotNull("Picks should now be attached.", pick4);
			AssertPickIsConfiguredCorrectly(pick4);

			AssertEquals(1, orderLine3.PickLines.Count);
			AssertEquals(false, orderLine3.PickLines[0].IsPicked);
			AssertEquals(10m, pick4.OrderedInventories[0].PickLineQuantity);
			AssertEquals(true, pick4.WP_IsAwaitingReplenishment);

			AssertNotNull("Picks should now be attached.", pick3);
			AssertPickIsConfiguredCorrectly(pick3);
			AssertEquals(false, pick3.WP_IsAwaitingReplenishment);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);

			Factory.Save(); // Create pick ID
			Factory.Save(); // Ensure we unhook logging

			AssertEquals("Pick1 has correct number of orders", 1, pick1.Orders.Count);
			AssertEquals("Pick2 has correct number of orders", 1, pick2.Orders.Count);
			AssertEquals("Pick3 has correct number of orders", 1, pick3.Orders.Count);
			AssertEquals("Pick4 has correct number of orders", 1, pick4.Orders.Count);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick1.WP_PickNo} by splitting the awaiting replenishment {pick2.HumanReadableName}. This pick contains Order(s): {order1.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick2.WP_PickNo} (awaiting replenishment) with Order(s): {order1.WD_DocketID}, {order2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick3.WP_PickNo} by splitting the awaiting replenishment {pick4.HumanReadableName}. This pick contains Order(s): {order3.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick4.WP_PickNo} (awaiting replenishment) with Order(s): {order3.WD_DocketID}, {order4.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_SplitingAwaitingReplenishmentPicks_DbHits()
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

			var processor = GetProcessor();
			var orderFacts = new List<IFact>();
			foreach (var (o1, o2, o3, o4, o5, o6) in ordersForPicks)
			{
				var waveFact = PrepareWaveFact();
				orderFacts.Add(waveFact);
				orderFacts.Add(MockOrderFact(o1, waveFact.PK, data.Org1.PK.ToGuid()).Object);
				orderFacts.Add(MockOrderFact(o2, waveFact.PK, data.Org1.PK.ToGuid()).Object);
				orderFacts.Add(MockOrderFact(o3, waveFact.PK, data.Org1.PK.ToGuid()).Object);
				orderFacts.Add(MockOrderFact(o4, waveFact.PK, data.Org1.PK.ToGuid()).Object);
				orderFacts.Add(MockOrderFact(o5, waveFact.PK, data.Org1.PK.ToGuid()).Object);
				orderFacts.Add(MockOrderFact(o6, waveFact.PK, data.Org1.PK.ToGuid()).Object);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 3 },
				{ OrgContactSchema.Constants.TableName, 3 },
				{ OrgCusCodeSchema.Constants.TableName, 3 },
				{ OrgAddressSchema.Constants.TableName, 7 },
				{ PkgPackageJobSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 19 }, // These hits come from ProcessResults not splitter code
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 3 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 15 }, // Caused by GetInventoriesQueryForNonFinalisedPick() which changes from every pick so left as number of Picks hits.
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickLineSchema.Constants.TableName, 4 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
			};

			var notificationsMock = new Mock<INotifications>();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				using (Globals.SetIsUserInteractiveForTest(false))
				{
					processor.ProcessResults(newFactory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
				}
			}
			newFactory.Save(); // Create pick ID
			newFactory.Save(); // Ensure we unhook logging

			AssertEquals("30 Picks created", 30, newFactory.Load<WhsPick>(new ZQuery()).Length);
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_OrdersHasShortFall_RemoveByPickPriorityAndReallocate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product1, 11m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");
			var ord3 = Helper.CreateWhsOrder(client, whs, "03");
			var ord4 = Helper.CreateWhsOrder(client, whs, "04");

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			ord1.WD_PickPriority = 0;
			ord2.WD_PickPriority = 1;
			ord3.WD_PickPriority = 2;
			ord4.WD_PickPriority = 3;

			Helper.CreateWhsOrderLine(ord1, product1, 2m);
			Helper.CreateWhsOrderLine(ord2, product1, 4m);
			Helper.CreateWhsOrderLine(ord3, product1, 5m);
			Helper.CreateWhsOrderLine(ord4, product1, 4m);

			Factory.Save();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord4, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);
			AssertOrderHasNoErrorEvent(ord1);
			AssertOrderHasNoErrorEvent(ord2);
			AssertOrderHasNoErrorEvent(ord3);
			AssertOrderHasErrorEvent(ord4, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = ord1.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_WhenClientPickingDetachWaveOrderParamIsDefault()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var ord1 = Helper.CreateWhsOrder(client, whs, "01");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No Stock was allocated to pick.")), Times.Once);
			AssertNull("Pick was not created for order.", ord1.Pick);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = Factory.LoadTop1<WhsPick>(new ZQuery());
			AssertNull(pick);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_SetClientPickingDetachWaveOrderParam_False()
		{
			TestProcessResults_WavedOrderFacts_ClientPickingDetachWaveOrderParam_WithDifferentValues(detachWaveOrderParamValue: false);
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_SetClientPickingDetachWaveOrderParam_True()
		{
			TestProcessResults_WavedOrderFacts_ClientPickingDetachWaveOrderParam_WithDifferentValues(detachWaveOrderParamValue: true);
		}

		void TestProcessResults_WavedOrderFacts_ClientPickingDetachWaveOrderParam_WithDifferentValues(bool detachWaveOrderParamValue)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var ord1 = Helper.CreateWhsOrder(client, whs, "01");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			SetDetachWavedOrdersWithBlockingShortfallValue(client, data.Whs1.PK, detachWaveOrderParamValue);
			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No Stock was allocated to pick.")), Times.Once);

			if (detachWaveOrderParamValue)
			{
				AssertNull("Pick was not created for order.", ord1.Pick);
				AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);

				var pick = Factory.LoadTop1<WhsPick>(new ZQuery());
				AssertNull(pick);
			}
			else
			{
				AssertUnsavedPickCreated(ord1);
				AssertOrderHasNoErrorEvent(ord1);
			}

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), detachWaveOrderParamValue ? Times.Once : Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_AllOrdersIsShortfallAndNoPickAreCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			Helper.CreateWhsOrderLine(ord1, product2, 1m);
			Helper.CreateWhsOrderLine(ord2, product1, 1m);
			Helper.CreateWhsOrderLine(ord2, product2, 1m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertNull("Pick was not created for order.", ord1.Pick);
			AssertNull("Pick was not created for order.", ord2.Pick);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasErrorEvent(ord2, Constants.EventReferenceParameterReasons.Shortfall);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), Times.Exactly(1));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task

			var pick = Factory.LoadTop1<WhsPick>(new ZQuery());
			AssertNull(pick);
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_OneIsShortFallAndOneIsRegular()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product1, 2m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R3", product2, 2m);

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			Helper.CreateWhsOrderLine(ord1, product2, 1m);
			Helper.CreateWhsOrderLine(ord2, product1, 1m);
			Helper.CreateWhsOrderLine(ord2, product2, 1m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var ord3 = Helper.CreateWhsOrder(client, whs, "03");
			var ord4 = Helper.CreateWhsOrder(client, whs, "04");

			Helper.CreateWhsOrderLine(ord3, product1, 1m);
			Helper.CreateWhsOrderLine(ord3, product2, 1m);
			Helper.CreateWhsOrderLine(ord4, product1, 1m);
			Helper.CreateWhsOrderLine(ord4, product2, 1m);

			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact2,
				waveFact1,
				MockOrderFact(ord3, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord4, waveFact2.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertNull("Pick was not created for order.", ord1.Pick);
			AssertNull("Pick was not created for order.", ord2.Pick);
			AssertUnsavedPickCreated(ord3);
			AssertUnsavedPickCreated(ord4);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasErrorEvent(ord2, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasNoErrorEvent(ord3);
			AssertOrderHasNoErrorEvent(ord4);

			var pick = ord3.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick.HumanReadableName} with Order(s): {ord3.WD_DocketID}, {ord4.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_OrdersWithDifferentWhsOrderFulfillmentRuleValues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			Helper.CreateWhsOrderLine(ord2, product1, 1m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;

			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertNull("Pick was not created for order.", ord1.Pick);
			AssertUnsavedPickCreated(ord2);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasNoErrorEvent(ord2);

			var pick = ord2.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick.HumanReadableName} with Order(s): {ord2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_OrdersWithDifferentDetachWavedOrdersWithBlockingShortfallValues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			SetDetachWavedOrdersWithBlockingShortfallValue(data.Org1, data.Whs1.PK, true);

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);
			SetDetachWavedOrdersWithBlockingShortfallValue(client2, data.Whs1.PK, false);

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client2, whs, "02");

			Helper.CreateWhsOrderLine(ord1, product1, 1m);
			Helper.CreateWhsOrderLine(ord2, product1, 1m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertNull("Pick was not created for order.", ord1.Pick);
			AssertUnsavedPickCreated(ord2);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasNoErrorEvent(ord2);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {ord2.Pick.HumanReadableName} with Order(s): {ord2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "After detaching Orders in shortfall, no Orders are attached to the Pick.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_PickingAFullPalletThenDetach()
		{
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 10);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 16);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var product1 = data.Part1;
			var product2 = data.Part2;
			var whs = data.Whs1;
			var client = data.Org1;

			Helper.CreateProductUnit(product1, "UNT", "PLT", 10m);

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");

			var ord1WhsorderLine = Helper.CreateWhsOrderLine(ord1, product1, 5m);
			Helper.CreateWhsOrderLine(ord2, product1, 5m);
			Helper.CreateWhsOrderLine(ord2, product2, 5m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(product1, client, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", ZDateTimeOffset.Today.AddDays(-1), product1, 5m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product1, 10m);
			Factory.Save();

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var waveFact2 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord1);
			AssertNull("Pick was not created for order.", ord2.Pick);
			AssertOrderHasNoErrorEvent(ord1);
			AssertOrderHasErrorEvent(ord2, Constants.EventReferenceParameterReasons.Shortfall);
			AssertEquals("After detaching Order2 from picking, order1 no longer meets the full pallet rule. Inventory should be retrieved from pick face location.", ord1WhsorderLine.PickLines.Single().InventoryLine.WE_WL, pickFaceLocation.PK);

			var pick = ord1.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_CrossDockAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 1m);

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");
			var ord3 = Helper.CreateWhsOrder(client, whs, "03");
			ord1.WD_PickPriority = 3;
			ord2.WD_PickPriority = 2;
			ord3.WD_PickPriority = 1;

			var orderLine1 = Helper.CreateWhsOrderLine(ord1, product1, 1m);
			Helper.CreateWhsOrderLine(ord2, product1, 1m);
			Helper.CreateWhsOrderLine(ord3, product1, 1m);

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var reservePickLine = Helper.CreateReservePickLine(orderLine1, receive.Inventory[0], 1m);

			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord1);
			AssertOrderHasNoErrorEvent(ord1);
			AssertOrderHasErrorEvent(ord2, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasErrorEvent(ord3, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = ord1.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Reserve pick line was not deleted.", false, reservePickLine.IsDeleted);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, bike, 1m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, bike, 1m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, bike, 1m);

			order1.WD_PickPriority = 1;
			order2.WD_PickPriority = 2;
			order1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			order2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			var notificationsMock = new Mock<INotifications>();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact();
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(order1, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
				MockOrderFact(order2, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(order2);
			AssertOrderHasNoErrorEvent(order2);
			AssertOrderHasErrorEvent(order1, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = order2.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Did *not* use PickByBOM after reallocating.", 0, order2.Lines[0].ChildComponentLines.Count);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes_FullyShortOrder()
			=> TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes(fullyShort: true);

		public void TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes_PartiallyShortOrder()
			=> TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes(fullyShort: false);

		void TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes(bool fullyShort)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product1, fullyShort ? 11m : 13m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");
			var ord3 = Helper.CreateWhsOrder(client, whs, "03");
			var ord4 = Helper.CreateWhsOrder(client, whs, "04");

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			ord1.WD_PickPriority = 0;
			ord2.WD_PickPriority = 1;
			ord3.WD_PickPriority = 2;
			ord4.WD_PickPriority = 3;

			Helper.CreateWhsOrderLine(ord1, product1, 2m);
			Helper.CreateWhsOrderLine(ord2, product1, 4m);
			Helper.CreateWhsOrderLine(ord3, product1, 5m);
			Helper.CreateWhsOrderLine(ord4, product1, 4m);

			Factory.Save();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact(cartonizeSplitCases: true);
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord4, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			strategyMock.Setup(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), false)).Returns(true);
			strategyMock.Setup(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false)).Returns(CartonisationResult.Cartonised);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					p.SetAllocatePackageLabelsStrategyForTest(strategyMock.Object);
					var allocation = new AllocateFIFOLegacyMock();
					allocation.Allocate(p, n, ps, poi);
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord1);
			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);
			AssertOrderHasNoErrorEvent(ord1);
			AssertOrderHasNoErrorEvent(ord2);
			AssertOrderHasNoErrorEvent(ord3);
			AssertOrderHasErrorEvent(ord4, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = ord1.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Should be cartonized.", true, pick.WP_IsCartonised);

			strategyMock.Verify(s => s.RunChecksPriorToCartonisingOrPickingByLabel(pick, false), Times.Once());
			strategyMock.Verify(s => s.CartoniseSplitCases(pick, false), Times.Once());

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes_MultipleShortOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;
			var product2 = data.Part2;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product2, 1m);

			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");
			var ord3 = Helper.CreateWhsOrder(client, whs, "03");

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			ord1.WD_PickPriority = 2;
			ord2.WD_PickPriority = 1;
			ord3.WD_PickPriority = 0;

			Helper.CreateWhsOrderLine(ord1, product1, 10m);
			Helper.CreateWhsOrderLine(ord1, product2, 2m);

			Helper.CreateWhsOrderLine(ord2, product1, 4m);
			Helper.CreateWhsOrderLine(ord2, product2, 2m);

			Helper.CreateWhsOrderLine(ord3, product1, 10m);

			Factory.Save();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact(cartonizeSplitCases: true);
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			var strategyMock = new Mock<IAllocatePackageLabelsStrategy>();

			strategyMock.Setup(s => s.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), false)).Returns(true);
			strategyMock.Setup(s => s.CartoniseSplitCases(It.IsAny<WhsPick>(), false)).Returns(CartonisationResult.Cartonised);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns(AllocationResult.AllocatedStock)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					p.SetAllocatePackageLabelsStrategyForTest(strategyMock.Object);
					var allocation = new AllocateFIFOLegacyMock();
					allocation.Allocate(p, n, ps, poi);
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord3);
			AssertOrderHasNoErrorEvent(ord3);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);
			AssertOrderHasErrorEvent(ord2, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = ord3.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Should be cartonized.", true, pick.WP_IsCartonised);

			strategyMock.Verify(s => s.RunChecksPriorToCartonisingOrPickingByLabel(pick, false), Times.Once());
			strategyMock.Verify(s => s.CartoniseSplitCases(pick, false), Times.Once());

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_DetachOrders_Cartonizes_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var product1 = data.Part1;

			whs.WW_IsPickByUOMEnabled = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(100), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			whs.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var packType = product1.Lookups.PackTypes.Single(p => p.F3_Code == product1.OP_StockKeepingUnit);
			packType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product1, 13m);
			Factory.Save();

			var ord1 = Helper.CreateWhsOrder(client, whs, "01");
			var ord2 = Helper.CreateWhsOrder(client, whs, "02");
			var ord3 = Helper.CreateWhsOrder(client, whs, "03");
			var ord4 = Helper.CreateWhsOrder(client, whs, "04");

			ord1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			ord4.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			ord1.WD_PickPriority = 0;
			ord2.WD_PickPriority = 1;
			ord3.WD_PickPriority = 2;
			ord4.WD_PickPriority = 3;

			Helper.CreateWhsOrderLine(ord1, product1, 2m);
			Helper.CreateWhsOrderLine(ord2, product1, 4m);
			Helper.CreateWhsOrderLine(ord3, product1, 5m);
			Helper.CreateWhsOrderLine(ord4, product1, 4m);

			Factory.Save();

			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact(cartonizeSplitCases: true);
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(ord1, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord2, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord3, waveFact1.PK, client.PK.ToGuid()).Object,
				MockOrderFact(ord4, waveFact1.PK, client.PK.ToGuid()).Object,
			};

			var notificationsMock = new Mock<INotifications>();
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(ord2);
			AssertUnsavedPickCreated(ord3);
			AssertUnsavedPickCreated(ord4);
			AssertOrderHasNoErrorEvent(ord2);
			AssertOrderHasNoErrorEvent(ord3);
			AssertOrderHasNoErrorEvent(ord4);
			AssertOrderHasErrorEvent(ord1, Constants.EventReferenceParameterReasons.Shortfall);

			var pick = ord2.Pick;
			AssertEquals("Pick status should have been reset.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals("Should be cartonized.", true, pick.WP_IsCartonised);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == "No picks were created from this run.")), Times.Never);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		public void TestProcessResults_WavedOrderFacts_SplitingAwaitingReplenishmentPicks_Cartonizes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_SplitOrdersFromPartiallyReplenishedPicks = true;

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(100), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var packType = data.Part1.Lookups.PackTypes.Single(p => p.F3_Code == data.Part1.OP_StockKeepingUnit);
			packType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			var pickFaceLocation1 = data.Whs1.FindLocation("A-1");
			var bulkLocation1 = data.Whs1.FindLocation("A-2");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation1);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickFaceLocation1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, pickFaceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, bulkLocation1, "");
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 10m, bulkLocation1, pickFaceLocation1);
			transferLine1.RunPreSaveValidation();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			Factory.Save();

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor();
			var waveFact1 = PrepareWaveFact(cartonizeSplitCases: true);
			var orderFacts = new IFact[]
			{
				waveFact1,
				MockOrderFact(order1, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
				MockOrderFact(order2, waveFact1.PK, data.Org1.PK.ToGuid()).Object,
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				processor.ProcessResults(Factory, new ProductionRulesEngineResult(orderFacts), notificationsMock.Object, new CancellationToken());
			}

			AssertUnsavedPickCreated(order1);
			AssertUnsavedPickCreated(order2);

			var pick1 = order1.Pick;
			AssertEquals("Pick1 is NOT awaiting replenishment.", false, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Pick1 is cartonized.", true, pick1.WP_IsCartonised);

			var pick2 = order2.Pick;
			AssertEquals("Pick2 is awaiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);
			AssertEquals("Pick2 is NOT cartonized.", false, pick2.WP_IsCartonised);

			Factory.Save();
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created {pick1.HumanReadableName} by splitting the awaiting replenishment {pick2.HumanReadableName}. This pick contains Order(s): {order1.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Pick {pick2.WP_PickNo} (awaiting replenishment) with Order(s): {order1.WD_DocketID}, {order2.WD_DocketID}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Type.IsFatal)), Times.Never); // Should have no errors in the service task
		}

		#endregion

		#region Implementation

		protected override string GetInformationMessageForNothingProcessed()
		{
			return "No picks were created from this run.";
		}

		protected override GuidRegistryItem GetErrorContactGroupRegistryItem()
		{
			return WarehouseDataRegistry.Instance.WaveCreationRulesFailureNotificationGroup;
		}

		Mock<IOrderFact> MockOrderFact(WhsOrder order, Guid waveFactPK, Guid clientPK)
		{
			var orderFact = new Mock<IOrderFact>();
			orderFact.Setup(o => o.PK).Returns(order.PK.ToGuid());
			orderFact.Setup(o => o.WaveFactPK).Returns(waveFactPK);
			orderFact.Setup(o => o.DocketID).Returns(order.WD_DocketID);

			var clientFact = new Mock<IOrganisationFact>();
			clientFact.Setup(c => c.PK).Returns(clientPK);

			orderFact.Setup(o => o.Client).Returns(new FactJoin<IOrganisationFact>(clientFact.Object));

			return orderFact;
		}

		WaveFact PrepareWaveFact(bool pickPalletsByLabel = false, bool pickCasesByLabel = false, bool cartonizeSplitCases = false, bool forcePickByCaseUOMTypeAllocation = false, bool forceSplitCaseUOMTypeAllocation = false)
		{
			return new WaveFact(pickPalletsByLabel, pickCasesByLabel, cartonizeSplitCases, forcePickByCaseUOMTypeAllocation, forceSplitCaseUOMTypeAllocation);
		}

		protected override IScheduledRuleProcessor GetProcessor() => new WaveCreationRuleProcessor(new PartiallyReplenishedPickSplitter());

		#endregion
	}
}
