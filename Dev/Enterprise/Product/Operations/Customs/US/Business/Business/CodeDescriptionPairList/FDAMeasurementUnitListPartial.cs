using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class FDAMeasurementUnitList
	{
		public static ZDecimal ConvertToInchesWithOneSixteenth(decimal qty, ZString uQ)
		{
			ZDecimal result = 0m;

			if (qty > 0 && !uQ.IsEmpty)
			{
				ZDecimal value = uQ == Codes.Centimeters ?
					Core.Constants.Length.Convert(qty, Core.Constants.Length.Centimetres, Core.Constants.Length.Inches) : qty;

				ZDecimal theOriginalDecimals = (value * 100) % 100;

				ZDecimal theLastTwoDigits = theOriginalDecimals;

				if (uQ != Codes.InchesWithOneSixteenthDecimals)
				{
					theLastTwoDigits = theOriginalDecimals * 0.16m;
				}

				result = value.Truncate(0) + (theLastTwoDigits / 100m);
			}

			return result;
		}
	}
}
