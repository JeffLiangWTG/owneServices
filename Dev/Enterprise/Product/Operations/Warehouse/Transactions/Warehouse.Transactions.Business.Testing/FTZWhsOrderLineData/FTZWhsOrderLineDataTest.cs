namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class FTZWhsOrderLineDataTest : WhsTestCaseWithFactory
	{
		public void TestProperties()
		{
			var ftzOrderLine = new FTZWhsOrderLineData(
				"AAA",
				"AAAAAA",
				5m,
				"BOX",
				"BBB",
				"US",
				"CCC",
				25m,
				10m,
				15m,
				"UNT",
				20m,
				"PLT",
				10m,
				"CU",
				"DDD");

			AssertEquals("AAA", ftzOrderLine.ProductCode);
			AssertEquals("AAAAAA", ftzOrderLine.ProductDesc);
			AssertEquals(5m, ftzOrderLine.Quantity);
			AssertEquals("BOX", ftzOrderLine.QuantityUnit);
			AssertEquals("BBB", ftzOrderLine.Tariff);
			AssertEquals("US", ftzOrderLine.CountryOfOriginCode);
			AssertEquals("CCC", ftzOrderLine.PrimaryPreference);
			AssertEquals(25m, ftzOrderLine.TotalReceiveQuantity);
			AssertEquals(10m, ftzOrderLine.TotalReceiveValueForDuty);
			AssertEquals(15m, ftzOrderLine.TotalReceiveCustomsQty);
			AssertEquals("UNT", ftzOrderLine.ReceiveCustomsQtyUnit);
			AssertEquals(20m, ftzOrderLine.TotalReceiveCustomsSecondQty);
			AssertEquals("PLT", ftzOrderLine.ReceiveCustomsSecondQtyUnit);
			AssertEquals(10m, ftzOrderLine.TotalReceiveCustomsThirdQty);
			AssertEquals("CU", ftzOrderLine.ReceiveCustomsThirdQtyUnit);
			AssertEquals("DDD", ftzOrderLine.ReceiveCustomsAddInfo);
		}
	}
}
