using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswer : AutoVoteExamSurveyAnswer, IBindableBooleanItem
	{
		public VoteExamSurveyAnswer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Strongly typed Answers

		public ZBool AnswerAsBool
		{
			get
			{
				ZBool result = false;

				try
				{
					result = (!HZ_Answer.IsEmpty) ? new ZBool(HZ_Answer) : ZBool.False;
				}
				catch (ZTypeValueException)
				{
					result = ZBool.False;
				}

				return result;
			}
			set { HZ_Answer = (value) ? value.ToString() : ""; }
		}

		public ZPropertyInfo AnswerAsBoolInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AnswerAsBool), x => HZ_AnswerInfo); }
		}

		public ZInt AnswerAsInt
		{
			get
			{
				ZInt result;
				ZInt.TryParse(HZ_Answer, out result);
				return result;
			}
			set { HZ_Answer = value.ToString(); }
		}

		public ZPropertyInfo AnswerAsIntInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AnswerAsInt), x => HZ_AnswerInfo); }
		}

		#endregion

		#region HZ_Answer

		public override ZString HZ_Answer
		{
			get { return base.HZ_Answer; }
			set
			{
				if (base.HZ_Answer != value)
				{
					ZString previousAnswer = HZ_Answer;
					base.HZ_Answer = value;
					RunExtraWebValidationsOnAnswer(previousAnswer);
				}
			}
		}

		public VoteExamSurveyAnswer[] GetOtherVotesWithRanking(ZString ranking)
		{
			VoteExamSurveyAnswer[] result;

			if (CampaignItem != null)
			{
				ZQuery query = new ZQuery(VoteExamSurveyAnswerSchema.HZ_Answer, ranking);
				query.AddToFilter(VoteExamSurveyAnswerSchema.PK, SQLComparisonOperator.NotEqual, PK);
				result = CampaignItem.PersistedAnswers.Find(query).FindActive();
			}
			else
			{
				result = System.Array.Empty<VoteExamSurveyAnswer>();
			}

			return result;
		}

		void RunExtraWebValidationsOnAnswer(ZString originalAnswer)
		{
			if (!IsValidationSuspended && Question != null && Globals.IsWeb && Question.IsVotingItem)
			{
				ReValidatePreviouslyNominatedVotes(originalAnswer);
			}
		}

		void ReValidatePreviouslyNominatedVotes(ZString ranking)
		{
			if (!ranking.IsEmpty)
			{
				VoteExamSurveyAnswer[] otherVotes = GetOtherVotesWithRanking(ranking);
				foreach (VoteExamSurveyAnswer otherVote in otherVotes)
				{
					if (!otherVote.IsValidationSuspended)
					{
						otherVote.Validation.ValidateHZ_Answer();
					}
				}
			}
		}

		#endregion

		#region CampaignItem

		public GlbCompanyCampaignItem CampaignItem
		{
			get { return Factory.Load<GlbCompanyCampaignItem>(HZ_G8); }
		}

		[RelatedBusinessObject("CampaignItem")]
		public override ZGuid HZ_G8
		{
			get { return base.HZ_G8; }
			set { base.HZ_G8 = value; }
		}

		#endregion

		#region Question

		public VoteExamSurveyQuestion Question
		{
			get { return Factory.Load<VoteExamSurveyQuestion>(HZ_HY); }
		}

		[RelatedBusinessObject("Question")]
		public override ZGuid HZ_HY
		{
			get { return base.HZ_HY; }
			set { base.HZ_HY = value; }
		}

		#endregion

		public bool IsEmpty
		{
			get { return HZ_Answer.IsEmpty && HZ_AnswerComment.IsEmpty; }
		}

		#region IBindableBooleanItem Members

		public ZBool BoolValue
		{
			get { return AnswerAsBool; }
			set { AnswerAsBool = value; }
		}

		public ZPropertyInfo BoolValueInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BoolValue), x => HZ_AnswerInfo); }
		}

		public ZString Text => Question?.QuestionTextForWeb ?? ZString.Empty;

		#endregion
	}
}
