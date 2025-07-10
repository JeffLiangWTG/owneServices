using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswerSet : NonPersistentBusinessObject, IVoteExamSurveyAnswerSet
	{
		public VoteExamSurveyAnswerSet(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public VoteExamSurveyAnswerSet(BusinessObjectFactory factory, List<GlbCompanyCampaignItem> campaignItems)
			: this(factory)
		{
			ExamCampaignItems = campaignItems;
		}

		public VoteExamSurveyAnswerSet(BusinessObjectFactory factory, GlbCompanyCampaignItem campaignItem)
			: this(factory, new List<GlbCompanyCampaignItem> { campaignItem })
		{
		}

		public VoteExamSurveyAnswerSet(BusinessObjectFactory factory, List<GlbCompanyCampaignItem> campaignItems, GlbCompanyCampaign campaign)
			: this(factory, campaignItems)
		{
			companyCampaign = campaign;
		}

		public VoteExamSurveyAnswerSet(BusinessObjectFactory factory, GlbCompanyCampaignItem campaignItem, GlbCompanyCampaign campaign)
			: this(factory, new List<GlbCompanyCampaignItem> { campaignItem }, campaign)
		{
		}

		void IVoteExamSurveyAnswerSet.StartVoteExamSurvey()
		{
			StartVoteExamSurvey();
		}

		List<GlbCompanyCampaignItem> IVoteExamSurveyAnswerSet.ExamCampaignItems => ExamCampaignItems;

		public virtual List<GlbCompanyCampaignItem> ExamCampaignItems { get; }

		string IVoteExamSurveyAnswerSet.JobSkillCode
		{
			get { return JobSkillCode; }
		}
		public string JobSkillCode { get; private set; }
		public string ExamVersion { get; private set; }

		void IVoteExamSurveyAnswerSet.SetJobSkillAndVersion(string jobSkillCode, string examVersion)
		{
			JobSkillCode = jobSkillCode;
			ExamVersion = examVersion;
		}

		public ZPropertyInfo HtmlSubmissionCompleteMessageInfo
		{
			get { return GetZPropertyInfo(nameof(HtmlSubmissionCompletedMessage)); }
		}

		public virtual ZString HtmlSubmissionCompletedMessage
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Res.GetString("dc67be97-d97f-42c3-9635-8ec7e445b398", "Your answers have been successfully submitted to our server."));
				builder.Append(" " + Res.GetString("5dcf1a2d-f603-447a-aadb-fb9d1ab1576f", "Thank you for participating."));
				return builder.ToString();
			}
		}

		public ZString VoteExamSurveyCampaignURL
		{
			get { return VoteExamSurveyUrlHelper.GetCampaignUrl(ExamCampaignItems.FirstOrDefault()); }
		}

		public bool IsStageTaken { get { return ExamCampaignItems.Any(x => x.G8_Stage == GlbCompanyCampaignItemLookups.StagesConstants.Taken); } }

		bool IVoteExamSurveyAnswerSet.HasPreviousSessionEnded
		{
			get { return HasPreviousSessionEnded; }
		}

		public virtual bool HasPreviousSessionEnded
		{
			get { return ExamCampaignItems.All(x => x.HasPreviousSessionEnded); }
		}

		bool IVoteExamSurveyAnswerSet.CanAutoStartVoteExamSurvey
		{
			get { return ExamCampaignItems.All(x => x.CanAutoStartVoteExamSurvey); }
		}

		protected virtual bool CanAutoStartVoteExamSurvey
		{
			get { return IsContinuingPreviousAttempt; }
		}

		bool IVoteExamSurveyAnswerSet.IsPreview
		{
			get { return false; }
		}

		bool IVoteExamSurveyAnswerSet.AutoSaveAnswers
		{
			get { return AutoSaveAnswers; }
		}

		protected virtual bool AutoSaveAnswers
		{
			get { return OrganisationsDataRegistry.Instance.AutoSaveVoteSurveyExamAnswers.Value; }
		}

		bool IVoteExamSurveyAnswerSet.IsContinuingPreviousAttempt
		{
			get { return IsContinuingPreviousAttempt; }
		}

		protected virtual bool IsContinuingPreviousAttempt
		{
			get { return ExamCampaignItems.All(x => x.IsContinuingPreviousAttempt); }
		}

		TimeSpan IVoteExamSurveyAnswerSet.RemainingDuration
		{
			get { return RemainingDuration; }
		}

		protected virtual TimeSpan RemainingDuration
		{
			get { return TimeSpan.Zero; }
		}

		string IVoteExamSurveyAnswerSet.SubmissionConfirmationMessage
		{
			get { return SubmissionConfirmationMessage; }
		}

		public virtual bool ResetVoteSurveyExam(bool forceReset)
		{
			return true;
		}

		protected virtual string SubmissionConfirmationMessage
		{
			get { return Res.GetString("e0749c51-c17e-47b7-b7b6-af3c5760a3e4", "Once submitted, you will not be able to modify your answers. Are you sure?"); }
		}

		public virtual GlbCompanyCampaign CompanyCampaign => companyCampaign ?? (companyCampaign = ExamCampaignItems.FirstOrDefault()?.CompanyCampaign);
		GlbCompanyCampaign companyCampaign;

		[ChildEditable(true)]
		public VoteExamSurveyAnswerWrapperCollection AnswerWrappers
		{
			get
			{
				if (answerWrappers == null)
				{
					answerWrappers = GetNewAnswerWrappers();
					answerWrappers.Load();
					RegisterEditableChildObject(answerWrappers);
				}
				return answerWrappers;
			}
		}

		VoteExamSurveyAnswerWrapperCollection answerWrappers;

		public bool HasLoadedAnswerWrappers
		{
			get { return answerWrappers != null; }
		}

		protected virtual VoteExamSurveyAnswerWrapperCollection GetNewAnswerWrappers()
		{
			VoteExamSurveyAnswerWrapperCollection answerWrapperCollection = null;
			foreach (var item in ExamCampaignItems)
			{
				if (answerWrapperCollection == null)
				{
					answerWrapperCollection = new VoteExamSurveyAnswerWrapperCollection(this, item, CompanyCampaign.G0_QuestionsPerWebPage);
				}
				else
				{
					answerWrapperCollection.AddRange(new VoteExamSurveyAnswerWrapperCollection(this, item, CompanyCampaign.G0_QuestionsPerWebPage));
				}
				item.AnswerWrappers = answerWrapperCollection;
			}
			return answerWrapperCollection;
		}

		public VoteExamSurveySubmittedAnswerCollection SubmittedAnswers
		{
			get
			{
				if (fSubmittedAnswers == null)
				{
					fSubmittedAnswers = GetNewSubmittedAnswers();
					fSubmittedAnswers.Load();
				}
				return fSubmittedAnswers;
			}
		}
		VoteExamSurveySubmittedAnswerCollection fSubmittedAnswers;

		protected virtual VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswers()
		{
			return new VoteExamSurveySubmittedAnswerCollection(this);
		}

		public VoteExamSurveyAnswerCollectionDictionary PersistedAnswers
		{
			get
			{
				if (persistedAnswers == null)
				{
					persistedAnswers = new VoteExamSurveyAnswerCollectionDictionary(this);
				}
				return persistedAnswers;
			}
		}
		VoteExamSurveyAnswerCollectionDictionary persistedAnswers;

		protected void InvalidateAnswersSets()
		{
			fSubmittedAnswers = null;
			persistedAnswers = null;
		}

		public void MergeAnswers()
		{
			foreach (var campaignItem in ExamCampaignItems)
			{
				campaignItem.MergeAnswers();
			}
		}

		ZString IVoteExamSurveyAnswerSet.CurrentQuestionsCountryCode
		{
			get { return CurrentQuestionsCountryCode; }
			set
			{
				foreach (var campaignItem in ExamCampaignItems)
				{
					campaignItem.CurrentQuestionsCountryCode = value;
				}
				CurrentQuestionsCountryCode = value;
			}
		}

		public ZString CurrentQuestionsCountryCode
		{
			get { return currentQuestionsCountryCode; }
			set
			{
				if (currentQuestionsCountryCode != value)
				{
					currentQuestionsCountryCode = value;
				}
			}
		}

		ZString currentQuestionsCountryCode;

		public ZShort CurrentPage
		{
			get { return currentPage; }
			set
			{
				if (value != currentPage)
				{
					SetNonPersistentPropertyValue(CurrentPageInfo, ref currentPage, value);
					PagedAnswerWrappers.Rebuild();
				}
			}
		}
		ZShort currentPage = 1;

		public ZPropertyInfo CurrentPageInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentPage)); }
		}

		public ZShort PageCount
		{
			get
			{
				if (CompanyCampaign != null)
				{
					var wrappers = AnswerWrappers;
					var questionsPerPage = CompanyCampaign.G0_QuestionsPerWebPage;

					if (questionsPerPage > 0 && wrappers.Count > questionsPerPage)
					{
						return (short)Math.Ceiling((decimal)wrappers.NonHeaderCount / questionsPerPage);
					}
				}

				return 1;
			}
		}

		public ZPropertyInfo PageCountInfo
		{
			get { return GetZPropertyInfo(nameof(PageCount)); }
		}
		public CodeDescriptionPairList PageNumbers
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				for (int i = 1; i <= PageCount; i++)
				{
					result.AddPair(i.ToString(CultureInfo.InvariantCulture));
				}
				return result;
			}
		}

		public PagedVoteExamSurveyAnswerWrapperCollection PagedAnswerWrappers
		{
			get
			{
				if (pagedAnswerWrappers == null)
				{
					pagedAnswerWrappers = new PagedVoteExamSurveyAnswerWrapperCollection(this);
					pagedAnswerWrappers.Rebuild();
				}
				return pagedAnswerWrappers;
			}
		}
		PagedVoteExamSurveyAnswerWrapperCollection pagedAnswerWrappers;

		protected virtual void StartVoteExamSurvey()
		{
			ZShort questionOrder = 0;
			foreach (var companyCampaignItem in ExamCampaignItems)
			{
				companyCampaignItem.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Taken;
				if (!companyCampaignItem.HasPreviousSessionEnded)
				{
					PersistedAnswers.GetCampaignItemAnswers(companyCampaignItem).RemoveAndDeleteAll();
					companyCampaignItem.InvalidateAnswersSets();
					companyCampaignItem.CurrentQuestionsCountryCode = CurrentQuestionsCountryCode;
				}
			}

			var wrappers = AnswerWrappers;
			foreach (var companyCampaignItem in ExamCampaignItems)
			{
				if (!companyCampaignItem.HasPreviousSessionEnded)
				{
					var campaign = companyCampaignItem.CompanyCampaign;
					var relevantCampaignQuestions = campaign.GetActualQuestions(CurrentQuestionsCountryCode);
					var randomisedQuestions = wrappers.RandomiseQuestionsIfRequired(relevantCampaignQuestions, campaign).ToArray();

					for (var index = 0; index < randomisedQuestions.Length; index++)
					{
						var question = randomisedQuestions[index];

						if (!question.IsHeader || index == randomisedQuestions.Length - 1)
						{
							questionOrder++;
						}
						var newAnswerWrapper = new VoteExamSurveyAnswerWrapper(question, this, companyCampaignItem, CompanyCampaign.G0_QuestionsPerWebPage, questionOrder);
						newAnswerWrapper.Answer.HZ_QuestionOrder = questionOrder;
						wrappers.Add(newAnswerWrapper);

						if (!question.IsSubQuestion)
						{
							ZShort subQuestionOrder = 0;
							foreach (VoteExamSurveyQuestion subQuestion in RandomiseSubQuestionsIfRequired(question, campaign.RandomiseQuestionAndMultipleChoiceOrder))
							{
								subQuestionOrder++;
								var answer = PersistedAnswers.LoadOrCreateNew(subQuestion, companyCampaignItem);
								answer.HZ_QuestionOrder = questionOrder;
								answer.HZ_SubQuestionOrder = subQuestionOrder;
							}
						}
					}
				}
			}
		}

		protected IEnumerable<VoteExamSurveyQuestion> RandomiseSubQuestionsIfRequired(VoteExamSurveyQuestion question, bool isCampaignRandomiseQuestionAndMultipleChoiceOrder)
		{
			if (question.Campaign != null && isCampaignRandomiseQuestionAndMultipleChoiceOrder && question.HY_IsRandomisable)
			{
				Random randomiser = GetNewRandom();
				var subQuestionList = new List<VoteExamSurveyQuestion>(question.SubQuestions.Cast<VoteExamSurveyQuestion>());

				while (subQuestionList.Count > 0)
				{
					VoteExamSurveyQuestion subQuestion = subQuestionList[randomiser.Next(0, subQuestionList.Count)];
					subQuestionList.Remove(subQuestion);
					yield return subQuestion;
				}
			}
			else
			{
				foreach (VoteExamSurveyQuestion subQuestion in question.SubQuestions)
				{
					yield return subQuestion;
				}
			}
		}

		Random GetNewRandom()
		{
#if DEBUG
			if (ZArchitecture.Environment.Globals.IsTest)
			{
				return new Random(31081);
			}
#endif
			return new Random(GetSeedForQuestionRandomiser());
		}

		public int GetSeedForQuestionRandomiser()
		{
			ZDateTime arbitraryDate = new ZDateTime(2008, 1, 1);
			return (int)((ReferenceDateForQuestionRandomiserSeed - arbitraryDate).TotalMilliseconds % int.MaxValue);
		}

		ZDateTime IVoteExamSurveyAnswerSet.ReferenceDateForQuestionRandomiserSeed { get { return ReferenceDateForQuestionRandomiserSeed; } }

		protected virtual ZDateTime ReferenceDateForQuestionRandomiserSeed
		{
			get { return ZDateTime.Now; }
		}

		void IVoteExamSurveyAnswerSet.SubmitAnswerSet(bool shouldCompleteAccreditations)
		{
			SubmitAnswerSet(shouldCompleteAccreditations);
		}
		protected virtual void SubmitAnswerSet(bool shouldCompleteAccreditations)
		{
			foreach (var campaignItem in ExamCampaignItems)
			{
				campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
				campaignItem.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Submitted;
			}
		}

		void IVoteExamSurveyAnswerSet.ClearSubmissionDate()
		{
			ClearSubmissionDate();
		}

		protected virtual void ClearSubmissionDate()
		{
			foreach (var campaignItem in ExamCampaignItems)
			{
				campaignItem.G8_ClosedDateUtc = ZDateTime.Empty;
				campaignItem.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Taken;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			foreach (var campaignItem in ExamCampaignItems)
			{
				campaignItem.RunPreSaveValidation();
			}
		}
	}
}
