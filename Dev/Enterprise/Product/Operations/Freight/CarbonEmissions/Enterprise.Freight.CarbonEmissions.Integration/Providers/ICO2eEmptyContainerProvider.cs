using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Integration
{
	public delegate (OrgAddress from, OrgAddress to, string transportMode) EmptyContainerAddressResolver(ICommonShipment shipment);

	public interface ICO2eEmptyContainerProvider : ICO2eTEUProvider
	{
		ZString ConsolNumber { get; }
		ZString ContainerNumber { get; }
		ZString ContainerJobID { get; }
		ZDecimal TotalWeight { get; }
		ZString TotalWeightUnit { get; }

		EmptyContainerAddressResolver EmptyPickupAddress { get; }
		EmptyContainerAddressResolver EmptyReturnAddress { get; }
	}
}
