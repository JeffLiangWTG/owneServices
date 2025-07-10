using CargoWise.Types;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Security;

namespace Enterprise.Freight.Integration
{
	public interface IDistanceCalculationConsumer
	{
		SecurityCheckpoint Checkpoint { get; }

		ZDecimal Distance { get; set; }
		ZString DistanceUnit { get; set; }
		DistanceCalculationConfiguration DistanceCalculationConfig { get; }
		DistanceCalculationAddress OriginAddress { get; }
		DistanceCalculationAddress DestinationAddress { get; }
	}
}
