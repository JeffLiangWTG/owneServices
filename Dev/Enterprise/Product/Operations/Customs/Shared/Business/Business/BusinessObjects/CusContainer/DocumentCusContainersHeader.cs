using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Summary description for DocumentCusContainerCollectionHeader.
	/// </summary>
	public class DocumentCusContainerCollectionHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentCusContainerCollectionHeader(DocumentCusContainerCollection documentCusContainerCollection) : base(documentCusContainerCollection.Factory)
		{
			fDocumentCusContainerCollection = documentCusContainerCollection;
		}

		public DocumentCusContainerCollection DocumentCusContainerCollection
		{
			get { return fDocumentCusContainerCollection; }
		}
		readonly DocumentCusContainerCollection fDocumentCusContainerCollection;
	}
}
