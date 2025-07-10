using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyPreviewBizO))]
	sealed class VoteExamSurveyPreviewBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Campaign, PreviewBizO.CompanyCampaign);
			AssertEquals("Should be assigned in the constructor", Campaign.Factory, PreviewBizO.Factory);
		}

		public void TestAnswerWrappersProxiedFromInnerCampaignItem()
		{
			Assert("Pre-condition", !Campaign.IsInDatabase);
			Assert("Pre-condition", !InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].IsInDatabase);
			Campaign.FillWithValidTestData();
			Campaign.Factory.Save();
			AssertEquals(InnerVoteExamSurveyAnswerSet.AnswerWrappers, PreviewBizO.AnswerWrappers);
		}

		[TestDate(2006, 1, 1)]
		public void TestSubmitAnswerSetAndClearSubmissionDate()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].G8_ClosedDateUtc);

			((IVoteExamSurveyAnswerSet)PreviewBizO).SubmitAnswerSet();
			AssertEquals(ZDateTime.Empty, InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].G8_ClosedDateUtc);

			((IVoteExamSurveyAnswerSet)PreviewBizO).ClearSubmissionDate();
			AssertEquals(ZDateTime.Empty, InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].G8_ClosedDateUtc);
		}

		public void TestInnerCampaignItem()
		{
			AssertEquals(Campaign.PK, InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].G8_G0);
			Assert(PreviewBizO.IsRegisteredEditableChildObject(InnerVoteExamSurveyAnswerSet));
			AssertNotNull("Contact should be assigned", InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].Recipient);
		}

		public void TestInnerCampaignItemShouldNotBeSavedWhenCampaignFactoryIsSaved()
		{
			Assert("Pre-condition", !Campaign.IsInDatabase);
			Assert("Pre-condition", !InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].IsInDatabase);

			Campaign.FillWithValidTestData();
			Campaign.Factory.Save();
			Assert("Should be saved", Campaign.IsInDatabase);
			Assert("Should not be saved", !InnerVoteExamSurveyAnswerSet.ExamCampaignItems[0].IsInDatabase);
		}

		public void TestPersistedAnswers()
		{
			AssertEquals(InnerVoteExamSurveyAnswerSet, PreviewBizO.PersistedAnswers.Master);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Factory for NonSavableCampaignItem in VoteExamSurveyPreviewBizO should not be saved")]
		public void TestExceptionThrownWhenSavingNonSavableLocalFactory()
		{
			InnerVoteExamSurveyAnswerSet.Factory.Save();
		}

		public void TestPagingPropertiesShouldBeProxied()
		{
			InnerVoteExamSurveyAnswerSet.CurrentPage = 2;

			AssertEquals(InnerVoteExamSurveyAnswerSet.CurrentPage, PreviewBizO.CurrentPage);
			AssertEquals(InnerVoteExamSurveyAnswerSet.CurrentPageInfo, PreviewBizO.CurrentPageInfo);
			AssertEquals(InnerVoteExamSurveyAnswerSet.PageCount, PreviewBizO.PageCount);
			AssertEquals(InnerVoteExamSurveyAnswerSet.PageCountInfo, PreviewBizO.PageCountInfo);
			AssertEquals(InnerVoteExamSurveyAnswerSet.PageNumbers.ElementsAsString, PreviewBizO.PageNumbers.ElementsAsString);
			AssertEquals(InnerVoteExamSurveyAnswerSet.PagedAnswerWrappers, PreviewBizO.PagedAnswerWrappers);
		}

		public void TestCurrentQuestionsCountryCode()
		{
			IVoteExamSurveyAnswerSet innerAnswerSet = InnerVoteExamSurveyAnswerSet;
			innerAnswerSet.CurrentQuestionsCountryCode = "AU";
			IVoteExamSurveyAnswerSet previewAnswerSet = PreviewBizO;
			AssertEquals("AU", innerAnswerSet.CurrentQuestionsCountryCode);

			innerAnswerSet.CurrentQuestionsCountryCode = "US";
			AssertEquals("US", innerAnswerSet.CurrentQuestionsCountryCode);
		}

		public void TestNonSavableCampaignItemType()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyPreviewBizO previewBizO = new VoteExamSurveyPreviewBizO(campaign);
			AssertEquals(typeof(VoteExamSurveyAnswerSet), previewBizO.AnswerWrappers.VoteExamSurveyAnswerSet.GetType());

			campaign = (GlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			previewBizO = new VoteExamSurveyPreviewBizO(campaign);
			AssertEquals(ObjectFactory.GetType<ILearningCentreCampaignItem>(), previewBizO.AnswerWrappers.VoteExamSurveyAnswerSet.ExamCampaignItems[0].GetType());
		}

		public void TestIVoteExamSurveyAnswerSetExplicitMembers()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			IVoteExamSurveyAnswerSet answerSet = new VoteExamSurveyPreviewBizO(campaign);
			Assert(!answerSet.HasPreviousSessionEnded);
			Assert(answerSet.IsPreview);
			Assert(!answerSet.AutoSaveAnswers);
			AssertEquals("", answerSet.SubmissionConfirmationMessage);
			AssertNoExceptionThrown(delegate
			{ answerSet.StartVoteExamSurvey(); });
			AssertEquals(TimeSpan.Zero, answerSet.RemainingDuration);
			Assert(!answerSet.IsContinuingPreviousAttempt);
			Assert(!answerSet.CanAutoStartVoteExamSurvey);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoteExamSurveyPreviewBizO(Campaign);
		}

		VoteExamSurveyPreviewBizO PreviewBizO
		{
			get
			{
				if (fPreviewBizO == null)
				{
					fPreviewBizO = new VoteExamSurveyPreviewBizO(Campaign);
				}
				return fPreviewBizO;
			}
		}

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.New<GlbCompanyCampaign>();
				}
				return fCampaign;
			}
		}

		IVoteExamSurveyAnswerSet InnerVoteExamSurveyAnswerSet
		{
			get
			{
				if (fInnerVoteExamSurveyAnswerSet == null)
				{
					PropertyInfo innerVoteExamSurveyAnswerSetProperty = typeof(VoteExamSurveyPreviewBizO).GetProperty("NonSavableVoteExamSurveyAnswerSetForPreview", BindingFlags.NonPublic | BindingFlags.Instance);
					fInnerVoteExamSurveyAnswerSet = (IVoteExamSurveyAnswerSet)innerVoteExamSurveyAnswerSetProperty.GetValue(PreviewBizO, null);
				}
				return fInnerVoteExamSurveyAnswerSet;
			}
		}

		VoteExamSurveyPreviewBizO fPreviewBizO;
		GlbCompanyCampaign fCampaign;
		IVoteExamSurveyAnswerSet fInnerVoteExamSurveyAnswerSet;
	}
}
