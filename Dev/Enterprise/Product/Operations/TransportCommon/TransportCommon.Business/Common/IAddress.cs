using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Business
{
	public interface IAddress
	{
		JobDocAddress GetAddress(DocAddressType docAddressType);
	}
}
