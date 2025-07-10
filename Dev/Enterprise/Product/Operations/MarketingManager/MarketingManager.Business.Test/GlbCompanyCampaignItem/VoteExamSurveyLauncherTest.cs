using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyLauncher))]
	sealed class VoteExamSurveyLauncherTest : NonPersistentBusinessObjectTestCase
	{
		public void TestVoteExamSurveyDetails()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Some Survey";
			campaign.G0_CampaignComment = @"Some	Comment
With Tabs and	Newlines and JS script <img src=x onerror=alert(document.domain)>";
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_RN_NKCountryCode = "DE";

			VoteExamSurveyLauncher launcher = new VoteExamSurveyLauncher(Factory, ZGuid.Empty, campaign.PK, "AU", "");
			List<KeyValuePair<string, string>> detailList = launcher.VoteExamSurveyDetails.ToList();
			AssertEquals(3, detailList.Count);
			AssertEquals("Survey Name", detailList[0].Key);
			AssertEquals("Some Survey", detailList[0].Value);
			AssertEquals("Description", detailList[1].Key);
			AssertEquals("Some&nbsp;&nbsp;&nbsp;&nbsp;Comment<br/>With Tabs and&nbsp;&nbsp;&nbsp;&nbsp;Newlines and JS script &lt;img src=x onerror=alert(document.domain)&gt;", detailList[1].Value);
			AssertEquals("No. of Questions", detailList[2].Key);
			AssertEquals("1", detailList[2].Value);
		}

		public void TestHtmlEncodedVoteExamSurveyDetails()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Campaign Survey Test<img src=x onerror=alert(document.domain)>)";

			var question = campaign.Questions.AddNew();
			question.HY_RN_NKCountryCode = "AU";

			var launcher = new VoteExamSurveyLauncher(Factory, ZGuid.Empty, campaign.PK, "AU", "");
			var detailList = launcher.VoteExamSurveyDetails.ToList();

			AssertEquals("Survey Name", detailList[0].Key);
			AssertEquals("Campaign Survey Test&lt;img src=x onerror=alert(document.domain)&gt;)", detailList[0].Value);
		}

		public void TestVoteExamSurveyDetails_ExcludeEmptyValues()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			campaign.G0_CampaignName = "Some Voting";

			VoteExamSurveyLauncher launcher = new VoteExamSurveyLauncher(Factory, ZGuid.Empty, campaign.PK, "AU", "");
			List<KeyValuePair<string, string>> detailList = launcher.VoteExamSurveyDetails.ToList();
			AssertEquals(2, detailList.Count);
			AssertEquals("Vote Name", detailList[0].Key);
			AssertEquals("No. of Votes Required", detailList[1].Key);
		}

		[TestDate(2007, 1, 1, 20, 0, 0)]
		public void TestErrorMessage()
		{
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			VoteExamSurveyLauncher launcher = new VoteExamSurveyLauncher(Factory, item.PK, ZGuid.Empty, "AU", "");
			AssertEquals("Either your session has expired or the campaign URL has been manually altered.", launcher.ErrorMessage);
			Assert(!launcher.CanRedirect);

			item.G8_G0 = Factory.New<GlbCompanyCampaign>().PK;
			AssertEquals("", launcher.ErrorMessage);
			Assert(launcher.CanRedirect);

			item.CompanyCampaign.G0_ActualCompletedDate = ZDateTime.Now;
			AssertEquals("This campaign has ended.", launcher.ErrorMessage);
			Assert(!launcher.CanRedirect);

			item.CompanyCampaign.G0_ActualCompletedDate = ZDateTime.Empty;
			item.G8_ClosedDateUtc = ZDateTime.UtcNow;
			AssertEquals("You have previously submitted your answers. Only one submission is allowed per participant.", launcher.ErrorMessage);
			Assert(!launcher.CanRedirect);

			launcher = new VoteExamSurveyLauncher(Factory, ZGuid.Empty, item.G8_G0, "AU", new ZDate(2006, 12, 31), "");
			AssertEquals("Your test link has expired.", launcher.ErrorMessage);
			Assert(!launcher.CanRedirect);

			launcher = new VoteExamSurveyLauncher(Factory, ZGuid.Empty, item.G8_G0, "AU", "");
			AssertEquals("", launcher.ErrorMessage);
			Assert(launcher.CanRedirect);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoteExamSurveyLauncher(Factory, Guid.Empty, Guid.Empty, "AU", "");
		}
	}
}
