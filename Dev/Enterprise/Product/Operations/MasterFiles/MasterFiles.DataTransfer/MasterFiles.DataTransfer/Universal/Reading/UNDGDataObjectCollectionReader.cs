using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UNDGDataObjectCollectionReader : DataObjectCollectionReader<UNDG, UNDGDataItem>
	{
		public UNDGDataObjectCollectionReader(IXmlImportLogger logger, UniversalObjectFactory factory, IUNDGDataItemProvider dataObject, UNDG[] undgDataObjects)
			: base(undgDataObjects)
		{
			_logger = logger;
			_factory = factory;
			_dataObject = dataObject;
		}

		readonly UniversalObjectFactory _factory;
		readonly IXmlImportLogger _logger;
		readonly IUNDGDataItemProvider _dataObject;

		protected override void AddToCollection(UNDGDataItem undgItem)
		{
			_dataObject.UNDGs.Add(undgItem);
		}

		protected override UNDGDataItem[] BusinessObjects
		{
			get { return _dataObject.UNDGs.ToArray(); }
		}

		protected override UNDGDataItem FindMatchingBusinessObject(UNDG undgDataObject)
		{
			return null;
		}

		protected override UNDGDataItem ReadIntoBusinessObject(UNDG undgDataObject, UNDGDataItem undgItem)
		{
			return new UNDGDataObjectReader(undgDataObject, _logger, _factory).ReadIntoBusinessObject();
		}

		protected override void RemoveFromCollection(UNDGDataItem undgItem)
		{
			_dataObject.UNDGs.Delete(undgItem);
		}
	}
}
