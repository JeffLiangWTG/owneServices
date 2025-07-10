using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class WeightConversionStrategyTest : UnitConversionStrategyTest<WeightConversionStrategy>
	{
		public void TestMetricConversions()
		{
			AssertUnitSequence(
				Constants.Weight.Milligrams,
				Constants.Weight.Grams,
				Constants.Weight.Kilograms,
				Constants.Weight.Tonnes,
				Constants.Weight.Kilotonnes
			);

			AssertUnitSequence(
				Constants.Weight.Hectograms,
				Constants.Weight.Kilograms
			);

			AssertUnitSequence(
				Constants.Weight.Decitons,
				Constants.Weight.Tonnes
			);
		}

		public void TestImperialConversions()
		{
			AssertUnitSequence(
				Constants.Weight.Ounces,
				Constants.Weight.Pounds,
				Constants.Weight.ShortTons,
				Constants.Weight.LongTons
			);

			AssertUnitSequence(
				Constants.Weight.OuncesTroy,
				Constants.Weight.PoundsTroy,
				Constants.Weight.ShortTons,
				Constants.Weight.LongTons
			);
		}

		#region Implementation

		protected override decimal Convert(decimal fromValue, string fromUnit, string toUnit)
		{
			return Constants.Weight.Convert(fromValue, fromUnit, toUnit);
		}

		#endregion
	}
}
