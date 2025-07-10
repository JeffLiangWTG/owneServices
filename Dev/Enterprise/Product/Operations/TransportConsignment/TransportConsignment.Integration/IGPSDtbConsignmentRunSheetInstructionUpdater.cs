using Enterprise.Telematics.Integration;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IGPSDtbConsignmentRunSheetInstructionUpdater
	{
		void ProcessLocation(IDeviceLocationWithEntity location);
	}
}
