//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbCompanyCampaignDripMarketingValidation
//
//    This class should be used for overriding validation in AutoGlbCompanyCampaignDripMarketingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignDripMarketingValidation : AutoGlbCompanyCampaignDripMarketingValidation
	{
		public GlbCompanyCampaignDripMarketingValidation(AutoGlbCompanyCampaignDripMarketing parent) : base(parent)
		{
		}

		public new GlbCompanyCampaignDripMarketing Parent
		{
			get { return (GlbCompanyCampaignDripMarketing)base.Parent; }
		}

		protected override void CheckGCD_ParentHorizontalId()
		{
			if (Parent.MasterCampaign != null && Parent.MasterCampaign.PK != Parent.GCD_G0_ParentTouch)
			{
				MandatoryValidation.CheckEntered(Parent.GCD_ParentHorizontalIdInfo);
			}
		}

		protected override void CheckGCD_G0_ParentTouch()
		{
			MandatoryValidation.CheckEntered(Parent.GCD_G0_ParentTouchInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateFilterStrips();
		}

		void ValidateFilterStrips()
		{
			RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.FilterRule, ModuleIDs.GlbCompanyCampaignContact);
		}
	}
}
