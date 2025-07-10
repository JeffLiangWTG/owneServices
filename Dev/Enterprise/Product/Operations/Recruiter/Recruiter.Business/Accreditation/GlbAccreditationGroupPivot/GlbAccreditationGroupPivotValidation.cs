//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationGroupPivotValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationGroupPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupPivotValidation : AutoGlbAccreditationGroupPivotValidation
	{
		public GlbAccreditationGroupPivotValidation(AutoGlbAccreditationGroupPivot parent) : base(parent)
		{
		}
	}
}
