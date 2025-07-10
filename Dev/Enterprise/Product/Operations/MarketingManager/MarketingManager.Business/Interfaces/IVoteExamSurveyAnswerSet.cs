using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public interface IVoteExamSurveyAnswerSet : IBusiness
	{
		bool HasPreviousSessionEnded { get; }
		bool CanAutoStartVoteExamSurvey { get; }
		bool IsPreview { get; }
		bool AutoSaveAnswers { get; }
		bool IsContinuingPreviousAttempt { get; }
		TimeSpan RemainingDuration { get; }
		string SubmissionConfirmationMessage { get; }

		ZDateTime ReferenceDateForQuestionRandomiserSeed { get; }

		ZString HtmlSubmissionCompletedMessage { get; }

		GlbCompanyCampaign CompanyCampaign { get; }

		List<GlbCompanyCampaignItem> ExamCampaignItems { get; }

		string JobSkillCode { get; }
		string ExamVersion { get; }
		void SetJobSkillAndVersion(string jobSkillCode, string examVersion);

		bool HasLoadedAnswerWrappers { get; }
		VoteExamSurveyAnswerWrapperCollection AnswerWrappers { get; }
		VoteExamSurveyAnswerCollectionDictionary PersistedAnswers { get; }

		ZString CurrentQuestionsCountryCode { get; set; }

		#region Paging

		ZShort CurrentPage { get; set; }
		ZPropertyInfo CurrentPageInfo { get; }
		ZShort PageCount { get; }
		ZPropertyInfo PageCountInfo { get; }
		CodeDescriptionPairList PageNumbers { get; }
		PagedVoteExamSurveyAnswerWrapperCollection PagedAnswerWrappers { get; }

		#endregion

		int GetSeedForQuestionRandomiser();
		void StartVoteExamSurvey();
		void SubmitAnswerSet(bool shouldCompleteAccreditations = true);
		void ClearSubmissionDate();
		void MergeAnswers();
	}
}
