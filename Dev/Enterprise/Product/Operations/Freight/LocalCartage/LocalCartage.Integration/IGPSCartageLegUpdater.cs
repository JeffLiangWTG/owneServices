using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface IGPSCartageLegUpdater
	{
		void ProcessLocation(IDeviceLocationWithEntity location);
		void UpdateLastProcessedEventTime(ZDateTime dateTime);
	}
}
