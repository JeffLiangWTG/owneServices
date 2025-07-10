using Enterprise.DocumentVisualizer.Integration;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Document
{
	sealed public class DtbBookingDocDataObjectProvider : IDtbBookingDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (parent is DtbBooking booking && dataContext == DataContext.CMRConsignmentNote)
			{
				return new DtbBookingCMRConsignmentNoteBuilder(booking, parameters).Build();
			}

			return null;
		}
	}
}
