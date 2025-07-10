using System.Collections.Generic;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public interface ITransportBookingTestData
	{
		IEnumerable<IDtbBooking> CreateTransportBookings();

		IDtbBooking DomesticFCLTransportBooking { get; }
		IDtbBooking DomesticLCLTransportBooking { get; }
		IDtbBooking DomesticMixedTransportBooking { get; }

		IDtbBooking ImportFCLTransportBooking { get; }
		IDtbBooking ImportLCLTransportBooking { get; }
		IDtbBooking ImportMixedTransportBooking { get; }

		IDtbBooking ExportFCLTransportBooking { get; }
		IDtbBooking ExportLCLTransportBooking { get; }
		IDtbBooking ExportMixedTransportBooking { get; }
	}
}
