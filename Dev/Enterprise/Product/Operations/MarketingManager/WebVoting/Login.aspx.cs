using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

using GlobalTraceSource = System.Diagnostics.TraceSource;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class Login : BasePage
	{
		protected Panel WarningMessagePanel;
		protected GlobalTraceSource TraceSource;

		#region Event Handlers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tracing")]
		protected void Page_Load(object sender, EventArgs e)
		{
			if (string.Equals(Request.QueryString[WebTraceSourceCreator.EnableTracingVariable], bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
			{
				Session[WebTraceSourceCreator.EnableTracingVariable] = bool.TrueString;
			}

			TraceSource = WebTraceSourceCreator.Initialise(Session);

			TraceSource?.TraceEvent(TraceEventType.Information, 2001, "Start of Login page load");
			TraceSource?.TraceEvent(TraceEventType.Information, 2002, "Before Bind Grouped Exam Campaign Summary");
			TraceSource?.TraceEvent(TraceEventType.Information, 2003, "Before Set Footer Text");
			SetFooterText();
			TraceSource?.TraceEvent(TraceEventType.Information, 2004, "Before Display Scheduled System Upgrade Warning");
			DisplayScheduledSystemUpgradeWarning(new VoteExamSurveySystemUpgradeScheduleTaskWarning());
			TraceSource?.TraceEvent(TraceEventType.Information, 2005, "After Display Scheduled System Upgrade Warning");
			CampaignDetailRepeater.Visible = DataSource.CanRedirect;
			CampaignDetailRepeater.ItemDataBound += new RepeaterItemEventHandler(CampaignDetailRepeater_ItemDataBound);

			bool redirecting = false;
			if (DataSource.Campaign != null && CheckMigratedExam(DataSource.Campaign))
			{
				redirecting = true;
				Response.Redirect(ContentMovedPage);
			}

			if (DataSource.ShouldAutoLaunch && !redirecting)
			{
				TraceSource?.TraceEvent(TraceEventType.Information, 2006, "Before Redirect");
				Redirect();
			}
		}

		bool CheckMigratedExam(GlbCompanyCampaign campaign)
		{
			return campaign != null && RecruiterDataRegistry.Instance.ExamMigrationStatus.Value.GetBoolFromCode(campaign.G0_CampaignID);
		}

		public string StartButtonText => Res.GetString("99CCC325-BA0D-4452-A3E4-016FE96BA4CE", "Start");

		public string SectionsLabelText => Res.GetString("7BACAF16-20A8-44AE-B35D-92AD7F87EAC8", "Sections");

		public string QuestionsLabelText => Res.GetString("318395A3-4FB7-49B1-A951-65A71952BC32", "Questions");

		public string DurationLabelText => Res.GetString("4556A0A0-FCE2-44C6-98F8-C2303E9CE8CA", "Duration");

		public string RequiredScoreLabelText => Res.GetString("C0045AB5-82C4-47B8-8198-E5C60419C612", "Required Score %");

		public string InclLabelText => Res.GetString("B7793EDE-0602-4070-8162-D2C1679E3029", "Incl");

		public string StatusLabelText => Res.GetString("51488CCD-0845-44DF-8415-68182F075651", "Status");

		public string CompletionDateLabelText => Res.GetString("7DB58B4C-776E-4281-A7CC-6010D2D3B902", "Completion Date");

		void SetFooterText()
		{
			var footerText = string.Empty;

			if (DataSource.Campaign?.IsExamCampaign ?? false)
			{
				footerText = RecruiterDataRegistry.Instance.ExamLandingPageFooterText.Value;
			}

			if (!string.IsNullOrWhiteSpace(footerText))
			{
				FooterTextForExamLiteral.Text = footerText;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html formatting")]
		void CampaignDetailRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.DataItem is KeyValuePair<string, string> &&
				string.IsNullOrEmpty(((KeyValuePair<string, string>)e.Item.DataItem).Key))
			{
				foreach (Control control in e.Item.Controls)
				{
					control.Dispose();
				}
				e.Item.Controls.Clear();
				e.Item.Controls.Add(new LiteralControl("<tr><td colspan='2'>&nbsp;</td></tr>"));
			}
		}

		protected void StartButton_Click(object sender, EventArgs e)
		{
			Redirect();
		}

		protected void ResultButton_Click(object sender, EventArgs e)
		{
			Server.Transfer("Submission.aspx");
		}

		#endregion

		#region Redirect

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tracing")]
		void Redirect()
		{
			var result = Request.Url.Query;
			TraceSource?.TraceEvent(TraceEventType.Information, 2007, "Before Response Redirect to Default Page");
			Response.Redirect(string.Concat(AppInstance.DefaultPage, result));
		}

		#endregion

		#region Data Source

		protected override bool IsPersistDataSourceBetweenPostbacks => false;

		public new VoteExamSurveyLauncher DataSource => (VoteExamSurveyLauncher)base.DataSource;

		protected override BusinessObject GetNewDataSource()
		{
			var campaignItemPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignItemSchema.Constants.PK);
			var campaignPK = CampaignPK;
			var countryCode = SecureQueryStringHelper.GetQuestionsCountryCode(Request.QueryString);
			var expiryDate = SecureQueryStringHelper.GetZDate(Request.QueryString, VoteExamSurveyUrlHelper.TestExpiryDateStringKey);
			var examSettingsCode = SecureQueryStringHelper.GetValue(Request.QueryString, VoteExamSurveyUrlHelper.ExamSettingsStringKey);
			var recipientInfo = SecureQueryStringHelper.GetRecipientInfo(Request.QueryString);
			if (recipientInfo == null)
			{
				return new VoteExamSurveyLauncher(Factory, campaignItemPK, campaignPK, countryCode, expiryDate, examSettingsCode);
			}
			else
			{
				return new VoteExamSurveyLauncher(Factory, campaignItemPK, campaignPK, countryCode, expiryDate, examSettingsCode,
					recipientInfo.recipientPK, recipientInfo.recipientTableCode);
			}
		}

		protected virtual string ContentMovedPage
		{
			get { return "https://myaccount.cargowise.com/Home/ContentMoved.aspx"; }
		}

		#endregion

		#region Campaign

		ZGuid fCampaignPK;

		ZGuid CampaignPK
		{
			get
			{
				if (fCampaignPK.IsEmpty)
				{
					fCampaignPK = SecureQueryStringHelper.GetZGuid(Request.QueryString, GlbCompanyCampaignSchema.Constants.PK);
				}
				return fCampaignPK;
			}
		}

		GlbCompanyCampaign fCampaign;

		public GlbCompanyCampaign Campaign => fCampaign ?? (fCampaign = Factory.Load<GlbCompanyCampaign>(CampaignPK));

		#endregion

		#region Scheduled System Upgrade Warning

		void DisplayScheduledSystemUpgradeWarning(VoteExamSurveySystemUpgradeScheduleTaskWarning warning)
		{
			var warningMessage = warning.Message;
			var showWarning = warningMessage.Length > 0;

#if DEBUG
			if (WarningCaptionLabel == null)
			{
				WarningCaptionLabel = new ZTextLabel();
			}
			if (WarningMessageLabel == null)
			{
				WarningMessageLabel = new ZTextLabel();
			}
#endif

			WarningCaptionLabel.Text = warning.CaptionText;
			WarningCaptionLabel.Visible = showWarning;

			WarningMessageLabel.Text = warningMessage;
			WarningMessageLabel.Visible = showWarning;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html script")]
		public string GetHTMLForIsRequiredCheckBox(object canTakeExam, object isRequired)
		{
			bool canTakeExam1 = (bool)canTakeExam;
			bool isRequired1 = (bool)isRequired;
			if (!canTakeExam1)
			{
				return "disabled=\"disabled\"";
			}
			else
			{
				if (isRequired1)
				{
					return "checked=\"checked\"";
				}
				else
				{
					return "";
				}
			}
		}
	}
}
