//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffChangeRequestValidation
//
//    This class should be used for overriding validation in AutoGlbStaffChangeRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestValidation : AutoGlbStaffChangeRequestValidation
	{
		public GlbStaffChangeRequestValidation(AutoGlbStaffChangeRequest parent) : base(parent)
		{
		}
	}
}
