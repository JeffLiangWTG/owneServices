using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public static class LearningCentreTestResultHelper
	{
		public static string GetTestResult(string examId, Guid contactPK, string examSettingsCode = "")
		{
			string result = "";

			var factory = new BusinessObjectFactory();
			var campaignQuery = new ZQuery(GlbCompanyCampaignSchema.G0_CampaignID, examId);
			campaignQuery.AddToFilter(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, Core.Constants.Recruiter.LearningCentreCampaignType);
			var campaign = factory.LoadTop1<LearningCentreCampaign>(campaignQuery);

			var examVersion = GetExamVersion(examSettingsCode, examId, factory);

			if (string.IsNullOrEmpty(examVersion) && campaign != null)
			{
				examVersion = campaign.DefaultExamVersion;
			}

			var contact = factory.Load<OrgContact>(contactPK);

			if (campaign != null && contact != null)
			{
				var applicantQuery = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, contact.Email);
				var applicant = factory.LoadTop1<HRJobApplicant>(applicantQuery);

				if (applicant != null)
				{
					var itemQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK);
					itemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientTableCode, HRJobApplicantSchema.Constants.Prefix);
					itemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, applicant.PK);
					var campaignItem = factory.LoadTop1<LearningCentreCampaignItem>(itemQuery);
					if (campaignItem != null)
					{
						if (string.IsNullOrEmpty(result))
						{
							result = campaignItem.ExamScoreAsString;
						}
					}
				}
			}

			return result;
		}

		public static bool CompletedExamsWithoutPersonalEmailAddress(Guid contactPK)
		{
			var factory = new BusinessObjectFactory();
			var contact = factory.Load<OrgContact>(contactPK);

			if (contact != null && contact.Person != null && contact.Person.PER_EmailAddress.IsEmpty)
			{
				var examAttemptQuery = new ZDBOnlyQuery(typeof(ExamAttempt));

				var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.PK, GlbCompanyCampaignItemSchema.G8_RecipientID);
				applicantSubQuery.AddToFilter(HRJobApplicantSchema.HA_EmailAddress, contact.OC_Email);

				var campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.PK, ExamAttemptSchema.EXA_G8);
				campaignItemSubQuery.AddSubQuery(applicantSubQuery, JoinCondition.And);

				examAttemptQuery.AddToFilter(ExamAttemptSchema.EXA_TestCompletedUtc, SQLComparisonOperator.NotEqual, DBNull.Value);
				examAttemptQuery.AddSubQuery(campaignItemSubQuery, JoinCondition.And);

				return factory.ExistsInDatabase(AutoExamAttempt.Schema.TableName, examAttemptQuery);
			}

			return false;
		}

		static string GetExamVersion(string examSettingsCode, string examId, BusinessObjectFactory factory)
		{
			string examVersion = string.Empty;
			if (!string.IsNullOrEmpty(examSettingsCode))
			{
				if (!string.IsNullOrEmpty(examId) && examSettingsCode.StartsWith(examId, StringComparison.OrdinalIgnoreCase))
				{
					return examSettingsCode.Split(new[] { '-' })[1];
				}
				var settings = factory.LoadFromNaturalKey<ExamSetting>(ExamSettingSchema.EXS_Code, examSettingsCode);
				if (settings != null)
				{
					examVersion = settings.EXS_ExamVersion;
				}
			}

			return examVersion;
		}

		public class JobSkillProgressDetails
		{
			public JobSkillProgressDetails()
			{
			}

			readonly List<TestResultDetails> testResults = new List<TestResultDetails>();
			public void Add(TestResultDetails details)
			{
				testResults.Add(details);
			}

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public TestResultDetails[] TestResults
			{
				get
				{
					return testResults.ToArray();
				}
			}
		}

		public class TestResultDetails
		{
			public string Id { get; set; }
			public string Version { get; set; }
			public int CorrectAnswersCount { get; set; }
			public int IncorrectAnswersCount { get; set; }
			public int EmptyAnswersCount { get; set; }
			public byte ExamScore { get; set; }
			public DateTime TestCompletedUtc { get; set; }
		}

		public static TestResultDetails GetTestResultDetails(string examId, Guid contactPK, string skillCode, string examSettingsCode = "")
		{
			return GetTestResultDetails(new BusinessObjectFactory(), examId, contactPK, skillCode, examSettingsCode);
		}

		static TestResultDetails GetTestResultDetails(BusinessObjectFactory factory, string examId, Guid contactPK, string skillCode, string examSettingsCode = "")
		{
			TestResultDetails result = new TestResultDetails();

			var campaignQuery = new ZQuery(GlbCompanyCampaignSchema.G0_CampaignID, examId);
			campaignQuery.AddToFilter(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, Core.Constants.Recruiter.LearningCentreCampaignType);
			var campaign = factory.LoadTop1<LearningCentreCampaign>(campaignQuery);

			var examVersion = GetExamVersion(examSettingsCode, examId, factory);
			if (string.IsNullOrEmpty(examVersion) && campaign != null)
			{
				if (string.IsNullOrEmpty(examVersion))
				{
					examVersion = campaign.DefaultExamVersion;
				}
			}

			result.Version = examVersion;

			var contact = factory.Load<OrgContact>(contactPK);

			if (campaign != null && contact != null)
			{
				var applicantQuery = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, contact.Email);
				var applicants = contact.Person.ApplicantCollection.Cast<HRJobApplicant>()
									.Append(factory.LoadTop1<HRJobApplicant>(applicantQuery))
									.Where(x => x != null)
									.Distinct().ToArray();

				var results = new List<TestResultDetails>();
				foreach (var applicant in applicants)
				{
					var itemQuery = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK);
					itemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientTableCode, HRJobApplicantSchema.Constants.Prefix);
					itemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, applicant.PK);
					var campaignItem = factory.LoadTop1<LearningCentreCampaignItem>(itemQuery);
					if (campaignItem != null)
					{
						var mostRecentExamResult = GetTestResultDetailsFromMostRecentCompletedExam(campaignItem, skillCode, examVersion);
						if (mostRecentExamResult != null)
						{
							results.Add(mostRecentExamResult);
						}
					}
				}

				if (results.Any())
				{
					result = results.OrderByDescending(x => x.TestCompletedUtc).First();
				}
			}

			result.Id = examId;
			return result;
		}

		static TestResultDetails GetTestResultDetailsFromMostRecentCompletedExam(LearningCentreCampaignItem campaignItem, string skillCode, string examVersion)
		{
			TestResultDetails result = new TestResultDetails()
			{
				CorrectAnswersCount = campaignItem.CorrectAnswersCount,
				IncorrectAnswersCount = campaignItem.IncorrectAnswersCount,
				EmptyAnswersCount = campaignItem.EmptyAnswersCount,
				ExamScore = campaignItem.ExamScore,
				Version = examVersion,
				TestCompletedUtc = DateTime.MinValue,
			};

			return result;
		}

		public static JobSkillProgressDetails GetJobSkillProgressDetails(Guid contactPK, string skillCode)
		{
			var result = new JobSkillProgressDetails();

			return result;
		}

		public static bool HasJobSkill(string skillCode, string staffCode)
		{
			if (string.IsNullOrEmpty(skillCode) || string.IsNullOrEmpty(staffCode))
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
			if (staff == null)
			{
				return false;
			}
			return HasJobSkill(staff);
		}

		public static bool HasJobSkill(GlbStaff staff)
		{
			return false;
		}

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct JobSkillItem
		{
			public Guid HS_PK;
			public string HS_Code;
			public string Description;
		}

		public static AttemptDetails GetLastAccreditationAttemptDetails(Guid contactPK, string accreditationCode)
		{
			var result = new AttemptDetails();
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(GlbPerson));
			var subquery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
			subquery.AddToFilter(OrgContactSchema.PK, contactPK);
			query.AddSubQuery(subquery, JoinCondition.And);

			var person = factory.LoadTop1<GlbPerson>(query);
			if (person == null)
			{
				return result;
			}

			var accreditation = factory.LoadFromNaturalKey<GlbAccreditation>(GlbAccreditationSchema.HAC_Code, accreditationCode);
			if (accreditation == null)
			{
				return result;
			}

			var attempt = person.GetCurrentAttempt(accreditation.PK, ZDateTime.Today.Date) ?? person.GetLastAttempt(accreditation.PK);
			if (attempt == null)
			{
				return result;
			}

			result.Code = accreditation.HAC_Code;
			result.Description = accreditation.HAC_Description;
			result.CertificateNumber = attempt.CertificateNumber;
			result.HAA_CommencementDate = attempt.HAA_CommencementDate.IsValid ? attempt.HAA_CommencementDate.ToDateTime() : DateTime.MinValue;
			result.HAA_CompletionDate = attempt.HAA_CompletionDate.IsValid ? attempt.HAA_CompletionDate.ToDateTime() : DateTime.MinValue;
			result.HAA_CompletionDueDate = attempt.HAA_CompletionDueDate.IsValid ? attempt.HAA_CompletionDueDate.ToDateTime() : DateTime.MinValue;
			result.HAA_ExpiryDate = attempt.HAA_ExpiryDate.IsValid ? attempt.HAA_ExpiryDate.ToDateTime() : DateTime.MinValue;

			return result;
		}

		static bool CheckChainIsAlive(GlbAccreditation accreditation, GlbPerson person, List<ZGuid> accredsProcessed)
		{
			var mainAccred = accreditation.MainAccreditation ?? accreditation;
			if (IsAlive(mainAccred, person))
			{
				return true;
			}

			accredsProcessed.Add(mainAccred.PK);

			foreach (GlbAccreditation accred in mainAccred.SubsequentAccreditations)
			{
				if (accred.HAC_IsRefresher)
				{
					continue;
				}

				if (accredsProcessed.Contains(accred.PK))
				{
					return false;
				}

				if (IsAlive(accred, person))
				{
					return true;
				}

				return CheckChainIsAlive(accred, person, accredsProcessed);
			}

			return false;
		}

		static bool IsAlive(GlbAccreditation accreditation, GlbPerson person)
		{
			var current = person.GetCurrentAttempt(accreditation.PK, ZDate.Today) as GlbAccreditationAttempt;
			bool isAlive = current != null && !current.CheckIsExpired(ZDate.Today);

			if (!isAlive)
			{
				var refresher = accreditation.RefresherAccreditation;
				if (refresher != null)
				{
					current = person.GetCurrentAttempt(accreditation.RefresherAccreditation.PK, ZDate.Today) as GlbAccreditationAttempt;
					isAlive = current != null && !current.CheckIsExpired(ZDate.Today);
				}
			}

			return isAlive;
		}

		public class AttemptDetails
		{
			public Guid HAA_PK { get; set; }
			public string Code { get; set; }
			public string Description { get; set; }
			public DateTime HAA_CommencementDate { get; set; }
			public DateTime HAA_CompletionDueDate { get; set; }
			public DateTime HAA_CompletionDate { get; set; }
			public DateTime HAA_ExpiryDate { get; set; }
			public string CertificateNumber { get; set; }

			public void AddExam(ExamDetails examDetails)
			{
				exams.Add(examDetails);
			}

			readonly List<ExamDetails> exams = new List<ExamDetails>();

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public ExamDetails[] Exams => exams.ToArray();
		}

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct ExamDetails
		{
			public string ExamSettings { get; set; }
			public string Description { get; set; }
			public DateTime CommenceDate { get; set; }
			public DateTime CompletionDate { get; set; }
			public byte ExamScore { get; set; }
			public bool Passed { get; set; }
		}
	}
}
