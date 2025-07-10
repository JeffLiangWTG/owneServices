using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business.Testing
{
	public class DistanceChargeableCalculationStrategyTest : RatingTestCase
	{
		public void TestGetChargableAmountKM()
		{
			var rateLine = GetRateLine(QuantityUnit.KM);
			var amountCalculator = new DistanceChargeableCalculationStrategy(rateLine.Calculator);

			var parameters = GetParameters(QuantityUnit.KM, rateLine.ChargeCode);
			var actualResult = amountCalculator.GetChargeableAmount(parameters, QuantityUnit.KM);

			AssertEquals(20m, actualResult.Amount);
		}

		public void TestGetChargableAmountMI()
		{
			var rateLine = GetRateLine(QuantityUnit.KM);
			var amountCalculator = new DistanceChargeableCalculationStrategy(rateLine.Calculator);

			var parameters = GetParameters(QuantityUnit.KM, rateLine.ChargeCode);
			var actualResult = amountCalculator.GetChargeableAmount(parameters, QuantityUnit.MI);

			AssertEquals(12.4m, actualResult.Amount, 3);
		}

		RateLine GetRateLine(ZString unit)
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");

			return rateEntry.AddRateLine("OCART", UnitCalculator.Code, unit);
		}

		AutoRatingCalculatorParameters GetParameters(ZString unit, AccChargeCode chargeCode)
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetPickupDistance(20m, unit);
			ratingCriteria.RateableMeasures = measures;

			var result = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			result.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			return result;
		}
	}
}
