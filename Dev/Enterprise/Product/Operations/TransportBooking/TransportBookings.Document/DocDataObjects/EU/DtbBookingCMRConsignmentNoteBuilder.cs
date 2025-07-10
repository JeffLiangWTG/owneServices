using CargoWise.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Document
{
	public class DtbBookingCMRConsignmentNoteBuilder : ICMRConsignmentNoteBuilder
	{
		readonly DtbBooking booking;
		readonly IDocDataObjectParameters parameters;

		public DtbBookingCMRConsignmentNoteBuilder(DtbBooking booking, IDocDataObjectParameters parameters)
		{
			this.booking = Argument.NotNull(booking, nameof(booking));
			this.parameters = Argument.NotNull(parameters, nameof(parameters));
		}

		public CMRConsignmentNoteDocDataObjectCollection Build()
		{
			var bookingCmr = new DtbBookingCMRConsignmentNote(booking, parameters);
			return new CMRConsignmentNoteDocDataObjectCollection([new CMRConsignmentNoteDocDataObject(bookingCmr)]);
		}
	}
}
