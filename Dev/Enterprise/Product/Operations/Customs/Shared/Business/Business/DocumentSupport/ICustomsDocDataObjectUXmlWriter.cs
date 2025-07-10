using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.Business.Documents.DocDataObjects
{
	public interface ICustomsDocDataObjectUXmlWriter
	{
		ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy dataObjectWriterStrategy, IDocument document);
	}
}
