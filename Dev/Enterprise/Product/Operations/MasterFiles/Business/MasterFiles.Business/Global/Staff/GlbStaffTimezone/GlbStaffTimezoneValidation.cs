//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffTimezoneValidation
//
//    This class should be used for overriding validation in AutoGlbStaffTimezoneValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffTimezoneValidation : AutoGlbStaffTimezoneValidation
	{
		public GlbStaffTimezoneValidation(AutoGlbStaffTimezone parent) : base(parent)
		{
		}
	}
}
