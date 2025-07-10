//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCampaignChildNodeValidation
//
//    This class should be used for overriding validation in AutoViewCampaignChildNodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewCampaignChildNodeValidation : AutoViewCampaignChildNodeValidation
	{
		public ViewCampaignChildNodeValidation(AutoViewCampaignChildNode parent) : base(parent)
		{
		}
	}
}
