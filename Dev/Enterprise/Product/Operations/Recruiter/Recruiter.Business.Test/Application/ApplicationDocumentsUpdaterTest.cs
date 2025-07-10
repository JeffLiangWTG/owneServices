using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Recruiter.Business.ApplicationDocumentsUpdater;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ApplicationDocumentsUpdaterTest : TestCaseWithFactory
	{
		HRRecruitmentJobCampaign openingMatching;
		HRRecruitmentJobCampaign openingNotMatching;
		HRJobApplication application1;
		HRJobApplication application2;
		HRJobApplication application3;
		HRJobApplicant applicant;

		public void TestBatchKeepsProcessingAfterError()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioResumeDocxPath, "CVR");
			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioCoverLetterDocxPath, "CVL");

			Factory.Save();

			var parser = new ApplicantResumeParserForTest(Path.GetDirectoryName(DonAntonioResumeDocxPath), application.PK, true, false, false, false, false, false, true);
			var updater = new ApplicationDocumentsUpdaterForTest(parser);
			updater.ProcessBatch(new[] { application });

			AssertEquals(1, updater.Stats.DocumentsSaved);
		}

		public void TestDoNotReprocessFailedBatch()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioResumeDocxPath, "CVR");
			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioCoverLetterDocxPath, "CVL");

			Factory.Save();

			var parser = new ApplicantResumeParserForTest(Path.GetDirectoryName(DonAntonioResumeDocxPath), application.PK, true, false, false, false, false, true);
			var updater = new ApplicationDocumentsUpdaterForTest(parser);
			updater.ProcessBatch(new[] { application });

			AssertEquals(1, updater.ProcessTokensCount);
		}

		class ApplicationDocumentsUpdaterForTest : ApplicationDocumentsUpdater
		{
			public ApplicationDocumentsUpdaterForTest(IApplicantResumeParser parser) : base(parser)
			{
			}

			public int ProcessTokensCount { get; private set; }
			protected override List<DataFromZip> ProcessTokens(List<string> tokens, IEnumerable<BusinessObject> bizOs)
			{
				if (tokens.Count > 0)
				{
					ProcessTokensCount++;
				}

				if (ProcessTokensCount < 5) // to prevent stack overflow in test
				{
					return base.ProcessTokens(tokens, bizOs);
				}
				else
				{
					tokens.Clear();
				}

				return new List<DataFromZip>();
			}
		}

		public void TestConversionFailed()
		{
			var subject = "Application received for Batman";
			ZDateTime applicationsDate = new ZDateTime(2019, 3, 1);
			var mailItem = PrepareData(subject, applicationsDate);
			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK, true, false, true));

			string[] errors = null;
			updater.Finished += (sender, e) => errors = e.Errors.ToArray();

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertNotNull(errors);
			AssertEquals(1, errors.Length);
			AssertEquals("FAIL - Conversion Failure: 1", errors[0]);
		}

		public void TestProcessEmailsBacklog_MatchJobOpening()
		{
			string subject = "Application received for Batman";
			ZDateTime applicationsDate = new ZDateTime(2019, 3, 1);
			var mailItem = PrepareData(subject, applicationsDate);

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(2, application1.Documents.Count);
			AssertEquals(0, application2.Documents.Count);
			AssertEquals(0, application3.Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_MatchSubmissionPeriod()
		{
			string subject = "Application received for Joker";
			ZDateTime applicationsDate = new ZDateTime(2019, 3, 1);
			var mailItem = PrepareData(subject, applicationsDate);

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(4, updater.Stats.DocumentsSaved);
			AssertEquals(2, application1.Documents.Count);
			AssertEquals(0, application2.Documents.Count);
			AssertEquals(2, application3.Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_MatchExactTime()
		{
			string subject = "Application received for Joker";
			ZDateTime applicationsDate = new ZDateTime(2019, 3, 20);
			var mailItem = PrepareData(subject, applicationsDate);

			var parser = new ApplicantResumeParserForTest(string.Empty, mailItem.PK);
			var updater = new ApplicationDocumentsUpdater(parser);

			var zipFileName = ZGuid.Empty + Separator + subject + ".msg";
			var resume = new ApplicantResume();
			resume.ResumeXml = resourceRetriever.Value.GetString("Enterprise.Recruiter.Business.Testing.Application.TestFiles.GoodXml.xml");
			resume.EmailAddress = "donantonio@wisetechglobal.com";

			var dataFromZip = new DataFromZip(zipFileName, ZGuid.Empty, Encoding.UTF8.GetBytes(resume.ResumeXml));
			var application = updater.ProcessParsedDataCore(mailItem, dataFromZip, resume, false, false);

			AssertEquals(true, application.IsInDatabase);
			AssertEquals(1, updater.Stats.DocumentsSaved);
			AssertEquals(1, application1.Documents.Count);
			AssertEquals(0, application2.Documents.Count);
			AssertEquals(0, application3.Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_CreateNewWhenUnmatched()
		{
			string subject = "Application received for Joker";
			ZDateTime applicationsDate = new ZDateTime(2018, 3, 1);
			var mailItem = PrepareData(subject, applicationsDate);

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));

			applicant.Applications.DeleteAll();
			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(0, application1.Documents.Count);
			AssertEquals(0, application2.Documents.Count);
			AssertEquals(0, application3.Documents.Count);
			AssertEquals(1, applicant.Applications.Count);
			AssertEquals(2, applicant.Applications[0].Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_CreateNewWhenOlderExists()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "donantonio@wisetechglobal.com";

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var openingOld = Factory.New<HRRecruitmentJobCampaign>();
			openingOld.HV_AdTitle = "Batman";
			openingOld.HV_CampaignStartDate = new ZDateTime(2018, 1, 1); //email date is 13.03.2019
			openingOld.HV_CampaignEndDate = new ZDateTime(2018, 5, 1);
			openingOld.HV_HJ_JobRole = jobRole.PK;

			var openingNew = Factory.New<HRRecruitmentJobCampaign>();
			openingNew.HV_AdTitle = "Batman";
			openingNew.HV_CampaignStartDate = new ZDateTime(2019, 3, 1); //email date is 13.03.2019
			openingNew.HV_CampaignEndDate = new ZDateTime(2019, 4, 1);
			openingNew.HV_HJ_JobRole = jobRole.PK;

			var oldApplication = Factory.New<HRJobApplication>();
			oldApplication.HP_HA = applicant.PK;
			oldApplication.HP_HV = openingOld.PK;
			oldApplication.HP_SubmissionTimeUtc = new ZDateTime(2018, 2, 2);

			MailItem mailItem = CreateEmail("Application received for Batman");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));
			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(2, applicant.Applications.Count);
			AssertEquals(0, oldApplication.Documents.Count);
			AssertEquals(2, applicant.Applications[1].Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_ExistingApplicant_MatchingEmail()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "test123@wisetechglobal.com"; //Different email from the resume
			applicant.HA_FullName = "fool name";
			applicant.HA_MobilePhone = "+61 444 444 4444";

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			MailItem mailItem = CreateEmail("Application received for Batman");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));
			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);

			var applicants = Factory.Load<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "test123@wisetechglobal.com"));
			AssertEquals(1, applicants.Length);

			AssertEquals(1, applicants[0].Applications.Count);
			AssertEquals(2, applicants[0].Applications[0].Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_ExistingApplicant_MatchingHomePhone()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "test123@wisetechglobal.com"; //Different from the resume
			applicant.HA_FullName = "fool name";
			applicant.HA_MobilePhone = "+61 444 444 5555";//Different from the resume
			applicant.HA_HomePhone = "02 2222 2222";

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			MailItem mailItem = CreateEmail("Application received for Batman");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));
			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);

			var applicants = Factory.Load<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "test123@wisetechglobal.com"));
			AssertEquals(1, applicants.Length);

			AssertEquals(1, applicants[0].Applications.Count);
			AssertEquals(2, applicants[0].Applications[0].Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_NoApplicantExistingPerson_MatchingMobile()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_EmailAddress = "donantonio@wisetechglobal.com";
			person.PER_FullName = "fool name";
			person.PER_MobilePhone = "12345";

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			MailItem mailItem = CreateEmail("Application received for Batman");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));
			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);

			var applicants = Factory.Load<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "donantonio@wisetechglobal.com"));
			AssertEquals(1, applicants.Length);

			AssertEquals(1, applicants[0].Applications.Count);
			AssertEquals(2, applicants[0].Applications[0].Documents.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);

			var persons = Factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_EmailAddress, "donantonio@wisetechglobal.com"));
			AssertEquals(1, persons.Length);
			AssertEquals(person.PK, applicants[0].HA_PER);
			AssertEquals("Should not update", "fool name", person.PER_FullName);
			AssertEquals("Should not update", "12345", person.PER_MobilePhone);
		}

		public void TestProcessEmailsBacklog_NoApplicant()
		{
			string subject = "Application received for Joker";
			var mailItem = CreateEmail(subject);
			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			var application = Factory.Load<HRJobApplication>(new ZQuery()).FirstOrDefault();

			AssertNotNull(application);

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(2, application.Documents.Count);
			AssertEquals(1, application.Applicant.Applications.Count);
			AssertEquals(MailManager.MailStatus.Processed, mailItem.MI_Status);
		}

		public void TestProcessEmailsBacklog_BadEmail()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Status = MailManager.MailStatus.MarkedForReprocessing;
			mailItem.MI_ReceivedDateTime = ZDateTime.Today;
			mailItem.MI_SendDateTime = new ZDateTime(2019, 3, 20);
			mailItem.MI_Direction = MailManager.MailDirection.Receive;
			mailItem.MI_Application = ApplicationDocumentsUpdater.HREmailsServiceTaskCode;
			mailItem.MI_Subject = "Application received for Batman";

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, mailItem.PK));

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());

			AssertEquals(MailManager.MailStatus.Failed, mailItem.MI_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessEmailsBacklog_WithoutAttachmentsWithEmailBody()
		{
			var testFilesDir = BaseSourcePath + @"Enterprise\Product\Operations\Recruiter\Recruiter.Business.Test\Application\TestFiles\";
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var mail = CreateEmail("Email1");
			mail.MailAttachments.RemoveAndDeleteAll();
			mail.MI_Body = "<html>email body...</html>";

			Factory.Save();

			var parser = new ApplicantResumeParserForTest(testFilesDir, mail.PK);
			var updater = new ApplicationDocumentsUpdater(parser);

			var getDataEventCallCount = 0;
			parser.GetDataEvent += () =>
			{
				getDataEventCallCount++;

				if (getDataEventCallCount == 1) //good email body
				{
					return (null, null, "GoodXml.xml");
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			var readFromXmlEventCallCount = 0;
			parser.ReadFromXmlEvent += (r) =>
			{
				readFromXmlEventCallCount++;
				if (getDataEventCallCount == 1) //good email body
				{
					r.Name = "Jim Green";
					r.EmailAddress = "jg@cw1.com";
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());
			AssertEquals(MailManager.MailStatus.Processed, mail.MI_Status);
			AssertEquals(1, getDataEventCallCount);
			AssertEquals(1, readFromXmlEventCallCount);
			AssertNotNull(Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "jg@cw1.com")));
			AssertEquals(1, updater.Stats.BatchesTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessEmailsBacklog_WithBadAttachmentsWithEmailBody()
		{
			var testFilesDir = BaseSourcePath + @"Enterprise\Product\Operations\Recruiter\Recruiter.Business.Test\Application\TestFiles\";
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var mail = CreateEmail("Email1");
			mail.MI_Body = "<html>email body...</html>";
			Factory.Save();

			var parser = new ApplicantResumeParserForTest(testFilesDir, mail.PK);
			var updater = new ApplicationDocumentsUpdater(parser);

			var getDataEventCallCount = 0;
			parser.GetDataEvent += () =>
			{
				getDataEventCallCount++;

				if (getDataEventCallCount == 1) //bad resume, null cover, null email body
				{
					return ("BadXml.xml", null, null);
				}
				else if (getDataEventCallCount == 2) //good email body
				{
					return (null, null, "GoodXml.xml");
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			var readFromXmlEventCallCount = 0;
			parser.ReadFromXmlEvent += (r) =>
			{
				readFromXmlEventCallCount++;
				if (getDataEventCallCount == 1) //bad resume, null cover, null email body
				{
					r.Name = "";
					r.EmailAddress = "";
				}
				else if (getDataEventCallCount == 2) //good email body
				{
					r.Name = "Jim Green";
					r.EmailAddress = "jg@cw1.com";
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());
			AssertEquals(MailManager.MailStatus.Processed, mail.MI_Status);
			AssertEquals(2, getDataEventCallCount);
			AssertEquals(2, readFromXmlEventCallCount);
			AssertNotNull(Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "jg@cw1.com")));
			AssertEquals(2, updater.Stats.BatchesTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessEmailsBacklog_WithBadAttachmentsWithBadEmailBody()
		{
			var testFilesDir = BaseSourcePath + @"Enterprise\Product\Operations\Recruiter\Recruiter.Business.Test\Application\TestFiles\";
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var mail = CreateEmail("Email1");
			mail.MI_Body = "<html>email body...</html>";
			Factory.Save();

			var parser = new ApplicantResumeParserForTest(testFilesDir, mail.PK);
			var updater = new ApplicationDocumentsUpdater(parser);

			var getDataEventCallCount = 0;
			parser.GetDataEvent += () =>
			{
				getDataEventCallCount++;

				if (getDataEventCallCount == 1) //bad resume, null cover, null email body
				{
					return ("BadXml.xml", null, null);
				}
				else if (getDataEventCallCount == 2) //bad email body
				{
					return (null, null, "BadXml.xml");
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			var readFromXmlEventCallCount = 0;
			parser.ReadFromXmlEvent += (r) =>
			{
				readFromXmlEventCallCount++;
				if (getDataEventCallCount == 1) //bad resume, null cover, null email body
				{
					r.Name = "";
					r.EmailAddress = "";
				}
				else if (getDataEventCallCount == 2) //bad email body
				{
					r.Name = "";
					r.EmailAddress = "";
				}
				else
				{
					throw new InvalidOperationException();
				}
			};

			updater.ProcessEmailsBacklog(new System.Threading.CancellationToken());
			AssertEquals(MailManager.MailStatus.Failed, mail.MI_Status);
			AssertEquals(2, getDataEventCallCount);
			AssertEquals(2, readFromXmlEventCallCount);
			AssertNull(Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_EmailAddress, "jg@cw1.com")));
			AssertEquals(2, updater.Stats.BatchesTotal);
		}

		MailItem PrepareData(string subject, ZDateTime applicationsDate)
		{
			applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "don Antonio";
			applicant.HA_EmailAddress = "donantonio@wisetechglobal.com";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2.HA_EmailAddress = "wrong@wisetechglobal.com";
			MailItem mailItem = CreateEmail(subject);

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			openingMatching = Factory.New<HRRecruitmentJobCampaign>();
			openingMatching.HV_AdTitle = "Batman";
			openingMatching.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			openingMatching.HV_CampaignEndDate = new ZDateTime(2019, 3, 20);
			openingMatching.HV_HJ_JobRole = jobRole.PK;

			openingNotMatching = Factory.New<HRRecruitmentJobCampaign>();
			openingNotMatching.HV_AdTitle = "Batman";
			openingNotMatching.HV_CampaignStartDate = new ZDateTime(2019, 4, 1); //email date is 13.03.2019
			openingNotMatching.HV_CampaignEndDate = new ZDateTime(2019, 5, 20);
			openingNotMatching.HV_HJ_JobRole = jobRole.PK;

			application1 = openingMatching.Applications.AddNew(); // matching by applicant, matching by date
			application1.HP_SubmissionTimeUtc = applicationsDate;
			application1.HP_HA = applicant.PK;

			application2 = openingMatching.Applications.AddNew(); // not matching by applicant, matching by date
			application2.HP_SubmissionTimeUtc = applicationsDate;
			application2.HP_HA = applicant2.PK;

			application3 = openingNotMatching.Applications.AddNew(); // matching by applicant, matching by date, not matching by opening
			application3.HP_SubmissionTimeUtc = applicationsDate;
			application3.HP_HA = applicant.PK;

			Factory.Save();
			return mailItem;
		}

		MailItem CreateEmail(string subject)
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Status = MailManager.MailStatus.MarkedForReprocessing;
			mailItem.MI_ReceivedDateTime = ZDateTime.Today;
			mailItem.MI_SendDateTime = new ZDateTime(2019, 3, 20);
			mailItem.MI_Direction = MailManager.MailDirection.Receive;
			mailItem.MI_Application = ApplicationDocumentsUpdater.HREmailsServiceTaskCode;
			mailItem.MI_Subject = subject;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			attachment.MA_FileName = "resume.pdf";
			return mailItem;
		}

		public void TestAttachApplicationDocumentsOneApplication()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioResumeDocxPath, "CVR");
			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioCoverLetterDocxPath, "CVL");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(Path.GetDirectoryName(DonAntonioResumeDocxPath), application.PK));
			updater.ProcessBatch(new[] { application });

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(2, application.Documents.Count);
		}

		public void TestAttachApplicationDocumentsMultipleApplications_ProcessBoth()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_HA = applicant.PK;
			application1.HP_SubmissionTimeUtc = new ZDateTime(2019, 1, 1);
			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_HA = applicant.PK;
			application2.HP_SubmissionTimeUtc = new ZDateTime(2019, 2, 2);

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioMsgPath, "CVR");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, application2.PK));
			updater.ProcessBatch(new[] { application1, application2 });

			AssertEquals(2, updater.Stats.ItemsTotal);
			AssertEquals(2, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(0, application1.Documents.Count);
			AssertEquals(2, application2.Documents.Count);
		}

		public void TestAttachApplicationDocumentsMultipleApplications_ProcessOne_Old()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_HA = applicant.PK;
			application1.HP_SubmissionTimeUtc = new ZDateTime(2019, 1, 1);
			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_HA = applicant.PK;
			application2.HP_SubmissionTimeUtc = new ZDateTime(2019, 2, 2);

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioMsgPath, "MSC");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, application1.PK));
			updater.ProcessBatch(new[] { application1 });

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(0, updater.Stats.BatchesSent);
			AssertEquals(0, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(0, updater.Stats.DocumentsSaved);
			AssertEquals(0, application1.Documents.Count);
			AssertEquals(0, application2.Documents.Count);
		}

		public void TestAttachApplicationDocumentsMultipleApplications_ProcessOne_Last()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_HA = applicant.PK;
			application1.HP_SubmissionTimeUtc = new ZDateTime(2019, 1, 1);
			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_HA = applicant.PK;
			application2.HP_SubmissionTimeUtc = new ZDateTime(2019, 2, 2);

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioMsgPath, "CVR");

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, application2.PK));
			updater.ProcessBatch(new[] { application2 });

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(0, application1.Documents.Count);
			AssertEquals(2, application2.Documents.Count);
		}

		public void TestAttachApplicationDocumentsMultipleResumes()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_SubmissionTimeUtc = new ZDateTime(2019, 1, 1);

			var batmanMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.batman.msg", "batman.msg");
			var edoc1 = applicant.DocManagerInfo.AddFileOrDocument(batmanMsgPath, "CVR");
			edoc1.DateAdded = new ZDateTime(2019, 1, 1);
			var edoc2 = applicant.DocManagerInfo.AddFileOrDocument(DonAntonioMsgPath, "CVR");
			edoc2.DateAdded = new ZDateTime(2019, 2, 2);

			Factory.Save();

			var updater = new ApplicationDocumentsUpdater(new ApplicantResumeParserForTest(string.Empty, application.PK));
			updater.ProcessBatch(new[] { application });

			AssertEquals(1, updater.Stats.ItemsTotal);
			AssertEquals(1, updater.Stats.ItemsProcessed);
			AssertEquals(1, updater.Stats.BatchesSent);
			AssertEquals(1, updater.Stats.BatchesProcessed);
			AssertEquals(1, updater.Stats.BatchesTotal);
			AssertEquals(2, updater.Stats.DocumentsSaved);
			AssertEquals(2, application.Documents.Count);
		}

		public void TestSendBatch()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioResumeDocxPath, "CVR");
			applicant.DocManagerInfo.AddFileOrDocument(DonAntonioCoverLetterDocxPath, "CVL");

			Factory.Save();

			var parser = new ApplicantResumeParserForTest(string.Empty, application.PK);
			var updater = new ApplicationDocumentsUpdater(parser);
			updater.ProcessBatch(new[] { application });

			var extractor = new ZipExtractor();

			using (var ms = new MemoryStream(parser.LastBatch))
			{
				Assert(extractor.ContainsFile(ms, application.PK + "~~~Don Antonio resume.docx"));
				Assert(extractor.ContainsFile(ms, application.PK + "~~~Don Antonio Cover letter.docx"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cvr = Factory.New<RefDocType>();
			cvr.RT_DocType = "CVR";
			cvr.RT_Desc = "resume";
			cvr.RT_ReferenceType = "ALL";

			var cvl = Factory.New<RefDocType>();
			cvl.RT_DocType = "CVL";
			cvl.RT_Desc = "cover";
			cvl.RT_ReferenceType = "ALL";

			Factory.Save();

			RecruiterDataRegistry.Instance.DocTypeCV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cvr.PK.ToGuid());
			RecruiterDataRegistry.Instance.DocTypeCoverLetter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cvl.PK.ToGuid());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string donAntonioResumeDocxPath;
		string DonAntonioResumeDocxPath
		{
			get
			{
				if (string.IsNullOrEmpty(donAntonioResumeDocxPath))
				{
					donAntonioResumeDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio resume.docx", "Don Antonio resume.docx");
				}
				return donAntonioResumeDocxPath;
			}
		}

		string donAntonioCoverLetterDocxPath;
		string DonAntonioCoverLetterDocxPath
		{
			get
			{
				if (string.IsNullOrEmpty(donAntonioCoverLetterDocxPath))
				{
					donAntonioCoverLetterDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio Cover letter.docx", "Don Antonio Cover letter.docx");
				}
				return donAntonioCoverLetterDocxPath;
			}
		}

		string donAntonioMsgPath;
		string DonAntonioMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(donAntonioMsgPath))
				{
					donAntonioMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.donantonio.msg", "donantonio.msg");
				}
				return donAntonioMsgPath;
			}
		}
	}
}
