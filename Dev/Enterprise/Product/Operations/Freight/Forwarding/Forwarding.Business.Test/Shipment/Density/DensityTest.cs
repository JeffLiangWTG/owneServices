using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DensityTest : TestCaseWithFactory
	{
		public void TestRefreshAllValuesSetPropertiesToNAWhenIsChargeableByWeightAndTotalWeightIsNotPositive()
		{
			var dummyDensity = new DummyDensity();
			dummyDensity.isRefreshAllowed = true;
			dummyDensity.isChargeableByWeight = true;
			dummyDensity.totalVolume = 1m;
			dummyDensity.calculatedVolumeWeight = 1m;

			dummyDensity.totalWeight = ZDecimal.Zero;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);

			dummyDensity.totalWeight = -1m;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);
		}

		public void TestRefreshAllValuesSetPropertiesToNAWhenIsChargeableByWeightAndCalculatedVolumeWeightIsNegative()
		{
			var dummyDensity = new DummyDensity();
			dummyDensity.isRefreshAllowed = true;
			dummyDensity.isChargeableByWeight = true;
			dummyDensity.totalWeight = 1m;
			dummyDensity.totalVolume = 1m;

			dummyDensity.calculatedVolumeWeight = ZDecimal.Zero;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, 0m);
			AssertEquals(dummyDensity.VolumeRatio, "1:1");
			AssertEquals(dummyDensity.DensityRemark, "Dense ++++");

			dummyDensity.calculatedVolumeWeight = -1m;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);
		}

		public void TestRefreshAllValuesSetPropertiesToNAWhenIsNotChargeableByWeightAndTotalVolumeIsNegative()
		{
			var dummyDensity = new DummyDensity();
			dummyDensity.isRefreshAllowed = true;
			dummyDensity.isChargeableByWeight = false;
			dummyDensity.totalWeight = 1m;
			dummyDensity.calculatedVolumeWeight = 1m;

			dummyDensity.totalVolume = ZDecimal.Zero;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, 0m);
			AssertEquals(dummyDensity.VolumeRatio, "1:1");
			AssertEquals(dummyDensity.DensityRemark, "Dense ++++");

			dummyDensity.totalVolume = -1m;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);
		}

		public void TestRefreshAllValuesSetPropertiesToNAWhenIsNotChargeableByWeightAndCalculatedVolumeWeightIsNotPositive()
		{
			var dummyDensity = new DummyDensity();
			dummyDensity.isRefreshAllowed = true;
			dummyDensity.isChargeableByWeight = false;
			dummyDensity.totalWeight = 1m;
			dummyDensity.totalVolume = 1m;

			dummyDensity.calculatedVolumeWeight = ZDecimal.Zero;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);

			dummyDensity.calculatedVolumeWeight = -1m;
			AssertNoExceptionThrown(() => dummyDensity.RefreshAllValues());
			AssertEquals(dummyDensity.DensityFactor, Density.DensityFactorForZeroDivision);
			AssertEquals(dummyDensity.VolumeRatio, Density.NAValueForZeroDivision);
			AssertEquals(dummyDensity.DensityRemark, Density.NAValueForZeroDivision);
		}
	}
}
