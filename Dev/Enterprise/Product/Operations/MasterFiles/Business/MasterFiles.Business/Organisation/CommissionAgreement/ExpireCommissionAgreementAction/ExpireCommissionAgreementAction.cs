using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ExpireCommissionAgreementAction : AutoExpireCommissionAgreementAction
	{
		public ExpireCommissionAgreementAction(IEnumerable<OrgCommissionAgreement> commissionAgreements)
		{
			Argument.NotNull(commissionAgreements, "commissionAgreements");

			CommissionAgreements = commissionAgreements;
		}

		readonly IEnumerable<OrgCommissionAgreement> CommissionAgreements;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Date = ZDateTime.UtcNow;
		}

		#endregion

		#region Properties

		public ZDateTime DateLocal
		{
			get { return Date.ToLocalBranchTime(); }
			set { Date = value.ToUniversalBranchTime(); }
		}

		public ZPropertyInfo DateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DateLocal), x => DateInfo); }
		}

		#endregion

		#region Execute

		public void Execute()
		{
			foreach (var commissionAgreement in CommissionAgreements)
			{
				commissionAgreement.Expire(Date.Date);
			}
		}

		#endregion
	}
}
