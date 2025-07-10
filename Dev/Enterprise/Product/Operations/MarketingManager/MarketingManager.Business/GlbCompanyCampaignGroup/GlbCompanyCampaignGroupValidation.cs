//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignGroupValidation
//
//    This class should be used for overriding validation in AutoGlbCompanyCampaignGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignGroupValidation : AutoGlbCompanyCampaignGroupValidation
	{
		public GlbCompanyCampaignGroupValidation(AutoGlbCompanyCampaignGroup parent) : base(parent)
		{
		}
	}
}
