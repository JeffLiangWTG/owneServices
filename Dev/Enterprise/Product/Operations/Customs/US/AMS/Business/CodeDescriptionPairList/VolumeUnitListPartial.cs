using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	partial class VolumeUnitList
	{
		public static ZString ConvertFromFreightVolume(string unit)
		{
			switch (unit)
			{
				case Core.Constants.Volume.CubicFeet:
					return Codes.CubicFeet;
				case Core.Constants.Volume.CubicMetres:
					return Codes.CubicMeters;
				default:
					return Core.Constants.Volume.ContainsCode(unit) ? Codes.CubicMeters : unit;
			}
		}
	}
}
