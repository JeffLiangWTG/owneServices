using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsDocketLineInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsDocketLineInfo>
	{
		#region TestConstructorForReceiveLines

		public void TestConstructorForReceiveLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location2);
			Factory.Save();

			var docketLineInfoCollection = new WhsDocketLineInfoCollection(receive.Lines);
			AssertNotNull(docketLineInfoCollection);
			AssertEquals(2, docketLineInfoCollection.Count);
			AssertEquals(receive.Lines[0].SupplierPart.OP_PartNum, docketLineInfoCollection[0].Product.Code);
			AssertEquals(receive.Lines[0].WE_TransactionQuantity, docketLineInfoCollection[0].Qty);
			AssertEquals("", docketLineInfoCollection[0].LocationFormattedCheckDigit);
			AssertEquals("11", docketLineInfoCollection[0].DestLocationFormattedCheckDigit);

			AssertEquals(receive.Lines[1].SupplierPart.OP_PartNum, docketLineInfoCollection[1].Product.Code);
			AssertEquals(receive.Lines[1].WE_TransactionQuantity, docketLineInfoCollection[1].Qty);
			AssertEquals("", docketLineInfoCollection[1].LocationFormattedCheckDigit);
			AssertEquals("22", docketLineInfoCollection[1].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestConstructorForTransferLines

		public void TestConstructorForTransferLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("WE_TransactionQuantity comes from each matching line", 5m, transfer.Lines[0].WE_TransactionQuantity);
			AssertEquals(10m, transfer.Lines[0].QtyToMoveIncludingMatchingLines);

			var docketLineInfoCollection = new WhsDocketLineInfoCollection(new WhsTransferLine[] { transferLine });
			AssertEquals(1, docketLineInfoCollection.Count);
			AssertEquals(10m, docketLineInfoCollection[0].Qty);
			AssertEquals(10m, docketLineInfoCollection[0].Packs);
			AssertEquals("11", docketLineInfoCollection[0].LocationFormattedCheckDigit);
			AssertEquals("22", docketLineInfoCollection[0].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestConstructorForAdjustmentLines

		public void TestConstructorForAdjustmentLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, location1);
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5, location2);
			Factory.Save();

			var docketLineInfoCollection = new WhsDocketLineInfoCollection(adjustment.Lines);
			AssertNotNull(docketLineInfoCollection);
			AssertEquals(2, docketLineInfoCollection.Count);
			AssertEquals(adjustment.Lines[0].SupplierPart.OP_PartNum, docketLineInfoCollection[0].Product.Code);
			AssertEquals(adjustment.Lines[0].WE_TransactionQuantity, docketLineInfoCollection[0].Qty);
			AssertEquals("", docketLineInfoCollection[0].LocationFormattedCheckDigit);
			AssertEquals("11", docketLineInfoCollection[0].DestLocationFormattedCheckDigit);

			AssertEquals(adjustment.Lines[1].SupplierPart.OP_PartNum, docketLineInfoCollection[1].Product.Code);
			AssertEquals(adjustment.Lines[1].WE_TransactionQuantity, docketLineInfoCollection[1].Qty);
			AssertEquals("", docketLineInfoCollection[1].LocationFormattedCheckDigit);
			AssertEquals("22", docketLineInfoCollection[1].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestConstructorForOrderLines

		public void TestConstructorForOrderLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			helper.CreateWhsOrderLine(order, data.Part1, 10);
			helper.CreateWhsOrderLine(order, data.Part1, 5);
			Factory.Save();

			var docketLineInfoCollection = new WhsDocketLineInfoCollection(order.Lines);
			AssertNotNull(docketLineInfoCollection);
			AssertEquals(2, docketLineInfoCollection.Count);
			AssertEquals(order.Lines[0].SupplierPart.OP_PartNum, docketLineInfoCollection[0].Product.Code);
			AssertEquals(order.Lines[0].WE_TransactionQuantity, docketLineInfoCollection[0].Qty);
			AssertEquals("", docketLineInfoCollection[0].LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfoCollection[0].DestLocationFormattedCheckDigit);

			AssertEquals(order.Lines[1].SupplierPart.OP_PartNum, docketLineInfoCollection[1].Product.Code);
			AssertEquals(order.Lines[1].WE_TransactionQuantity, docketLineInfoCollection[1].Qty);
			AssertEquals("", docketLineInfoCollection[1].LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfoCollection[1].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestConstructorForWorkOrderLines

		public void TestConstructorForWorkOrderLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";

			var order = helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			helper.CreateWhsWorkOrderLine(order, data.Part1, 10);
			helper.CreateWhsWorkOrderLine(order, data.Part1, 5);
			Factory.Save();

			var docketLineInfoCollection = new WhsDocketLineInfoCollection(order.Lines);
			AssertNotNull(docketLineInfoCollection);
			AssertEquals(2, docketLineInfoCollection.Count);
			AssertEquals(order.Lines[0].SupplierPart.OP_PartNum, docketLineInfoCollection[0].Product.Code);
			AssertEquals(order.Lines[0].WE_TransactionQuantity, docketLineInfoCollection[0].Qty);
			AssertEquals("", docketLineInfoCollection[0].LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfoCollection[0].DestLocationFormattedCheckDigit);

			AssertEquals(order.Lines[1].SupplierPart.OP_PartNum, docketLineInfoCollection[1].Product.Code);
			AssertEquals(order.Lines[1].WE_TransactionQuantity, docketLineInfoCollection[1].Qty);
			AssertEquals("", docketLineInfoCollection[1].LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfoCollection[1].DestLocationFormattedCheckDigit);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsDocketLineInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsDocketLineInfo);
		}

		protected override WhsDocketLineInfo GetNewObjectInfo()
		{
			return new WhsDocketLineInfo();
		}

		protected override DataObjectInfoCollection<WhsDocketLineInfo> GetNewObjectInfoCollection()
		{
			return new WhsDocketLineInfoCollection();
		}

		#endregion
	}
}
