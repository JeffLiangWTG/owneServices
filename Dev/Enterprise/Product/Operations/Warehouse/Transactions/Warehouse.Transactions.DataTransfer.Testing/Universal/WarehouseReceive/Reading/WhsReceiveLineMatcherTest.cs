using System;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsReceiveLineMatcherTest : WhsTestCaseWithFactory
	{
		#region TestLineMatchedByLineAndSubLine

		public void TestLineMatchedByLineAndSubLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, 1, 2).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, 2, 1).InDocketLine;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.LineNumber = 2;
			orderLine.SubLineNumber = 1;
			orderLine.Product = new Product();

			AssertEquals("Should find a match based on Line and Subline", receiveLine2, GetReceiveLineMatcher().FindExistingBizOForNonCustoms(receive, orderLine, Array.Empty<WhsReceiveLine>()));
		}
		#endregion

		#region TestFindExistingBizOForNonCustoms_MatchedNoMoreThanOnce

		public void TestFindExistingBizOForNonCustoms_MatchedNoMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, 2, 3).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, 2, 3).InDocketLine;
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, 2, 3).InDocketLine;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.LineNumber = 2;
			orderLine.SubLineNumber = 3;

			var line1 = GetReceiveLineMatcher().FindExistingBizOForNonCustoms(receive, orderLine, Array.Empty<WhsReceiveLine>());
			var line2 = GetReceiveLineMatcher().FindExistingBizOForNonCustoms(receive, orderLine, new[] { line1 });
			var line3 = GetReceiveLineMatcher().FindExistingBizOForNonCustoms(receive, orderLine, new[] { line1, line2 });

			AssertContainsExactElementsInAnyOrder("Should still expect the same 3 lines.", receive.Lines, new[] { receiveLine1, receiveLine2, receiveLine3 });
			AssertContainsExactElementsInAnyOrder("Expect all 3 lines to have been matched.", receive.Lines, new[] { line1, line2, line3 });
		}

		#endregion

		#region TestLineMatchedByEntryKey

		public void TestLineMatchedByEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			receiveLine1.WE_BondedEntryKey = "AAA-777";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			receiveLine2.WE_BondedEntryKey = "BBB-888";
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;
			orderLine.CustomsData = new CustomsEntryInfo();
			orderLine.CustomsData.EntryKey = "BBB";
			orderLine.CustomsData.EntryLineNumber = 888;
			AssertEquals("Precondition:", "BBB-888", orderLine.CustomsData.GetFormattedCustomsEntryKeyWithLineNo());

			AssertEquals("Should find a match based on BondedEntryKey", receiveLine2, GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, Array.Empty<WhsReceiveLine>()));
		}

		#endregion

		#region TestLineMatchedByProductCode

		public void TestLineMatchedByProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1).InDocketLine;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part2.OP_PartNum;

			AssertEquals("Should find a match based on product code", receiveLine2, GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, Array.Empty<WhsReceiveLine>()));
		}

		public void TestLineMatchedByProductCodePassedIn_NotFromOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part2.OP_PartNum;

			AssertEquals("Should NOT find a match, because Product Code was not passed in.", null, GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, null, Array.Empty<WhsReceiveLine>()));
		}

		public void TestFindExistingBizO_LineMatchedNoMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;

			var line1 = GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, Array.Empty<WhsReceiveLine>());
			var line2 = GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, new[] { line1 });
			var line3 = GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, new[] { line1, line2 });

			AssertContainsExactElementsInAnyOrder("Should still expect the same 3 lines.", receive.Lines, new[] { receiveLine1, receiveLine2, receiveLine3 });
			AssertContainsExactElementsInAnyOrder("Expect all 3 lines to have been matched.", receive.Lines, new[] { line1, line2, line3 });
		}

		#endregion

		#region TestLineMatchedByPackingLine

		public void TestLineMatchedByPackingLineID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			receiveLine.WE_AllocationKey = "PackingLine0001";
			Factory.Save();

			var packingLine = new PackingLine();
			packingLine.PackingLineID = "PackingLine0001";
			var packedItem = new PackedItem();
			packedItem.Product = new Product();
			packedItem.Product.Code = data.Part1.OP_PartNum;

			AssertEquals("Should find a match based on product code", receiveLine, GetReceiveLineMatcher().FindExistingBizOForForwardingShipment(receive, "PackingLine0001", packedItem.Product, Array.Empty<WhsReceiveLine>()));
		}

		public void TestLineMatchedByPackingLineID_NotFromPackingLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			receiveLine.WE_AllocationKey = "PackingLine0001";
			Factory.Save();

			var packingLine = new PackingLine();
			packingLine.PackingLineID = "PackingLine0001";
			var packedItem = new PackedItem();
			packedItem.Product = new Product();
			packedItem.Product.Code = data.Part1.OP_PartNum;

			AssertEquals("Should NOT find a match, because Product Code was not passed in.", null, GetReceiveLineMatcher().FindExistingBizOForForwardingShipment(receive, "PackingLine0001", null, Array.Empty<WhsReceiveLine>()));
		}

		public void TestLineMatchedByPackingLineID_LineMatchedNoMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;
			receiveLine1.WE_AllocationKey = "PackingLine0001";
			receiveLine2.WE_AllocationKey = "PackingLine0001";
			receiveLine3.WE_AllocationKey = "PackingLine0001";
			Factory.Save();

			var packingLine = new PackingLine();
			packingLine.PackingLineID = "PackingLine0001";
			var packedItem = new PackedItem();
			packedItem.Product = new Product();
			packedItem.Product.Code = data.Part1.OP_PartNum;

			var line1 = GetReceiveLineMatcher().FindExistingBizOForForwardingShipment(receive, "PackingLine0001", packedItem.Product, Array.Empty<WhsReceiveLine>());
			var line2 = GetReceiveLineMatcher().FindExistingBizOForForwardingShipment(receive, "PackingLine0001", packedItem.Product, new[] { line1 });
			var line3 = GetReceiveLineMatcher().FindExistingBizOForForwardingShipment(receive, "PackingLine0001", packedItem.Product, new[] { line1, line2 });

			AssertContainsExactElementsInAnyOrder("Should still expect the same 3 lines.", receive.Lines, new[] { receiveLine1, receiveLine2, receiveLine3 });
			AssertContainsExactElementsInAnyOrder("Expect all 3 lines to have been matched.", receive.Lines, new[] { line1, line2, line3 });
		}

		#endregion

		#region TestUnsavedDocketLineWontMatch

		public void TestUnsavedDocketLineWontMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1).InDocketLine;

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;
			AssertNull("Unsaved receive line should not match.", GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, Array.Empty<WhsReceiveLine>()));
			Factory.Save();

			AssertEquals("Saved receive line should match.", receiveLine, GetReceiveLineMatcher().FindExistingBizOForCustoms(receive, orderLine, orderLine.Product, Array.Empty<WhsReceiveLine>()));
		}

		#endregion

		#region Implementation

		IWhsReceiveLineMatcher GetReceiveLineMatcher() => new WhsReceiveLineMatcher();

		#endregion

	}
}
