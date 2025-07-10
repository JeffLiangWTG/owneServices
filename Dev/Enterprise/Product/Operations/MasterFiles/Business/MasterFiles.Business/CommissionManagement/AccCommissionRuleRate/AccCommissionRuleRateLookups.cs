//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleRateLookups
//
//    This class should be used for overriding collections in AutoAccCommissionRuleRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleRateLookups : AutoAccCommissionRuleRateLookups
	{
		public AccCommissionRuleRateLookups(AutoAccCommissionRuleRate parent) : base(parent)
		{
		}

		#region Commission Types

		public ReadOnlyCodeDescriptionPairList CommissionTypes
		{
			get { return CommissionLookups.New(Factory).CommissionTypes; }
		}

		#endregion

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
