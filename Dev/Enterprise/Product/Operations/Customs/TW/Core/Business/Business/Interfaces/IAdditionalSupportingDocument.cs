using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business.Business
{
	public interface IAdditionalSupportingDocument
	{
		public SupportingDocumentCollection GetSupportingDocuments();
		public IStorageDocsBaseCollection[] GetAllEDocs();
	}
}
