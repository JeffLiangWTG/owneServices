using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class HRCChargeableCalculationStrategyTest : RatingTestCase
	{
		public void TestGetChargeableAmount_CustomDeclaration_EmptyMeasures()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.AdapterType = AdapterType.CustomsDeclaration;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			AssertEquals("Precondition: empty measure", 0, measures.MeasureTypeCount);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.TL_TI = rateEntry.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.KG;

			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var chargeableAmountCalculator = new HRCChargeableCalculationStrategy(calculator);
			AssertEquals(0m, chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
			AssertNullOrEmpty("GIVEN HRC calculator with empty measure WHEN autorate customDeclaration THEN should not throw KeyNotFoundException", ErrorReporter.LastMessageReported);
		}

		public void TestGetChargeableAmountForWeight()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;

			var parts = new RateablePartList { WeightUnit = "KG" };
			parts.AddPart(new RateablePart { WeightMeasure = new ClientProviderValues(34, 20, 14) });
			measures.AddPartList(MeasureType.Weight, parts);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_TI = rateEntry.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.KG;
			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var chargeableAmountCalculator = new HRCChargeableCalculationStrategy(calculator);
			calculatorParameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			rateLine.UseOnlyActualWeightMeasure = false;
			AssertEquals(20m, chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			rateLine.UseOnlyActualWeightMeasure = true;
			AssertEquals(34m, chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmountForVolume()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;

			var parts = new RateablePartList { VolumeUnit = "M3" };
			parts.AddPart(new RateablePart { Volume = 34 });
			measures.AddPartList(MeasureType.Volume, parts);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.UseOnlyActualWeightMeasure = true;
			rateLine.TL_TI = rateEntry.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.M3;
			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var chargeableAmountCalculator = new HRCChargeableCalculationStrategy(calculator);
			calculatorParameters.AddLineMeasureMatch(MeasureType.Volume, rateLine, 0);

			AssertEquals(34m, chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmount_WhenMeasureTypeIsInvalid_DoesNotThrowException()
		{
			try
			{
				var ratingCriteria = new RatingCriteria(null, Factory);
				ratingCriteria.ValuesCanBeSet = true;
				var measures = new RateableMeasureSet(AdapterType.Shipment);
				ratingCriteria.RateableMeasures = measures;

				var parts = new RateablePartList { WeightUnit = "KG" };
				parts.AddPart(new RateablePart { Weight = 34 });
				measures.AddPartList(MeasureType.Weight, parts);

				var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

				var clientRate = Factory.New<ClientRate>();
				var rateEntry = clientRate.AddRateEntry("AIR");
				var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "INV");
				rateLine.UseOnlyActualWeightMeasure = true;
				var calculator = CalculatorFactory.GetCalculator(rateLine);
				var chargeableAmountCalculator = new HRCChargeableCalculationStrategy(calculator);

				AssertNoExceptionThrown("Unable to load measure for unit: INV", () => chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, string.Empty));
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestGetChargeableAmount_WhenNoMeasures_Return0()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.RateableMeasures = new RateableMeasureSet(AdapterType.Shipment);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine("ODOC", HighestRateCalculator.Code, QuantityUnit.KG);
			rateLine.UseOnlyActualWeightMeasure = true;

			var rateLineItem = rateLine.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			rateLineItem.TM_BreakWeightVolume = QuantityUnit.KG;
			rateLineItem.TM_RelevantValue = 10m;

			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var chargeableAmountCalculator = new HRCChargeableCalculationStrategy(calculator);

			AssertEquals(0m, chargeableAmountCalculator.GetChargeableAmount(calculatorParameters, QuantityUnit.KG).Amount);
		}
	}
}
