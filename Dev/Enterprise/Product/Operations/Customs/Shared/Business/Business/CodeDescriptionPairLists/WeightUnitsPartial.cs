using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Customs.Business
{
	public partial class WeightUnits
	{
		public static ZDecimal ConvertWeightToKilogramsIfRequired(ZDecimal cargoGrossWeight, ZString weightUnitOfMeasure)
		{
			switch (weightUnitOfMeasure)
			{
				case Constants.Weight.Pounds:
				case Constants.Weight.Kilograms:
					return cargoGrossWeight;
				default:
					return Constants.Weight.Convert(cargoGrossWeight, weightUnitOfMeasure, Constants.Weight.Kilograms);
			}
		}

		public static ZString GetWeightUOM(ZString weightUnitOfMeasure)
		{
			switch (weightUnitOfMeasure)
			{
				case Constants.Weight.Pounds:
					return Codes.Pounds;
				default:
					return Codes.Kilograms;
			}
		}
	}
}
