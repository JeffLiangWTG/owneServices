//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgPatternMatchValidation
//
//    This class should be used for overriding validation in AutoOrgPatternMatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchValidation : AutoOrgPatternMatchValidation
	{
		public OrgPatternMatchValidation(AutoOrgPatternMatch parent) : base(parent)
		{
		}
	}
}
