using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public abstract class UnitConversionStrategy
	{
		public void ReScale(ref ZDecimal magnitude, ref ZString unit, int precision, int scale)
		{
			ZDecimal newMagnitude = magnitude;
			ZString newUnit = unit;

			while (!newMagnitude.IsWithinSqlPrecisionAndScale(precision, scale))
			{
				string proposedUnit = NextLargerUnit(newUnit);

				if (proposedUnit == null)
				{
					break;
				}

				newMagnitude = Convert(magnitude, unit, proposedUnit);
				newUnit = proposedUnit;
			}

			magnitude = newMagnitude;
			unit = newUnit;
		}

		protected abstract decimal Convert(decimal magnitude, string oldUnit, string newUnit);
		protected abstract string NextLargerUnit(string currentUnit);
	}
}
