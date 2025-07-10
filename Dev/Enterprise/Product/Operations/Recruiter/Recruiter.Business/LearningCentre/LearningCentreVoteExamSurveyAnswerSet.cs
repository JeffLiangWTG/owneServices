using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreVoteExamSurveyAnswerSet : VoteExamSurveyAnswerSet
	{
		public LearningCentreVoteExamSurveyAnswerSet(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public LearningCentreVoteExamSurveyAnswerSet(BusinessObjectFactory factory, List<LearningCentreCampaignItem> campaignItems)
			: base(factory, campaignItems.ToList<GlbCompanyCampaignItem>())
		{
			foreach (var campaignItem in campaignItems)
			{
				if (campaignItem.CompanyCampaign != null
					&& string.IsNullOrEmpty(campaignItem.CompanyCampaign.CurrentExamVersion))
				{
					campaignItem.CompanyCampaign.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(ExamVersion));
				}
			}
		}

		public LearningCentreVoteExamSurveyAnswerSet(BusinessObjectFactory factory, LearningCentreCampaignItem learningCenterCampaignItem)
			: this(factory, new List<LearningCentreCampaignItem>() { learningCenterCampaignItem })
		{
		}

		public HRJobApplicant JobApplicant
		{
			get { return Factory.Load<HRJobApplicant>(ExamCampaignItems[0].G8_RecipientID); }
		}

		public HRJobApplicant Recipient
		{
			get { return ExamCampaignItems[0].Recipient as HRJobApplicant; }
		}

		public new LearningCentreCampaign CompanyCampaign
		{
			get { return base.CompanyCampaign as LearningCentreCampaign; }
		}

		protected override ZDateTime ReferenceDateForQuestionRandomiserSeed
		{
			get
			{
				return ZDateTime.UtcNow;
			}
		}

		void SetCompletedTimeUtc(ZDateTime value)
		{
		}

		public new LearningCentreSubmittedAnswerCollection SubmittedAnswers
		{
			get { return (LearningCentreSubmittedAnswerCollection)base.SubmittedAnswers; }
		}

		protected override VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswers()
		{
			return new LearningCentreSubmittedAnswerCollection(this);
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		protected override void StartVoteExamSurvey()
		{
			base.StartVoteExamSurvey();
			foreach (LearningCentreCampaignItem examItem in ExamCampaignItems)
			{
				var examAttempt = examItem.LastAttempt;

				if (examAttempt == null || !examAttempt.EXA_TestCompletedUtc.IsEmpty)
				{
					examAttempt = Factory.New<ExamAttempt>();
					examAttempt.ExamBegin(examItem);
				}
			}
		}

		protected override void SubmitAnswerSet(bool shouldCompleteAccreditations)
		{
			base.SubmitAnswerSet(shouldCompleteAccreditations);
			SubmittedExamAttempts.Clear();

			foreach (LearningCentreCampaignItem examItem in ExamCampaignItems)
			{
				var examAttempt = examItem.GetLatestIncompleteAttempt();
				if (examAttempt != null)
				{
					examAttempt.ExamEnd(examItem);
					SubmittedExamAttempts.Add(examAttempt);
				}

				if (shouldCompleteAccreditations)
				{
					examItem.CompleteAccreditationsIfRequired();
				}
			}
		}

		readonly List<ExamAttempt> SubmittedExamAttempts = new List<ExamAttempt>();

		protected override void ClearSubmissionDate()
		{
			base.ClearSubmissionDate();
			SetCompletedTimeUtc(ZDateTime.Empty);

			SubmittedExamAttempts.ForEach(x => x.Rollback());
			SubmittedExamAttempts.Clear();
		}

		protected override bool IsContinuingPreviousAttempt
		{
			get { return ExamCampaignItems.Any(x => x.IsContinuingPreviousAttempt); }
		}

		public override bool ResetVoteSurveyExam(bool forceReset)
		{
			if (ExamCampaignItems.All(x => x.G8_Stage == GlbCompanyCampaignItemLookups.StagesConstants.Reset || x.G8_Stage == string.Empty))
			{
				return true;
			}

			int failedCount = 0;
			foreach (LearningCentreCampaignItem item in ExamCampaignItems)
			{
				if (item.HasPreviousSessionEnded && (forceReset || item.CanResetVoteSurveyExam))
				{
					item.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Reset;
					item.G8_ClosedDateUtc = ZDateTime.Empty;
					PersistedAnswers.GetCampaignItemAnswers(item).RemoveAndDeleteAll();
					item.InvalidateAnswersSets();
				}
				else
				{
					failedCount++;
				}
			}
			InvalidateAnswersSets();
			if (failedCount > 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected override TimeSpan RemainingDuration
		{
			get
			{
				return TimeSpan.Zero;
			}
		}

		public ZInt LastCompletedCorrectAnswersCount
		{
			get
			{
				return GetCount(SubmittedAnswers.CorrectAnswers, ref correctAnswersCount);
			}
		}

		public ZInt LastCompletedIncorrectAnswersCount
		{
			get
			{
				return GetCount(SubmittedAnswers.IncorrectAnswers, ref incorrectAnswersCount);
			}
		}

		public ZInt LastCompletedEmptyAnswersCount
		{
			get
			{
				return GetCount(SubmittedAnswers.EmptyAnswers, ref emptyAnswersCount);
			}
		}

		public ZDateTime LastCompletedTestCommenced
		{
			get
			{
				return ZDateTime.UtcNow;
			}
		}

		public ZDateTime LastCompletedTestCompleted
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public ZByte LastCompletedTestResultScore
		{
			get
			{
				return 0;
			}
		}

		#region Exam Score
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

		ZInt GetCount(IEnumerable<ILearningCentreSubmittedAnswer> answers, ref int? cacheField)
		{
			return (ZInt)(cacheField ?? (cacheField = answers.Count()));
		}

		int? correctAnswersCount;
		int? incorrectAnswersCount;
		int? emptyAnswersCount;

		#endregion

		#region Html Submission Completed Message
		public override ZString HtmlSubmissionCompletedMessage
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(Res.GetString("6cce43c7-3d8b-4bf3-a4fe-da3b043807dd", "Your answers have been successfully submitted."));

				return builder.ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html formatting constant strings")]
		static class HtmlFormattingStrings
		{
			internal const string HtmlRowString = "<TR><TD class='testResultCaption'>{0}: </TD><TD class='testResultValue'>{1}</TD></TR>";
			internal const string HtmlRowString1 = "<TR><TD colspan='2' class='testResultCaption'>{0}</TD></TR>";
			internal const string CaptionHtmlRowString = "<TR><TD colspan='2'><B/>{0}</B></TD></TR>";
			internal const string EmptyHtmlRowString = "<TR><TD colspan='2'>&nbsp;</TD></TR>";
			internal const string HtmlExamResultTableBegin = @"<DIV class='bottomSection'>
			<TABLE class='testsGrid' cellspacing='0' rules='all' border='1' align='center'>
				<THEAD>
					<TR class='gridHeader'>
						<TH class='lastExamCol1' scope = 'col'> Exam Sections</TH>
						<TH class='lastExamCol2' scope='col'>Required Score %</TH>
						<TH class='lastExamCol1' scope='col'>Score</TH>
						<TH class='lastExamCol2' scope='col'>Status</TH>
					</TR>
				</THEAD>
				<TBODY>";
			internal const string HtmlExamResultTableRow = "<TR class='gridRow'>" +
				"<TD>{0}</TD><TD>{1}</TD><TD>{2}</TD><TD {3}>{4}</TD>" +
			"</TR>";
		}

		#endregion
	}
}
