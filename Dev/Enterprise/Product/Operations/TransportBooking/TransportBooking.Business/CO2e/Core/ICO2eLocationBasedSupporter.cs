using Enterprise.Freight.Integration;

namespace Enterprise.TransportBookings.Business
{
	[ResponseImporter("Enterprise.TransportBookings.DataTransfer.Universal.CO2eLocationBasedResponseImporter, Enterprise.TransportBookings.DataTransfer")]
	public interface ICO2eLocationBasedSupporter : ICO2eCalculationSupporter
	{
	}
}
