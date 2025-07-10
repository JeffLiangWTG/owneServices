using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class UnitConversionStrategyTest<T> : TestCase
			where T : UnitConversionStrategy, new()
	{
		public void AssertUnitSequence(params string[] units)
		{
			for (int i = 1; i < units.Length; i++)
			{
				ZDecimal magnitude = 1000m;
				ZString unit = units[i - 1];

				Strategy.ReScale(ref magnitude, ref unit, 6, 3);

				AssertEquals("unit", units[i], unit);
				AssertEquals("magnitude", Convert(1000, units[i - 1], units[i]), magnitude, 0.001m);
			}
		}

		#region Implementation

		protected abstract decimal Convert(decimal fromValue, string fromUnit, string toUnit);

		protected T Strategy
		{
			get { return strategy ?? (strategy = new T()); }
		}
		T strategy;

		#endregion
	}
}
