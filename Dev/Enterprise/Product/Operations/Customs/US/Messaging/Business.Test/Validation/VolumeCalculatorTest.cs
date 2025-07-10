using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	class VolumeCalculatorTest : TestCase
	{
		public void TestCalculate()
		{
			var validator = new VolumeValidator();
			AssertEquals(10m, VolumeCalculator.Calculate(new ZVolume(10m, Core.Constants.Volume.CubicYards)));
			AssertEquals(10m, VolumeCalculator.Calculate(new ZVolume(10m, Core.Constants.Volume.CubicInches)));
			AssertEquals(10m, VolumeCalculator.Calculate(new ZVolume(10m, Core.Constants.Volume.CubicFeet)));
			AssertEquals(10m, VolumeCalculator.Calculate(new ZVolume(10m, Core.Constants.Volume.CubicMetres)));
			AssertEquals(10m, VolumeCalculator.Calculate(new ZVolume(10m, "Z!")));
			AssertEquals(validator.MaximumWholeVolumeAllowed, VolumeCalculator.Calculate(new ZVolume(validator.MaximumWholeVolumeAllowed, Core.Constants.Volume.CubicMetres)));
			AssertEquals(ZDecimal.Zero, VolumeCalculator.Calculate(new ZVolume(validator.MaximumWholeVolumeAllowed + 1, Core.Constants.Volume.CubicMetres)));
		}
	}
}
