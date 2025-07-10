using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySubmittedAnswerCollection : NonPersistentBusinessObjectCollection<VoteExamSurveySubmittedAnswer>
	{
		VoteExamSurveySubmittedAnswerCollection(GlbCompanyCampaign campaign)
			: base(campaign != null ? campaign.Factory : null)
		{
			this.campaign = campaign;
		}
		public VoteExamSurveySubmittedAnswerCollection(VoteExamSurveyAnswerSet voteExamSurveyAnswerSet)
			: this(voteExamSurveyAnswerSet.CompanyCampaign)
		{
			campaignItems = voteExamSurveyAnswerSet.ExamCampaignItems;
			this.persistedAnswers = voteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers;
			questions =
				(from VoteExamSurveyAnswer persistedAnswer in this.persistedAnswers
				 select (!persistedAnswer.Question.IsSubQuestion || campaign.IsVoteCampaign)
					? persistedAnswer.Question
					: persistedAnswer.Question.GetParentQuestion()).Distinct();
		}

		public VoteExamSurveySubmittedAnswerCollection(GlbCompanyCampaignItem campaignItem)
			: this(campaignItem.CompanyCampaign)
		{
			campaignItems = new[] { campaignItem };

			if (campaign != null)
			{
				questions =
					(from VoteExamSurveyAnswer persistedAnswer in campaignItem.PersistedAnswers
					 select (!persistedAnswer.Question.IsSubQuestion || campaignItem.CompanyCampaign.IsVoteCampaign)
						 ? persistedAnswer.Question
						 : persistedAnswer.Question.GetParentQuestion()).Distinct();
			}
			else
			{
				questions = Enumerable.Empty<VoteExamSurveyQuestion>();
			}
		}

		public VoteExamSurveySubmittedAnswerCollection(VoteExamSurveyQuestion question)
			: this(question.Campaign)
		{
			questions = new[] { question };
			campaignItems = (question.Campaign != null)
				? question.Campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
				: Enumerable.Empty<GlbCompanyCampaignItem>();
		}

		public ZInt PopulatedAnswersCount
		{
			get
			{
				if (populatedAnswersCount == null)
				{
					populatedAnswersCount = (ZInt)PopulatedAnswers.Count();
				}
				return populatedAnswersCount.Value;
			}
		}

		ZInt? populatedAnswersCount;

		public override void Load()
		{
			RemoveAll();

			foreach (GlbCompanyCampaignItem campaignItem in campaignItems)
			{
				foreach (VoteExamSurveyQuestion question in questions)
				{
					if (ShouldCreateSubmittedAnswer(campaignItem, question))
					{
						Add(GetNewSubmittedAnswer(campaignItem, question));
					}
				}
			}

			if (SortInformation == null)
			{
				Sort(new SubmittedAnswerComparer(campaign));
			}
			IsLoaded = true;
		}

		protected virtual bool ShouldCreateSubmittedAnswer(GlbCompanyCampaignItem campaignItem, VoteExamSurveyQuestion question)
		{
			return !question.IsHeader && (campaignItem.CompanyCampaign.PK == question.Campaign.PK) && (!campaign.IsVoteCampaign || HasVote(campaignItem, question));
		}

		protected virtual VoteExamSurveySubmittedAnswer GetNewSubmittedAnswer(GlbCompanyCampaignItem item, VoteExamSurveyQuestion question)
		{
			return new VoteExamSurveySubmittedAnswer(item, question);
		}

		public IEnumerable<VoteExamSurveySubmittedAnswer> PopulatedAnswers
		{
			get
			{
				if (IsLoaded)
				{
					foreach (VoteExamSurveySubmittedAnswer submittedAnswer in this)
					{
						if (submittedAnswer.IsPopulated)
						{
							yield return submittedAnswer;
						}
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Should not create new SubmittedAnswer");
		}

		bool HasVote(GlbCompanyCampaignItem campaignItem, VoteExamSurveyQuestion voteItem)
		{
			return campaignItem.PersistedAnswers.FindByQuestion(voteItem) != null;
		}

		protected override bool AllowSort
		{
			get { return true; }
		}

		public readonly GlbCompanyCampaign campaign;
		public readonly IEnumerable<GlbCompanyCampaignItem> campaignItems;
		public readonly IEnumerable<VoteExamSurveyQuestion> questions;
		public readonly IEnumerable<VoteExamSurveyAnswer> persistedAnswers;

		#region Comparers

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return (property.Name.EndsWith("ActualOrderForBinding"))
			  ? new PropertyComparer(property.ComponentType, "Question+ActualOrder", direction)
			  : base.GetComparerForSort(property, direction);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public class SubmittedAnswerComparer : IComparer<VoteExamSurveySubmittedAnswer>
		{
			public SubmittedAnswerComparer(GlbCompanyCampaign campaign)
			{
				if (campaign != null)
				{
					compareMethod = GetCompareMethodDelegate(campaign);
				}
			}

			[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
			CompareMethodDelegate GetCompareMethodDelegate(GlbCompanyCampaign campaign)
			{
				CompareMethodDelegate result;

				if (campaign.IsVoteCampaign && campaign.VoteHeader != null && campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
				{
					result = (VoteExamSurveySubmittedAnswer x, VoteExamSurveySubmittedAnswer y) =>
					{
						int answerComparisonResult = x.AnswerAsInt.CompareTo(y.AnswerAsInt);
						return (answerComparisonResult == 0) ? x.Question.HY_Question.CompareTo(y.Question.HY_Question) : answerComparisonResult;
					};
				}
				else
				{
					result = (VoteExamSurveySubmittedAnswer x, VoteExamSurveySubmittedAnswer y) =>
					{
						return x.Question.HY_QuestionOrder.CompareTo(y.Question.HY_QuestionOrder);
					};
				}

				return result;
			}

			int IComparer<VoteExamSurveySubmittedAnswer>.Compare(VoteExamSurveySubmittedAnswer x, VoteExamSurveySubmittedAnswer y)
			{
				return (compareMethod != null) ? compareMethod(x, y) : 0;
			}

			readonly CompareMethodDelegate compareMethod;
			delegate int CompareMethodDelegate(VoteExamSurveySubmittedAnswer x, VoteExamSurveySubmittedAnswer y);
		}

		#endregion
	}
}
