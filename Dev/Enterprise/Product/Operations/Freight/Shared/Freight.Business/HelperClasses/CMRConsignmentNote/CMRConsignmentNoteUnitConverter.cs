using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Business
{
	public static class CMRConsignmentNoteUnitConverter
	{
		public static ZDecimal WeightInKilograms(ZDecimal weight, ZString unit)
		{
			return new ZWeight(weight, unit).InKilogramsSafe;
		}

		public static ZDecimal VolumeInCubicMeters(ZDecimal volume, ZString unit)
		{
			return new ZVolume(volume, unit).InCubicMetres;
		}
	}
}
