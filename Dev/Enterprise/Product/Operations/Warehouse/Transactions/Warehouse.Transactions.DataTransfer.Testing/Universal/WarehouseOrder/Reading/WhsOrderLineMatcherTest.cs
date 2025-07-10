using System;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsOrderLineMatcherTest : WhsTestCaseWithFactory
	{
		public void TestLineMatchedByOrderLineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine1.WE_LineNo = 432;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine2.WE_LineNo = 55;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;
			orderLine.LineNumber = 55;

			AssertEquals("Should find a match based on Inwards BondedEntryKey", whsOrderLine2, GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, Array.Empty<WhsOrderLine>()));
		}

		public void TestLineMatchedByProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine1.WE_LineNo = 5;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part2, 1);
			whsOrderLine2.WE_LineNo = 5;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part2.OP_PartNum;
			orderLine.LineNumber = 5;

			AssertEquals("Should find a match based on product code", whsOrderLine2, GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, Array.Empty<WhsOrderLine>()));
		}

		public void TestLineMatchedByProductCodePassedIn_NotFromOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(whsOrder, data.Part2, 1);
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part2.OP_PartNum;

			AssertEquals("Should NOT find a match, because Product Code was not passed in.", null, GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, null, Array.Empty<WhsOrderLine>()));
		}

		public void TestFindExistingBizO_LineMatchedNoMoreThanOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var whsOrderLine1 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine1.WE_LineNo = 1;
			var whsOrderLine2 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine2.WE_LineNo = 1;
			var whsOrderLine3 = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine3.WE_LineNo = 1;
			Factory.Save();

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;
			orderLine.LineNumber = 1;

			var line1 = GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, Array.Empty<WhsOrderLine>());
			var line2 = GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, new[] { line1 });
			var line3 = GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, new[] { line1, line2 });

			AssertContainsExactElementsInAnyOrder("Should still expect the same 3 lines.", whsOrder.Lines, new[] { whsOrderLine1, whsOrderLine2, whsOrderLine3 });
			AssertContainsExactElementsInAnyOrder("Expect all 3 lines to have been matched.", whsOrder.Lines, new[] { line1, line2, line3 });
		}

		public void TestSavedStatusOfDocketLineWontEffectMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, 1);
			whsOrderLine.WE_LineNo = 64;

			var orderLine = new OrderLine();
			orderLine.Product = new Product();
			orderLine.Product.Code = data.Part1.OP_PartNum;
			orderLine.LineNumber = 64;

			AssertEquals("Unsaved receive line should match.", whsOrderLine, GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, Array.Empty<WhsOrderLine>()));
			Factory.Save();

			AssertEquals("Saved receive line should match.", whsOrderLine, GetOrderLineMatcher().FindExistingBizOByOrderLineNo(whsOrder, orderLine, orderLine.Product, Array.Empty<WhsOrderLine>()));
		}

		#region Implementation

		IWhsOrderLineMatcher GetOrderLineMatcher() => new WhsOrderLineMatcher();

		#endregion

	}
}
