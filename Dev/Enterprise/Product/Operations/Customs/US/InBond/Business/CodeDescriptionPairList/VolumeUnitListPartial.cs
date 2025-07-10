namespace Enterprise.Customs.US.InBond.Business
{
	partial class VolumeUnitList
	{
		public static string ConvertFromStandardCode(string code)
		{
			var result = code;
			switch (code)
			{
				case Core.Constants.Volume.CubicDecimetres:
					result = Codes.CubicDecimeters;
					break;
				case Core.Constants.Volume.CubicFeet:
					result = Codes.CubicFeet;
					break;
				case Core.Constants.Volume.CubicInches:
					result = Codes.CubicInches;
					break;
				case Core.Constants.Volume.CubicMetres:
					result = Codes.CubicMeters;
					break;
				case Core.Constants.Volume.Litre:
					result = Codes.Liter;
					break;
			}
			return result;
		}
	}
}
