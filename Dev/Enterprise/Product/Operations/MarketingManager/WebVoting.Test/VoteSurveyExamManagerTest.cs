using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class VoteSurveyExamManagerTest : TestCaseWithFactory
	{
		public void TestStartVoteExamSurvey_LoadOnlyOneCampaignItem()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "ContactOne";
			contact1.OC_Email = "ContactOne@test.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "ContactTwo";
			contact2.OC_Email = "ContactTwo@test.com";
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "ContactThree";
			contact3.OC_Email = "ContactThree@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var manager1 = new VoteSurveyExamManager();
			manager1.StartVoteExamSurvey(factory1, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact3.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
			AssertEquals("GlbCompanyCampaignItem table hits", 1, factory1.GetTableHitCount(GlbCompanyCampaignItemSchema.Constants.TableName));
			AssertEquals("GlbCompanyCampaignItem records loaded", 1, ((IBusinessObjectFactoryInternals)factory1).AllBusinessObjects.Count(x => x is GlbCompanyCampaignItem));
		}

		public void TestStartVoteExamSurvey_Concurrency()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var campaignItemList = new List<GlbCompanyCampaignItem>(1);
			var manager1Event = new AutoResetEvent(false);
			var manager2Event = new AutoResetEvent(false);

			var thread1 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory1 = new BusinessObjectFactory();
					var manager1 = new VoteSurveyExamManagerForTest(manager1Event, campaignItemList);
					manager1.StartVoteExamSurvey(factory1, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
				}
			});

			var thread2 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory2 = new BusinessObjectFactory();
					var manager2 = new VoteSurveyExamManagerForTest(manager2Event, campaignItemList);
					manager2.StartVoteExamSurvey(factory2, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
				}
			});

			thread2.Start();
			thread1.Start();

			manager1Event.WaitOne();
			manager2Event.WaitOne();

			AssertEquals("Only one campaign item should be created", 1, campaignItemList.Count);
		}

		public void TestStartVoteExamSurvey_ContactRecipientShouldLoadApplicant()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "ContactOne";
			contact1.OC_Email = "ContactOne@test.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "ContactTwo";
			contact2.OC_Email = "ContactTwo@test.com";

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var manager1 = new VoteSurveyExamManager();
			manager1.StartVoteExamSurvey(factory1, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact2.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
			var factoryBizOs = ((IBusinessObjectFactoryInternals)factory1).AllBusinessObjects;
			AssertEquals("GlbCompanyCampaignItem records loaded", 1, factoryBizOs.Count(x => x is GlbCompanyCampaignItem));
			var campaignItem = factoryBizOs.FirstOrDefault(x => x is GlbCompanyCampaignItem) as GlbCompanyCampaignItem;
			AssertEquals("Applicant should have been used", HRJobApplicantSchema.Constants.Prefix, campaignItem.G8_RecipientTableCode);
			AssertNotNull(factory1.Load<HRJobApplicant>(campaignItem.G8_RecipientID));
		}

		public void TestStartVoteExamSurvey_StaffRecipientShouldLoadApplicant()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "StaffOne";
			staff.GS_EmailAddress = "StaffOne@test.com";
			staff.GS_Code = "STO";

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var manager1 = new VoteSurveyExamManager();
			manager1.StartVoteExamSurvey(factory1, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(staff.PK, GlbStaffSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
			var factoryBizOs = ((IBusinessObjectFactoryInternals)factory1).AllBusinessObjects;
			AssertEquals("GlbCompanyCampaignItem records loaded", 1, factoryBizOs.Count(x => x is GlbCompanyCampaignItem));
			var campaignItem = factoryBizOs.FirstOrDefault(x => x is GlbCompanyCampaignItem) as GlbCompanyCampaignItem;
			AssertEquals("Applicant should have been used", HRJobApplicantSchema.Constants.Prefix, campaignItem.G8_RecipientTableCode);
			AssertNotNull(factory1.Load<HRJobApplicant>(campaignItem.G8_RecipientID));
		}

		public void TestStartVoteExamSurvey_ApplicantRecipientShouldNotResetRecipientID()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientID = applicant.PK;
			campaignItem1.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var manager1 = new VoteSurveyExamManager();
			manager1.StartVoteExamSurvey(factory1, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(applicant.PK, HRJobApplicantSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
			var factoryBizOs = ((IBusinessObjectFactoryInternals)factory1).AllBusinessObjects;
			AssertEquals("GlbCompanyCampaignItem records loaded", 1, factoryBizOs.Count(x => x is GlbCompanyCampaignItem));
			var campaignItem = factoryBizOs.FirstOrDefault(x => x is GlbCompanyCampaignItem) as GlbCompanyCampaignItem;
			AssertEquals("Applicant should have been used", HRJobApplicantSchema.Constants.Prefix, campaignItem.G8_RecipientTableCode);
			AssertEquals("Applicant should not have been reset", false, campaignItem.G8_RecipientID.IsEmpty);
			AssertNotNull(factory1.Load<HRJobApplicant>(campaignItem.G8_RecipientID));
		}

		public void TestSubmitAnswer_Concurrency()
		{
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			SubmissionStat stat = new SubmissionStat() { IsAlreadySubmitted = false, CountOfSubmission = 0 };

			var manager1Event = new AutoResetEvent(false);
			var manager1 = new VoteSurveyExamManagerForTest(manager1Event, stat);

			var manager2Event = new AutoResetEvent(false);
			var manager2 = new VoteSurveyExamManagerForTest(manager2Event, stat);

			var wrapper = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			var threadA1 = new Thread(() => { manager1.SaveAnswersOnSubmitButtonClick(wrapper, () => { return true; }); });
			var threadA2 = new Thread(() => { manager2.SaveAnswersOnSubmitButtonClick(wrapper, () => { return true; }); });
			threadA2.Start();
			threadA1.Start();
			manager1Event.WaitOne();
			manager2Event.WaitOne();
			AssertEquals("SaveAnswersOnSubmitButtonClick(): Should be only one submission", 1, stat.CountOfSubmission);

			manager1Event.Reset();
			manager2Event.Reset();
			stat.IsAlreadySubmitted = false;
			var threadB1 = new Thread(() => { manager1.SaveAnswersOnTimeOut(wrapper, () => { return true; }); });
			var threadB2 = new Thread(() => { manager2.SaveAnswersOnTimeOut(wrapper, () => { return true; }); });
			threadB2.Start();
			threadB1.Start();
			manager1Event.WaitOne();
			manager2Event.WaitOne();
			AssertEquals("SaveAnswersOnTimeOut(): Should be only one submission", 2, stat.CountOfSubmission);

			manager1Event.Reset();
			manager2Event.Reset();
			stat.IsAlreadySubmitted = false;
			var threadC1 = new Thread(() => { manager1.SaveAnswersWhenNavigatingPages(wrapper, () => { return true; }); });
			var threadC2 = new Thread(() => { manager2.SaveAnswersWhenNavigatingPages(wrapper, () => { return true; }); });
			threadC2.Start();
			threadC1.Start();
			manager1Event.WaitOne();
			manager2Event.WaitOne();
			AssertEquals("SaveAnswersWhenNavigatingPages(): Should be only one submission", 3, stat.CountOfSubmission);
		}

		public void TestSaveAnswersOnTimeOutWithSuspender()
		{
			var campaignItem = Factory.NewWithValidTestData<LearningCentreCampaignItem>();
			var stat = new SubmissionStat() { IsAlreadySubmitted = false, CountOfSubmission = 0 };

			var manager1Event = new AutoResetEvent(false);
			var manager1 = new VoteSurveyExamManagerForTest(manager1Event, stat);

			var wrapper = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			manager1Event.Reset();

			stat.IsAlreadySubmitted = false;
			var isSuspended = campaignItem.SavingAnswersOnTimeOutSuspender.IsSuspended;
			var hasPreviousSessionEnded = campaignItem.HasPreviousSessionEnded;
			AssertEquals(false, isSuspended);

			manager1.SaveAnswersOnTimeOut(wrapper, () =>
			{
				isSuspended = campaignItem.SavingAnswersOnTimeOutSuspender.IsSuspended;
				hasPreviousSessionEnded = campaignItem.HasPreviousSessionEnded;
				return true;
			});

			AssertEquals(true, isSuspended);
			isSuspended = campaignItem.SavingAnswersOnTimeOutSuspender.IsSuspended;
			AssertEquals(false, isSuspended);
			AssertEquals(1, stat.CountOfSubmission);
		}

		class VoteSurveyExamManagerForTest : VoteSurveyExamManager
		{
			public VoteSurveyExamManagerForTest(AutoResetEvent testEvent, List<GlbCompanyCampaignItem> campaignItemList)
			  : base()
			{
				this.testEvent = testEvent;
				this.campaignItemList = campaignItemList;
			}

			public VoteSurveyExamManagerForTest(AutoResetEvent testEvent, SubmissionStat stat)
			  : base()
			{
				this.testEvent = testEvent;
				this.stat = stat;
			}

			readonly AutoResetEvent testEvent;
			readonly List<GlbCompanyCampaignItem> campaignItemList;
			readonly SubmissionStat stat;

			protected override IVoteExamSurveyAnswerSet StartVoteExamSurveyInConcurrencyLock(BusinessObjectFactory factory, ZGuid campaignItemPK, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZString countryCode, ZString jobSkillCode, string[] campaignCollection, bool canCreateNewCampaignItem, ZString examVersion)
			{
				var groupedExams = new List<GlbCompanyCampaignItem>();
				if (campaignCollection.Length > 0)
				{
					foreach (var groupedCampaignPK in campaignCollection)
					{
						ZGuid cPK;
						if (ZGuid.TryParse(groupedCampaignPK, out cPK) || recipientInfo != null || !campaignItemPK.IsEmpty)
						{
							var companyCamapaignItem = StartVoteExamSurveyCore(factory, campaignItemPK, cPK, recipientInfo, jobSkillCode, canCreateNewCampaignItem, true, examVersion);
							groupedExams.Add(companyCamapaignItem);
						}
					}
				}
				else
				{
					var companyCamapaignItem = StartVoteExamSurveyCore(factory, campaignItemPK, ZGuid.Empty, recipientInfo, jobSkillCode, canCreateNewCampaignItem, true, examVersion);
					groupedExams.Add(companyCamapaignItem);
				}
				return null;
			}

			protected override GlbCompanyCampaignItem StartVoteExamSurveyCore(BusinessObjectFactory factory, ZGuid campaignItemPK, ZGuid campaignPK, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZString jobSkillCode, bool canCreateNewCampaignItem, bool isPartOfGroupedExam, ZString examVersion)
			{
				GlbCompanyCampaignItem result = null;
				if (campaignItemList.Count > 0)
				{
					result = campaignItemList[0];
				}
				else
				{
					result = factory.NewWithValidTestData<GlbCompanyCampaignItem>();
					campaignItemList.Add(result);
				}
				testEvent.Set();
				return result;
			}

			protected override SubmissionResult SaveAnswersOnSubmitButtonClickCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
			{
				return DoSubmission();
			}

			protected override SubmissionResult SaveAnswersOnTimeOutCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
			{
				saveDataSourceFunction?.Invoke();
				return DoSubmission();
			}

			protected override SubmissionResult SaveAnswersWhenNavigatingPagesCore(IVoteExamSurveyAnswerSet dataSource, Func<bool> saveDataSourceFunction)
			{
				return DoSubmission();
			}

			SubmissionResult DoSubmission()
			{
				if (!stat.IsAlreadySubmitted)
				{
					stat.CountOfSubmission++;
					Thread.Sleep(100);
					stat.IsAlreadySubmitted = true;
				}
				testEvent.Set();
				return SubmissionResult.SubmitSucceeded;
			}
		}

		public class SubmissionStat
		{
			public bool IsAlreadySubmitted { get; set; }
			public int CountOfSubmission { get; set; }
		}
	}
}
