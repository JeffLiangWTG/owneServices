//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffRemunerationValidation
//
//    This class should be used for overriding validation in AutoGlbStaffRemunerationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffRemunerationValidation : AutoGlbStaffRemunerationValidation
	{
		public GlbStaffRemunerationValidation(AutoGlbStaffRemuneration parent) : base(parent)
		{
		}
	}
}
