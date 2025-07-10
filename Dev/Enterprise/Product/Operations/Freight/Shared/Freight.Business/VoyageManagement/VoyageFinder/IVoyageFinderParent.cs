using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IVoyageFinderParent
	{
		ZString TransportMode { get; }
		ZString LoadPort { get; }
		ZString DischargePort { get; }
		ZGuid CarrierPK { get; }
	}
}
