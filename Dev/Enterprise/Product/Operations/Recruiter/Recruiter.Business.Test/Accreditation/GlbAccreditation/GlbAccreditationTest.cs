using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditation))]
	sealed class GlbAccreditationTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 2, 2)]
		public void TestCompleteExamWhenSomeExamsAreCredited()
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_EmailAddress = "per@so.n";

			var cco = CreateAccreditation("CCO", "CCO", false, null);
			var rco = CreateAccreditation("RCO", "CCO", true, cco);
			var ccs = CreateAccreditation("CCS", "CCS", false, cco);

			person.PER_FullName = "full name";
			person.PER_EmailAddress = "person@person.com";

			var applicant = (HRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_FullName = "full name";
			applicant.HA_EmailAddress = "email@address.com";
			applicant.HA_PER = person.PK;

			var exam1 = Factory.New<LearningCentreCampaign>();
			exam1.FillWithValidTestData();
			var exam2 = Factory.New<LearningCentreCampaign>();
			exam2.FillWithValidTestData();

			var question1 = exam1.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_Question = "QA1";
			question1.HY_ExamCorrectAnswer = "Y";

			var question2 = exam2.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question2.HY_Question = "QA1";
			question2.HY_ExamCorrectAnswer = "Y";

			string version1 = "STD";
			string version2 = "XXX";

			var settings1 = CreateExamSetting(exam1, version1);
			CreateExamSetting(exam1, version2);
			CreateExamSetting(exam2, version1);

			var campaignItem1 = Factory.New<LearningCentreCampaignItem>();
			campaignItem1.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = applicant.PK;
			campaignItem1.G8_G0 = exam1.PK;

			var campaignItem2 = Factory.New<LearningCentreCampaignItem>();
			campaignItem2.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = applicant.PK;
			campaignItem2.G8_G0 = exam2.PK;

			var examAttempt1 = Factory.New<IExamAttempt>(); // main, completed and expired
			var examAttempt2 = Factory.New<IExamAttempt>(); // refresher, completed and actual
			var examAttempt3 = Factory.New<IExamAttempt>(); // rogue
			var examAttempt4 = Factory.New<IExamAttempt>(); // new main

			examAttempt1.EXA_G8 = campaignItem1.PK;
			examAttempt2.EXA_G8 = campaignItem1.PK;
			examAttempt3.EXA_G8 = campaignItem2.PK;
			examAttempt4.EXA_G8 = campaignItem2.PK;

			examAttempt1.EXA_Score = 91;
			examAttempt1.EXA_TestCommencedUtc = new ZDateTime(2019, 1, 1);
			examAttempt1.EXA_TestCompletedUtc = new ZDateTime(2019, 1, 1);
			examAttempt1.EXA_Version = version1;

			examAttempt2.EXA_Score = 92;
			examAttempt2.EXA_TestCommencedUtc = new ZDateTime(2019, 11, 11);
			examAttempt2.EXA_TestCompletedUtc = new ZDateTime(2019, 11, 11);
			examAttempt2.EXA_Version = version2;

			examAttempt3.EXA_Score = 1;
			examAttempt3.EXA_TestCommencedUtc = new ZDateTime(2019, 5, 5);
			examAttempt3.EXA_TestCompletedUtc = new ZDateTime(2019, 5, 5);
			examAttempt3.EXA_Version = version1;

			Factory.Save();

			var mainAttempt = Factory.New<IGlbAccreditationAttempt>();
			mainAttempt.HAA_CommencementDate = new ZDateTime(2019, 1, 1).Date;
			mainAttempt.HAA_CompletionDate = new ZDateTime(2019, 1, 1).Date;
			mainAttempt.HAA_CompletionDueDate = new ZDateTime(2020, 1, 1).Date;
			mainAttempt.HAA_ExpiryDate = new ZDateTime(2020, 1, 1).Date;
			mainAttempt.HAA_HAC = cco.PK;
			mainAttempt.HAA_PER = person.PK;

			var refAttempt = Factory.New<IGlbAccreditationAttempt>();
			refAttempt.HAA_CommencementDate = new ZDateTime(2019, 11, 11).Date;
			refAttempt.HAA_CompletionDate = new ZDateTime(2019, 11, 11).Date;
			refAttempt.HAA_CompletionDueDate = new ZDateTime(2020, 1, 1).Date;
			refAttempt.HAA_ExpiryDate = new ZDateTime(2021, 1, 1).Date;
			refAttempt.HAA_HAC = rco.PK;
			refAttempt.HAA_PER = person.PK;

			var failedCCSAttempt = Factory.New<IGlbAccreditationAttempt>();
			failedCCSAttempt.HAA_CommencementDate = new ZDateTime(2019, 5, 5).Date;
			failedCCSAttempt.HAA_CompletionDueDate = new ZDateTime(2019, 6, 6).Date;
			failedCCSAttempt.HAA_ExpiryDate = new ZDateTime(2019, 6, 6).Date;
			failedCCSAttempt.HAA_HAC = ccs.PK;
			failedCCSAttempt.HAA_PER = person.PK;

			Factory.Save();

			((IVoteExamSurveyAnswerSet)new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem2)).StartVoteExamSurvey();

			Factory.Save();

			person.AccreditationAttemptCollection.Reload();
			ccs.Attempts.Reload(true);
			AssertEquals(1, ccs.Attempts.Count);
		}

		GlbAccreditation CreateAccreditation(string code, string certCode, bool isRefresher, IGlbAccreditation parent)
		{
			var accred = Factory.New<GlbAccreditation>();
			accred.HAC_Code = code;
			accred.HAC_CertificateCode = certCode;
			accred.HAC_Description = code + " desc";
			accred.HAC_IsRefresher = isRefresher;
			if (isRefresher)
			{
				accred.HAC_RefresherCertificateExpiryType = "RCD";
			}

			if (parent != null)
			{
				var pivot = Factory.New<GlbAccreditationRequirementPivot>();
				pivot.HAR_HAC = accred.PK;
				pivot.HAR_HAC_Parent = parent.PK;
			}

			return accred;
		}

		IExamSetting CreateExamSetting(GlbCompanyCampaign exam, string version)
		{
			var setting = Factory.New<IExamSetting>();
			setting.EXS_G0 = exam.PK;
			setting.EXS_ExamExpiryTimeInMinutes = 10;
			setting.EXS_ExamVersion = version;
			setting.EXS_MaximumAskedQuestionsPerExam = 1;
			setting.EXS_TestResultsExpireAfterHours = 1;

			return setting;
		}

		public void TestShouldCreateNewRefresherAttempt_GraceExpiry()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_Code = "AC1";
			accred.HAC_CertificateCode = "C01";

			var refresher = Factory.NewWithValidTestData<GlbAccreditation>();
			refresher.HAC_Code = "AC2";
			refresher.HAC_CertificateCode = "C01";
			refresher.HAC_IsRefresher = true;
			refresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			var attempt1 = accred.Attempts.AddNew();
			attempt1.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt1.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt1.HAA_CompletionDueDate = new ZDate(2019, 6, 6);
			attempt1.HAA_ExpiryDate = new ZDate(2019, 6, 6);
			attempt1.HAA_PER = applicant.HA_PER;

			var attempt2 = refresher.Attempts.AddNew();
			attempt2.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt2.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt2.HAA_CompletionDueDate = new ZDate(2019, 6, 6);
			attempt2.HAA_ExpiryDate = new ZDate(2019, 6, 6);
			attempt2.HAA_PER = applicant.HA_PER;

			Factory.Save();

			AssertEquals(false, accred.ShouldCreateNewAttempt(applicant.Person, new ZDate(2019, 6, 1)));
			AssertEquals(true, refresher.ShouldCreateNewAttempt(applicant.Person, new ZDate(2019, 6, 1)));
		}

		public void TestShouldCreateNewAttempt_AfterFullExpired()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_Code = "AC1";
			accred.HAC_CertificateCode = "C01";

			var refresher = Factory.NewWithValidTestData<GlbAccreditation>();
			refresher.HAC_Code = "AC2";
			refresher.HAC_CertificateCode = "C01";
			refresher.HAC_IsRefresher = true;
			refresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			var attempt = accred.Attempts.AddNew();
			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt.HAA_CompletionDueDate = new ZDate(2019, 6, 6);
			attempt.HAA_ExpiryDate = new ZDate(2019, 6, 6);
			attempt.HAA_PER = applicant.HA_PER;

			Factory.Save();

			AssertEquals(true, accred.ShouldCreateNewAttempt(applicant.Person, ZDate.Today));
		}

		[TestDate(2020, 2, 2)]
		public void TestShouldCreateNewAttempt_MainExpiredRefresherActive()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_Code = "AC1";
			accred.HAC_CertificateCode = "C01";

			var refresher = Factory.NewWithValidTestData<GlbAccreditation>();
			refresher.HAC_Code = "AC2";
			refresher.HAC_CertificateCode = "C01";
			refresher.HAC_IsRefresher = true;
			refresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			var attempt = accred.Attempts.AddNew();
			attempt.HAA_CommencementDate = new ZDate(2019, 1, 1);
			attempt.HAA_CompletionDate = new ZDate(2019, 5, 5);
			attempt.HAA_CompletionDueDate = new ZDate(2019, 6, 6);
			attempt.HAA_ExpiryDate = new ZDate(2020, 1, 1);
			attempt.HAA_PER = applicant.HA_PER;

			var refAttempt = refresher.Attempts.AddNew();
			refAttempt.HAA_CommencementDate = new ZDate(2019, 12, 31);
			refAttempt.HAA_CompletionDate = new ZDate(2019, 12, 31);
			refAttempt.HAA_CompletionDueDate = new ZDate(2020, 1, 1);
			refAttempt.HAA_ExpiryDate = new ZDate(2021, 1, 1);
			refAttempt.HAA_PER = applicant.HA_PER;

			Factory.Save();

			AssertEquals(false, accred.ShouldCreateNewAttempt(applicant.Person, ZDate.Today));
		}

		public void TestShouldCreateNewAttempt_HasManualCert()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_Code = "AC1";
			accred.HAC_CertificateCode = "C01";

			var refresher = Factory.NewWithValidTestData<GlbAccreditation>();
			refresher.HAC_Code = "AC2";
			refresher.HAC_CertificateCode = "C01";
			refresher.HAC_IsRefresher = true;
			refresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			var group = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group.HJG_ParentID = accred.PK;
			group.HJG_ParentTableCode = accred.TablePrefix;

			var exam = Factory.NewWithValidTestData<LearningCentreCampaign>();
			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			var jobSkillPivot = Factory.NewWithValidTestData<GlbAccreditationJobSkillPivot>();
			jobSkillPivot.HAJ_HJG = group.PK;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var cert = applicant.Certificates.AddNew();
			cert.XZ_Type = "C01";
			cert.XZ_IssueDate = ZDateTime.UtcNow.AddDays(-1);
			cert.XZ_ExpiryOrDueDate = ZDateTime.UtcNow.AddMonths(1);

			Factory.Save();

			AssertEquals(0, accred.Attempts.Count);
			AssertNull(accred.GetCurrentAttemptForPerson(applicant.Person, ZDateTime.UtcNow.Date));

			var campaignItem = exam.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;

			AssertEquals(true, accred.ShouldCreateNewAttempt(applicant.Person, ZDate.Today));
		}

		public void TestCompleteAttempt()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_CertificateCode = "CUS";
			accred.HAC_MustCompleteInDays = 10;

			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accred.PK;
			group1.HJG_ParentTableCode = accred.TablePrefix;
			group1.HJG_Threshold = 2;
			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accred.PK;
			group2.HJG_ParentTableCode = accred.TablePrefix;
			group2.HJG_Threshold = 1;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			group1.SkillPivots.Reload(true);
			group2.SkillPivots.Reload(true);
			accred.Attempts.Reload(true);
			AssertEquals(0, accred.Attempts.Count);
			AssertNull(accred.GetCurrentAttemptForPerson(applicant.Person, ZDate.Today));
		}

		[TestDate(2019, 11, 11)]
		public void TestIsCompleted()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			var person1 = Factory.New<GlbPerson>();
			var person2 = Factory.New<GlbPerson>();

			var attemptCompleted = Factory.New<GlbAccreditationAttempt>();
			attemptCompleted.HAA_HAC = accred.PK;
			attemptCompleted.HAA_CommencementDate = new ZDate(2019, 11, 1);
			attemptCompleted.HAA_CompletionDate = new ZDate(2019, 11, 2);
			attemptCompleted.HAA_ExpiryDate = new ZDate(2019, 11, 30);
			attemptCompleted.HAA_PER = person1.PK;

			AssertEquals(true, accred.IsCompleted(person1, ZDate.Today));
			AssertEquals(false, accred.IsCompleted(person1, ZDate.Today.AddMonths(2)));

			var attemptExpired = Factory.New<GlbAccreditationAttempt>();
			attemptExpired.HAA_HAC = accred.PK;
			attemptExpired.HAA_CommencementDate = new ZDate(2019, 11, 1);
			attemptExpired.HAA_CompletionDate = new ZDate(2019, 11, 2);
			attemptExpired.HAA_ExpiryDate = new ZDate(2019, 11, 3);
			attemptExpired.HAA_PER = person2.PK;

			AssertEquals(false, accred.IsCompleted(person2, ZDate.Today));
			AssertEquals(true, accred.IsCompleted(person2, new ZDate(2019, 11, 2)));

			var attemptFailed = Factory.New<GlbAccreditationAttempt>();
			attemptFailed.HAA_HAC = accred.PK;
			attemptFailed.HAA_CommencementDate = new ZDate(2019, 11, 3);
			attemptFailed.HAA_CompletionDueDate = new ZDate(2019, 11, 4);
			attemptFailed.HAA_PER = person1.PK;

			AssertEquals(true, accred.IsCompleted(person1, ZDate.Today));
			AssertEquals(false, accred.IsCompleted(person1, ZDate.Today.AddMonths(2)));

			var attemptCommenced = Factory.New<GlbAccreditationAttempt>();
			attemptCommenced.HAA_HAC = accred.PK;
			attemptCommenced.HAA_CommencementDate = new ZDate(2019, 11, 5);
			attemptCommenced.HAA_CompletionDueDate = new ZDate(2019, 11, 30);
			attemptCommenced.HAA_ExpiryDate = new ZDate(2019, 11, 30);
			attemptCommenced.HAA_PER = person1.PK;

			AssertEquals(true, accred.IsCompleted(person1, ZDate.Today));
			AssertEquals(false, accred.IsCompleted(person1, ZDate.Today.AddMonths(2)));
		}

		[TestDate(2019, 1, 1)]
		public void TestCompleteAttempt_WithPreReq()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_CertificateCode = "CUS";
			accred1.HAC_MustCompleteInDays = 100;

			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accred1.PK;
			group1.HJG_ParentTableCode = accred1.TablePrefix;
			group1.HJG_Threshold = 1;

			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred2.HAC_CertificateCode = "XXX";
			accred2.HAC_MustCompleteInDays = 100;

			var pivot = Factory.New<GlbAccreditationRequirementPivot>();
			pivot.HAR_HAC = accred1.PK;
			pivot.HAR_HAC_Parent = accred2.PK;

			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accred2.PK;
			group2.HJG_ParentTableCode = accred2.TablePrefix;
			group2.HJG_Threshold = 1;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			group1.SkillPivots.Reload(true);
			group2.SkillPivots.Reload(true);
			accred1.Attempts.Reload(true);
			accred2.Attempts.Reload(true);

			AssertEquals(0, accred1.Attempts.Count);
			AssertNull(accred1.GetCurrentAttemptForPerson(applicant.Person, ZDate.Today));

			AssertEquals(0, accred2.Attempts.Count);
			AssertNull(accred2.GetCurrentAttemptForPerson(applicant.Person, ZDate.Today));

			TestDateAttribute.Date = new ZDateTime(2019, 2, 2).ToDateTime();
			Factory.Save();

			var attempt2 = accred2.GetCurrentAttemptForPerson(applicant.Person, ZDateTime.UtcNow.Date);
			AssertNull(attempt2);
		}

		[TestDate(2018, 09, 07, 2, 0, 0)]
		public void TestCompletedDateEarlierThanCommenced()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_RL_NKHomePort = "USLAX";

			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DP1";

			GlbStaff createUser = Factory.NewWithValidTestData<GlbStaff>();
			createUser.GS_Code = "AAA";
			createUser.GS_LoginName = "AAA User";
			createUser.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_CertificateCode = "CUS";
			accred.HAC_MustCompleteInDays = 5;

			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accred.PK;
			group1.HJG_ParentTableCode = accred.TablePrefix;
			group1.HJG_Threshold = 2;
			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accred.PK;
			group2.HJG_ParentTableCode = accred.TablePrefix;
			group2.HJG_Threshold = 1;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			group1.SkillPivots.Reload(true);
			group2.SkillPivots.Reload(true);
			accred.Attempts.Reload(true);
			AssertNull(accred.GetCurrentAttemptForPerson(applicant.Person, ZDateTime.UtcNow.Date));
		}

		public void TestShouldCreateNewAttempt_DifferentFactory()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = new BusinessObjectFactory().NewWithValidTestData<GlbPerson>();

			accreditation.ShouldCreateNewAttempt(person2, ZDateTime.Now.Date);
			Assert(ErrorReporter.LastMessageReported.Contains("You passed a GlbPerson with a different factory to GlbAccreditation.Factory."));
			ErrorReporter.Clear();

			accreditation.ShouldCreateNewAttempt(person1, ZDateTime.Now.Date);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestHAC_RefresherCertificateExpiryType()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			AssertEquals(true, accreditation1.HAC_IsRefresher_ReadOnly);
			AssertEquals("Should be read only since this is a main accreditation", true, accreditation1.HAC_RefresherCertificateExpiryTypeInfo.ReadOnly);
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			AssertEquals("Should not be read only since this is a refresher accreditation", false, accreditation2.HAC_RefresherCertificateExpiryTypeInfo.ReadOnly);
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			Factory.Save();

			var accreditation2InNewFactory = new BusinessObjectFactory().Load<GlbAccreditation>(accreditation2.PK);
			AssertEquals(true, accreditation2InNewFactory.HAC_IsRefresher);
			AssertEquals(false, accreditation2InNewFactory.HAC_RefresherCertExpirationType_ReadOnly);
			AssertEquals("C01", accreditation2InNewFactory.HAC_CertificateCode);
			AssertEquals(RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod, accreditation2InNewFactory.HAC_RefresherCertificateExpiryType);

			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			Factory.Save();

			accreditation2InNewFactory = new BusinessObjectFactory().Load<GlbAccreditation>(accreditation2.PK);
			AssertEquals(true, accreditation2InNewFactory.HAC_IsRefresher);
			AssertEquals(false, accreditation2InNewFactory.HAC_RefresherCertExpirationType_ReadOnly);
			AssertEquals("C01", accreditation2InNewFactory.HAC_CertificateCode);
			AssertEquals(RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault, accreditation2InNewFactory.HAC_RefresherCertificateExpiryType);
		}

		public void TestResetHAC_IsRefresherShouldResetHAC_RefresherCertificateExpiryType()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;
			AssertEquals(RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod, accreditation2.HAC_RefresherCertificateExpiryType);

			accreditation2.HAC_IsRefresher = false;
			AssertEquals(ZString.Empty, accreditation2.HAC_RefresherCertificateExpiryType);
		}

		public void TestGetFullAccreditationExpiryDate()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			accreditation1.StartAttempt(person1);

			AssertEquals("Precondition", 1, person1.AccreditationAttemptCollection.Count);
			var lastAttempt = (GlbAccreditationAttempt)person1.AccreditationAttemptCollection[0];
			lastAttempt.HAA_CompletionDate = ZDate.Today.AddDays(2);
			lastAttempt.HAA_ExpiryDate = ZDate.Today.AddDays(5);
			Factory.Save();

			AssertEquals("Should be invalid", ZDate.Empty, accreditation1.GetFullAccreditationExpiryDate(person1, ZDate.Today));
			AssertEquals("Should be the expiry date of the main accreditation", ZDate.Today.AddDays(5), accreditation2.GetFullAccreditationExpiryDate(person1, ZDate.Today));
		}
	}
}
