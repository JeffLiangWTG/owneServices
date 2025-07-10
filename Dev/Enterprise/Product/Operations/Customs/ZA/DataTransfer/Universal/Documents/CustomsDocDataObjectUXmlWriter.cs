using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.ZA;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.UniversalDataBuss.Integration;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Documents
{
	public class CustomsDocDataObjectUXmlWriter : ICustomsDocDataObjectUXmlWriter
	{
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy strategy, IDocument document)
		{
			var dataContext = document?.DataContext ?? string.Empty;
			var docDataObject = document?.Data?.Value as DocDataObject;

			switch (dataContext)
			{
				case DataContext.CargoDuesBrokerage:
					if (docDataObject is CargoDues cargoDues)
					{
						var manager = new DataWritingManager(strategy);
						var writer = new CargoDuesDataObjectWriter(manager);

						return writer.GetDataObject(cargoDues);
					}
					break;
			}

			return null;
		}
	}
}
