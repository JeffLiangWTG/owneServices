using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class DefaultChargeableCalculationStrategyTest : RatingTestCase
	{
		#region Warehouse PackageLine

		public void TestGetChargeableAmount_WarehousePackageLine_Weight()
			=> AssertGetChargeableAmount_WarehousePackageLine
			(
				rateLineUnit: Weight.Kilograms,
				expectedUnit: Weight.Kilograms,
				expectedMeasureType: MeasureType.WarehousePackageWeight
			);

		public void TestGetChargeableAmount_WarehousePackageLine_Volume()
			=> AssertGetChargeableAmount_WarehousePackageLine
			(
				rateLineUnit: Volume.CubicMetres,
				expectedUnit: Volume.CubicMetres,
				expectedMeasureType: MeasureType.WarehousePackageVolume
			);

		public void TestGetChargeableAmount_WarehousePackageLine_Package()
			=> AssertGetChargeableAmount_WarehousePackageLine
			(
				rateLineUnit: PkgUnit.Basket,
				expectedUnit: PkgUnit.Basket,
				expectedMeasureType: MeasureType.WarehousePackage
			);

		void AssertGetChargeableAmount_WarehousePackageLine(string rateLineUnit, string expectedUnit, MeasureType expectedMeasureType)
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US", "WOUTSTO", 1m, rateLineUnit);
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			var ratingCriteria = new RatingCriteria(null, Factory);
			var parameters = new Mock<AutoRatingCalculatorParametersWithoutFilter>(ratingCriteria, new FreightAutoRater(new RatingContext()));
			parameters.Setup(m => m.CalculateChargeableForWarehouse(rateLine, expectedUnit, expectedMeasureType)).Returns(new Quantity(20m, expectedUnit));

			var amountCalculator = new DefaultChargeableCalculationStrategy(rateLine.Calculator);
			AssertEquals(20m, amountCalculator.GetChargeableAmount(parameters.Object, string.Empty).Amount);
		}

		#endregion

		public void TestGetChargeableAmountWithWeight()
		{
			TestChargeableAmount(QuantityUnit.KG, QuantityUnit.KG);
		}

		public void TestGetChargeableAmountWithVolume()
		{
			TestChargeableAmount(QuantityUnit.M3, QuantityUnit.M3);
		}

		public void TestGetChargeableAmountWithLoadingMeters()
		{
			TestChargeableAmount(QuantityUnit.LM, QuantityUnit.LM);
		}

		#region Implementation

		void TestChargeableAmount(string weightVolume, string unit)
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			rateLine.TL_RateCalculator = FlatCalculator.Code;
			rateLine.TL_WeightVolume = weightVolume;

			var parameters = new Mock<AutoRatingCalculatorParametersWithoutFilter>(ratingCriteria, new FreightAutoRater(new RatingContext()));
			parameters.Setup(m => m.GetChargeableAmount(rateLine)).Returns(new Quantity(20m, unit));

			var amountCalculator = new DefaultChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(20m, amountCalculator.GetChargeableAmount(parameters.Object, string.Empty).Amount);
		}

		#endregion
	}
}
