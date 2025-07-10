using System.Collections.Generic;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignDripMarketing))]
	public class GlbCompanyCampaignDripMarketingRelatedFilterTest : RelatedModuleFilterSupportableTestCase<GlbCompanyCampaignDripMarketing>
	{
		protected override GlbCompanyCampaignDripMarketing GetNewBusinessObject()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var pivot = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			pivot.GCD_G0_NextTouch = master.PK;

			return pivot;
		}

		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(GlbCompanyCampaignDripMarketing businessObject)
		{
			return new[] { new FilterRuleTestSet(null, () => businessObject.FilterRule, "GlbCompanyCampaignContactFilterBusinessObject") };
		}

		protected override void ValidateBusinessObject(GlbCompanyCampaignDripMarketing businessObject)
		{
			businessObject.Validation.ValidateAll();
		}

		protected override string FilterDescriptionForValidationTest => "Contact Details Verified By";
	}
}
