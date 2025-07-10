//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffEntitlementValidation
//
//    This class should be used for overriding validation in AutoGlbStaffEntitlementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEntitlementValidation : AutoGlbStaffEntitlementValidation
	{
		public GlbStaffEntitlementValidation(AutoGlbStaffEntitlement parent) : base(parent)
		{
		}
	}
}
