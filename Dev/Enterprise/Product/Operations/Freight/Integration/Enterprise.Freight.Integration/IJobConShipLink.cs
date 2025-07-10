using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobConShipLink
	{
		ZGuid JN_JK { get; set; }
		ZGuid JN_JS { get; set; }
	}
}
