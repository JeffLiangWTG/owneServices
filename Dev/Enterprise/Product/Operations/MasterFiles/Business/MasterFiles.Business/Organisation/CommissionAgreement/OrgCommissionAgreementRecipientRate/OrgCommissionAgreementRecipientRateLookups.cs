//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementRecipientRateLookups
//
//    This class should be used for overriding collections in AutoOrgCommissionAgreementRecipientRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientRateLookups : AutoOrgCommissionAgreementRecipientRateLookups
	{
		public OrgCommissionAgreementRecipientRateLookups(AutoOrgCommissionAgreementRecipientRate parent) : base(parent)
		{
		}

		#region Commission Periods

		public ReadOnlyCodeDescriptionPairList CommissionPeriods
		{
			get { return CommissionLookups.New(Factory).CommissionPeriods; }
		}

		public ReadOnlyCodeDescriptionPairList EnabledCommissionPeriods
		{
			get { return CommissionLookups.New(Factory).EnabledCommissionPeriods; }
		}

		#endregion
	}
}
