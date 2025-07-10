using System.Linq;
using System.Web.UI;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.WebVoting
{
	public class DefaultPageForTest : Default, IPostBackEventHandler
	{
		#region InternalMembers
#if DEBUG
		internal void OnLoadInternal(System.EventArgs e) => OnLoad(e);
		internal void SetSubmitButtonInternal(ZArchitecture.Web.GUI.WebControls.ZButton button) => SubmitButton = button;
		internal ZArchitecture.Web.GUI.WebControls.ZButton SubmitButtonInternal => SubmitButton;
		internal void SetContentPlaceHolderInternal(System.Web.UI.WebControls.PlaceHolder placeholder) => ContentPlaceHolder = placeholder;
		internal System.Web.UI.WebControls.PlaceHolder ContentPlaceHolderInternal => ContentPlaceHolder;
		internal void SetVoteTitleLabelInternal(ZArchitecture.Web.GUI.WebControls.ZTextLabel label) => VoteTitleLabel = label;
		internal bool SupressErrorDialogInternal => SuppressErrorDialog;
		internal bool ShowLoginStatusInternal => ShowLoginStatus;
		internal bool IsPersistDataSourceBetweenPostbacksInternal => IsPersistDataSourceBetweenPostbacks;
		internal bool ShowLogOffLinkButtonInternal => ShowLogOffLinkButton;
		internal BusinessObject GetNewDataSourceInternal() => GetNewDataSource();
		internal void OnPreRenderInternal(System.EventArgs e) => OnPreRender(e);
		internal void OnPreRenderCompleteInternal(System.EventArgs e) => OnPreRenderComplete(e);
		internal string GetSubmissionRedirectParametersInternal() => GetSubmissionRedirectParameters();
		internal void SubmitButton_ClickInternal(object sender, System.EventArgs e) => SubmitButton_Click(sender, e);
#endif
		#endregion

		bool DataSourceTimeExpired;
		public void SetDataSourceTimeExpired(bool bExpired)
		{
			DataSourceTimeExpired = bExpired;
		}
		public new IVoteExamSurveyAnswerSet DataSource
		{
			get
			{
				if (DataSourceTimeExpired)
				{
					return null;
				}
				else
				{
					return base.DataSource;
				}
			}
		}

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
							ReportTimeExpired("Your exam can’t be submitted, as not all questions have been answered.");
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
							ReportTimeExpired("Your exam is not submitted.");
						}
						else if ((item.CompanyCampaign?.IsExamCampaign ?? false) && (!item.AnswerWrappers.Any() || item.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().Any(x => !x.IsAnswered)))
						{
							ReportTimeExpired("Your exam can’t be submitted, as not all questions have been answered.");
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
						ReportTimeExpired("Your exam is not submitted.");
						break;
					}
			}
		}
	}
}
