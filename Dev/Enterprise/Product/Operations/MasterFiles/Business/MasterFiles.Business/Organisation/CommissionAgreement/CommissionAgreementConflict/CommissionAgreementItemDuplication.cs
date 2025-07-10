using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementItemDuplication : ICommissionAgreementDuplication
	{
		public CommissionAgreementItemDuplication(OrgCommissionAgreementItem commissionAgreementItem)
		{
			Argument.NotNull(commissionAgreementItem, "commissionAgreementItem");

			this.CommissionAgreementItem = commissionAgreementItem;
		}

		public readonly OrgCommissionAgreementItem CommissionAgreementItem;

		public virtual string ToDisplayText()
		{
			var commissionAgreement = CommissionAgreementItem.CommissionAgreement;
			return commissionAgreement != null ? commissionAgreement.HumanReadableName : ZString.Empty;
		}
	}
}
