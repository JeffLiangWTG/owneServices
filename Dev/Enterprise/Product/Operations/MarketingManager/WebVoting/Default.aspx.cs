using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

using GlobalTraceSource = System.Diagnostics.TraceSource;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class Default : BasePage, IPostBackEventHandler
	{
		public string JobSkillCode;
		public string ExamSettingsCode;
		protected GlobalTraceSource TraceSource;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tracing")]
		protected void Page_Load(object sender, EventArgs e)
		{
			TraceSource = WebTraceSourceCreator.Initialise(Session);
			TraceSource?.TraceEvent(TraceEventType.Information, 2011, "Default page load");

			// do not move this to OnLoad() as this has to happen before Bind() but after ZPage.OnLoad()
			var eventArgument = Request.Form["__EVENTARGUMENT"];
			if (eventArgument != AutoSubmission && eventArgument != CancelSubmission && eventArgument != ManualSubmission && (((BusinessObject)DataSource) == null || DataSource.HasPreviousSessionEnded))
			{
				TraceSource?.TraceEvent(TraceEventType.Information, 2012, "Before Get Campaign Url");
				var item = DataSource as VoteExamSurveyAnswerSet;
				string url = (item != null) ? VoteExamSurveyUrlHelper.GetCampaignUrl(DataSource.ExamCampaignItems.FirstOrDefault()) : AppInstance.LoginPage;
				TraceSource?.TraceEvent(TraceEventType.Information, 2013, "After Get Campaign Url");
				Response.Redirect(url);
			}

			SuppressErrorDialog = true;

			if (DataSource != null)
			{
				if (!IsPostBack)
				{
					SetupSubmitButtonConfirmation();
				}
				TraceSource?.TraceEvent(TraceEventType.Information, 2014, "Before Add Vote Exam Survey User Control");
				AddVoteExamSurveyUserControl();
				GetVoteExamTile();
				TraceSource?.TraceEvent(TraceEventType.Information, 2015, "After exam setup");
				currentPage = DataSource.CurrentPage;
				if (ErrorMessageContainer != null)
				{
					ErrorMessageContainer.Visible = false;
				}
			}
		}

		void GetVoteExamTile()
		{
			this.VoteTitleLabel.Text = DataSource.CompanyCampaign.G0_CampaignNameLocalized;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded javascript")]
		void SetupSubmitButtonConfirmation()
		{
			SubmitButton.OnClientClick = string.Format("javascript: return window.confirm('{0}');", DataSource.SubmissionConfirmationMessage);
		}

		#region Add User Control

		void AddVoteExamSurveyUserControl()
		{
			if (Campaign != null)
			{
				Control control;
				if (Campaign.IsVoteCampaign)
				{
					control = VoteControl.New(Campaign);
				}
				else
				{
					control = LoadExamSurveyUserControl();
					SubmitButton.Visible = false;
				}
				control.ID = "VoteExamSurveyUserControl";
				ContentPlaceHolder.Controls.Add(control);
			}
		}

		ExamSurveyUserControl LoadExamSurveyUserControl()
		{
			ExamSurveyUserControl control = (ExamSurveyUserControl)LoadControl("ExamSurveyUserControl.ascx");
			control.AutoPostBack = DataSource?.AutoSaveAnswers ?? false;
			return control;
		}

		#endregion

		#region Submission

		#region Save Answers On Submit Button Click
		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			HandleSubmitButtonClick();
		}

		public void HandleSubmitButtonClick()
		{
			SuppressErrorDialog = false;
			if (ErrorMessageContainer != null)
			{
				ErrorMessageContainer.Visible = true;
			}

			if (DataSource != null)
			{
				var submissionResult = new VoteSurveyExamManager().SaveAnswersOnSubmitButtonClick((VoteExamSurveyAnswerSet)DataSource, SaveDataSourceFactory);
				HandleSubmissionResult(submissionResult);
			}
		}

		#endregion

		#region Save Answers On Time Out

		public const string AutoSubmission = "__AUTOSUBMIT__";
		public const string ManualSubmission = "__MANUALSUBMIT__";
		public const string CancelSubmission = "__CANCELSUBMIT__";

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			switch (eventArgument)
			{
				case AutoSubmission:
					{
						var containsUnansweredQuestion = false;

						var item = DataSource as VoteExamSurveyAnswerSet;
						if (item != null)
						{
							containsUnansweredQuestion = item.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().Any(x => !x.IsAnswered);
						}

						if (containsUnansweredQuestion)
						{
							ReportTimeExpired(Res.GetString("06cd19c0-16db-402c-a277-f05b262d7b43", "Your exam can’t be submitted, as not all questions have been answered."));
						}
						else
						{
							Page.ZClientScript.RegisterStartupScript(GetType(), "HandleAutoSubmit", "HandleAutoSubmit();", true);
						}

						break;
					}
				case ManualSubmission:
					{
						var item = DataSource as VoteExamSurveyAnswerSet;
						if (item == null || item.CompanyCampaign.PK.IsEmpty)
						{
							ReportTimeExpired(Res.GetString("06bf0c13-a756-4bc8-854a-5984fc942430", "Your exam is not submitted."));
						}
						else if ((item.CompanyCampaign?.IsExamCampaign ?? false) && (!item.AnswerWrappers.Any() || item.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().Any(x => !x.IsAnswered)))
						{
							ReportTimeExpired(Res.GetString("06cd19c0-16db-402c-a277-f05b262d7b43", "Your exam can’t be submitted, as not all questions have been answered."));
						}
						else
						{
							var submissionResult = new VoteSurveyExamManager().SaveAnswersOnTimeOut(item, SaveDataSourceFactoryNoValidation);
							HandleSubmissionResult(submissionResult);
						}
						break;
					}
				case CancelSubmission:
					{
						ReportTimeExpired(Res.GetString("06bf0c13-a756-4bc8-854a-5984fc942430", "Your exam is not submitted."));
						break;
					}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is the name of a query string parameter., It is part of a URL.")]
#if DEBUG
		internal
#endif
		void ReportTimeExpired(string message)
		{
			var timeExpiredText = Res.GetString("06b6c0aa-d8c9-44d0-a920-9db83b60d746", "Time Expired");
			var queryString = new SecureQueryString();
			queryString.Add("message", message);
			queryString.Add("title", timeExpiredText);
			queryString.Add("pageTitle", timeExpiredText);
			Response.Redirect(ZString.Format("~/Error.aspx?data={0}", WebUtility.UrlEncode(queryString.ToString())));
		}

		#endregion

		#region Save Answers When Navigating Pages

		protected override void OnPreRenderComplete(EventArgs e)
		{
			base.OnPreRenderComplete(e);
			if (IsPostBack && !submissionFailed && DataSource != null && DataSource.HasChanges && (DataSource.AutoSaveAnswers || currentPage != DataSource.CurrentPage))
			{
				var item = DataSource as VoteExamSurveyAnswerSet;
				if (item != null)
				{
					var submissionResult = new VoteSurveyExamManager().SaveAnswersWhenNavigatingPages(item, SaveDataSourceFactoryNoValidation);
					if (submissionResult == VoteSurveyExamManager.SubmissionResult.AlreadySubmitted)
					{
						Response.Redirect(VoteExamSurveyUrlHelper.GetCampaignUrl(DataSource.ExamCampaignItems.FirstOrDefault()));
					}
				}
			}
		}
		#endregion

#if DEBUG
		internal
#endif
		void HandleSubmissionResult(VoteSurveyExamManager.SubmissionResult submissionResult)
		{
			if (submissionResult == VoteSurveyExamManager.SubmissionResult.AlreadySubmitted)
			{
				Response.Redirect(VoteExamSurveyUrlHelper.GetCampaignUrl(DataSource.ExamCampaignItems.FirstOrDefault()));
			}
			else if (submissionResult == VoteSurveyExamManager.SubmissionResult.SubmitSucceeded)
			{
				var requiredCampaigns = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey);
				if (!requiredCampaigns.IsNullOrEmpty())
				{
					var requiredcampaignCollection = requiredCampaigns.Split(new char[] { ',' });
					if (requiredcampaignCollection.Length > 1)
					{
						string[] newRequiredCampaignCollection = new string[requiredcampaignCollection.Length - 1];
						Array.Copy(requiredcampaignCollection, 1, newRequiredCampaignCollection, 0, newRequiredCampaignCollection.Length);
						StringBuilder newRequiredCampaigns = new StringBuilder();
						foreach (var campaignPK in newRequiredCampaignCollection)
						{
							newRequiredCampaigns.Append(campaignPK).Append(",");
						}
						if (newRequiredCampaigns.Length > 0)
						{
							newRequiredCampaigns.Remove(newRequiredCampaigns.Length - 1, 1);
						}
						string encryptedText = Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
						SecureQueryString secureQueryString = new SecureQueryString(encryptedText);
						secureQueryString.Remove(SecureQueryString.TimeStampKey);
						secureQueryString.Remove(VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey);
						secureQueryString.Add(VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey, newRequiredCampaigns.ToString());
						var result = string.Format(CultureInfo.CurrentCulture, "?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()));
						Response.Redirect(string.Concat(AppInstance.DefaultPage, result));
					}
				}
				if (!Globals.IsTest)
				{
					string result = GetSubmissionRedirectParameters();
					Response.Redirect(string.Concat("Submission.aspx", result));
				}
			}
			else if (submissionResult == VoteSurveyExamManager.SubmissionResult.SubmitFailed)
			{
				submissionFailed = true;
			}
		}

		protected string GetSubmissionRedirectParameters()
		{
			string encryptedText = Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
			SecureQueryString secureQueryString = new SecureQueryString(encryptedText);
			secureQueryString.Remove(SecureQueryString.TimeStampKey);
			secureQueryString.Remove(GlbCompanyCampaignItemSchema.Constants.PK);
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, DataSource.ExamCampaignItems.FirstOrDefault().PK.ToString());

			var sb = new ZStringBuilder();
			foreach (string key in Request.QueryString.Keys)
			{
				if (!key.Equals(VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, StringComparison.CurrentCultureIgnoreCase))
				{
					sb.Append($"&{key}={Request.QueryString[key]}");
				}
			}

			var result = string.Format(CultureInfo.CurrentCulture, "?{0}={1}{2}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(secureQueryString.ToString()), sb.ToString());

			return result;
		}

		bool submissionFailed;

		#endregion

		#region Page Overrides

		protected override void AddClientSideScript(string errorMessage, string key)
		{
			var script = "<script>window.onload=ShowErrorMessage();</script>";
			ErrorMessageContainer.InnerHtml = errorMessage.Replace("\\n", "<br/>") + script;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (submissionFailed)
			{
				DataSource?.ClearSubmissionDate();
			}
			SubmitButton.Enabled = DataSource != null && !DataSource.IsPreview;
			Title = Campaign?.CampaignTypeCaption ?? "";
		}

		protected override bool ShowLoginStatus
		{
			get { return DataSource != null && !DataSource.IsPreview; }
		}

		protected override bool ShowLogOffLinkButton
		{
			get { return false; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		short currentPage;

		#endregion

		#region DataSource

		public new IVoteExamSurveyAnswerSet DataSource
		{
			get { return (IVoteExamSurveyAnswerSet)base.DataSource; }
		}

		GlbCompanyCampaign Campaign
		{
			get { return DataSource?.CompanyCampaign; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected override BusinessObject GetNewDataSource()
		{
			IVoteExamSurveyAnswerSet result = null;
			var campaignPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignSchema.Constants.PK);
			var campaignItemPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignItemSchema.Constants.PK);
			var recipientInfo = SecureQueryStringHelper.GetRecipientInfo(Request.QueryString);
			GlbCompanyCampaign campaign = Factory.Load<GlbCompanyCampaign>(campaignPK);

			JobSkillCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.JobSkillCodeStringKey);
			ExamSettingsCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.ExamSettingsStringKey);
			if (campaign != null && recipientInfo == null)
			{
				result = new VoteExamSurveyPreviewBizO(campaign);
				result.StartVoteExamSurvey();
			}
			else
			{
				result = StartVoteExamSurvey(recipientInfo);
			}

			if (result != null)
			{
				result.SetJobSkillAndVersion(JobSkillCode, ExamSettingsCode);
			}

			return (BusinessObject)result;
		}

		IVoteExamSurveyAnswerSet StartVoteExamSurvey(GlbCompanyCampaignItem.RecipientInfo recipientInfo)
		{
			var campaignItemPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignItemSchema.Constants.PK);
			var campaignPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignSchema.Constants.PK);
			var countryCode = SecureQueryStringHelper.GetQuestionsCountryCode(Request.QueryString);
			var jobSkillCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.JobSkillCodeStringKey);
			var examSettingsCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.ExamSettingsStringKey);
			var requiredCampaigns = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey);
			var examVersion = ObjectFactory.Get<IExamSettingCodeHelper>().GetExamVersion(examSettingsCode, campaignItemPK, Factory);

			string[] requiredCampaignCollection;
			if (!requiredCampaigns.IsNullOrEmpty())
			{
				requiredCampaignCollection = requiredCampaigns.Split(new char[] { ',' }).Take(1).ToArray();
			}
			else
			{
				requiredCampaignCollection = new string[] { campaignPK.ToString() };
			}

			var result = new VoteSurveyExamManager().StartVoteExamSurvey(Factory, campaignItemPK, recipientInfo, countryCode, jobSkillCode,
				requiredCampaignCollection, !IsPostBack, examSettingsCode);
			return result;
		}

		#endregion

		#region Error Reporting

		[System.Web.Services.WebMethod()]
		[System.Web.Script.Services.ScriptMethod()]
		public static void ReportError(string message)
		{
			ErrorReporter.ReportOnce("Enterprise.MarketingManager.WebVoting.ReportError", message);
		}

		#endregion

		#region Test
#if DEBUG

		new Control LoadControl(string virtualPath)
		{
			Control result = null;

			if (Globals.IsTest)
			{
				if (virtualPath == "ExamSurveyUserControl.ascx")
				{
					result = new ExamSurveyUserControl();
				}
			}
			else
			{
				result = base.LoadControl(virtualPath);
			}

			return result;
		}

		#region Page overrides for Test

		protected override HtmlForm GetForm(Control parent)
		{
			return (Globals.IsTest)
				? new HtmlForm()
				: base.GetForm(parent);
		}

		#endregion

#endif
		#endregion
	}
}
