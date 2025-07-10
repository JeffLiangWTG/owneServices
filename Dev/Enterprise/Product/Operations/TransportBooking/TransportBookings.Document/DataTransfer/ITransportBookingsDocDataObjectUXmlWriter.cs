using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.Document
{
	public interface ITransportBookingsDocDataObjectUXmlWriter
	{
		ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified);
	}
}
