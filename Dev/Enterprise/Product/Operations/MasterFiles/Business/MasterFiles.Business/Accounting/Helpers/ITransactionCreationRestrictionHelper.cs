using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransactionCreationRestrictionHelper
	{
		bool AllowToCreatePaymentApproval(AccPaymentApproval paymentApproval, out ResourceString errorMessage);

		bool AllowToCreateTransaction(AccTransactionHeader transaction, out ResourceString errorMessage);

		bool CheckOrgHeaderAllowsPosting(AccTransactionHeader transaction, out ResourceString errorMessage, out bool isErrorMessage);
	}
}
