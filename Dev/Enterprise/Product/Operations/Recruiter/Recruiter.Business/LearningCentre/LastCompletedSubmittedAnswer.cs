using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class LastCompletedSubmittedAnswer : LearningCentreSubmittedAnswer
	{
		public LastCompletedSubmittedAnswer(LearningCentreCampaignItem campaignItem, LearningCentreQuestion question)
			: base(campaignItem, question)
		{
		}

		ExamAnswerArchive examAnswerArchive;
		bool isLatestSubmittedAnswerLoaded;
		public override ZString PersistedAnswer
		{
			get
			{
				var result = "";
				var answer = GetPersistedAnswerFromQuestion();
				if (answer != null)
				{
					result = answer.AnswerAsString;
				}
				else
				{
					result = base.PersistedAnswer;
				}
				return result;
			}
		}
		protected override bool IsAnswered(bool answeredCorrectly)
		{
			var answer = GetPersistedAnswerFromQuestion();
			if (answer != null)
			{
				if (answeredCorrectly)
				{
					return answer.IsAnsweredCorrectly;
				}
				else
				{
					return answer.IsAnsweredIncorrectly;
				}
			}

			return base.IsAnswered(answeredCorrectly);
		}

		protected new ExamAnswerArchive GetPersistedAnswerFromQuestion()
		{
			if (!isLatestSubmittedAnswerLoaded)
			{
				isLatestSubmittedAnswerLoaded = true;
				var examAttempts = Factory.Load<ExamAttempt>(new ZQuery(ExamAttemptSchema.EXA_G8, this.CampaignItem.PK)).OrderByDescending(x => x.EXA_TestCommencedUtc);
				var lastAttempt = examAttempts.FirstOrDefault();
				examAnswerArchive = null;
				if (lastAttempt != null && lastAttempt.EXA_TestCompletedUtc.IsEmpty)
				{
					var lastFinishedAttempt = examAttempts.Where(x => !x.EXA_TestCompletedUtc.IsEmpty).OrderByDescending(x => x.EXA_TestCompletedUtc).FirstOrDefault();
					var examAnswerArchives = new ExamAnswerArchiveCollection(Factory);
					if (lastFinishedAttempt == null)
					{
						examAnswerArchives.AddRange(lastAttempt.ExamAnswers);
					}
					else
					{
						examAnswerArchives.AddRange(lastFinishedAttempt.ExamAnswers);
					}

					examAnswerArchive = examAnswerArchives.Cast<ExamAnswerArchive>().FirstOrDefault(x => x.QuestionPK == Question.PK);
				}
			}
			return examAnswerArchive;
		}
		IList<VoteExamSurveyQuestion> selectedMultipleChoiceOptionsList;
		public IList<VoteExamSurveyQuestion> SelectedMultipleChoiceOptionsList
		{
			get
			{
				if (selectedMultipleChoiceOptionsList == null)
				{
					selectedMultipleChoiceOptionsList = SelectedMultipleChoiceOptions.ToList();
				}
				return selectedMultipleChoiceOptionsList;
			}
		}

		public bool IsEmptyAnswer()
		{
			VoteExamSurveyQuestion question = Question;
			if (question.IsMultipleChoiceQuestion)
			{
				return SelectedMultipleChoiceOptionsList.Count == 0;
			}
			else
			{
				return PersistedAnswer.IsEmpty;
			}
		}

		public bool IsRepliedAnswer()
		{
			bool result = false;
			result = GetPersistedAnswerFromQuestion() != null;
			if (!result)
			{
				if (Question.IsMultipleChoiceQuestion)
				{
					result = SelectedMultipleChoiceOptionsList.Count > 0;
				}
				else
				{
					result = base.GetPersistedAnswerFromQuestion() != null;
				}
			}
			return result;
		}

		public override IEnumerable<VoteExamSurveyQuestion> SelectedMultipleChoiceOptions
		{
			get
			{
				var answer = GetPersistedAnswerFromQuestion();
				if (answer != null)
				{
					return answer.SelectedMultipleChoiceOptions;
				}
				else
				{
					return base.SelectedMultipleChoiceOptions;
				}
			}
		}
	}
}
