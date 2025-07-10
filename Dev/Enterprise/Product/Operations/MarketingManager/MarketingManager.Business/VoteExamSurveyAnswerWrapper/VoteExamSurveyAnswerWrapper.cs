using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswerWrapper : VoteExamSurveyAnswerWrapperBase
	{
		public VoteExamSurveyAnswerWrapper(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage)
			: base(question, voteExamSurveyAnswerSet, campaignItem, questionsPerPage)
		{
		}

		public VoteExamSurveyAnswerWrapper(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage, ZShort questionOrder)
			: base(question, voteExamSurveyAnswerSet, campaignItem, questionsPerPage, questionOrder)
		{
		}

		public VoteExamSurveyAnswerWrapper(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage, VoteExamSurveyAnswer answer)
			: base(question, voteExamSurveyAnswerSet, campaignItem, questionsPerPage, answer)
		{
		}

		#region SingleAnswer

		public VoteExamSurveyAnswer SingleAnswer
		{
			get { return base.Answer; }
		}

		#endregion

		#region MultipleAnswers

		public ExamSurveySubAnswerCollection GetMultipleAnswers()
		{
			MultipleAnswers.Load();
			OrderedSubQuestionPKs = new List<ZGuid>(MultipleAnswers.Select(x => x.Question.PK));
			return MultipleAnswers;
		}

		[BusinessObjectTestExclude]
		public List<ZGuid> OrderedSubQuestionPKs
		{
			get;
			private set;
		}

		ExamSurveySubAnswerCollection MultipleAnswers
		{
			get
			{
				if (fMultipleAnswers == null)
				{
					fMultipleAnswers = new ExamSurveySubAnswerCollection(Question, VoteExamSurveyAnswerSet, QuestionOrder);
					RegisterEditableChildObject(MultipleAnswers);
				}
				return fMultipleAnswers;
			}
		}

		ExamSurveySubAnswerCollection fMultipleAnswers;

		#endregion

		#region IsAnswered

		public ZBool IsAnswered
		{
			get { return (Question.IsMultipleChoiceQuestion) ? HasValidMultipleChoiceSelections : !SingleAnswer.IsEmpty; }
		}

		public ZPropertyInfo IsAnsweredInfo
		{
			get
			{
				string humanReadableName = Res.GetString("952984be-cda1-417c-a97a-6f1de7abb632", "Question {0}", (VoteExamSurveyAnswerSet != null) ? QuestionOrder.ToString() : "");
				return GetZPropertyInfo(nameof(IsAnswered), humanReadableName);
			}
		}

		void ValidateIsAnswered()
		{
			IsAnsweredInfo.ClearAllNotifications();
			if (Globals.IsWeb && !Question.HY_IsOptional && !Question.Campaign.IsVoteCampaign)
			{
				if (Question.IsMultipleChoiceQuestion && !HasValidMultipleChoiceSelections)
				{
					string errorMessage = (Question.HY_Min != Question.HY_Max)
					  ? Res.GetString("fb9661ed-a213-487b-b366-2ffb7dd4667d", "You need to select at least {0} and at most {1} options.", Question.HY_Min, Question.HY_Max)
					  : Res.GetString("b7069fb2-158c-450d-bcea-fed9274454f3", "You need to select {0} option(s).", Question.HY_Min);
					errorMessage += " " + Res.GetString("50a4d05a-4377-428b-9c78-53bf18c13112", "You have selected {0} option(s).", GetNumberOfSelectedMultipleChoiceOptions());
					IsAnsweredInfo.AddError(errorMessage);
				}
				else if (!Question.IsMultipleChoiceQuestion && !Question.IsHeader && Question.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption && !IsAnswered)
				{
					IsAnsweredInfo.AddError(Res.GetString("11958215-9901-4fd1-b1ca-66afbce2c0f1", "This question is mandatory. Please fill in the answer."));
				}
			}
		}

		bool HasValidMultipleChoiceSelections
		{
			get
			{
				int numberOfSelections = GetNumberOfSelectedMultipleChoiceOptions();
				return Question.HY_Min <= numberOfSelections && Question.HY_Max >= numberOfSelections;
			}
		}

		int GetNumberOfSelectedMultipleChoiceOptions()
		{
			int result = 0;

			if (VoteExamSurveyAnswerSet != null)
			{
				foreach (VoteExamSurveyQuestion question in Question.SubQuestions)
				{
					foreach (var campaignItem in VoteExamSurveyAnswerSet.ExamCampaignItems)
					{
						VoteExamSurveyAnswer answer = VoteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(question, campaignItem);
						if (answer != null && answer.AnswerAsBool)
						{
							result++;
						}
					}
				}
			}
			return result;
		}

		#endregion

		public IEnumerable<VoteExamSurveyAnswer> GetAllPersistedAnswers()
		{
			var questionPks = new List<ZGuid>(1);
			questionPks.Add(Question.PK);
			if (Question.SubQuestions != null)
			{
				questionPks.AddRange(Question.SubQuestions.Cast<VoteExamSurveyQuestion>().Select(q => q.PK));
			}
			var query = new ZQuery(VoteExamSurveyAnswerSchema.HZ_HY, questionPks);
			if (CampaignItem != null)
			{
				return VoteExamSurveyAnswerSet.PersistedAnswers.Find(query, CampaignItem);
			}
			else
			{
				return VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateIsAnswered();
		}

		public ExamSurveyMultipleChoiceAnswerCapturerBizO MultipleChoiceAnswerCapturer
		{
			get
			{
				if (fMultipleChoiceAnswerCapturer == null)
				{
					fMultipleChoiceAnswerCapturer = new ExamSurveyMultipleChoiceAnswerCapturerBizO(this);
				}
				return fMultipleChoiceAnswerCapturer;
			}
		}

		ExamSurveyMultipleChoiceAnswerCapturerBizO fMultipleChoiceAnswerCapturer;
	}
}
