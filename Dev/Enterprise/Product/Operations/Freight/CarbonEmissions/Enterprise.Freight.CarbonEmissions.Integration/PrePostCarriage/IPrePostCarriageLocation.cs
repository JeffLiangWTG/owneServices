using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Integration;

public interface IPrePostCarriageLocation
{
	public ISupportWebAddressValidation Address { get; }

	public ZString UNLOCO { get; }

	public bool IsPort { get; }

	public bool IsEmpty { get; }

	public string TransportMode { get; }
}
