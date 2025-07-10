using CargoWise.Types;
using Enterprise.Integration.Packing;

namespace Enterprise.Packing.Business
{
	public class PkgDefaultUnits : IPackageDefaultUQs
	{
		ZString IPackageDefaultUQs.DefaultDimensionUnit
		{
			get { return PackingRegistry.Instance.DimensionUnit.Value; }
		}

		ZString IPackageDefaultUQs.DefaultVolumeUnit
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		ZString IPackageDefaultUQs.DefaultWeightUnit
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}
	}
}

