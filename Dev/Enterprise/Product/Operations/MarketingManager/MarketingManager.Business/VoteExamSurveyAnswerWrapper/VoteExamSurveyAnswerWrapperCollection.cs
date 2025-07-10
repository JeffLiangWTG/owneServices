using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswerWrapperCollection : NonPersistentBusinessObjectCollection<VoteExamSurveyAnswerWrapperBase>
	{
		public VoteExamSurveyAnswerWrapperCollection(VoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage)
			: base(voteExamSurveyAnswerSet.Factory)
		{
			this.VoteExamSurveyAnswerSet = voteExamSurveyAnswerSet;
			this.QuestionsPerPage = questionsPerPage;
			this.CampaignItem = campaignItem;
		}

		public override void Load()
		{
			RemoveAndDeleteAll();
			nonHeaderCount = 0;

			foreach (var persistedAnswer in VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Where(x => x.HZ_SubQuestionOrder == 0).OrderBy(x => x.HZ_QuestionOrder).ThenBy(x => x.Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.Header ? 1 : 0))
			{
				VoteExamSurveyAnswerWrapperBase newAnswerWrapper = new VoteExamSurveyAnswerWrapper(persistedAnswer.Question, VoteExamSurveyAnswerSet, persistedAnswer.CampaignItem, QuestionsPerPage, persistedAnswer);
				Add(newAnswerWrapper);

				if (!newAnswerWrapper.Question.IsHeader)
				{
					nonHeaderCount++;
				}
			}

			IsLoaded = true;
		}

		public int NonHeaderCount
		{
			get
			{
				if (nonHeaderCount == 0)
				{
					nonHeaderCount = this.OfType<VoteExamSurveyAnswerWrapperBase>().Count(x => !x.Question.IsHeader);
				}

				return nonHeaderCount;
			}
		}
		int nonHeaderCount;

		public VoteExamSurveyAnswerWrapperBase FindByQuestion(VoteExamSurveyQuestion question, GlbCompanyCampaignItem campaignItem)
		{
			return FindByQuestion<VoteExamSurveyAnswerWrapperBase>(question, campaignItem);
		}

		public T FindByQuestion<T>(VoteExamSurveyQuestion question, GlbCompanyCampaignItem campaignItem)
		  where T : VoteExamSurveyAnswerWrapperBase
		{
			foreach (T wrapper in this)
			{
				if (wrapper.Question.PK == question.PK && wrapper.Question.Campaign.PK == campaignItem.G8_G0)
				{
					return wrapper;
				}
			}

			return null;
		}

		public IEnumerable<T> FindByPage<T>(int pageNumber, bool includeHeader)
			where T : VoteExamSurveyAnswerWrapperBase
		{
			if (VoteExamSurveyAnswerSet.CompanyCampaign != null)
			{
				int questionsPerWebPage = VoteExamSurveyAnswerSet.CompanyCampaign.G0_QuestionsPerWebPage;
				int minIndex = (pageNumber - 1) * questionsPerWebPage;
				int maxIndex = minIndex + questionsPerWebPage;

				foreach (T wrapper in this)
				{
					int index = wrapper.IndexForPaging;
					if (index >= minIndex && index < maxIndex && (includeHeader || !wrapper.Question.IsHeader))
					{
						yield return wrapper;
					}
				}
			}
		}

		public void DeleteEmptyPersistedAnswers()
		{
			foreach (VoteExamSurveyAnswerWrapper answerWrapper in this)
			{
				foreach (VoteExamSurveyAnswer persistedAnswer in answerWrapper.GetAllPersistedAnswers())
				{
					if (persistedAnswer.IsEmpty && (answerWrapper.IsAnswered || persistedAnswer.Question.IsSubQuestion))
					{
						persistedAnswer.Delete();
					}
				}
			}
		}

		#region Randomise Questions

		public IEnumerable<VoteExamSurveyQuestion> RandomiseQuestionsIfRequired(VoteExamSurveyQuestion[] questions, GlbCompanyCampaign companyCampaign)
		{
			return (companyCampaign != null && companyCampaign.RandomiseQuestionAndMultipleChoiceOrder)
				? RandomiseQuestions(questions, companyCampaign.MaxQuestionsPerExam)
				: questions;
		}

		IEnumerable<VoteExamSurveyQuestion> RandomiseQuestions(VoteExamSurveyQuestion[] questions, short maxQuestionCount)
		{
			QuestionIndexes questionIndexes = new QuestionIndexes(VoteExamSurveyAnswerSet, questions);
			return (questionIndexes.HasQuestionHeaders)
				? GetQuestionsGroupedByQuestionHeader(questions, questionIndexes, maxQuestionCount)
				: GetUngroupedQuestions(questions, questionIndexes, maxQuestionCount);
		}

		IEnumerable<VoteExamSurveyQuestion> GetUngroupedQuestions(VoteExamSurveyQuestion[] questions, QuestionIndexes questionIndexes, short maxQuestionCount)
		{
			List<VoteExamSurveyQuestion> list = new List<VoteExamSurveyQuestion>(maxQuestionCount);
			questionIndexes.ForEach(maxQuestionCount, (int questionIndex) =>
			{
				list.Add(questions[questionIndex]);
			});

			foreach (VoteExamSurveyQuestion question in list)
			{
				yield return question;
			}
		}

		IEnumerable<VoteExamSurveyQuestion> GetQuestionsGroupedByQuestionHeader(VoteExamSurveyQuestion[] questions, QuestionIndexes questionIndexes, short maxQuestionCount)
		{
			SortedDictionary<int, IEnumerable<VoteExamSurveyQuestion>> dictionary = GetQuestionsGroupedByQuestionHeaderDictionary(questions, questionIndexes, maxQuestionCount);

			foreach (int questionHeaderIndex in dictionary.Keys)
			{
				if (questionHeaderIndex > -1)
				{
					yield return questions[questionHeaderIndex];
				}

				foreach (VoteExamSurveyQuestion actualQuestion in dictionary[questionHeaderIndex])
				{
					yield return actualQuestion;
				}
			}
		}

		SortedDictionary<int, IEnumerable<VoteExamSurveyQuestion>> GetQuestionsGroupedByQuestionHeaderDictionary(VoteExamSurveyQuestion[] questions, QuestionIndexes questionIndexes, short maxQuestionCount)
		{
			SortedDictionary<int, IEnumerable<VoteExamSurveyQuestion>> result = new SortedDictionary<int, IEnumerable<VoteExamSurveyQuestion>>();

			questionIndexes.ForEach(maxQuestionCount, (int questionIndex) =>
			{
				int headerIndex = questionIndexes.GetHeaderIndex(questionIndex);
				VoteExamSurveyQuestion questionToInclude = questions[questionIndex];

				IEnumerable<VoteExamSurveyQuestion> questionList;
				if (!result.TryGetValue(headerIndex, out questionList))
				{
					questionList = new List<VoteExamSurveyQuestion>();
					((List<VoteExamSurveyQuestion>)questionList).Add(questionToInclude);
					result.Add(headerIndex, questionList);
				}
				else
				{
					((List<VoteExamSurveyQuestion>)questionList).Add(questionToInclude);
				}
			});

			return result;
		}

		#region QuestionIndexes

		class QuestionIndexes
		{
			public QuestionIndexes(IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, VoteExamSurveyQuestion[] questions)
			{
				List<int> questionHeaderIndexes = new List<int>();
				List<int> optionalQuestionIndexes = new List<int>();
				List<int> mandatoryQuestionIndexes = new List<int>();

				for (int i = 0; i < questions.Length; i++)
				{
					VoteExamSurveyQuestion question = questions[i];
					if (question.IsHeader)
					{
						questionHeaderIndexes.Add(i);
					}
					else
					{
						ZString currentQuestionsCountryCode = voteExamSurveyAnswerSet.CurrentQuestionsCountryCode;
						if (!currentQuestionsCountryCode.IsEmpty && question.HY_RN_NKCountryCode == currentQuestionsCountryCode)
						{
							mandatoryQuestionIndexes.Add(i);
						}
						else
						{
							optionalQuestionIndexes.Add(i);
						}
					}
				}

				this.headers = questionHeaderIndexes.ToArray();
				this.optionalQuestions = optionalQuestionIndexes.ToArray();
				this.mandatoryQuestions = mandatoryQuestionIndexes.ToArray();
				this.voteExamSurveyAnswerSet = voteExamSurveyAnswerSet;
			}

			public int GetHeaderIndex(int questionIndex)
			{
				int arrayIndex = Array.BinarySearch(headers, questionIndex);
				// arrayIndex always represents the bitwise complement of the next larger element in the array.
				arrayIndex = ~arrayIndex;
				return (arrayIndex > 0) ? headers[arrayIndex - 1] : -1;
			}

			public void ForEach(int maxQuestionCount, Action<int> action)
			{
				List<int> questionsList = new List<int>();
				questionsList.AddRange(Shuffle(mandatoryQuestions));
				questionsList.AddRange(Shuffle(optionalQuestions));
				int[] allQuestions = (maxQuestionCount > 0 && questionsList.Count > maxQuestionCount)
					? questionsList.GetRange(0, maxQuestionCount).ToArray()
					: questionsList.ToArray();

				Shuffle(allQuestions);
				foreach (int questionIndex in allQuestions)
				{
					action(questionIndex);
				}
			}

			int[] Shuffle(int[] indexes)
			{
				Random rand = new Random(GetSeedForRandomiser());
				int randArrayIndex;
				int tempArrayIndex;

				for (int i = indexes.Length - 1; i >= 0; i--)
				{
					randArrayIndex = rand.Next(i + 1);
					if (randArrayIndex != i)
					{
						tempArrayIndex = indexes[i];
						indexes[i] = indexes[randArrayIndex];
						indexes[randArrayIndex] = tempArrayIndex;
					}
				}

				return indexes;
			}

			int GetSeedForRandomiser()
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return 31081;
				}
#endif

				return voteExamSurveyAnswerSet.GetSeedForQuestionRandomiser();
			}

			public bool HasQuestionHeaders
			{
				get { return headers.Length > 0; }
			}

			readonly int[] headers;
			readonly int[] optionalQuestions;
			readonly int[] mandatoryQuestions;
			readonly IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet;
		}

		#endregion

		#endregion

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Should not create new AnswerWrapper, every AnswerWrapper should be associated with a Question");
		}

		public readonly VoteExamSurveyAnswerSet VoteExamSurveyAnswerSet;
		public readonly int QuestionsPerPage;
		public readonly GlbCompanyCampaignItem CampaignItem;
	}
}
