using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WeightUnitsTest : TestCase
	{
		public void TestConvertWeightToKilogramsIfRequiredAndGetWeightUOM()
		{
			AssertEquals(200m, WeightUnits.ConvertWeightToKilogramsIfRequired(200, Constants.Weight.Kilograms));
			AssertEquals(WeightUnits.Codes.Kilograms, WeightUnits.GetWeightUOM(Constants.Weight.Kilograms));

			AssertEquals(200m, WeightUnits.ConvertWeightToKilogramsIfRequired(200, Constants.Weight.Pounds));
			AssertEquals(WeightUnits.Codes.Pounds, WeightUnits.GetWeightUOM(Constants.Weight.Pounds));

			AssertEquals(2000m, WeightUnits.ConvertWeightToKilogramsIfRequired(2, Constants.Weight.Tonnes));
			AssertEquals(WeightUnits.Codes.Kilograms, WeightUnits.GetWeightUOM(Constants.Weight.Tonnes));

			AssertEquals(200m, WeightUnits.ConvertWeightToKilogramsIfRequired(200000, Constants.Weight.Grams));
			AssertEquals(WeightUnits.Codes.Kilograms, WeightUnits.GetWeightUOM(Constants.Weight.Grams));
		}
	}
}
