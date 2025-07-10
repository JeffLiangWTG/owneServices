using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportCommon.Registry
{
	public sealed class TransportBookingRegistryProvider : ITransportBookingRegistryProvider
	{
		public IRegistryItem TransportBookingChargeableFactor => TransportRegistry.Instance.TransportBookingChargeableFactor;
	}
}
