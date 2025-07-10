using System;
using System.Diagnostics;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;

using GlobalTraceSource = System.Diagnostics.TraceSource;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class Submission : BasePage
	{
		protected GlobalTraceSource TraceSource;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tracing, May be an identifier or GUID.")]
		protected void Page_Load(object sender, EventArgs e)
		{
			TraceSource = WebTraceSourceCreator.Initialise(Session);
			TraceSource?.TraceEvent(TraceEventType.Information, 2021, "Start Page Load");
			Session.Abandon();

			var voteExamSurveyAnswerSetDataSource = DataSource as VoteExamSurveyAnswerSet;
			if (voteExamSurveyAnswerSetDataSource != null)
			{
				SubmissionMessageLabel.InnerHtml = voteExamSurveyAnswerSetDataSource.HtmlSubmissionCompletedMessage;
			}

			Page.ZClientScript.RegisterClientScriptBlock(GetType(), "Exam", "<script type='text/javascript' src='https://myaccount.cargowise.com/Portals/MA/exam.js'/>");
		}

		protected override BusinessObject GetNewDataSource()
		{
			var campaignItemPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignItemSchema.Constants.PK);
			var jobSkillCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.JobSkillCodeStringKey);
			var countryCode = SecureQueryStringHelper.GetQuestionsCountryCode(Request.QueryString);
			var campaignPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignSchema.Constants.PK);
			var examSettingsCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.ExamSettingsStringKey);
			var campaignItem = Factory.Load<GlbCompanyCampaignItem>(campaignItemPK);
			var examVersion = ObjectFactory.Get<IExamSettingCodeHelper>().GetExamVersion(examSettingsCode, campaignItemPK, Factory);

			if (campaignItem != null)
			{
				return (BusinessObject)VoteSurveyExamManager.GetVoteExamSurveyAnswerSet(Factory, campaignItem.PK, campaignItem.G8_RecipientID, jobSkillCode, examVersion);
			}
			return null;
		}

		protected override bool ShowLogOffLinkButton
		{
			get { return false; }
		}

		protected override void AddClientSideScript(string errorMessage, string key)
		{
			//Should not show any error because anwsers have been submitted and saved
		}
	}
}
