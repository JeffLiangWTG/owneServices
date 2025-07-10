using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IConsolOrShipment
	{
		bool IsInDatabase { get; }
		ZString HumanReadableName { get; }
	}
}
