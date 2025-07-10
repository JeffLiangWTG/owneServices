using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.TransportBookings.Document
{
	public interface IDtbBookingDocDataObjectProvider
	{
		object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters);
	}
}
