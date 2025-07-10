using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MarketingManager.GUI
{
	public abstract class VoteExamSurveyPlugIn : ZPlugIn
	{
		public VoteExamSurveyPlugIn(GlbCompanyCampaign campaign)
			: base(campaign)
		{
			HookValueChangedEvents();
			ToggleEnabled();
		}

		protected override sealed Control GetNewUserControl()
		{
			VoteExamSurveyUserControl result = new VoteExamSurveyUserControl();
			result.QuestionsControl = GetNewQuestionsControl();
			result.QuestionDetailsControl = GetNewQuestionDetailsControl();

			result.ResultsControlOverride = GetNewResultsControlOverride();
			if (result.ResultsControlOverride == null)
			{
				result.ResultsByRecipientControl = GetNewResultsByRecipientControl();
				result.ResultsByQuestionControl = GetNewResultsByQuestionControl();
				result.ResultsSummaryControl = GetNewResultSummaryControl();
			}
			return result;
		}

		protected virtual QuestionsUserControl GetNewQuestionsControl()
		{
			return new QuestionsUserControl();
		}

		protected virtual QuestionDetailsUserControl GetNewQuestionDetailsControl()
		{
			return new QuestionDetailsUserControl();
		}

		protected virtual ResultsByRecipientUserControl GetNewResultsByRecipientControl()
		{
			return new ResultsByRecipientUserControl(BusinessEntity);
		}

		protected virtual ResultsByQuestionUserControl GetNewResultsByQuestionControl()
		{
			return new ResultsByQuestionUserControl();
		}

		protected virtual ZUserControl GetNewResultSummaryControl()
		{
			return new ResultsSummaryUserControl();
		}

		protected virtual Control GetNewResultsControlOverride()
		{
			return null;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		public new GlbCompanyCampaign BusinessEntity
		{
			get { return (GlbCompanyCampaign)base.BusinessEntity; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HostBusinessEntity;
		}

		protected virtual void HookValueChangedEvents()
		{
			HookValueChangedEventToToggleEnabled(BusinessEntity.G0_BroadcastVoteSurveyExamInfo);
		}

		protected void HookValueChangedEventToToggleEnabled(ZPropertyInfo info)
		{
			info.ValueChanged += delegate
			{ ToggleEnabled(); };
		}

		void HandleValueChanged(object sender, EventArgs args)
		{
			ToggleEnabled();
		}

		void ToggleEnabled()
		{
			Enabled = ShouldBeEnabled;

			if (Enabled)
			{
				(UserControl as VoteExamSurveyUserControl)?.SetDataBinding(BusinessEntity, "");
			}
		}

		protected virtual bool ShouldBeEnabled
		{
			get { return BusinessEntity.G0_BroadcastVoteSurveyExam == SupportedCampaignType; }
		}

		protected abstract string SupportedCampaignType { get; }
	}
}
