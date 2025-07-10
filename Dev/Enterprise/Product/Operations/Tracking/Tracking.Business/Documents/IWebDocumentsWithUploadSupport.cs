using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public interface IWebDocumentsWithUploadSupport : IWebDocumentsSupport
	{
		DocumentUploadSupport DocumentUploadHelper { get; }
		DocManagerInfo DocManagerInfo { get; }
		void ResetDocumentHelper();
	}
}
