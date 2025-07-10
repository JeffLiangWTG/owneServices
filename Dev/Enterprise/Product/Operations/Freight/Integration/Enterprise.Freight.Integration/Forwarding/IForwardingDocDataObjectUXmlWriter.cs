using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IForwardingDocDataObjectUXmlWriter
	{
		ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified);
	}
}
