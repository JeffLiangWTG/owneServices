//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffChangeRequestTemplateValidation
//
//    This class should be used for overriding validation in AutoGlbStaffChangeRequestTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestTemplateValidation : AutoGlbStaffChangeRequestTemplateValidation
	{
		public GlbStaffChangeRequestTemplateValidation(AutoGlbStaffChangeRequestTemplate parent) : base(parent)
		{
		}
	}
}

