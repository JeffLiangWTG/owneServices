//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbGroupOrgContactLinkValidation
//
//    This class should be used for overriding validation in AutoGlbGroupOrgContactLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupOrgContactLinkValidation : AutoGlbGroupOrgContactLinkValidation
	{
		public GlbGroupOrgContactLinkValidation(AutoGlbGroupOrgContactLink parent) : base(parent)
		{
		}
	}
}

