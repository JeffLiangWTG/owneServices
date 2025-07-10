using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SupportingDocumentCollection : NonPersistentBusinessObjectCollection<SupportingDocument>
	{
		public SupportingDocumentCollection(BusinessObjectFactory factory, string msgType)
			: base(factory)
		{
			this.msgType = msgType;
			MaxCountValidationEnable(10);
		}
		readonly string msgType;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SupportingDocument(Factory, msgType);
		}

		#region StorageDocs

		public StorageDocList StorageDocs
		{
			get { return storageDocs ?? (storageDocs = new StorageDocList()); }
		}
		StorageDocList storageDocs;

		public void SetStorageDocs(IStorageDocsBaseCollection storageDocsCollection)
		{
			StorageDocs.SetupDocList(storageDocsCollection);
		}

		#endregion
	}
}
