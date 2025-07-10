using System.Collections.Generic;
using CargoWise.Types;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Common.Business
{
	public interface ICartageLooseCargo
	{
		ZInt BookedPackages { get; }
		ZString BookedPackType { get; }
		ZDecimal BookedWeight { get; }
		ZString BookedWeightUnit { get; }
		ZDecimal BookedVolume { get; }
		ZString BookedVolumeUnit { get; }
		ZDecimal BookedHeight { get; }
		ZDecimal BookedLength { get; }
		ZDecimal BookedWidth { get; }
		ZString BookedDimensionUnit { get; }
		IReadOnlyCollection<UNDGDataItem> DangerousGoods { get; }
	}
}
