namespace Enterprise.Rating.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Business;

	public class RateLineConversionFactorValidation : ConversionFactorValidation
	{
		public RateLineConversionFactorValidation(ConversionFactorViewModel parent, RateLine rateLine)
			: base(parent)
		{
			this.rateLine = Argument.NotNull(rateLine, "rateLine");
		}

		protected override void CheckConversionFactorString()
		{
			if (!Parent.ConversionFactorString.IsEmpty)
			{
				base.CheckConversionFactorString();
			}
			else if (!rateLine.UseOnlyActualWeightMeasure)
			{
				if (QuantityUnit.IsLoadingMeter(rateLine.TL_WeightVolume))
				{
					Parent.ConversionFactorStringInfo.AddError(ErrorMessages.ConversionFactorMustBeEntered);
				}
			}
		}

		protected override IEnumerable<UnitsList> GetSupportedUnits()
		{
			return new[]
			{
				new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes),
				new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes),
				new UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
			};
		}

		readonly RateLine rateLine;
	}
}

