using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	public class GlbCompanyCampaignLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCampaignTypeList()
		{
			AssertEquals(typeof(CampaignTypeList), Campaign.Lookups.CampaignTypeList.GetType());
			AssertEquals(7, Campaign.Lookups.CampaignTypeList.Count);
			AssertEquals(CampaignTypeList.Descriptions.Broadcast, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.Broadcast));
			AssertEquals(CampaignTypeList.Descriptions.Voting, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.Voting));
			AssertEquals(CampaignTypeList.Descriptions.Survey, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.Survey));
			AssertEquals(CampaignTypeList.Descriptions.LinkTracking, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.LinkTracking));
			AssertEquals(CampaignTypeList.Descriptions.TargetList, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.TargetList));
			AssertEquals(CampaignTypeList.Descriptions.DripMarketing, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.DripMarketing));
			AssertEquals(CampaignTypeList.Descriptions.InsideSales, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(CampaignTypeList.Codes.InsideSales));
		}

		public void TestDripMarketingTouchTypeList()
		{
			AssertEquals(typeof(DripMarketingTouchTypeList), Campaign.Lookups.DripMarketingTouchTypeList.GetType());
			AssertEquals(5, Campaign.Lookups.DripMarketingTouchTypeList.Count);
			AssertEquals(DripMarketingTouchTypeList.Descriptions.Broadcast, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(DripMarketingTouchTypeList.Codes.Broadcast));
			AssertEquals(DripMarketingTouchTypeList.Descriptions.Voting, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(DripMarketingTouchTypeList.Codes.Voting));
			AssertEquals(DripMarketingTouchTypeList.Descriptions.Survey, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(DripMarketingTouchTypeList.Codes.Survey));
			AssertEquals(DripMarketingTouchTypeList.Descriptions.LinkTracking, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(DripMarketingTouchTypeList.Codes.LinkTracking));
			AssertEquals(DripMarketingTouchTypeList.Descriptions.TargetList, Campaign.Lookups.CampaignTypeList.GetDescriptionFromCode(DripMarketingTouchTypeList.Codes.TargetList));
		}

		public void TestInsideSalesTouchTypeList()
		{
			AssertEquals(typeof(InsideSalesTouchTypeList), Campaign.Lookups.InsideSalesTouchTypeList.GetType());
			AssertEquals(2, Campaign.Lookups.InsideSalesTouchTypeList.Count);
			AssertEquals(InsideSalesTouchTypeList.Descriptions.OpportunityCreation, Campaign.Lookups.InsideSalesTouchTypeList.GetDescriptionFromCode(InsideSalesTouchTypeList.Codes.OpportunityCreation));
			AssertEquals(InsideSalesTouchTypeList.Descriptions.PreApproachEmail, Campaign.Lookups.InsideSalesTouchTypeList.GetDescriptionFromCode(InsideSalesTouchTypeList.Codes.PreApproachEmail));
		}

		public void TestFilteredOrganisations()
		{
			AssertNotNull("Fitlered Organisations list", Campaign.Lookups.FilteredOrganisations);
		}

		public void TestMediaCategoryList()
		{
			AssertNotNull("Media Category List should not be null", Campaign.Lookups.MediaCategoryList);
		}

		public void TestContactDataSourceList()
		{
			AssertNotNull("Contact Data Source List should not be null", Campaign.Lookups.ContactDataSourceList);
		}

		public void TestMediaTypesList()
		{
			AssertNotNull("Media Types List should not be null", Campaign.Lookups.ActiveMediaTypesList);
			AssertNotNull("Media Types List should not be null", Campaign.Lookups.MediaTypesList);
		}

		public virtual void TestEmailSenderOptionList()
		{
			AssertNotNull("Email Sender Option List doesn't have to be null", Campaign.Lookups.EmailSenderOptionList);
			Assert(Campaign.Lookups.EmailSenderOptionList.ContainsOnly(EmailSenderOptionCodeDescriptionList.Codes.COR,
				EmailSenderOptionCodeDescriptionList.Codes.EML,
				EmailSenderOptionCodeDescriptionList.Codes.ORG,
				EmailSenderOptionCodeDescriptionList.Codes.SPS));
		}

		public void TestEmailSenderRoleList()
		{
			var staffRoles = Factory.New<OrgStaffAssignments>();
			Assert("StaffAssignmentRoles > 0", staffRoles.Lookups.StaffRoles.Count > 0);
		}

		public virtual void TestMediaLabels()
		{
			OrganisationsDataRegistry.Instance.CampaignCategory1Label.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, (NoResString)"Test Category Label");
			OrganisationsDataRegistry.Instance.CampaignCategory2Label.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, (NoResString)"Test Type Label");
			AssertEquals("Test Category Label", Campaign.Lookups.MediaCategoryLabel);
			AssertEquals("Test Type Label", Campaign.Lookups.MediaTypeLabel);
		}

		public void TestCampaignStages()
		{
			AssertNotNull("Campaign Stages should not be null", Campaign.Lookups.CampaignStages);
		}

		public void TestDefaultAnswerTypes()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			string expectedCodesAsString = new VoteExamSurveyAnswerTypeList(Campaign).CodesAsString;
			AssertEquals(expectedCodesAsString, Campaign.Lookups.DefaultAnswerTypes.CodesAsString);
		}

		public void TestWithoutParent()
		{
			GlbCompanyCampaignLookups lookups = new GlbCompanyCampaignLookups(Factory);
			AssertNotNull(lookups.CampaignTypeList);
		}

		public void TestPublishedList()
		{
			SetupSubscriptionRulesRegistry();
			AssertEquals("Campaign is CRM", false, Campaign.IsHRCampaign);
			AssertEquals("Should use CRM list", true, Campaign.Lookups.PublishedList.ContainsCode("CD1"));
			AssertEquals("Should use CRM list", true, Campaign.Lookups.PublishedList.ContainsCode("CD2"));
			AssertEquals("Should use CRM list", false, Campaign.Lookups.PublishedList.ContainsCode("CD3"));
			AssertEquals("Should use CRM list", false, Campaign.Lookups.PublishedList.ContainsCode("CD4"));

			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals("Campaign is HRM", true, Campaign.IsHRCampaign);
			AssertEquals("Should use HRM list", false, Campaign.Lookups.PublishedList.ContainsCode("CD1"));
			AssertEquals("Should use HRM list", false, Campaign.Lookups.PublishedList.ContainsCode("CD2"));
			AssertEquals("Should use HRM list", true, Campaign.Lookups.PublishedList.ContainsCode("CD3"));
			AssertEquals("Should use HRM list", true, Campaign.Lookups.PublishedList.ContainsCode("CD4"));
		}

		public void TestPublishedDescriptionList()
		{
			SetupSubscriptionRulesRegistry();
			AssertEquals("Campaign is CRM", false, Campaign.IsHRCampaign);
			AssertEquals("Should use CRM list", true, Campaign.Lookups.PublishedDescriptionList.ContainsCode(DefaultCRMRuleDescription.ToString()));
			AssertEquals("Should use CRM list", true, Campaign.Lookups.PublishedDescriptionList.ContainsCode(OtherCRMRuleDescription.ToString()));
			AssertEquals("Should use CRM list", false, Campaign.Lookups.PublishedDescriptionList.ContainsCode(DefaultHRMRuleDescription.ToString()));
			AssertEquals("Should use CRM list", false, Campaign.Lookups.PublishedDescriptionList.ContainsCode(OtherHRMRuleDescription.ToString()));

			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals("Campaign is HRM", true, Campaign.IsHRCampaign);
			AssertEquals("Should use HRM list", false, Campaign.Lookups.PublishedDescriptionList.ContainsCode(DefaultCRMRuleDescription.ToString()));
			AssertEquals("Should use HRM list", false, Campaign.Lookups.PublishedDescriptionList.ContainsCode(OtherCRMRuleDescription.ToString()));
			AssertEquals("Should use HRM list", true, Campaign.Lookups.PublishedDescriptionList.ContainsCode(DefaultHRMRuleDescription.ToString()));
			AssertEquals("Should use HRM list", true, Campaign.Lookups.PublishedDescriptionList.ContainsCode(OtherHRMRuleDescription.ToString()));
		}

		public void TestDefaultPublishedList()
		{
			SetupSubscriptionRulesRegistry();
			AssertEquals("Precondition: Campaign is CRM", false, Campaign.IsHRCampaign);

			var result = Campaign.Lookups.DefaultPublishedList;
			AssertEquals("CRM default code", "CD1", result.Code);
			AssertEquals("CRM default description", DefaultCRMRuleDescription, result.Description);

			Campaign.G0_IsSalesAndMarketing = false;
			AssertEquals("Precondition: Campaign is HRM", true, Campaign.IsHRCampaign);

			result = Campaign.Lookups.DefaultPublishedList;
			AssertEquals("HRM default code", "CD3", result.Code);
			AssertEquals("HRM default description", DefaultHRMRuleDescription, result.Description);

			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SubscriptionRuleCollection());
			AssertNull("Should be null if no default set (HRM)", Campaign.Lookups.DefaultPublishedList);

			Campaign.G0_IsSalesAndMarketing = true;
			AssertNull("Should be null if no default set (CRM)", Campaign.Lookups.DefaultPublishedList);
		}

		void SetupSubscriptionRulesRegistry()
		{
			var rules = new SubscriptionRuleCollection();

			DefaultCRMRuleDescription = (NoResString)"Default Published CRM List AAA";
			DefaultHRMRuleDescription = (NoResString)"Default Published HRM List CCC";
			OtherCRMRuleDescription = (NoResString)"Published CRM List BBB";
			OtherHRMRuleDescription = (NoResString)"Published HRM List DDD";

			rules.AddNewRule("CD1", DefaultCRMRuleDescription, true, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			rules.AddNewRule("CD2", OtherCRMRuleDescription, false, false, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });
			rules.AddNewRule("CD3", DefaultHRMRuleDescription, true, true, new string[] { "TELEV;EXIST;DESC1;SUM1", "PRINT;EXIST;DESC2;SUM2" });
			rules.AddNewRule("CD4", OtherHRMRuleDescription, false, true, new string[] { "PRINT;EXIST;DESC1;SUM1", "PRINT;SLT30;DESC2;SUM2" });

			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
		}

		MultilingualString DefaultCRMRuleDescription;
		MultilingualString OtherCRMRuleDescription;
		MultilingualString DefaultHRMRuleDescription;
		MultilingualString OtherHRMRuleDescription;

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.New<GlbCompanyCampaign>();
				}
				return fCampaign;
			}
		}

		GlbCompanyCampaign fCampaign;
	}
}
