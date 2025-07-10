using Enterprise.Core;
using Enterprise.Integration.Packing;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgDefaultUnitsTest : PackingTestCaseWithFactory
	{
		public void TestDefaultUnits()
		{
			var defaultUnits1 = (IPackageDefaultUQs)new PkgDefaultUnits();
			AssertEquals(PackingRegistry.Instance.DimensionUnit.Value, defaultUnits1.DefaultDimensionUnit);
			AssertEquals(PackingRegistry.Instance.VolumeUnit.Value, defaultUnits1.DefaultVolumeUnit);
			AssertEquals(PackingRegistry.Instance.WeightUnit.Value, defaultUnits1.DefaultWeightUnit);

			PackingRegistry.Instance.SetDimensionUnitForTest(Constants.Length.Yards);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.TeaChest);
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.MetricCarat);

			var defaultUnits2 = (IPackageDefaultUQs)new PkgDefaultUnits();
			AssertEquals(PackingRegistry.Instance.DimensionUnit.Value, defaultUnits2.DefaultDimensionUnit);
			AssertEquals(PackingRegistry.Instance.VolumeUnit.Value, defaultUnits2.DefaultVolumeUnit);
			AssertEquals(PackingRegistry.Instance.WeightUnit.Value, defaultUnits2.DefaultWeightUnit);
			AssertEquals(Constants.Length.Yards, defaultUnits2.DefaultDimensionUnit);
			AssertEquals(Constants.Volume.TeaChest, defaultUnits2.DefaultVolumeUnit);
			AssertEquals(Constants.Weight.MetricCarat, defaultUnits2.DefaultWeightUnit);
		}
	}
}
