using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreCampaignItem : GlbCompanyCampaignItem, ILearningCentreCampaignItem
	{
		public LearningCentreCampaignItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G8_DeliveryMethod = Core.Constants.Recruiter.LearningCentreCampaignType;
		}

		public new LearningCentreCampaign CompanyCampaign
		{
			get { return base.CompanyCampaign as LearningCentreCampaign; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public new LearningCentreSubmittedAnswerCollection SubmittedAnswers
		{
			get { return (LearningCentreSubmittedAnswerCollection)base.SubmittedAnswers; }
		}

		protected override VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswers()
		{
			return new LearningCentreSubmittedAnswerCollection(this);
		}

		public ZByte LastCompletedTestResultScore
		{
			get
			{
				return 0;
			}
		}

		public ExamAnswerArchiveCollection LastCompletedTestAnswers
		{
			get
			{
				var lastTestResultRegardlessOfCompletion = new ExamAnswerArchiveCollection(Factory);
				lastTestResultRegardlessOfCompletion.Populate(this);
				return lastTestResultRegardlessOfCompletion;
			}
		}

		protected override ZDateTime ReferenceDateForQuestionRandomiserSeed
		{
			get
			{
				return ZDateTime.UtcNow;
			}
		}

		#region Exam Score

		public ZByte ExamScore
		{
			get { return (ZByte)CalculateExamScore(); }
		}

		public ZPropertyInfo ExamScoreInfo
		{
			get { return GetZPropertyInfo(nameof(ExamScore)); }
		}

		public ZString ExamScoreAsString
		{
			get
			{
				int correctAnswersCount = CorrectAnswersCount;
				int totalAnswersCount = correctAnswersCount + IncorrectAnswersCount + EmptyAnswersCount;
				return Res.GetString("e965b8bf-86b0-476a-aeca-1af76bf9e5c4", "{0}% ({1} out of {2})", ExamScore, correctAnswersCount, totalAnswersCount);
			}
		}

		public ZInt CorrectAnswersCount
		{
			get { return GetCount(SubmittedAnswers.CorrectAnswers, ref correctAnswersCount); }
		}

		public ZPropertyInfo CorrectAnswersCountInfo
		{
			get { return GetZPropertyInfo(nameof(CorrectAnswersCount)); }
		}

		public ZInt IncorrectAnswersCount
		{
			get { return GetCount(SubmittedAnswers.IncorrectAnswers, ref incorrectAnswersCount); }
		}

		public ZPropertyInfo IncorrectAnswersCountInfo
		{
			get { return GetZPropertyInfo(nameof(IncorrectAnswersCount)); }
		}

		public ZInt EmptyAnswersCount
		{
			get { return GetCount(SubmittedAnswers.EmptyAnswers, ref emptyAnswersCount); }
		}

		public ZPropertyInfo EmptyAnswersCountInfo
		{
			get { return GetZPropertyInfo(nameof(EmptyAnswersCount)); }
		}

		byte CalculateExamScore()
		{
			byte score;

			decimal correctAnswerCount = SubmittedAnswers.CorrectAnswers.Count();
			decimal totalAnswerCount = correctAnswerCount + SubmittedAnswers.IncorrectAnswers.Count() + SubmittedAnswers.EmptyAnswers.Count();

			if (totalAnswerCount > 0)
			{
				score = (correctAnswerCount > totalAnswerCount)
					? (byte)100
					: (byte)Utilities.Round((correctAnswerCount / totalAnswerCount) * 100, 0);
			}
			else
			{
				score = 0;
			}

			return score;
		}

		ZInt GetCount(IEnumerable<ILearningCentreSubmittedAnswer> answers, ref int? cacheField)
		{
			return (ZInt)(cacheField ?? (cacheField = answers.Count()));
		}

		int? correctAnswersCount;
		int? incorrectAnswersCount;
		int? emptyAnswersCount;

		#endregion

		#region ScaledTestResults

		public ScaledTestResultByCategoryCollection ScaledTestResults
		{
			get
			{
				if (scaledTestResults == null)
				{
					scaledTestResults = new ScaledTestResultByCategoryCollection(this);
					scaledTestResults.Load();
				}
				return scaledTestResults;
			}
		}

		ScaledTestResultByCategoryCollection scaledTestResults;

		#endregion

		public HRJobApplicant JobApplicant
		{
			get { return Factory.Load<HRJobApplicant>(G8_RecipientID); }
		}

		public OrgContact Contact
		{
			get { return Factory.Load<OrgContact>(G8_RecipientID); }
		}

		protected override string DocManagerCode
		{
			get { return Core.Constants.DocManagerCodes.LearningCentreCampaignItem; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!G8_ClosedDateUtc.IsEmpty && (G8_ClosedDateUtcInfo.HasChanges || PersistedAnswers.HasChanges))
			{
				EnsureSubmittedAnswersAreCurrent();
				CalculateJobTestRatings();
			}
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!G8_ClosedDateUtc.IsEmpty && G8_ClosedDateUtcInfo.HasChanges)
			{
				EnsureSubmittedAnswersAreCurrent();
			}
		}
		void EnsureSubmittedAnswersAreCurrent()
		{
			InvalidateAnswersSets();
		}

		void CalculateJobTestRatings()
		{
		}

		public ExamAttempt GetLatestIncompleteAttempt()
		{
			var attemptQuery = new ZQuery(ExamAttemptSchema.EXA_G8, PK);
			attemptQuery.AddToFilter(ExamAttemptSchema.EXA_TestCompletedUtc, DBNull.Value);
			attemptQuery.OrderBy = ExamAttemptSchema.Constants.EXA_TestCommencedUtc + OrderByClause.Descending;
			return Factory.LoadTop1<ExamAttempt>(attemptQuery);
		}

		public override bool ResetVoteSurveyExam(bool forceReset)
		{
			LearningCentreVoteExamSurveyAnswerSet answerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, this);
			return answerSet.ResetVoteSurveyExam(forceReset);
		}

		public void StartAccreditations(ZGuid? accreditationPk = null)
		{
			if (JobApplicant != null)
			{
				foreach (var accreditation in CompanyCampaign.RelatedAccreditations)
				{
					if (accreditationPk == null || accreditation.PK == accreditationPk.Value)
					{
						accreditation.StartAttempt(JobApplicant.Person);
					}
				}
			}
		}

		public ExamAttempt LastAttempt
		{
			get { return ExamAttempts.OrderByDescending(a => a.EXA_TestCommencedUtc).FirstOrDefault(); }
		}

		ExamAttemptCollection attempts;
		public ExamAttemptCollection ExamAttempts
		{
			get
			{
				if (attempts == null)
				{
					attempts = new ExamAttemptCollection(this);
				}

				return attempts;
			}
		}

		public void CompleteAccreditationsIfRequired(ZGuid? accreditationPk = null)
		{
			if (JobApplicant != null)
			{
				foreach (var accreditation in CompanyCampaign.RelatedAccreditations)
				{
					if (accreditationPk == null || accreditation.PK == accreditationPk.Value)
					{
						accreditation.CompleteAttemptIfRequired(JobApplicant.Person);
					}
				}
			}
		}

		public void ReloadSkillRatingTestHistory()
		{
			throw new NotImplementedException();
		}

		public override bool HasPreviousSessionEnded
		{
			get { return base.HasPreviousSessionEnded || (!SavingAnswersOnTimeOutSuspender.IsSuspended); }
		}

		public TimeSpan RemainingDuration
		{
			get
			{
				TimeSpan result = TimeSpan.Zero;
				if (CompanyCampaign != null)
				{
					result = TimeSpan.Zero;
				}
				return result;
			}
		}

		public override bool IsContinuingPreviousAttempt
		{
			get { return false; }
		}

		public override string HasPreviousSessionEndedMessage
		{
			get
			{
				return Res.GetString("c9d33547-bfc7-46b4-9c3e-3085ebc5b51e",
					"You have to wait for {0} hour(s) after your last attempt to resit the exam.",
					0);
			}
		}

		public override bool CanResetVoteSurveyExam
		{
			get { return !IsLockedOut; }
		}

		bool IsLockedOut
		{
			get { return false; }
		}

#if DEBUG
		public bool HasPreviousSessionEnded_Exposed { get { return HasPreviousSessionEnded; } }
#endif
	}
}
