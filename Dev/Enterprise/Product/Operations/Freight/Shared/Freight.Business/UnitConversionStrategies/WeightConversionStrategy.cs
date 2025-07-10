using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public sealed class WeightConversionStrategy : UnitConversionStrategy
	{
		protected override decimal Convert(decimal magnitude, string oldUnit, string newUnit)
		{
			return Constants.Weight.Convert(magnitude, oldUnit, newUnit);
		}
		protected override string NextLargerUnit(string currentUnit)
		{
			switch (currentUnit)
			{
				case Constants.Weight.Milligrams:
					return Constants.Weight.Grams;
				case Constants.Weight.Grams:
					return Constants.Weight.Kilograms;
				case Constants.Weight.Hectograms:
					return Constants.Weight.Kilograms;
				case Constants.Weight.Kilograms:
					return Constants.Weight.Tonnes;
				case Constants.Weight.Decitons:
					return Constants.Weight.Tonnes;
				case Constants.Weight.Tonnes:
					return Constants.Weight.Kilotonnes;

				case Constants.Weight.Ounces:
					return Constants.Weight.Pounds;
				case Constants.Weight.Pounds:
					return Constants.Weight.ShortTons;

				case Constants.Weight.OuncesTroy:
					return Constants.Weight.PoundsTroy;
				case Constants.Weight.PoundsTroy:
					return Constants.Weight.ShortTons;

				case Constants.Weight.ShortTons:
					return Constants.Weight.LongTons;

				default:
					return null;
			}
		}
	}
}
