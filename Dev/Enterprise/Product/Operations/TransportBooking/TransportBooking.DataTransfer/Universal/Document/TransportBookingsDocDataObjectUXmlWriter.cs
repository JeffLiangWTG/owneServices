
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.TransportBookings.Document;
using Enterprise.UniversalDataBuss.Integration;
using DataContext = Enterprise.TransportBookings.Document.DataContext;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class TransportBookingsDocDataObjectUXmlWriter : ITransportBookingsDocDataObjectUXmlWriter
	{
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			var manager = new TransportBookingsDocDataWritingManager(writerStrategy);
			var docDataObject = document?.Data?.Value;
			var dataContext = document?.DataContext ?? string.Empty;

			switch (dataContext)
			{
				case DataContext.FFMMessageRequest:
					if (docDataObject is FFMMessage ffmMessage)
					{
						var writer = new FFMMessageDataObjectWriter(manager);
						return writer.GetDataObject(ffmMessage);
					}
					break;
			}
			return null;
		}
	}
}
