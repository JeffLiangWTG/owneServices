using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Business
{
	public interface IDtbTransport : IBusiness
	{
		IJobHeader Job { get; }
	}
}
