using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySummary : AutoVoteExamSurveySummary
	{
		#region Constructors

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public VoteExamSurveySummary(VoteExamSurveyQuestion question)
		{
			this.question = question;
			AnalyzeCampaignResult();
		}

		#endregion

		#region Properties

		protected VoteExamSurveyQuestion Question
		{
			get { return question; }
		}
		readonly VoteExamSurveyQuestion question;

		public ZString Answer1CountAsText
		{
			get { return Answer1Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 1) ? "0" : string.Empty) : Answer1Count.ToString(); }
		}

		public ZString Answer2CountAsText
		{
			get { return Answer2Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 2) ? "0" : string.Empty) : Answer2Count.ToString(); }
		}

		public ZString Answer3CountAsText
		{
			get { return Answer3Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 3) ? "0" : string.Empty) : Answer3Count.ToString(); }
		}

		public ZString Answer4CountAsText
		{
			get { return Answer4Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 4) ? "0" : string.Empty) : Answer4Count.ToString(); }
		}

		public ZString Answer5CountAsText
		{
			get { return Answer5Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 5) ? "0" : string.Empty) : Answer5Count.ToString(); }
		}

		public ZString Answer6CountAsText
		{
			get { return Answer6Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 6) ? "0" : string.Empty) : Answer6Count.ToString(); }
		}

		public ZString Answer7CountAsText
		{
			get { return Answer7Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 7) ? "0" : string.Empty) : Answer7Count.ToString(); }
		}

		public ZString Answer8CountAsText
		{
			get { return Answer8Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 8) ? "0" : string.Empty) : Answer8Count.ToString(); }
		}

		public ZString Answer9CountAsText
		{
			get { return Answer9Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 9) ? "0" : string.Empty) : Answer9Count.ToString(); }
		}

		public ZString Answer10CountAsText
		{
			get { return Answer10Count == 0 ? ((IsDisplayZeroForEmpty && NumberOfAnswers >= 10) ? "0" : string.Empty) : Answer10Count.ToString(); }
		}

		public ZString NumberOfRecipientAsText
		{
			get { return IsHeader ? string.Empty : NumberOfRecipient.ToString(); }
		}

		public ZString NumberOfRecipientRepliedAsText
		{
			get { return IsHeader ? string.Empty : NumberOfRecipientReplied.ToString(); }
		}

		public ZString RecipientAnsweredAsText
		{
			get { return IsHeader ? string.Empty : RecipientAnswered.ToString(); }
		}

		public ZString RecipientSkippedAsText
		{
			get { return IsHeader ? string.Empty : RecipientSkipped.ToString(); }
		}

		public ZString AverageAsText
		{
			get { return IsHeader || IsText ? string.Empty : Average.ToString("F"); }
		}

		public ZString AverageAsPercentageAsText
		{
			get { return IsHeader || IsText ? string.Empty : Enterprise.ZArchitecture.Core.Utilities.Round(AverageAsPercentage, 0).ToString(); }
		}

		public ZString NumberOfAnswersAsText
		{
			get { return IsHeader || IsText ? string.Empty : NumberOfAnswers.ToString(); }
		}

		#endregion

		#region Analysis Result

		protected int answeredNumber;
		protected int ratingSum;

		protected virtual void AnalyzeCampaignResult()
		{
			this.QuestionText = question.QuestionTextForWeb;
			this.Number = question.ActualOrder;

			if (question.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.Header)
			{
				this.IsHeader = false;
				this.IsDisplayZeroForEmpty = false;
				answeredNumber = 0;
				ratingSum = 0;

				GetAnswerText();

				foreach (VoteExamSurveySubmittedAnswer answer in SubmittedAnswers)
				{
					if (answer.Answer != string.Empty)
					{
						switch (question.HY_AnswerType)
						{
							case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
								GetMultipleChoiceRating(answer);
								break;
							case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
							case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
							case VoteExamSurveyAnswerTypeList.Codes.YesNo:
								GetTextScaleRating(answer);
								break;
							case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
							case VoteExamSurveyAnswerTypeList.Codes.Percentage:
								GetNumericScaleRating(answer);
								break;
							case VoteExamSurveyAnswerTypeList.Codes.VotingItem:
								GetVotingItemRating(answer);
								break;
							case VoteExamSurveyAnswerTypeList.Codes.FreeText:
								answeredNumber++;
								break;
							default:
								break;
						}
					}
				}

				this.NumberOfRecipient = question.Campaign.CampaignsItemsSent.Count;
				this.NumberOfRecipientReplied = CountRepliedRecipients();

				if (question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.VotingItem &&
					question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
				{
					this.RecipientAnswered = this.NumberOfRecipientReplied;
					this.RecipientSkipped = 0;
					this.Answer2Count = this.RecipientAnswered - this.Answer1Count;

					if (this.RecipientAnswered != 0)
					{
						decimal average = (this.Answer1Count + this.Answer2Count * 2) / (decimal)this.RecipientAnswered;
						this.Average = new ZDecimal(average);
					}
				}
				else
				{
					if (question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.FreeText)
					{
						this.IsText = true;
					}
					else
					{
						this.IsText = false;
					}
					this.RecipientAnswered = answeredNumber;
					this.RecipientSkipped = new ZInt(this.NumberOfRecipientReplied - answeredNumber);

					if (this.RecipientAnswered != 0)
					{
						decimal average = ratingSum / (decimal)this.RecipientAnswered;
						this.Average = new ZDecimal(average);
					}
				}

				CalculateAverageAsPercentage();
			}
			else
			{
				this.IsHeader = true;
			}
		}

		protected virtual VoteExamSurveySubmittedAnswerCollection SubmittedAnswers
		{
			get { return question.SubmittedAnswers; }
		}

		protected void GetVotingItemRating(VoteExamSurveySubmittedAnswer answer)
		{
			if (answer.Question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
			{
				int value = answer.AnswerAsInt;

				CountAnswer(value);
				ratingSum += value;
				answeredNumber++;
			}
			else if (answer.Question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
			{
				if (answer.Answer == "Y")
				{
					this.Answer1Count++;
					answeredNumber++;
				}
			}
			else
			{
			}
		}

		protected void GetNumericScaleRating(VoteExamSurveySubmittedAnswer answer)
		{
			int value = 0;
			bool isParseSuccessfully = Int32.TryParse(answer.Answer, out value);

			if (isParseSuccessfully)
			{
				ratingSum += value;
				answeredNumber++;
			}
		}

		protected void GetMultipleChoiceRating(VoteExamSurveySubmittedAnswer answer)
		{
			int? selectedIndex = ExtractNumberFromAnswer(answer.Answer);
			if (selectedIndex != null)
			{
				CountAnswer(selectedIndex.Value);
				ratingSum += selectedIndex.Value;
				answeredNumber++;
			}
		}

		protected void GetTextScaleRating(VoteExamSurveySubmittedAnswer answer)
		{
			ZInt selectedOptionOrder = ZInt.Zero;
			bool isParseSuccessfully = ZInt.TryParse(answer.PersistedAnswer, out selectedOptionOrder);

			if (isParseSuccessfully)
			{
				CountAnswer(selectedOptionOrder);
				ratingSum += selectedOptionOrder;
				answeredNumber++;
			}
		}

		protected virtual int CountRepliedRecipients()
		{
			return question.Factory.GetCachedValue("VoteExamSurveySummary.CountRepliedRecipients()" + question.Campaign.PK.ToString(),
				() =>
				{
					var campaignItemQuery = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
					campaignItemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, question.Campaign.PK);
					var anwserSubQuery = new ZDBOnlySubQuery(typeof(VoteExamSurveyAnswer), VoteExamSurveyAnswerSchema.HZ_G8);
					anwserSubQuery.AddToFilter(JoinCondition.Or, VoteExamSurveyAnswerSchema.HZ_Answer, SQLComparisonOperator.NotEqual, ZString.Empty);
					anwserSubQuery.AddToFilter(JoinCondition.Or, VoteExamSurveyAnswerSchema.HZ_AnswerComment, SQLComparisonOperator.NotEqual, ZString.Empty);
					campaignItemQuery.AddSubQuery(anwserSubQuery, JoinCondition.And);

					return question.Factory.GetDatabaseCount(typeof(GlbCompanyCampaignItem), campaignItemQuery);
				});
		}

		protected void GetAnswerText()
		{
			switch (question.HY_AnswerType)
			{
				case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
					GetMultipleChoiceAnswersText();
					break;
				case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
				case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
				case VoteExamSurveyAnswerTypeList.Codes.YesNo:
					GetCodeDescriptionAnswersText();
					break;
				case VoteExamSurveyAnswerTypeList.Codes.VotingItem:
					GetVotingItemAnswersText();
					break;
				default:
					break;
			}
		}

		void GetVotingItemAnswersText()
		{
			if (question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
			{
				int maxNumber = question.Campaign.VoteHeader.HY_Max;
				this.NumberOfAnswers = maxNumber;
				this.Answer1 = maxNumber >= 1 ? "1" : string.Empty;
				this.Answer2 = maxNumber >= 2 ? "2" : string.Empty;
				this.Answer3 = maxNumber >= 3 ? "3" : string.Empty;
				this.Answer4 = maxNumber >= 4 ? "4" : string.Empty;
				this.Answer5 = maxNumber >= 5 ? "5" : string.Empty;
				this.Answer6 = maxNumber >= 6 ? "6" : string.Empty;
				this.Answer7 = maxNumber >= 7 ? "7" : string.Empty;
				this.Answer8 = maxNumber >= 8 ? "8" : string.Empty;
				this.Answer9 = maxNumber >= 9 ? "9" : string.Empty;
				this.Answer10 = maxNumber >= 10 ? "10" : string.Empty;
			}
			else if (question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
			{
				this.Answer1 = "YES";
				this.Answer2 = "NO";
				this.NumberOfAnswers = 2;
			}
			else
			{
			}
		}

		void GetMultipleChoiceAnswersText()
		{
			this.IsDisplayZeroForEmpty = true;
			ICollection answers = VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(question);
			VoteExamSurveySubQuestionCollection multipleChoiceAnswerCollcetion = answers as VoteExamSurveySubQuestionCollection;
			if (multipleChoiceAnswerCollcetion != null)
			{
				this.NumberOfAnswers = answers.Count;
				foreach (VoteExamSurveyQuestion answer in multipleChoiceAnswerCollcetion)
				{
					int order = Convert.ToInt32(answer.ActualOrder);
					SetAnswerText(order, answer.QuestionTextForWeb);
				}
			}
		}

		void GetCodeDescriptionAnswersText()
		{
			this.IsDisplayZeroForEmpty = true;
			ICollection answers = VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(question);
			CodeDescriptionPairList descriptionList = answers as CodeDescriptionPairList;
			if (descriptionList != null)
			{
				this.NumberOfAnswers = answers.Count;
				for (int i = 0; i < descriptionList.Count; i++)
				{
					SetAnswerText(i + 1, descriptionList[i].Description);
				}
			}
		}

		protected void CalculateAverageAsPercentage()
		{
			if (this.RecipientAnswered > 0)
			{
				switch (question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
					case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
					case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
					case VoteExamSurveyAnswerTypeList.Codes.YesNo:
						CalculateAverageAsPercentageForMultipleOptionsQuestions();
						break;
					case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
						CalculateAverageAsPercentageForNumericQuestions();
						break;
					case VoteExamSurveyAnswerTypeList.Codes.Percentage:
						CalculateAverageAsPercentageForPercentageQuestions();
						break;
					case VoteExamSurveyAnswerTypeList.Codes.VotingItem:
						CalculateAverageAsPercentageForVoting();
						break;
					default:
						break;
				}
			}
		}

		void CalculateAverageAsPercentageForVoting()
		{
			if (question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
			{
				decimal max = question.Campaign.VoteHeader.HY_Max;
				this.AverageAsPercentage = new ZDecimal((this.Average - 1) * 100 / (max - 1));
			}
			else if (question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
			{
				this.AverageAsPercentage = new ZDecimal((this.Average - 1) * 100 / (this.NumberOfAnswers - 1));
			}
			else
			{
			}
		}

		void CalculateAverageAsPercentageForPercentageQuestions()
		{
			this.AverageAsPercentage = new ZDecimal(this.Average);
		}

		void CalculateAverageAsPercentageForNumericQuestions()
		{
			this.AverageAsPercentage = new ZDecimal((this.Average - question.HY_Min) * 100 / (question.HY_Max - question.HY_Min));
		}

		void CalculateAverageAsPercentageForMultipleOptionsQuestions()
		{
			this.AverageAsPercentage = new ZDecimal((this.Average - 1) * 100 / (this.NumberOfAnswers - 1));
		}

		protected virtual void CountAnswer(int selectedIndex)
		{
			if (selectedIndex >= 1 && selectedIndex <= 10)
			{
				string propertyName = AnswerCountPropertyNames[selectedIndex - 1];
				this[propertyName] = ((ZInt)this[propertyName]) + 1;
			}
		}

		readonly string[] AnswerCountPropertyNames = new string[]
		{
			VoteExamSurveySummary.Schema.Answer1Count,
			VoteExamSurveySummary.Schema.Answer2Count,
			VoteExamSurveySummary.Schema.Answer3Count,
			VoteExamSurveySummary.Schema.Answer4Count,
			VoteExamSurveySummary.Schema.Answer5Count,
			VoteExamSurveySummary.Schema.Answer6Count,
			VoteExamSurveySummary.Schema.Answer7Count,
			VoteExamSurveySummary.Schema.Answer8Count,
			VoteExamSurveySummary.Schema.Answer9Count,
			VoteExamSurveySummary.Schema.Answer10Count
		};

		protected virtual void SetAnswerText(int selectedIndex, string text)
		{
			if (selectedIndex >= 1 && selectedIndex <= 10 && !string.IsNullOrEmpty(text))
			{
				string propertyName = AnswerTextPropertyNames[selectedIndex - 1];
				int maxLength = FindPropertyInfo(propertyName).MaxLength;
				this[propertyName] = text.Length <= maxLength ? text : text.Substring(0, maxLength);
			}
		}

		readonly string[] AnswerTextPropertyNames = new string[]
		{
			VoteExamSurveySummary.Schema.Answer1,
			VoteExamSurveySummary.Schema.Answer2,
			VoteExamSurveySummary.Schema.Answer3,
			VoteExamSurveySummary.Schema.Answer4,
			VoteExamSurveySummary.Schema.Answer5,
			VoteExamSurveySummary.Schema.Answer6,
			VoteExamSurveySummary.Schema.Answer7,
			VoteExamSurveySummary.Schema.Answer8,
			VoteExamSurveySummary.Schema.Answer9,
			VoteExamSurveySummary.Schema.Answer10
		};

		static int? ExtractNumberFromAnswer(ZString answer)
		{
			int? selectedIndex = 0;
			Match match = Regex.Match(answer, @"[0-9]+");
			if (match.Success)
			{
				selectedIndex = Convert.ToInt32(match.Value);
			}

			return selectedIndex;
		}

		#endregion
	}
}
