using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.CreditControl
{
	public interface ICreditControlledDocumentsApproval
	{
		bool RequestAlreadyApproved { get; }

		void Initialize(BusinessObject parentBO, ZGuid menuItemPK, int[] authorizationLevel, string requestReasonDescription = null);

		void SetStatus(ZString status, bool approveAllDocuments);
	}
}
