using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyQuestionCollection : VoteExamSurveyQuestionSet
	{
		public VoteExamSurveyQuestionCollection(GlbCompanyCampaign master)
			: this(master, true)
		{
		}

		public VoteExamSurveyQuestionCollection(GlbCompanyCampaign master, ZQuery additionalFilter)
			: this(master, true, additionalFilter)
		{
		}

		public VoteExamSurveyQuestionCollection(GlbCompanyCampaign master, bool? isActiveFilterValue)
			: base(master, isActiveFilterValue)
		{
		}

		public VoteExamSurveyQuestionCollection(GlbCompanyCampaign master, bool? isActiveFilterValue, ZQuery additionalFilter)
			: base(master, isActiveFilterValue, additionalFilter)
		{
		}

		protected override VoteExamSurveyQuestionSet GetNewInstance(bool? isActiveFilterValue)
		{
			return new VoteExamSurveyQuestionCollection((GlbCompanyCampaign)Relationship.Master, isActiveFilterValue);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(VoteExamSurveyQuestionSchema.HY_SubQuestionOrder, ZShort.Zero);
			return result;
		}

		protected override void SetDefaultsForNewElementCore(VoteExamSurveyQuestion newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (this != InactiveQuestions)
			{
				newElement.HY_QuestionOrder = GetNextQuestionOrder();
				newElement.HY_AnswerType = campaign.G0_DefaultAnswerType;
				newElement.HY_IsOptional = campaign.IsSurveyCampaign;
			}
		}

		public VoteExamSurveyQuestion AddNewVoteQuestionHeader()
		{
			VoteExamSurveyQuestion questionHeader = AddNew();
			questionHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			questionHeader.HY_Question = DefaultVotingQuestionHeaderText;
			questionHeader.HY_Min = DefaultMinVoteCount;
			questionHeader.HY_Max = DefaultMaxVoteCount;
			return questionHeader;
		}

		protected override void OnAdded(VoteExamSurveyQuestion businessObject)
		{
			base.OnAdded(businessObject);
			if (Count > 1 && campaign.IsVoteCampaign)
			{
				ErrorReporter.ReportOnce("VoteExamSurveyQuestionCollection_VoteQuestionHeaderAddedMoreThanOnce", "VoteExamSurveyQuestion should not be added more than once for Voting Campaign");
			}
		}

		public override SchemaShortColumn SchemaOrderColumn
		{
			get { return VoteExamSurveyQuestionSchema.HY_QuestionOrder; }
		}

		const byte DefaultMinVoteCount = 10;
		const byte DefaultMaxVoteCount = 10;
		static string DefaultVotingQuestionHeaderText
		{
			get { return Res.GetString("ca37ad29-4371-495a-9c21-75af9a329e44", "Please nominate your votes"); }
		}
	}
}
