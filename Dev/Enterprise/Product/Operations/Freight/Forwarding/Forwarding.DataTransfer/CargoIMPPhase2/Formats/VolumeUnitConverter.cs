namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class VolumeUnitConverter : ListConverter<VolumeUnits, QuantitiesVolumeUnitList>
	{
		public override QuantitiesVolumeUnitList Convert(VolumeUnits data, FormattingResult formattingResult)
		{
			QuantitiesVolumeUnitList result = null;
			switch (data)
			{
				case VolumeUnits.CubicMetres:
					result = QuantitiesVolumeUnitList.CubicMetres;
					break;
				case VolumeUnits.CubicFeet:
					result = QuantitiesVolumeUnitList.CubicFeet;
					break;
			}

			return result;
		}
	}
}
