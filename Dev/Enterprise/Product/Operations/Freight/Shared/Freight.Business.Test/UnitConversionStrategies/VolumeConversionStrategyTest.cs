using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VolumeConversionStrategyTest : UnitConversionStrategyTest<VolumeConversionStrategy>
	{
		public void TestMetricConversion()
		{
			AssertUnitSequence(
				Constants.Volume.Litre,
				Constants.Volume.CubicMetres,
				Constants.Volume.MegaLitre
			);

			AssertUnitSequence(
				Constants.Volume.CubicDecimetres,
				Constants.Volume.CubicMetres
			);
		}

		public void TestImperialConversions()
		{
			AssertUnitSequence(
				Constants.Volume.CubicInches,
				Constants.Volume.CubicFeet,
				Constants.Volume.CubicYards
			);
		}

		public void TestMiscConversions()
		{
			AssertUnitSequence(
				Constants.Volume.TeaChest,
				Constants.Volume.CubicMetres
			);
		}

		#region Implementation

		protected override decimal Convert(decimal fromValue, string fromUnit, string toUnit)
		{
			return Constants.Volume.Convert(fromValue, fromUnit, toUnit);
		}

		#endregion
	}
}
