using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LCMarginPercentFallBackCalculatorTest : TestCaseWithFactory
	{
		public void TestGetMarginPercentagesForConsigneeMarkUp()
		{
			_ = LCHeaderWithConsigneeMarkUps;

			var calculator = new LCMarginPercentFallBackCalculator(lcHistory);
			var result = calculator.GetLCMarginPercentagesForFallBack();

			AssertEquals("Mark-up %1", 10m, result.LCMarginPercentage1);
			AssertEquals("Mark-up %2", 15m, result.LCMarginPercentage2);
			AssertEquals("Mark-up %3", 17m, result.LCMarginPercentage3);
		}

		public void TestGetMarginPercentageForProductMarkUp()
		{
			_ = LCHeaderWithProductMarkUps;

			var calculator = new LCMarginPercentFallBackCalculator(lcHistory);
			var result = calculator.GetLCMarginPercentagesForFallBack();

			AssertEquals("Mark-up %1", 20m, result.LCMarginPercentage1);
			AssertEquals("Mark-up %2", 25m, result.LCMarginPercentage2);
			AssertEquals("Mark-up %3", 27m, result.LCMarginPercentage3);
		}

		public void TestGetMarginPercentageForLCHistoryMarkup()
		{
			_ = LCHeaderWithHistoryMarkUps;

			var calculator = new LCMarginPercentFallBackCalculator(lcHistory);
			var result = calculator.GetLCMarginPercentagesForFallBack();

			AssertEquals("Mark-up %1", 30m, result.LCMarginPercentage1);
			AssertEquals("Mark-up %2", 35m, result.LCMarginPercentage2);
			AssertEquals("Mark-up %3", 37m, result.LCMarginPercentage3);
		}

		public void TestGetMarginPercentageForNoneDefined()
		{
			var lCHeader = Factory.New<LandedCostHeader>();
			var lCHistory = lCHeader.Histories.AddNew();

			var calculator = new LCMarginPercentFallBackCalculator(lCHistory);
			var result = calculator.GetLCMarginPercentagesForFallBack();

			AssertEquals("None defined", 0m, result.LCMarginPercentage1);
			AssertEquals("None defined", 0m, result.LCMarginPercentage2);
			AssertEquals("None defined", 0m, result.LCMarginPercentage3);
		}

		TestHelper helper;
		LandedCostHistory lcHistory;
		OrgHeader consignee;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestHelper(Factory);
		}

		LandedCostHeader LCHeaderWithConsigneeMarkUps
		{
			get
			{
				var result = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

				consignee = Factory.New<OrgHeader>();
				consignee.MiscServ.OM_LandedCostMarginPercent1 = 10m;
				consignee.MiscServ.OM_LandedCostMarginPercent2 = 15m;
				consignee.MiscServ.OM_LandedCostMarginPercent3 = 17m;

				helper.DummyHeader.ConsigneeExposed = consignee;

				lcHistory = result.Histories.AddNew();
				lcHistory.UltimateDistributee = helper.Ultimate1;

				return result;
			}
		}

		LandedCostHeader LCHeaderWithProductMarkUps
		{
			get
			{
				var result = LCHeaderWithConsigneeMarkUps;

				var marginPertange = new LCMarginPercentages();
				marginPertange.LCMarginPercentage1 = 20m;
				marginPertange.LCMarginPercentage2 = 25m;
				marginPertange.LCMarginPercentage3 = 27m;

				helper.Ultimate1.ProductSpecificLCMarginPercentageExposed = marginPertange;

				return result;
			}
		}

		LandedCostHeader LCHeaderWithHistoryMarkUps
		{
			get
			{
				var result = LCHeaderWithProductMarkUps;

				lcHistory.LH_LandedCostMarginPercent1 = 30m;
				lcHistory.LH_LandedCostMarginPercent2 = 35m;
				lcHistory.LH_LandedCostMarginPercent3 = 37m;

				return result;
			}
		}
	}
}
