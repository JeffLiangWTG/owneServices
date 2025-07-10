using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IGoods
	{
		ZInt BookedPackages { get; }
		ZDecimal BookedWeight { get; }
		ZDecimal BookedVolume { get; }

		ZInt DeliveredPackages { get; }
		ZDecimal DeliveredWeight { get; }
		ZDecimal DeliveredVolume { get; }

		ZString PackagesUnit { get; }
		ZString WeightUnit { get; }
		ZString VolumeUnit { get; }
	}
}
