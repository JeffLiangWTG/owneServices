using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Certification;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting
{
	public class VoteSurveyExamManager
	{
		public VoteSurveyExamManager()
		{
		}

		#region StartVoteExamSurvey

		public IVoteExamSurveyAnswerSet StartVoteExamSurvey(BusinessObjectFactory factory, ZGuid campaignItemPK, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZString countryCode, ZString jobSkillCode, string[] campaignCollection, bool canCreateNewCampaignItem, ZString examSettingsCode)
		{
			const int timeout = 60 * 1000 * 10;

			var mutexName = "Enterprise.MarketingManager.WebVoting.Default:";
			if (!campaignItemPK.IsEmpty)
			{
				mutexName += "{" + campaignItemPK.ToString() + "}";
			}
			else if (!jobSkillCode.IsEmpty && recipientInfo != null)
			{
				mutexName += "{" + jobSkillCode.ToString() + "}";
				mutexName += "{" + recipientInfo.recipientPK.ToString() + "}";
			}

			using (var mutex = new System.Threading.Mutex(false, mutexName))
			{
				if (mutex.WaitOne(timeout))
				{
					try
					{
						var examVersion = ObjectFactory.Get<IExamSettingCodeHelper>().GetExamVersion(examSettingsCode, campaignItemPK, factory);
						return StartVoteExamSurveyInConcurrencyLock(factory, campaignItemPK, recipientInfo, countryCode, jobSkillCode, campaignCollection, canCreateNewCampaignItem, examVersion);
					}
					finally
					{
						mutex.ReleaseMutex();
					}
				}
			}
			return null;
		}

		public static IVoteExamSurveyAnswerSet GetVoteExamSurveyAnswerSet(BusinessObjectFactory factory, ZGuid campaignItemPK, ZGuid recipientPK, string jobSkillCode, string examVersion)
		{
			IVoteExamSurveyAnswerSet answerSet = null;
			var groupedExams = new List<GlbCompanyCampaignItem>();
			var companyCamapaignItem = factory.Load<GlbCompanyCampaignItem>(campaignItemPK);
			if (companyCamapaignItem != null)
			{
				groupedExams.Add(companyCamapaignItem);
			}

			if (groupedExams.Count > 0)
			{
				if (groupedExams.FirstOrDefault() is LearningCentreCampaignItem)
				{
					answerSet = new LearningCentreVoteExamSurveyAnswerSet(factory, groupedExams.Select(x => (LearningCentreCampaignItem)x).ToList());
				}
				else if (groupedExams.FirstOrDefault() is GlbCompanyCampaignItem)
				{
					answerSet = new VoteExamSurveyAnswerSet(factory, groupedExams.Select(x => x).ToList());
				}

				if (answerSet != null)
				{
					answerSet.SetJobSkillAndVersion(jobSkillCode, examVersion);
					foreach (var campaignItem in answerSet.ExamCampaignItems)
					{
						campaignItem.CompanyCampaign.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(examVersion));
					}
				}
			}
			return answerSet;
		}

		protected virtual IVoteExamSurveyAnswerSet StartVoteExamSurveyInConcurrencyLock(BusinessObjectFactory factory, ZGuid campaignItemPK, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZString countryCode, ZString jobSkillCode, string[] campaignCollection, bool canCreateNewCampaignItem, ZString examVersion)
		{
			var groupedExams = new List<GlbCompanyCampaignItem>();
			GlbCompanyCampaign campaign = null;

			if (campaignCollection.Any())
			{
				foreach (var groupedCampaignPK in campaignCollection)
				{
					if (ZGuid.TryParse(groupedCampaignPK, out var campaignPk) || recipientInfo != null || !campaignItemPK.IsEmpty)
					{
						if (campaign is null && !campaignPk.IsEmpty)
						{
							campaign = factory.Load<GlbCompanyCampaign>(campaignPk);
						}

						var companyCamapaignItem = StartVoteExamSurveyCore(factory, campaignItemPK, campaignPk, recipientInfo, jobSkillCode, canCreateNewCampaignItem, true, examVersion);
						if (companyCamapaignItem != null)
						{
							groupedExams.Add(companyCamapaignItem);
						}
					}
				}
			}
			else
			{
				var companyCamapaignItem = StartVoteExamSurveyCore(factory, campaignItemPK, ZGuid.Empty, recipientInfo, jobSkillCode, canCreateNewCampaignItem, true, examVersion);
				if (companyCamapaignItem != null)
				{
					groupedExams.Add(companyCamapaignItem);
				}
			}

			if (groupedExams.Count == 0)
			{
				return null;
			}

			IVoteExamSurveyAnswerSet examWrapper = null;
			if (groupedExams.FirstOrDefault() is LearningCentreCampaignItem)
			{
				examWrapper = new LearningCentreVoteExamSurveyAnswerSet(factory, groupedExams.Select(x => (LearningCentreCampaignItem)x).ToList());
			}
			else if (groupedExams.FirstOrDefault() is GlbCompanyCampaignItem)
			{
				examWrapper = new VoteExamSurveyAnswerSet(factory, groupedExams, campaign);
			}

			if (examWrapper != null)
			{
				examWrapper.CurrentQuestionsCountryCode = countryCode;
				examWrapper.SetJobSkillAndVersion(jobSkillCode, examVersion);

				foreach (var item in groupedExams)
				{
					if (item.IsInDatabase)
					{
						item.Reload();
					}
				}

				var canStartVoteExamSurvey = (examWrapper as VoteExamSurveyAnswerSet).ResetVoteSurveyExam(forceReset: false);
				canStartVoteExamSurvey &= groupedExams.All(x => x.G8_Stage != GlbCompanyCampaignItemLookups.StagesConstants.Taken);

				if (canStartVoteExamSurvey)
				{
					examWrapper.StartVoteExamSurvey();
					examWrapper.ExamCampaignItems.ForEach(x =>
					{
						x.MarkAsVerified();
					});
				}
				factory.Save();
				return examWrapper;
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected virtual GlbCompanyCampaignItem StartVoteExamSurveyCore(BusinessObjectFactory factory, ZGuid campaignItemPK, ZGuid campaignPK, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZString jobSkillCode, bool canCreateNewCampaignItem, bool isPartOfGroupedExam, ZString examVersion)
		{
			var campaignItem = factory.Load<GlbCompanyCampaignItem>(campaignItemPK);
			var campaign = factory.Load<GlbCompanyCampaign>(campaignPK);

			campaign?.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(examVersion));
			campaignItem?.CompanyCampaign?.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(examVersion));

			if (campaignItem == null && campaign != null && recipientInfo != null)
			{
				var recipientPk = recipientInfo.recipientPK;
				ZString recipientTableCode = recipientInfo.recipientTableCode;

				if (campaign is ILearningCentreCampaign && (recipientTableCode == OrgContactSchema.Constants.Prefix || recipientTableCode == GlbStaffSchema.Constants.Prefix))
				{
					BusinessObject applicant = null;

					switch (recipientTableCode)
					{
						case OrgContactSchema.Constants.Prefix:
							if (canCreateNewCampaignItem)
							{
								applicant = ObjectFactory.Get<IOrgContactCertificateApplicantCreator>().LoadOrCreateFromContact(recipientPk) as BusinessObject;
							}
							else
							{
								applicant = ObjectFactory.Get<IOrgContactCertificateApplicantCreator>().LoadFromContact(recipientPk) as BusinessObject;
							}

							break;
						case GlbStaffSchema.Constants.Prefix:
							if (canCreateNewCampaignItem)
							{
								applicant = ObjectFactory.Get<IGlbStaffCertificateApplicantCreator>().LoadOrCreateFromStaff(recipientPk) as BusinessObject;
							}
							else
							{
								applicant = ObjectFactory.Get<IGlbStaffCertificateApplicantCreator>().LoadFromStaff(recipientPk) as BusinessObject;
							}

							break;
					}

					recipientPk = applicant != null ? applicant.PK : ZGuid.Empty;

					recipientTableCode = HRJobApplicantSchema.Constants.Prefix;
				}

				var sentItemType = campaign.TypeOfSentItem;
				var recipientQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignPK)
					.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, recipientPk);

				campaignItem = (GlbCompanyCampaignItem)factory.LoadTop1(sentItemType, recipientQuery);

				if (canCreateNewCampaignItem && campaignItem == null)
				{
					campaignItem = (GlbCompanyCampaignItem)factory.New(sentItemType);
					campaignItem.G8_G0 = campaignPK;
					campaignItem.G8_RecipientID = recipientPk;
					campaignItem.G8_RecipientTableCode = recipientTableCode;
				}
			}

			return campaignItem;
		}

		#endregion

		#region Answer Submission

		#region On Submit Button Click

		public SubmissionResult SaveAnswersOnSubmitButtonClick(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
		{
			var submissionResult = default(SubmissionResult);
			submissionResult = SubmitAnswersWithConcurrencyHandle(dataSource.ExamCampaignItems[0].PK, () => { return SaveAnswersOnSubmitButtonClickCore(dataSource, saveDataSourceFunction); });
			return submissionResult;
		}

		protected virtual SubmissionResult SaveAnswersOnSubmitButtonClickCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveAction)
		{
			if (IsAnswersAlreadySubmitted(dataSource))
			{
				return SubmissionResult.AlreadySubmitted;
			}
			else
			{
				dataSource.MergeAnswers();
				dataSource.RunPreSaveValidation();
				if (!dataSource.ExamCampaignItems.All(x => x.HasErrors) && !(dataSource as VoteExamSurveyAnswerSet).HasErrors)
				{
					dataSource.AnswerWrappers.DeleteEmptyPersistedAnswers();
				}
				dataSource.SubmitAnswerSet(false);
				if (saveAction())
				{
					return SubmissionResult.SubmitSucceeded;
				}
				else
				{
					return SubmissionResult.SubmitFailed;
				}
			}
		}

		#endregion

		#region On Timeout

		public SubmissionResult SaveAnswersOnTimeOut(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
		{
			var submissionResult = default(SubmissionResult);
			foreach (var campaignItem in dataSource.ExamCampaignItems)
			{
				using (campaignItem.SavingAnswersOnTimeOutSuspender.GetSuspender())
				{
					submissionResult = SubmitAnswersWithConcurrencyHandle(campaignItem.PK, () => { return SaveAnswersOnTimeOutCore(dataSource, saveDataSourceFunction); });
				}
			}
			return submissionResult;
		}

		protected virtual SubmissionResult SaveAnswersOnTimeOutCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
		{
			if (IsAnswersAlreadySubmitted(dataSource))
			{
				return SubmissionResult.AlreadySubmitted;
			}
			else
			{
				dataSource.MergeAnswers();
				dataSource.AnswerWrappers.DeleteEmptyPersistedAnswers();
				dataSource.SubmitAnswerSet(false);
				if (saveDataSourceFunction())
				{
					return SubmissionResult.SubmitSucceeded;
				}
				else
				{
					return SubmissionResult.SubmitFailed;
				}
			}
		}

		#endregion

		#region On Page Navigation

		public SubmissionResult SaveAnswersWhenNavigatingPages(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveAction)
		{
			var submissionResult = default(SubmissionResult);
			foreach (var campaignItem in dataSource.ExamCampaignItems)
			{
				submissionResult = SubmitAnswersWithConcurrencyHandle(campaignItem.PK, () => { return SaveAnswersWhenNavigatingPagesCore(dataSource, saveAction); });
			}
			return submissionResult;
		}

		protected virtual SubmissionResult SaveAnswersWhenNavigatingPagesCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
		{
			if (IsAnswersAlreadySubmitted(dataSource))
			{
				return SubmissionResult.AlreadySubmitted;
			}
			else
			{
				if (saveDataSourceFunction())
				{
					return SubmissionResult.SubmitSucceeded;
				}
				else
				{
					return SubmissionResult.SubmitFailed;
				}
			}
		}

		#endregion

		public enum SubmissionResult
		{
			AlreadySubmitted,
			SubmitSucceeded,
			SubmitFailed
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Mutex name as identifier")]
		SubmissionResult SubmitAnswersWithConcurrencyHandle(ZGuid dataSourcePK, Func<SubmissionResult> submitFunction)
		{
			var mutexName = "Enterprise.MarketingManager.WebVoting.Default:{" + dataSourcePK.ToString() + "}";
			const int timeout = 60 * 1000 * 10;

			using (var mutex = new System.Threading.Mutex(false, mutexName))
			{
				if (mutex.WaitOne(timeout))
				{
					try
					{
						return submitFunction();
					}
					finally
					{
						mutex.ReleaseMutex();
					}
				}
			}
			return SubmissionResult.SubmitFailed;
		}

		bool IsAnswersAlreadySubmitted(IVoteExamSurveyAnswerSet dataSource)
		{
			bool result = false;
			if (dataSource != null)
			{
				foreach (var campaignItem in dataSource.ExamCampaignItems)
				{
					campaignItem.Reload();
				}
				result = dataSource.HasPreviousSessionEnded;
			}
			return result;
		}

		#endregion
	}
}
