using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackageSummary
	{
		ZString DimensionsCaption { get; }
		ZString Dimensions { get; }

		ZString PackageIDCaption { get; }
		ZString PackageID { get; }

		ZString WeightCaption { get; }
		ZString Weight { get; }

		ZString VolumeCaption { get; }
		ZString Volume { get; }

		ZString Contents { get; }
		ZString Status { get; }
		bool IsStatusSignificant { get; }

		ZString TemperatureCaption { get; }
		ZString Temperature { get; }
		bool IsTemperatureControlled { get; }

		ZString CommodityCaption { get; }
		ZString Commodity { get; }

		ZString PackageSequenceCaption { get; }
		ZString PackageSequence { get; }

		ZString IsHeldCaption { get; }
		ZString IsHeld { get; }

		ZString HandlingUnitCaption { get; }
		ZString HandlingUnit { get; }
	}
}
