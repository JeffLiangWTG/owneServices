using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Integration
{
	public interface ICO2eLegProvider : ICO2eProvider
	{
		ZDecimal DynamicTotalCO2e { get; set; }
		ZString LoadPort { get; }
		ZString DiscPort { get; }
		ZString VoyageFlight { get; }
		ZString AircraftType { get; }
		ZString TransportMode { get; }
		ZString VesselLloydsNumber { get; }
		OrgHeader Carrier { get; }
		ICO2eLegBasedSupporter CurrentCO2eCalcSupporter { get; set; }
	}
}
