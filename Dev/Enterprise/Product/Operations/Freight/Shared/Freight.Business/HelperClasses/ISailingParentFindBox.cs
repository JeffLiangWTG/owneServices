using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface ISailingParentFindBox
	{
		BusinessObjectFactory Factory { get; }
		ZString LoadPort { get; }
		ZString DischargePort { get; }
		ZString Origin { get; }
		ZString Destination { get; }
		ZGuid SailingPK { get; set; }
		ZString TransportMode { get; }
	}
}
