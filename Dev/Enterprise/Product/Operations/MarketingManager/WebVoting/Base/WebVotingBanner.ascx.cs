using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	public class WebVotingBanner : BaseUserControl
	{
		protected System.Web.UI.HtmlControls.HtmlTable BannerTable;
		protected System.Web.UI.HtmlControls.HtmlTableCell BannerCell;

		protected
#if DEBUG
		internal
#endif
		HyperLink LogoImage;

		protected Panel CountdownPanel;
		protected Label CountdownLabel;
		protected HiddenField CountdownHiddenField;

		protected Label WarningCaptionLabel;

		protected
#if DEBUG
		internal
#endif
		Label WarningMessageLabel;

#if DEBUG
		protected internal ZPage PageInternal => Page;
#endif

#if DEBUG
		internal
#endif
		void Page_Load(object sender, EventArgs e)
		{
			LogoImage.ImageUrl = ZAppInstance.LogoImage;
			LogoImage.NavigateUrl = ZAppInstance.HomePage;
			LogoImage.ToolTip = ZAppInstance.CompanyName;
			LogoImage.CssClass = "LogoImage";

			DisplayScheduledSystemUpgradeWarning(new VoteExamSurveySystemUpgradeScheduleTaskWarning(), IsDefaultPage);
			SetupCountdownIfApplicable();
			SetupClientScriptBlocks();
		}

		#region Scheduled System Upgrade Warning

#if DEBUG
		internal
#endif
		void DisplayScheduledSystemUpgradeWarning(VoteExamSurveySystemUpgradeScheduleTaskWarning warning, bool isDefaultPage)
		{
			var warningMessage = warning.Message;
			var showWarning = warningMessage.Length > 0 && isDefaultPage;

#if DEBUG
			if (WarningCaptionLabel == null)
			{
				WarningCaptionLabel = new Label();
			}
			if (WarningMessageLabel == null)
			{
				WarningMessageLabel = new Label();
			}
#endif

			WarningCaptionLabel.Text = warning.CaptionText;
			WarningCaptionLabel.Visible = showWarning;

			WarningMessageLabel.Text = warningMessage;
			WarningMessageLabel.Visible = showWarning;
		}

		bool IsDefaultPage
		{
			get
			{
				var defaultPage = Page as Default;
				return defaultPage != null;
			}
		}

		#endregion

		#region Setup Scripts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		void SetupClientScriptBlocks()
		{
			if ((Page as Default) != null)
			{
				string manualSubmitRef = ((Page)Page).ClientScript.GetPostBackEventReference(Page, Default.ManualSubmission);
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ManualSubmitFunction", ZString.Format("function manualSubmit() {{{0}}}", manualSubmitRef), true);
				string cancelSubmitRef = ((Page)Page).ClientScript.GetPostBackEventReference(Page, Default.CancelSubmission);
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "CancelSubmitFunction", ZString.Format("function cancelSubmit() {{{0}}}", cancelSubmitRef), true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant, May be an identifier or GUID.")]
		void SetupCountdownIfApplicable()
		{
			Default defaultPage = Page as Default;
			if (defaultPage != null)
			{
				IVoteExamSurveyAnswerSet answerSet = defaultPage.DataSource;
				TimeSpan remainingDuration = (answerSet != null) ? answerSet.RemainingDuration : TimeSpan.Zero;
				if (remainingDuration.TotalSeconds > 0)
				{
					CountdownPanel.Visible = true;
					string hiddenFieldName = ZGuid.NewZGuid().ToString();
					Page.ZClientScript.RegisterHiddenField(hiddenFieldName, ((int)Math.Ceiling(remainingDuration.TotalSeconds)).ToString());
					string startCountdownTimerScript = string.Format("getVarsAndStartCountdown('{0}', '{1}', '{2}');", CountdownPanel.ClientID, CountdownLabel.ClientID, hiddenFieldName);
					Page.ZClientScript.RegisterStartupScript(GetType(), "StartCountdownTimer", startCountdownTimerScript, true);
					string autoSubmitRef = ((Page)Page).ClientScript.GetPostBackEventReference(Page, Default.AutoSubmission);
					Page.ZClientScript.RegisterClientScriptBlock(GetType(), "AutoSubmitFunction", string.Format("function autoSubmit() {{{0}}}", autoSubmitRef), true);
				}
			}
		}

		public string TimeExpiredConfirmationText =>
			Res.GetString("06f1d1da-3ff6-417e-a5bd-482f14265fb5", "Time Expired. Would you like to submit your answers?").Replace("'", "\\'");

		#endregion

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.Load += new EventHandler(this.Page_Load);
		}

		#endregion
	}
}
