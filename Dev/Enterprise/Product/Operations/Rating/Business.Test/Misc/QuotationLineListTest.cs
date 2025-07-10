using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business
{
	sealed class QuotationLineListTest : TestCaseWithFactory
	{
		public void TestAddDoesNotActuallyAddNullLines()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			var lines = new QuotationLineList();

			AssertEquals("Count", 0, lines.Count);

			lines.Add(QuotationLine.New(rateLine, "Test"));
			AssertEquals("Count", 1, lines.Count);

			lines.Add(null);
			AssertEquals("Count", 1, lines.Count);

			lines.Add(QuotationLine.New(rateLine, "Test"));
			AssertEquals("Count", 2, lines.Count);
		}
	}
}
