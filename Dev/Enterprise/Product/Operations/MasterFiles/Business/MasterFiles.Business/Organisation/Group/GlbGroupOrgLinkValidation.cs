//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbGroupOrgLinkValidation
//
//    This class should be used for overriding validation in AutoGlbGroupOrgLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupOrgLinkValidation : AutoGlbGroupOrgLinkValidation
	{
		public GlbGroupOrgLinkValidation(AutoGlbGroupOrgLink parent) : base(parent)
		{
		}
	}
}
