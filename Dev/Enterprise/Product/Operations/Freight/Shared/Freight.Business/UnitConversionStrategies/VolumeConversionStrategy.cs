using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public sealed class VolumeConversionStrategy : UnitConversionStrategy
	{
		protected override decimal Convert(decimal magnitude, string oldUnit, string newUnit)
		{
			return Constants.Volume.Convert(magnitude, oldUnit, newUnit);
		}
		protected override string NextLargerUnit(string currentUnit)
		{
			switch (currentUnit)
			{
				case Constants.Volume.CubicDecimetres:
				case Constants.Volume.Litre:
					return Constants.Volume.CubicMetres;
				case Constants.Volume.CubicMetres:
					return Constants.Volume.MegaLitre;

				case Constants.Volume.CubicInches:
					return Constants.Volume.CubicFeet;
				case Constants.Volume.CubicFeet:
					return Constants.Volume.CubicYards;

				case Constants.Volume.TeaChest:
					return Constants.Volume.CubicMetres;

				default:
					return null;
			}
		}
	}
}
