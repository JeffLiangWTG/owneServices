using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface ICommissionAgreementController
	{
		IZForm ShowEditFormForRate(OrgCommissionAgreementRecipientRate agreementRecipientRate);
	}
}
