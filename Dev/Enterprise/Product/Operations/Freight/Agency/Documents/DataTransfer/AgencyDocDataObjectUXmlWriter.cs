using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.Documents.DataTransfer
{
	public class AgencyDocDataObjectUXmlWriter : IAgencyDocDataObjectUXmlWriter
	{
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy writerStrategy, IDocument document, MessageType messageType = MessageType.Unspecified)
		{
			return null;
		}
	}
}
