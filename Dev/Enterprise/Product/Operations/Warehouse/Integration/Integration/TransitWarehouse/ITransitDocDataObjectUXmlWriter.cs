using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Integration.TransitWarehouse
{
	public interface ITransitDocDataObjectUXmlWriter
	{
		ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified);
	}
}
