//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHROnBoardingEntitlementLookups
//
//    This class should be used for overriding collections in AutoHROnBoardingEntitlementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingEntitlementLookups : AutoHROnBoardingEntitlementLookups
	{
		public HROnBoardingEntitlementLookups(AutoHROnBoardingEntitlement parent) : base(parent)
		{
		}

		public HROnBoardingCollection HROnBoardings => hrOnBoardings ?? (hrOnBoardings = new HROnBoardingCollection(Factory));

		HROnBoardingCollection hrOnBoardings;
	}
}
