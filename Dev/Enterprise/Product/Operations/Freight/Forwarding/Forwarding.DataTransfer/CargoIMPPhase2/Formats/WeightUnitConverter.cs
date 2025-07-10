
namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class WeightUnitConverter : ListConverter<WeightUnits, QuantitiesWeightUnitList>
	{
		public override QuantitiesWeightUnitList Convert(WeightUnits data, FormattingResult formattingResult)
		{
			QuantitiesWeightUnitList result = null;
			switch (data)
			{
				case WeightUnits.Kilograms:
					result = QuantitiesWeightUnitList.Kilograms;
					break;
				case WeightUnits.Pounds:
					result = QuantitiesWeightUnitList.Pounds;
					break;
			}

			return result;
		}
	}
}
