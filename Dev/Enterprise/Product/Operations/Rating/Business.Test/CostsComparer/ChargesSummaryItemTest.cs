using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class ChargesSummaryItemTest : TestCaseWithFactory
	{
		public void TestChargesSummaryItem()
		{
			var item1 = new ChargesSummaryItem("MIN", 100m);
			var item2 = new ChargesSummaryItem("BAS", 50m);
			var item3 = new ChargesSummaryItem("UNT", 6m);
			var item4 = new ChargesSummaryItem("-", 50m, 0m, 50m);
			var item5 = new ChargesSummaryItem("-", 100m, 5m, 0m);
			var item6 = new ChargesSummaryItem("+", 50m, 5m, 0m);
			var item7 = new ChargesSummaryItem("+", 1000m, 4m, 0m);
			var item11 = new ChargesSummaryItem("MIN", 110m);
			var item12 = new ChargesSummaryItem("BAS", 55m);
			var item13 = new ChargesSummaryItem("UNT", 7m);
			var item14 = new ChargesSummaryItem("-", 50m, 5m, 0m);
			var item16 = new ChargesSummaryItem("+", 50m, 4m, 0m);

			AssertEquals(-1, item1.CompareTo(item2));
			AssertEquals(-1, item2.CompareTo(item3));
			AssertEquals(-1, item3.CompareTo(item4));
			AssertEquals(-1, item4.CompareTo(item5));
			AssertEquals(-1, item5.CompareTo(item6));
			AssertEquals(-1, item6.CompareTo(item7));

			AssertEquals(0, item1.CompareTo(item11));
			AssertEquals(0, item2.CompareTo(item12));
			AssertEquals(0, item3.CompareTo(item13));
			AssertEquals(0, item4.CompareTo(item14));
			AssertEquals(0, item6.CompareTo(item16));

			AssertEquals(210m, (item1 + item11).Value);
			AssertEquals(5m, (item4 + item14).Value);
			AssertEquals(50m, (item4 + item14).FlatAmount);
			AssertEquals(9m, (item6 + item16).Value);
			AssertEquals(11m, (item6 + item3).Value);
		}

		public void TestChargesSummaryItemCurrencyConverter()
		{
			var item = new ChargesSummaryItem("-", 50m, 10m, 100m);
			Assert(item.Convert(new TestCurrencyConverter(Factory), Helper.Currencies["AUD"], Helper.Currencies["USD"]));
			AssertEquals(50m, item.Break);
			AssertEquals(7m, item.Value);
			AssertEquals(70m, item.FlatAmount);

			Assert(!item.Convert(new TestCurrencyConverter(Factory), Helper.Currencies["AUD"], Helper.Currencies["UAH"]));
			AssertEquals(7m, item.Value);
			AssertEquals(70m, item.FlatAmount);

			Assert(item.Convert(new TestCurrencyConverter(Factory), Helper.Currencies["USD"], Helper.Currencies["AUD"]));
			AssertEquals(10m, item.Value);
			AssertEquals(100m, item.FlatAmount);

			Assert(item.Convert(new TestCurrencyConverter(Factory), Helper.Currencies["AUD"], Helper.Currencies["AUD"]));
			AssertEquals(10m, item.Value);
			AssertEquals(100m, item.FlatAmount);
		}

		#region Implementation

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}
}
