using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyPreviewBizO : NonPersistentBusinessObject, IVoteExamSurveyAnswerSet, IObsoleteValidation
	{
		public VoteExamSurveyPreviewBizO(GlbCompanyCampaign campaign)
			: base(campaign.Factory)
		{
			this.Campaign = campaign;
		}

		public GlbCompanyCampaign CompanyCampaign
		{
			get { return Campaign; }
		}

		public VoteExamSurveyAnswerWrapperCollection AnswerWrappers
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.AnswerWrappers; }
		}

		public VoteExamSurveyAnswerCollectionDictionary PersistedAnswers
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.PersistedAnswers; }
		}

		void IVoteExamSurveyAnswerSet.SubmitAnswerSet(bool shouldCompleteAccreditations)
		{
		}

		bool IVoteExamSurveyAnswerSet.HasLoadedAnswerWrappers { get; }

		void IVoteExamSurveyAnswerSet.ClearSubmissionDate()
		{
		}
		ZString IVoteExamSurveyAnswerSet.HtmlSubmissionCompletedMessage { get; }

		ZDateTime IVoteExamSurveyAnswerSet.ReferenceDateForQuestionRandomiserSeed { get; }

		public ZShort CurrentPage
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.CurrentPage; }
			set { NonSavableVoteExamSurveyAnswerSetForPreview.CurrentPage = value; }
		}

		public ZPropertyInfo CurrentPageInfo
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview == null ? null : NonSavableVoteExamSurveyAnswerSetForPreview.CurrentPageInfo; }
		}

		public ZShort PageCount
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.PageCount; }
		}

		public ZPropertyInfo PageCountInfo
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview == null ? null : NonSavableVoteExamSurveyAnswerSetForPreview.PageCountInfo; }
		}

		public CodeDescriptionPairList PageNumbers
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.PageNumbers; }
		}

		public PagedVoteExamSurveyAnswerWrapperCollection PagedAnswerWrappers
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.PagedAnswerWrappers; }
		}

		ZString IVoteExamSurveyAnswerSet.CurrentQuestionsCountryCode
		{
			get { return NonSavableVoteExamSurveyAnswerSetForPreview.CurrentQuestionsCountryCode; }
			set { NonSavableVoteExamSurveyAnswerSetForPreview.CurrentQuestionsCountryCode = value; }
		}

		IVoteExamSurveyAnswerSet NonSavableVoteExamSurveyAnswerSetForPreview
		{
			get
			{
#if DEBUG
				if (!Campaign.IsInDatabase)
				{
					Campaign.FillWithValidTestData();
					Campaign.Factory.Save();
				}
#endif

				if (fNonSavableVoteExamSurveyAnswerSetForPreview == null && Campaign != null)
				{
					var fNonSavableCampaignItemForPreview = (GlbCompanyCampaignItem)NonSavableLocalFactory.New(Campaign.CampaignsItemsSent.TypeOfElements);
					fNonSavableCampaignItemForPreview.G8_G0 = Campaign.PK;
					fNonSavableCampaignItemForPreview.G8_RecipientID = NonSavableLocalFactory.New<OrgContact>().PK;
					fNonSavableVoteExamSurveyAnswerSetForPreview = new VoteExamSurveyAnswerSet(NonSavableLocalFactory, fNonSavableCampaignItemForPreview, Campaign);
					RegisterEditableChildObject(fNonSavableVoteExamSurveyAnswerSetForPreview);
				}
				return fNonSavableVoteExamSurveyAnswerSetForPreview;
			}
		}

		protected BusinessObjectFactory NonSavableLocalFactory
		{
			get
			{
				if (fNonSavableLocalFactory == null)
				{
					fNonSavableLocalFactory = CreateNonSavableLocalFactory();
					fNonSavableLocalFactory.Saving += delegate
					{
						throw new NotSupportedException("Factory for NonSavableCampaignItem in VoteExamSurveyPreviewBizO should not be saved");
					};
				}
				return fNonSavableLocalFactory;
			}
		}

		protected virtual BusinessObjectFactory CreateNonSavableLocalFactory()
		{
			return new BusinessObjectFactory();
		}

		readonly GlbCompanyCampaign Campaign;
		IVoteExamSurveyAnswerSet fNonSavableVoteExamSurveyAnswerSetForPreview;
		BusinessObjectFactory fNonSavableLocalFactory;

		#region IVoteExamSurveyAnswerSet Explicit Members

		bool IVoteExamSurveyAnswerSet.HasPreviousSessionEnded
		{
			get { return false; }
		}

		bool IVoteExamSurveyAnswerSet.IsPreview
		{
			get { return true; }
		}

		bool IVoteExamSurveyAnswerSet.AutoSaveAnswers
		{
			get { return false; }
		}

		string IVoteExamSurveyAnswerSet.SubmissionConfirmationMessage
		{
			get { return ""; }
		}

		void IVoteExamSurveyAnswerSet.StartVoteExamSurvey()
		{
			NonSavableVoteExamSurveyAnswerSetForPreview.StartVoteExamSurvey();
		}

		public int GetSeedForQuestionRandomiser()
		{
			return 0;
		}

		public void MergeAnswers()
		{
		}

		TimeSpan IVoteExamSurveyAnswerSet.RemainingDuration
		{
			get { return TimeSpan.Zero; }
		}

		bool IVoteExamSurveyAnswerSet.IsContinuingPreviousAttempt
		{
			get { return false; }
		}

		bool IVoteExamSurveyAnswerSet.CanAutoStartVoteExamSurvey
		{
			get { return false; }
		}

		public List<GlbCompanyCampaignItem> ExamCampaignItems { get => NonSavableVoteExamSurveyAnswerSetForPreview.ExamCampaignItems; }

		public string JobSkillCode
		{
			get => fNonSavableVoteExamSurveyAnswerSetForPreview.JobSkillCode;
		}

		public string ExamVersion
		{
			get { return fNonSavableVoteExamSurveyAnswerSetForPreview.ExamVersion; }
		}

		void IVoteExamSurveyAnswerSet.SetJobSkillAndVersion(string jobSkillCode, string examVersion)
		{
			fNonSavableVoteExamSurveyAnswerSetForPreview.SetJobSkillAndVersion(jobSkillCode, examVersion);
		}
		#endregion
	}
}
