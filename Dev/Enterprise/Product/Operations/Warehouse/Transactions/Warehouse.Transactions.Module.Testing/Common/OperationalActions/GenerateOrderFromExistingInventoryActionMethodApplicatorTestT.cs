using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class GenerateOrderFromExistingInventoryActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction_Simple

		public void TestAction_Simple()
		{
			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.OH_IsConsignee = true;
			Applicator.ConsigneePK = consignee.PK;

			var data = new TestDataSimpleEnvironment(Factory);
			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).WI_ReceiveCrossDockOrderNo = "OR1";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m).WI_ReceiveCrossDockOrderNo = "OR1";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m).WI_ReceiveCrossDockOrderNo = "OR1";
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert("Precondition: Receive should be finalised: Errors: " + receive.Notifications.GetErrors().ToUniqueMessageListString(), receive.IsFinalised);

			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			WhsOrder[] orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 2 order lines", 2, orders[0].Lines.Count);
			AssertNull("Cross Dock Location should be empty.", orders[0].CrossDockLocation);

			WhsOrderLine[] lines = orders[0].Lines.ToArray<WhsOrderLine>();
			AssertEquals(1, lines.Count(line => line.ReservedPickLines.Count == 1));
			AssertEquals(1, lines.Count(line => line.ReservedPickLines.Count == 2));
			AssertEquals(1, lines.Count(line => line.WE_TransactionQuantity == 30m));
			AssertEquals(1, lines.Count(line => line.WE_TransactionQuantity == 15m));
		}

		#endregion

		#region TestAction_Complex

		public void TestAction_Complex()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, Helper.CreateWarehouse("W1"), "R1", new TestNotificationBuffer());
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m); // Order No
			var receive2 = Helper.CreateWhsReceive(data.Org1, Helper.CreateWarehouse("W2"), "R2", new TestNotificationBuffer());
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m); // Required By Date
			var receive3 = Helper.CreateWhsReceive(data.Org1, Helper.CreateWarehouse("W3"), "R3", new TestNotificationBuffer());
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m); // Consignee
			var receive4 = Helper.CreateWhsReceive(data.Org1, Helper.CreateWarehouse("W4"), "R4", new TestNotificationBuffer());
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part1, 10m); // No Cross Docking info set
			var receive5 = Helper.CreateWhsReceive(data.Org1, Helper.CreateWarehouse("W5"), "R5", new TestNotificationBuffer());
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive5, data.Part1, 0m);  // 0 quantity

			var today = ZDateTimeOffset.Today;

			var consignee = Helper.CreateClient("CONSIGNEE-1");
			consignee.OH_IsConsignee = true;
			Applicator.ConsigneePK = consignee.PK;

			inventory1.WI_ReceiveCrossDockOrderNo = "OR1";
			// inventory3 consignee is set from Applicator.ConsigneePK property.
			// inventory4 has no Cross Docking information.
			inventory5.WI_ReceiveCrossDockOrderNo = "OR2"; // 0 quantity - no Order should be created.

			Factory.Save();

			const bool SaveOnSuccess = true;
			var actualLogTextLines = SimulateRun(InventoriesOrReceives, SaveOnSuccess).messages;
			AssertLogTextPartsMatch(TestAction_Complex_ExpectedLogTextLines, actualLogTextLines);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			orders.FirstOrDefault(o => o.WD_WW_Whs == inventory2.WI_WW_Whs).RequiredDate = today.ToZDateTime();

			AssertEquals("Order should be created when Inventory has an Order No.", "OR1", inventory1.ReservedPickLines[0].DocketLine.Docket.WD_ExternalReference);
			AssertEquals("Order should be created when Inventory has a Required by Date.", today.EndOfDay().AddSeconds(-59), inventory2.ReservedPickLines[0].DocketLine.Docket.WD_RequiredDate);
			AssertEquals("Order should be created when Inventory has a Consignee.", consignee, ((WhsOrder)inventory3.ReservedPickLines[0].DocketLine.Docket).Consignee);
			TestAction_Complex_AssertOrderCreatedForInventoryWithNoCrossDockingInfo(inventory4);
			AssertEquals("No Order be created when quantity is 0.", 0, inventory5.ReservedPickLines.Count);

			foreach (var order in orders)
			{
				AssertEquals("Each Order should have exactly 1 line.", 1, order.Lines.Count);
				AssertEquals("Each Order should have stock from exactly 1 Inventory line.", 10m, order.Lines[0].WE_TransactionQuantity);
				AssertEquals("Order was not Cross-Docked from exactly 1 Inventory line.", 1, order.Lines[0].ReservedPickLines.Count);
			}
		}

		List<string> TestAction_Complex_ExpectedLogTextLines
		{
			get
			{
				var expectedLogTextLines = new List<string>(new string[] {
					"<-- Summary -->",
					"INFO: Order [HL W00000006] has been generated.",
					"INFO: Order [HL W00000007] has been generated.",
					"INFO: Order [HL W00000008] has been generated.",
					"INFO: Order [HL W00000009] has been generated." });
				expectedLogTextLines.AddRange(TestAction_Complex_ExpectedLogTextLinesAdditional);

				return expectedLogTextLines;
			}
		}

		protected virtual string[] TestAction_Complex_ExpectedLogTextLinesAdditional => Array.Empty<string>();

		protected void AssertLogTextPartsMatch(List<string> expectedLogTextLines, List<string> actualLogTextLines)
		{
			AssertEquals("Expected and Actual Log Text consist of different number of lines.", expectedLogTextLines.Count, actualLogTextLines.Count);

			foreach (string logTextLine in expectedLogTextLines)
			{
				AssertCollectionContains(string.Format("Couldn't find line ({0}) in Actual Log Text.", logTextLine), logTextLine, actualLogTextLines);
			}
		}

		protected void TestAction_Complex_AssertOrderCreatedForInventoryWithNoCrossDockingInfo(WhsInventoryView inventoryWithNoCrossDockingInfo)
		{
			AssertNotNull("Order should be created even if the Inventory has no Cross Docking info.", inventoryWithNoCrossDockingInfo.ReservedPickLines[0].DocketLine.Docket);
		}

		#endregion

		#region TestActions_GetLineData

		public void TestActions_GetLineData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var today = ZDateTime.Today;
			var todayOffset = ZDateTimeOffset.Today;
			Applicator.ConsigneePK = consignee.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = (WhsReceiveLine)Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, today.Date.AddDays(2), today.Date.AddDays(-2), "PA1", "PA2", "PA3", "").InDocketLine;
			inventory.WE_SerialNumber = "SN1";
			inventory.ConsigneeNameOrPK = consignee.PK.ToString();
			inventory.WE_RequiredByDate = todayOffset;
			inventory.WE_ReceiveCrossDockOrderNo = "OR1";

			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals(data.Org1.PK, orders[0].WD_OH_Client);
			AssertEquals(data.Whs1.PK, orders[0].WD_WW_Whs);
			AssertEquals("OR1", orders[0].WD_ExternalReference);
			AssertEquals(todayOffset.EndOfDay().AddSeconds(-59), orders[0].WD_RequiredDate);
			AssertEquals(consignee.PK, orders[0].ConsigneePK);

			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals(data.Part1.PK, orders[0].Lines[0].WE_OP);
			AssertEquals(1m, orders[0].Lines[0].WE_TransactionQuantity);
			AssertEquals(today.AddDays(2), orders[0].Lines[0].WE_ExpiryDate);
			AssertEquals(today.AddDays(-2), orders[0].Lines[0].WE_PackingDate);
			AssertEquals("PA1", orders[0].Lines[0].WE_PartAttrib1);
			AssertEquals("PA2", orders[0].Lines[0].WE_PartAttrib2);
			AssertEquals("PA3", orders[0].Lines[0].WE_PartAttrib3);
			AssertEquals("SN1", orders[0].Lines[0].WE_SerialNumber);
		}

		#endregion

		#region TestAction_WhereCannotCrossDockInventory

		public void TestAction_WhereCannotCrossDockInventory()
		{
			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.OH_IsConsignee = true;
			Applicator.ConsigneePK = consignee.PK;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLocation1 = data.Whs1.FindLocation("A-1");
			var inventoryLocation2 = data.Whs1.FindLocation("A-2");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, inventoryLocation2);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, inventoryLocation1);
			inventory1.WI_ReceiveCrossDockOrderNo = "OR1";
			inventory2.WI_ReceiveCrossDockOrderNo = "OR1";
			inventory3.WI_ReceiveCrossDockOrderNo = "OR1";
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, inventoryLocation2, inventoryLocation1);
			transferLine.RunPreSaveValidation(); // commits stock

			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 2 order lines", 2, orders[0].Lines.Count);

			var lines = orders[0].Lines.ToArray<WhsOrderLine>();
			var orderLine1 = lines.Single(l => l.WE_OP == data.Part1.PK);
			var orderLine2 = lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals(1, orderLine1.ReservedPickLines.Count);
			AssertEquals(1, orderLine2.ReservedPickLines.Count);
			AssertEquals(inventory1.InDocketLine.PK, orderLine1.ReservedPickLines[0].WZ_WE_InventoryLine);
			AssertEquals(inventory3.InDocketLine.PK, orderLine2.ReservedPickLines[0].WZ_WE_InventoryLine);
		}

		#endregion

		#region TestGenerateOrder_ForSameOrderNoConsigneeAndRequireDate_CreatesOneOrder

		[TestDate(2013, 03, 20)]
		public void TestGenerateOrder_ForSameOrderNoConsigneeAndRequireDate_CreatesOneOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Helper.CreateClient("CNS");
			Applicator.ConsigneePK = consignee.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			line1.WI_ReceiveCrossDockOrderNo = "OR1";
			line2.WI_ReceiveCrossDockOrderNo = "OR1";
			((WhsReceiveLine)line1.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			((WhsReceiveLine)line2.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Order lines should be merged.", 30m, orders.Single().Lines.Single().WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_TotalLineVolumeAndWeightOutOfRange_NotFitInDB

		public void TestGenerateOrder_TotalLineVolumeAndWeightOutOfRange_NotFitInDB_OneTotalHasIssue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 100;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			data.Part1.OP_Weight = 1;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			data.Part2.OP_Cubic = 1;
			data.Part2.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			data.Part2.OP_Weight = 200;
			data.Part2.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			var consignee = Helper.CreateClient("123");
			var overridingConsignee = Helper.CreateClient("XYZ");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10002m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10003m);
			receive.WD_TotalCubic = 10000; // calculated value is not fit in db
			receive.WD_TotalWeight = 10000; // calculated value is not fit in db
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			Applicator.ConsigneePK = overridingConsignee.PK;

			const bool SaveOnSuccess = true;
			List<string> actualLogTextLines = SimulateRun(InventoriesOrReceives, SaveOnSuccess).messages;

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			var orderForProduct1 = orders.Single(o => o.Lines[0].WE_OP == data.Part1.PK);
			var orderForProduct2 = orders.Single(o => o.Lines[0].WE_OP == data.Part2.PK);
			AssertEquals("Should ignore one total cube", 0m, orderForProduct1.WD_TotalCubic);
			AssertEquals("Should ignore keep valid total Weight", 10002m, orderForProduct1.WD_TotalWeight);
			AssertEquals("Should ignore keep valid total cube", 10003m, orderForProduct2.WD_TotalCubic);
			AssertEquals("Should ignore one total Weight", 0m, orderForProduct2.WD_TotalWeight);

			var logProduct1 = new string[] {
				$"WARNING: Order [HL {orderForProduct1.WD_DocketID}] 'Total Line Volume' has been changed from 1000200 to 0 to skip the error[The number 1,000,200 is too large, the maximum value allowed for Volume is 999,999.999.], you may need to correct it manually." ,
				 $"INFO: Order [HL {orderForProduct1.WD_DocketID}] has been generated." };

			var logProduct2 = new string[] {
				$"WARNING: Order [HL {orderForProduct2.WD_DocketID}] 'Total Line Weight' has been changed from 2000600 to 0 to skip the error[The number 2,000,600 is too large, the maximum value allowed for Weight is 999,999.999.], you may need to correct it manually." ,
				$"INFO: Order [HL {orderForProduct2.WD_DocketID}] has been generated." };

			var expectedLogText = new List<string>() { "<-- Summary -->" };
			expectedLogText.AddRange(logProduct1);
			expectedLogText.AddRange(logProduct2);

			AssertLogTextPartsMatch(expectedLogText, actualLogTextLines);
		}

		#endregion

		#region TestGenerateOrder_TotalLineVolumeAndWeightOutOfRange_NotFitInDB_BothTotalHaveIssue

		public void TestGenerateOrder_TotalLineVolumeAndWeightOutOfRange_NotFitInDB_BothTotalHaveIssue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 100;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			data.Part1.OP_Weight = 200;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			var consignee1 = Helper.CreateClient("123");
			var consignee2 = Helper.CreateClient("XYZ");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10002m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10003m);
			receive.WD_TotalCubic = 10000; // calculated value is not fit in db
			receive.WD_TotalWeight = 10000; // calculated value is not fit in db
			((WhsReceiveLine)inventory1.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee2.PK;
			Factory.Save();

			const bool SaveOnSuccess = true;
			var actualLogTextLines = SimulateRun(InventoriesOrReceives, SaveOnSuccess).messages;

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			var order1 = orders.Single(o => o.Lines[0].WE_TransactionQuantity == 10002m);
			var order2 = orders.Single(o => o.Lines[0].WE_TransactionQuantity == 10003m);
			AssertEquals("Should ignore total cube", true, orders.All(o => o.WD_TotalCubic == 0m));
			AssertEquals("Should ignore total Weight", true, orders.All(o => o.WD_TotalWeight == 0m));

			var logProduct1 = new string[] {
				$"WARNING: Order [HL {order1.WD_DocketID}] 'Total Line Volume' has been changed from 1000200 to 0 to skip the error[The number 1,000,200 is too large, the maximum value allowed for Volume is 999,999.999.], you may need to correct it manually." ,
				$"WARNING: Order [HL {order1.WD_DocketID}] 'Total Line Weight' has been changed from 2000400 to 0 to skip the error[The number 2,000,400 is too large, the maximum value allowed for Weight is 999,999.999.], you may need to correct it manually." ,
				$"INFO: Order [HL {order1.WD_DocketID}] has been generated." };

			var logProduct2 = new string[] {
				$"WARNING: Order [HL {order2.WD_DocketID}] 'Total Line Volume' has been changed from 1000300 to 0 to skip the error[The number 1,000,300 is too large, the maximum value allowed for Volume is 999,999.999.], you may need to correct it manually." ,
				$"WARNING: Order [HL {order2.WD_DocketID}] 'Total Line Weight' has been changed from 2000600 to 0 to skip the error[The number 2,000,600 is too large, the maximum value allowed for Weight is 999,999.999.], you may need to correct it manually." ,
				$"INFO: Order [HL {order2.WD_DocketID}] has been generated." };

			var expectedLogText = new List<string>() { "<-- Summary -->" };
			expectedLogText.AddRange(logProduct1);
			expectedLogText.AddRange(logProduct2);

			AssertLogTextPartsMatch(expectedLogText, actualLogTextLines);
		}

		#endregion

		#region TestGenerateOrder_TotalLineVolumeAndWeight

		public void TestGenerateOrder_TotalLineVolumeAndWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Cubic = 100;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			data.Part1.OP_Weight = 1;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			data.Part2.OP_Cubic = 1;
			data.Part2.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			data.Part2.OP_Weight = 200;
			data.Part2.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			var product3 = Helper.CreateProduct(data.Org1, "P3");
			product3.OP_Cubic = 1;
			product3.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			product3.OP_Weight = 1;
			product3.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			var consignee1 = Helper.CreateClient("AAA", "C1");
			var consignee2 = Helper.CreateClient("BBB", "C2");
			var consignee3 = Helper.CreateClient("CCC", "C3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10002m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10003m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, product3, 10004m);
			receive.WD_TotalCubic = 10000; // calculated value is not fit in db
			receive.WD_TotalWeight = 10000; // calculated value is not fit in db
			((WhsReceiveLine)inventory1.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee2.PK;
			((WhsReceiveLine)inventory3.InDocketLine).ConsigneeDocAddress.OrganisationPK = consignee3.PK;
			Factory.Save();

			const bool SaveOnSuccess = true;
			var actualLogTextLines = SimulateRun(InventoriesOrReceives, SaveOnSuccess).messages;

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			var orderForProduct1 = orders.Single(o => o.Lines[0].WE_OP == data.Part1.PK);
			var orderForProduct2 = orders.Single(o => o.Lines[0].WE_OP == data.Part2.PK);
			var orderForProduct3 = orders.Single(o => o.Lines[0].WE_OP == product3.PK);
			AssertEquals("Should ignore one total cube", 0m, orderForProduct1.WD_TotalCubic);
			AssertEquals("Should ignore keep valid total Weight", 10002m, orderForProduct1.WD_TotalWeight);
			AssertEquals("Should ignore one total Weight", 0m, orderForProduct2.WD_TotalWeight);
			AssertEquals("Should ignore keep valid total cube", 10003m, orderForProduct2.WD_TotalCubic);
			AssertEquals("Should ignore one total Weight", 0m, orderForProduct2.WD_TotalWeight);
			AssertEquals("Should ignore keep valid total Weight", 10004m, orderForProduct3.WD_TotalWeight);
			AssertEquals("Should ignore keep valid total cube", 10004m, orderForProduct3.WD_TotalCubic);

			var logProduct1 = new string[] {
				$"WARNING: Order [HL {orderForProduct1.WD_DocketID}] 'Total Line Volume' has been changed from 1000200 to 0 to skip the error[The number 1,000,200 is too large, the maximum value allowed for Volume is 999,999.999.], you may need to correct it manually." ,
				$"INFO: Order [HL {orderForProduct1.WD_DocketID}] has been generated." };

			var logProduct2 = new string[] {
				$"WARNING: Order [HL {orderForProduct2.WD_DocketID}] 'Total Line Weight' has been changed from 2000600 to 0 to skip the error[The number 2,000,600 is too large, the maximum value allowed for Weight is 999,999.999.], you may need to correct it manually." ,
				$"INFO: Order [HL {orderForProduct2.WD_DocketID}] has been generated." };

			var logProduct3 = new string[] { $"INFO: Order [HL {orderForProduct3.WD_DocketID}] has been generated." };

			var expectedLogText = new List<string>() { "<-- Summary -->" };
			expectedLogText.AddRange(logProduct1);
			expectedLogText.AddRange(logProduct2);
			expectedLogText.AddRange(logProduct3);

			AssertLogTextPartsMatch(expectedLogText, actualLogTextLines);
		}

		#endregion

		#region TestGenerateOrder_ConsigneePK

		public void TestGenerateOrder_WhenLineConsigneeAndOverridingConsigneePkAreEmpty()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R42", data.Part1, 5m);

			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"ERROR: Could not generate an Order for Product P1 from Receive [HL W00000001] because Receive Line 1 has no Consignee Address.\r\n";

			// Act

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// Assert

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting no Order generated", 0, orders.Length);
		}

		public void TestGenerateOrder_WhenLineConsigneeIsEmptyButHasOverridingConsigneePk()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var overriddenConsignee = Helper.CreateClient("XYZ");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R42", data.Part1, 5m);
			var inventory = receive.Lines[0];
			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			Applicator.ConsigneePK = overriddenConsignee.PK;

			// Act

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// Assert

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting 1 Order generated", 1, orders.Length);

			var order = orders.Single();
			AssertEquals("Expecting Order with overridden Consignee", overriddenConsignee.PK, order.ConsigneePK);
			AssertEquals("Should *not* have populated consignee on the receive line.", true, inventory.ConsigneeDocAddress.IsEmpty);
		}

		public void TestGenerateOrder_WhenLineConsigneeIsOverrideAddressAndHasOverridingConsigneePk()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var overriddenConsignee = Helper.CreateClient("XYZ");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R42", data.Part1, 5m);
			var inventory = receive.Lines[0];
			inventory.ConsigneeDocAddress.E2_AddressOverride = true;
			inventory.ConsigneeDocAddress.E2_Address1 = "SOME OVERRIDEN ADDRESS";

			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			Applicator.ConsigneePK = overriddenConsignee.PK;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting 1 Order generated", 1, orders.Length);

			var order = orders.Single();
			AssertEquals("Expecting Order with override address Consignee", true, order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("Expecting Order with override address Consignee", "SOME OVERRIDEN ADDRESS", order.ConsigneeDocAddress.E2_Address1);

			AssertEquals("Receive line should retain override address Consignee", true, inventory.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("Receive line should retain override address Consignee", "SOME OVERRIDEN ADDRESS", inventory.ConsigneeDocAddress.E2_Address1);
		}

		public void TestGenerateOrder_WhenSomeLineConsigneesAreEmptyButHasDifferentOveridingConsigneePk()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var exisitingConsignee = Helper.CreateClient("123");
			var overridingConsignee = Helper.CreateClient("XYZ");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // No Consignee
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Other Consignee
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = exisitingConsignee.PK;

			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n";

			Applicator.ConsigneePK = overridingConsignee.PK;

			// Act

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// Assert

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting 2 Orders generated", 2, orders.Length);

			var ordersWithOverridingConsignee = orders.Where(order => order.ConsigneePK == overridingConsignee.PK).ToArray();
			AssertEquals("Expecting 1 Order with overridden Consignee", 1, ordersWithOverridingConsignee.Length);
			AssertEquals("Expecting 1 Order Line", 1, ordersWithOverridingConsignee[0].Lines.Count);

			var ordersWithExistingConsignee = orders.Where(order => order.ConsigneePK == exisitingConsignee.PK).ToArray();
			AssertEquals("Expecting 1 Order with existing Consignee", 1, ordersWithExistingConsignee.Length);
			AssertEquals("Expecting 1 Order Line", 1, ordersWithExistingConsignee[0].Lines.Count);
		}

		public void TestGenerateOrder_WhenSomeLineConsigneesAreEmptyButHasOverridingConsigneePkSameAsExistingOne()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var exisitingConsignee = Helper.CreateClient("123");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // No Consignee
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Other Consignee
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = exisitingConsignee.PK;

			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			Applicator.ConsigneePK = exisitingConsignee.PK;

			// Act

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// Assert

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting 1 Order generated", 1, orders.Length);

			var ordersWithExistingConsignee = orders.Where(order => order.ConsigneePK == exisitingConsignee.PK).ToArray();
			AssertEquals("Expecting 1 Order with existing Consignee", 1, ordersWithExistingConsignee.Length);
			AssertEquals("Expecting 1 Order Lines", 1, ordersWithExistingConsignee[0].Lines.Count);
			AssertEquals("Expecting correct quantity", 10m, ordersWithExistingConsignee[0].Lines[0].WE_PackQuantity);
		}

		public void TestGenerateOrder_WhenSomeLineConsigneesAreEmptyAndOverridingConsigneeIsEmpty()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var exisitingConsignee = Helper.CreateClient("123");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // No Consignee
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Other Consignee
			((WhsReceiveLine)inventory2.InDocketLine).ConsigneeDocAddress.OrganisationPK = exisitingConsignee.PK;

			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n" +
				"ERROR: Could not generate an Order for Product P1 from Receive [HL W00000001] because Receive Line 1 has no Consignee Address.\r\n";

			// Act

			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// Assert

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Expecting 1 Order generated", 1, orders.Length);

			var ordersWithExistingConsignee = orders.Where(order => order.ConsigneePK == exisitingConsignee.PK).ToArray();
			AssertEquals("Expecting 1 Order with existing Consignee", 1, ordersWithExistingConsignee.Length);
			AssertEquals("Expecting 1 Order Line", 1, ordersWithExistingConsignee[0].Lines.Count);
		}

		public void TestGenerateOrder_FinaliseOrderAndPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var exisitingConsignee = Helper.CreateClient("123");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 5m);
			var inventory = receive.Lines[0].Inventory[0];
			((WhsReceiveLine)inventory.InDocketLine).ConsigneeDocAddress.OrganisationPK = exisitingConsignee.PK;
			Factory.Save();

			const string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			Applicator.ConsigneePK = exisitingConsignee.PK;

			// Act
			ApplyApplicator(InventoriesOrReceives, expectedLogText, true);

			// newOrder - the "Release" form opened from oldOrder form.
			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var orderCollectionNewFactory = newFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Precondition - expected 1 Order in a newFactory.", 1, orderCollectionNewFactory.Length);
			var newOrder = orderCollectionNewFactory.Single();
			newOrder.WD_RequiredDate = ZDateTimeOffset.Today;

			var pick = newFactory.New<WhsPick>();
			pick.PickOrders(newOrder);
			pick.FinaliseOrder(newOrder);
			pick.FinalisePick();
			pick.RunPreSaveValidation();
			newFactory.Save();

			// Assert
			var oldOrder = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Single();
			oldOrder.RunPreSaveValidation();
			AssertEquals("There should be no errors on the order.", false, oldOrder.GetErrors().Any());
		}

		#endregion

		#region TestGenerateOrder_ConsidersExpectedQuantity

		public void TestGenerateOrder_ConsidersExpectedQuantity_UnfinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 10m;
			receiveLine.ConsigneeNameOrPK = consignee.PK.ToString();
			receiveLine.WE_RequiredByDate = ZDateTimeOffset.Today;
			receiveLine.WE_ReceiveCrossDockOrderNo = "OR1";

			Factory.Save();

			AssertEquals("Precondition: receive is not finalised.", false, receive.IsFinalised);
			AssertEquals("Precondition: WE_ClientOrderedUnits", 15m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: WE_TransactionQuantity", 10m, receiveLine.WE_TransactionQuantity);

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 15m, orders[0].Lines[0].WE_TransactionQuantity);
		}

		public void TestGenerateOrder_ConsidersExpectedQuantity_FinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 10m;
			receiveLine.ConsigneeNameOrPK = consignee.PK.ToString();
			receiveLine.WE_RequiredByDate = ZDateTimeOffset.Today;
			receiveLine.WE_ReceiveCrossDockOrderNo = "OR1";
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: WE_ClientOrderedUnits", 15m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: WE_TransactionQuantity", 10m, receiveLine.WE_TransactionQuantity);

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's transaction quantity/stock on hand.", 10m, orders[0].Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_BondedStock

		public void TestGenerateOrder_BondedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var location = data.Whs1.FindLocation("A-1");
			var bondedAreaPK = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			location.WLV_WA_PutawayArea = bondedAreaPK;
			location.WLV_WA_PickingArea = bondedAreaPK;
			Factory.Save();

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, location.PK, "123-1", "", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Order));
			AssertEquals("Should be 1 order", 1, orders.Length);
			AssertEquals("Should be Customs order", OrderType.Codes.Customs, orders[0].WD_DocketSubType);
			AssertEquals("Should be 1 order lines", 1, orders[0].Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 25m, orders[0].Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_MixedStock

		public void TestGenerateOrder_MixedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var location1 = data.Whs1.FindLocation("A-1");
			var bondedAreaPK = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			location1.WLV_WA_PutawayArea = bondedAreaPK;
			location1.WLV_WA_PickingArea = bondedAreaPK;
			Factory.Save();

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var bondedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			bondedReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inv = Helper.CreateWhsReceiveInventoryLine(bondedReceive, data.Part1, 10m, location1.PK, "123-1", "", 2m);
			inv.InDocketLine.WE_ReceiveCrossDockOrderNo = "OR1";
			bondedReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(bondedReceive);

			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R02");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location2);
			receiveLine.WE_ReceiveCrossDockOrderNo = "OR2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n" +
				"INFO: Order [HL W00000004] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 2 orders", 2, orders.Length);

			var customsOrder = orders.Single(o => o.WD_DocketSubType == OrderType.Codes.Customs);
			AssertEquals("External Ref correct.", "OR1", customsOrder.WD_ExternalReference);
			AssertEquals("Should be 1 order lines", 1, customsOrder.Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, customsOrder.Lines[0].WE_TransactionQuantity);

			var normalOrder = orders.Single(o => o.WD_DocketSubType == OrderType.Codes.Order);
			AssertEquals("External Ref correct.", "OR2", normalOrder.WD_ExternalReference);
			AssertEquals("Should be 1 order lines", 1, normalOrder.Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 15m, normalOrder.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_MixedCustomsStockAndPermits

		public void TestGenerateOrder_MixedCustomsStockAndPermits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var whs2 = Helper.CreateWarehouse("2", "B", 2, 1);
			whs2.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Helper.EnableWarehouseForBond(whs2, true);
			Factory.Save();

			var whs1Loc1 = data.Whs1.FindLocation("A-1");
			var bondedAreaWhs1PK = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			whs1Loc1.WLV_WA_PutawayArea = bondedAreaWhs1PK;
			whs1Loc1.WLV_WA_PickingArea = bondedAreaWhs1PK;

			var whs2Loc1 = whs2.FindLocation("B-1");
			var bondedAreaWhs2PK = whs2.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;
			whs2Loc1.WLV_WA_PutawayArea = bondedAreaWhs2PK;
			whs2Loc1.WLV_WA_PickingArea = bondedAreaWhs2PK;
			Factory.Save();

			var consignee = Helper.CreateClient("CONSIGNEE");
			Applicator.ConsigneePK = consignee.PK;

			var bondedReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			bondedReceive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inv1 = Helper.CreateWhsReceiveInventoryLine(bondedReceive1, data.Part1, 10m, whs1Loc1.PK, "123-1", "", 2m);
			inv1.InDocketLine.WE_ReceiveCrossDockOrderNo = "OR1";
			bondedReceive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(bondedReceive1);

			var bondedReceive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R02");
			bondedReceive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inv2 = Helper.CreateWhsReceiveInventoryLine(bondedReceive2, data.Part1, 15m, whs2Loc1.PK, "124-3", "", 2m);
			inv2.InDocketLine.WE_ReceiveCrossDockOrderNo = "OR2";
			bondedReceive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(bondedReceive2);
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n" +
				"INFO: Order [HL W00000004] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should be 2 orders", 2, orders.Length);

			var customsOrder = orders.Single(o => o.WD_DocketSubType == OrderType.Codes.Customs);
			AssertEquals("External Ref correct.", "OR2", customsOrder.WD_ExternalReference);
			AssertEquals("Should be 1 order lines", 1, customsOrder.Lines.Count);
			var customsOrderLine = customsOrder.Lines[0];
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 15m, customsOrderLine.WE_TransactionQuantity);

			var customsWithPermitsOrder = orders.Single(o => o.WD_DocketSubType == OrderType.Codes.CustomsReleaseWithPermit);
			AssertEquals("External Ref correct.", "OR1", customsWithPermitsOrder.WD_ExternalReference);
			AssertEquals("Should be 1 order lines", 1, customsWithPermitsOrder.Lines.Count);
			var customsWithPermitsOrderLine = customsWithPermitsOrder.Lines[0];
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, customsWithPermitsOrderLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_CrossDockLocation

		public void TestGenerateOrder_CrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consignee = Helper.CreateClient("CNS");
			var dockDoorLocationType = Helper.CreateLocationType("DOD", "test", false, 0, LocationClasses.Codes.DDL);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Factory.Save();

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 10m);
			receive.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000002] has been generated.\r\n";

			const bool SaveOnSuccess = true;
			Applicator.CrossDockLocationPK = location1.PK;

			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should have 1 order", 1, orders.Length);

			var order = orders.Single();
			AssertEquals("Should have this Cross Dock Location.", location1, order.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order.Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, order.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_CrossDockLocation_ErrorMessage

		public void TestGenerateOrder_CrossDockLocation_ErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consignee = Helper.CreateClient("CNS");
			var overriddenConsignee = Helper.CreateClient("XYZ");
			var dockDoorLocationType = Helper.CreateLocationType("DOD", "test", false, 0, LocationClasses.Codes.DDL);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var warehouse2 = Helper.CreateWarehouse("W2");
			var row2 = helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 2);
			var location2 = row2.Locations[0];
			location2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Factory.Save();

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 10m);
			receive1.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee.PK;
			receive1.Lines[0].WE_ReceiveCrossDockOrderNo = "RCD1";
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R02", data.Part1, 9m);
			var receive3 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R03", data.Part1, 8m);
			receive3.Lines[0].WE_ReceiveCrossDockOrderNo = "RCD2";
			Factory.Save();

			string expectedLogText =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000004] has been generated.\r\n" +
				"INFO: Order [HL W00000005] has been generated.\r\n" +
				"ERROR: Could not generate an Order for Receive [HL W00000002] because the Order's Warehouse does not match the selected Cross-Dock Location's Warehouse.\r\n";

			const bool SaveOnSuccess = true;
			Applicator.CrossDockLocationPK = location1.PK;
			Applicator.ConsigneePK = overriddenConsignee.PK;

			ApplyApplicator(InventoriesOrReceives, expectedLogText, SaveOnSuccess);

			var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should have 2 orders", 2, orders.Length);

			var order1 = orders.Where(o => o.WD_ExternalReference == "RCD1").Single();
			AssertEquals("Should have this Cross Dock Location.", location1, order1.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order1.Lines.Count);
			AssertEquals("Quantity should be 10.", 10m, order1.Lines[0].WE_TransactionQuantity);

			var order2 = orders.Where(o => o.WD_ExternalReference == "RCD2").Single();
			AssertEquals("Should have this Cross Dock Location.", location1, order2.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order2.Lines.Count);
			AssertEquals("Quantity should be 8.", 8m, order2.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestGenerateOrder_CrossDockLocation_RunTwice

		public void TestGenerateOrder_CrossDockLocation_RunTwice()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consignee = Helper.CreateClient("CNS");
			var dockDoorLocationType = Helper.CreateLocationType("DOD", "test", false, 0, LocationClasses.Codes.DDL);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var warehouse2 = Helper.CreateWarehouse("W2");
			var row2 = helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 2);
			var location2 = row2.Locations[0];
			location2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			Factory.Save();

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 10m);
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R02", data.Part1, 10m);
			Factory.Save();

			string expectedLogText1 =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000003] has been generated.\r\n" +
				"ERROR: Could not generate an Order for Receive [HL W00000002] because the Order's Warehouse does not match the selected Cross-Dock Location's Warehouse.\r\n";
			const bool SaveOnSuccess = true;
			Applicator.ConsigneePK = consignee.PK;
			Applicator.CrossDockLocationPK = location1.PK;
			ApplyApplicator(InventoriesOrReceives, expectedLogText1, SaveOnSuccess);

			var orders1 = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should have 1 order", 1, orders1.Length);
			var order1 = orders1.Single();
			AssertEquals("Should have this Cross Dock Location.", location1, order1.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order1.Lines.Count);
			AssertEquals("Order line's transaction quantity is receive line's client ordered units.", 10m, order1.Lines[0].WE_TransactionQuantity);

			var receive3 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R03", data.Part2, 8m);
			receive2.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			string expectedLogText2 =
				"<-- Summary -->\r\n" +
				"INFO: Order [HL W00000005] has been generated.\r\n" +
				"ERROR: Could not generate an Order for Receive [HL W00000002] because the Order's Warehouse does not match the selected Cross-Dock Location's Warehouse.\r\n";
			Applicator.ConsigneePK = consignee.PK;
			Applicator.CrossDockLocationPK = location1.PK;
			ApplyApplicator(InventoriesOrReceives, expectedLogText2, SaveOnSuccess);

			var orders2 = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should have 2 orders", 2, orders2.Length);

			var order2 = orders2.Where(o => o.Lines[0].WE_TransactionQuantity == 8m).Single();
			AssertEquals("Should have this Cross Dock Location.", location1, order2.CrossDockLocation);
		}

		#endregion

		#region TestGenerateOrder_GroupBy_OverBatchSize

		public void TestGenerateOrder_GroupBy_OverBatchSize()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consignee1 = Helper.CreateClient("AAA");
			var overriddenConsignee = Helper.CreateClient("XYZ");
			var dockDoorLocationType = Helper.CreateLocationType("DOD", "test", false, 0, LocationClasses.Codes.DDL);
			var warehouse2 = Helper.CreateWarehouse("W2");
			var row2 = helper.CreateRowAndGenerateLocations(warehouse2, "ROW2", 2, 2);
			var location2 = row2.Locations[0];
			location2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 10m);
			var receiveLine1 = receive1.Lines[0];
			receiveLine1.ConsigneeDocAddress.OrganisationPK = consignee1.PK;

			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R02", data.Part1, 9m);
			var receiveLine2 = receive2.Lines[0];
			receiveLine2.ConsigneeDocAddress.E2_AddressOverride = true;
			receiveLine2.ConsigneeDocAddress.E2_Address1 = "SOME ADDRESS";

			var receive3 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R03", data.Part2, 10m);
			var receive4 = helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R04", data.Part2, 9m);

			var receive5 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R05", data.Part1, 10m);
			var receiveLine5 = receive5.Lines[0];
			receiveLine5.ConsigneeDocAddress.OrganisationPK = consignee1.PK;

			var receive6 = helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R06", data.Part1, 9m);
			var receiveLine6 = receive6.Lines[0];
			receiveLine6.ConsigneeDocAddress.E2_AddressOverride = true;
			receiveLine6.ConsigneeDocAddress.E2_Address1 = "SOME ADDRESS";
			Factory.Save();

			using (RawDataRegistry.Instance.OperationalActionsRecordBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var actionSupporter = GetOperationalActionSupporter();
				var context = new OperationalActionContext(actionSupporter, "CreateOrderFromExistingInventory");
				var action = Factory.New<OperationalAction>();
				action.Context = context;
				Factory.Save();

				var selectedInventories = new MockTargetRecordSelectionForTest(new SelectedRecords(GetInventoryOrReceives().Select(o => o.PK).ToArray()));
				var runner = new OperationalActionRunner(action, context.Supporter.RootType, selectedInventories) { RunOnAllMatchingRecords = true };
				var applicator = GetApplicator(Factory);
				applicator.ConsigneePK = overriddenConsignee.PK;
				runner.MethodApplicators.Add(applicator);
				var dummyLog = new DummyOperationalActionLog();
				runner.BatchRun(dummyLog, (log, factory) => { factory.Save(); });

				AssertMultilineASCIIEquals(@"
INFO: Starting Batch 1 of 3: ...
INFO: Starting Section: Generate Order ...
INFO: Starting Batch 2 of 3: ...
INFO: Starting Section: Generate Order ...
INFO: Starting Batch 3 of 3: ...
INFO: Starting Section: Generate Order ...", dummyLog.MessagesString());
			}

			var newFactory = new BusinessObjectFactory();
			var orders = newFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));
			AssertEquals("Should have 4 orders", 4, orders.Length);

			var order1 = orders.Where(o => o.WD_WW_Whs == data.Whs1.PK && o.Lines[0].WE_OP == data.Part1.PK).Single();
			AssertNull("Cross Dock Location should be empty.", order1.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order1.Lines.Count);
			AssertEquals(consignee1.PK, order1.ConsigneeDocAddress.OrganisationPK);
			var orderLine1 = order1.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("orderLine1 should have 10+10=20 units.", 20m, orderLine1.WE_TransactionQuantity);

			var order2 = orders.Where(o => o.WD_WW_Whs == data.Whs1.PK && o.Lines[0].WE_OP == data.Part2.PK).Single();
			AssertNull("Cross Dock Location should be empty.", order2.CrossDockLocation);
			AssertEquals("Should have 1 order line", 1, order2.Lines.Count);
			AssertEquals(overriddenConsignee.PK, order2.ConsigneeDocAddress.OrganisationPK);
			var orderLine2 = order2.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("orderLine2 should have 10 units.", 10m, orderLine2.WE_TransactionQuantity);

			var order3 = orders.Where(o => o.WD_WW_Whs == warehouse2.PK && o.Lines[0].WE_OP == data.Part1.PK).Single();
			AssertNull("Cross Dock Location should be empty.", order3.CrossDockLocation);
			AssertEquals("Should have 1 order lines", 1, order3.Lines.Count);
			AssertEquals(true, order3.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("SOME ADDRESS", order3.ConsigneeDocAddress.E2_Address1);
			var orderLine3 = order3.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertEquals("orderLine3 should have 9+9=18 units.", 18m, orderLine3.WE_TransactionQuantity);

			var order4 = orders.Where(o => o.WD_WW_Whs == warehouse2.PK && o.Lines[0].WE_OP == data.Part2.PK).Single();
			AssertNull("Cross Dock Location should be empty.", order4.CrossDockLocation);
			AssertEquals("Should have 1 order lines", 1, order4.Lines.Count);
			AssertEquals(overriddenConsignee.PK, order4.ConsigneeDocAddress.OrganisationPK);
			var orderLine4 = order4.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("orderLine4 should have 9 units.", 9m, orderLine4.WE_TransactionQuantity);
		}

		#endregion

		#region Implementation

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		BusinessObject[] InventoriesOrReceives
		{
			get { return GetInventoryOrReceives(); }
		}

		protected abstract BusinessObject[] GetInventoryOrReceives();

		protected abstract WhsOperationalActionSupporter GetOperationalActionSupporter();

		protected abstract GenerateOrderFromExistingInventoryActionMethodApplicator GetApplicator(BusinessObjectFactory factory);

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		new GenerateOrderFromExistingInventoryActionMethodApplicator Applicator => (GenerateOrderFromExistingInventoryActionMethodApplicator)base.Applicator;
	}
}
