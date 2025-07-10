using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ApprovedCommissionAgreementCollection : OrgCommissionAgreementCollection
	{
		public ApprovedCommissionAgreementCollection(OrgOpportunity opportunity)
			: base(opportunity, GetApprovedAgreementsFilter())
		{
		}

		static ZQuery GetApprovedAgreementsFilter()
		{
			return new ZQuery(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, SQLComparisonOperator.NotEqual, null);
		}

		#region Default Values

		protected override void SetDefaultsForNewElementCore(OrgCommissionAgreement newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CA0_LastApprovedDateUtc = ZDateTime.UtcNow;
		}

		#endregion
	}
}
