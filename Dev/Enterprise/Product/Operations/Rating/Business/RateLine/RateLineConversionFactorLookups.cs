namespace Enterprise.Rating.Business
{
	using System.Linq;
	using CargoWise.Common;
	using Enterprise.Core;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;

	public class RateLineConversionFactorLookups : ConversionFactorLookups
	{
		public RateLineConversionFactorLookups(ConversionFactorViewModel parent, RateLine rateLine)
			: base(parent)
		{
			this.rateLine = Argument.NotNull(rateLine, "rateLine");
		}

		protected override CodeDescriptionPairList GetConversionFactors()
		{
			var unitsSystem = GetUnitsSystem(rateLine.TL_WeightVolume);

			if (Constants.Weight.ContainsCode(rateLine.TL_WeightVolume))
			{
				return new ConversionFactorList(Constants.Weight.Codes, unitsSystem);
			}
			else if (Constants.Volume.ContainsCode(rateLine.TL_WeightVolume))
			{
				return new ConversionFactorList(Constants.Volume.Codes, unitsSystem);
			}
			else if (RatingConstants.Units.IsLoadingMeter(rateLine.TL_WeightVolume))
			{
				return new ConversionFactorList(Constants.LoadingLength.Codes.ToArray(), unitsSystem);
			}
			else
			{
				return new ConversionFactorList(System.Array.Empty<string>(), unitsSystem);
			}
		}

		static UnitsSystem GetUnitsSystem(string unit)
		{
			return Constants.Weight.IsImperial(unit) || Constants.Volume.IsImperial(unit) ? UnitsSystem.Imperial : UnitsSystem.Metric;
		}

		readonly RateLine rateLine;
	}
}

