//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementRecipientLookups
//
//    This class should be used for overriding collections in AutoOrgCommissionAgreementRecipientLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientLookups : AutoOrgCommissionAgreementRecipientLookups
	{
		public OrgCommissionAgreementRecipientLookups(AutoOrgCommissionAgreementRecipient parent) : base(parent)
		{
		}

		#region SalesReps

		public GlbStaffCollection SalesReps
		{
			get { return new SalesRepCollection(Factory); }
		}

		#endregion

		#region Commission Types

		public ReadOnlyCodeDescriptionPairList CommissionTypes
		{
			get { return CommissionLookups.New(Factory).CommissionTypes; }
		}

		#endregion
	}
}
