using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreQuestionCollection))]
	sealed class LearningCentreQuestionCollectionTest : ActiveBusinessObjectCollectionTestCase<LearningCentreQuestionCollection>
	{
		public void TestRelationshipFilter()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question1 = Factory.New<LearningCentreQuestion>();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_G0 = campaign.PK;
			LearningCentreQuestion question2 = Factory.New<LearningCentreQuestion>();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question2.HY_G0 = campaign.PK;
			LearningCentreQuestion question3 = Factory.New<LearningCentreQuestion>();
			question3.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			question3.HY_G0 = campaign.PK;

			LearningCentreQuestionCollection collection = new LearningCentreQuestionCollection(campaign, true);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(question1, collection);
			AssertCollectionContains(question2, collection);
		}

		public void TestIImportCollectionInfoProviderMembers()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			IImportCollectionInfoProvider provider = campaign.Questions;
			AssertNull(provider.ContextKey);
			AssertNotNull(provider.ImportCollectionInfo.Collection);
			AssertEquals(9, provider.ImportCollectionInfo.Properties.Count());

			IEnumerable<IImportPropertyInfo> info = provider.ImportCollectionInfo.Properties;
			AssertImportPropertyInfo(info.ElementAt(0), AutoVoteExamSurveyQuestion.Schema.HY_QuestionOrder, "No.");
			AssertImportPropertyInfo(info.ElementAt(1), AutoVoteExamSurveyQuestion.Schema.HY_SubQuestionOrder, "Option No.");
			AssertImportPropertyInfo(info.ElementAt(2), AutoVoteExamSurveyQuestion.Schema.HY_Question, "Text");
			AssertImportPropertyInfo(info.ElementAt(3), AutoVoteExamSurveyQuestion.Schema.HY_AnswerType, "Type");
			AssertImportPropertyInfo(info.ElementAt(4), AutoVoteExamSurveyQuestion.Schema.HY_IsRandomisable, "Randomizable");
			AssertImportPropertyInfo(info.ElementAt(5), AutoVoteExamSurveyQuestion.Schema.HY_RN_NKCountryCode, "Country/Region");
			AssertImportPropertyInfo(info.ElementAt(6), AutoVoteExamSurveyQuestion.Schema.HY_QuestionCategory, "Category");
			AssertImportPropertyInfo(info.ElementAt(7), AutoVoteExamSurveyQuestion.Schema.HY_ExamCorrectAnswer, "Correct Answer");
			AssertImportPropertyInfo(info.ElementAt(8), AutoVoteExamSurveyQuestion.Schema.HY_AnswerWeighting, "Answer Weighting");
		}

		void AssertImportPropertyInfo(IImportPropertyInfo propertyInfo, string mappingName, string headerText)
		{
			AssertEquals(mappingName, propertyInfo.MappingName);
			AssertEquals(headerText, propertyInfo.HeaderText);
		}

		protected override LearningCentreQuestionCollection GetCollectionToTest()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return campaign.Questions;
		}
	}
}
