using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface ICMRAddressFormatter
	{
		ZString GetFullAddress(IDocAddress address);
	}
}
