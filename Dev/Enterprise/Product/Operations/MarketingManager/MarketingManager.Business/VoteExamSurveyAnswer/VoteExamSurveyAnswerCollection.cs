using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class VoteExamSurveyAnswerArrayExtensions
	{
		public static VoteExamSurveyAnswer[] FindActive(this VoteExamSurveyAnswer[] answers)
		{
			List<VoteExamSurveyAnswer> result = new List<VoteExamSurveyAnswer>();

			foreach (VoteExamSurveyAnswer answer in answers)
			{
				if (answer.Question != null && answer.Question.HY_IsActive)
				{
					result.Add(answer);
				}
			}

			return result.ToArray();
		}
	}

	public class VoteExamSurveyAnswerCollection : DependentBusinessObjectCollection<VoteExamSurveyAnswer, GlbCompanyCampaignItem>
	{
		public VoteExamSurveyAnswerCollection(GlbCompanyCampaignItem master)
			: base(master)
		{
		}

		public VoteExamSurveyAnswerCollection(GlbCompanyCampaignItem master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public VoteExamSurveyAnswer[] GetCompletedAnswers()
		{
			ZQuery query = new ZQuery(VoteExamSurveyAnswerSchema.HZ_Answer, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(JoinCondition.Or, VoteExamSurveyAnswerSchema.HZ_AnswerComment, SQLComparisonOperator.NotEqual, ZString.Empty);
			return Find(query).FindActive();
		}

		public VoteExamSurveyAnswer FindByQuestion(VoteExamSurveyQuestion question)
		{
			return (question != null) ? FindByQuestion(question.PK) : null;
		}

		public VoteExamSurveyAnswer FindByQuestionWithQuery(ZGuid questionPK)
		{
			VoteExamSurveyAnswer result = null;

			ZQuery answerQuery = new ZQuery(VoteExamSurveyAnswerSchema.HZ_HY, questionPK);
			VoteExamSurveyAnswer[] answers = Find(answerQuery);
			if (answers.Length > 0)
			{
				result = answers[0];
			}

			return result;
		}

		public VoteExamSurveyAnswer FindByQuestion(ZGuid questionPK)
		{
			VoteExamSurveyAnswer result = null;
			if (IsLoaded)
			{
				foreach (VoteExamSurveyAnswer answer in this)
				{
					if (answer.HZ_HY == questionPK)
					{
						result = answer;
					}
				}
			}
			else
			{
				result = FindByQuestionWithQuery(questionPK);
			}
			return result;
		}

		public VoteExamSurveyAnswer LoadOrCreateNew(VoteExamSurveyQuestion question)
			=> FindByQuestion(question)
				?? CreateNew(question);

		public VoteExamSurveyAnswer CreateNew(VoteExamSurveyQuestion question)
		{
			VoteExamSurveyAnswer result = AddNew();
			result.HZ_HY = question.PK;
			return result;
		}

		public new VoteExamSurveyAnswer[] Find(ZQuery query)
		{
			return (VoteExamSurveyAnswer[])base.Find(query);
		}
	}
}
