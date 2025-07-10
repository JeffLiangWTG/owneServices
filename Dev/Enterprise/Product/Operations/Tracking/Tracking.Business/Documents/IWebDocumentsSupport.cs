using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public interface IWebDocumentsSupport : IWebDocumentsSupportBase
	{
		OrgContact LoggedInContact { get; }
		DocumentSupport DocumentHelper { get; }
	}
}
