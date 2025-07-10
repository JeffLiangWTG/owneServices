using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	[TestedType(typeof(HREmailsServiceTask))]
	public sealed class HREmailsServiceTaskTest : ServiceTaskTestCase<HREmailsServiceTask>
	{
		IDisposable userContext;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			userContext = Env.Instance.TemporaryServiceTaskContext("HRE", canRunInAnyBranch: true);
		}

		protected override void TearDownCore()
		{
			userContext.Dispose();

			base.TearDownCore();

			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		[TestDate(2019, 4, 15)]
		public void TestRunTask()
		{
			SetupTestDataForTestRunTask();
			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");
				PopulateEmailReaderForTestRunTask(reader);
				return reader;
			};
			serviceTask.RunTask();
			AssertLogsAndProcessedMailForTestRunTask();
		}

		[TestDate(2019, 4, 15)]
		public void TestRunTask_WithImapSetup()
		{
			SetupTestDataForTestRunTask();
			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(MailRetrievalProtocols.IMAP, popServer, 110, popUsername, "", "");
				PopulateEmailReaderForTestRunTask(reader);
				return reader;
			};
			serviceTask.RunTask();
			AssertLogsAndProcessedMailForTestRunTask();
		}

		public void TestRunTask_ServerExceptionWhileGettingStringEmailFromPosition()
		{
			SetupTestDataForTestRunTask();

			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;
			var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();

			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, "testServer", 110, "testAcc", "", "");
				reader.AddEmailBundle(email);
				reader.ThrowServerException = true;
				return reader;
			};
			serviceTask.RunTask();

			AssertLogs(
				new LogForTest(LogType.Debug, "Attempting to connect to Server:testServer | Mailbox:testAcc", null),
				new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
				new LogForTest(LogType.Debug, "Reading email no. 1", null),
				new LogForTest(LogType.Error, "Message unavailable", null));
		}

		public void TestRunTask_SocketExceptionWhileGettingStringEmailFromPosition()
		{
			SetupTestDataForTestRunTask();

			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;
			var email = new EmailBuilderForTesting()
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();

			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, "testServer", 110, "testAcc", "", "");
				reader.AddEmailBundle(email);
				reader.ThrowSocketException = true;
				return reader;
			};
			serviceTask.RunTask();
			AssertLogs(
				new LogForTest(LogType.Debug, "Attempting to connect to Server:testServer | Mailbox:testAcc", null),
				new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
				new LogForTest(LogType.Debug, "Reading email no. 1", null),
				new LogForTest(LogType.Warning, "Connection lost", null));
		}

		public void TestRunTask_ExceptionWhileProcessingEmail()
		{
			SetupTestDataForTestRunTask();

			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, "testServer", 110, "testAcc", "", "");
				reader.AddEmailBundle(new HRServiceEmail[1]);
				return reader;
			};
			serviceTask.RunTask();
			var expectedException = GetExpectedExceptionForTestRunTask_ExceptionWhileProcessingEmail();
			AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:testServer | Mailbox:testAcc", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Error, "Failed to read email from server. Index:1 EmailText:", null));
		}

		public void TestRunTask_RecoverFromExceptionWhileProcessingEmail()
		{
			SetupTestDataForTestRunTask();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var email1 = new EmailBuilderForTesting()
					.Subject("Test 1")
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();

			var email2 = new EmailBuilderForTesting()
					.Subject("Test 2")
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();

			var email3 = new EmailBuilderForTesting()
					.Subject("Test 3")
					.Body("BodyThatWouldNotGetSent")
					.GetEmail();

			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, "testServer", 110, "testAcc", "", "");
				reader.AddEmailBundle(email1, email2, email3);
				reader.ThrowServerExceptionOnSecondEmail = true;
				return reader;
			};
			serviceTask.RunTask();
			AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:testServer | Mailbox:testAcc", null),
					new LogForTest(LogType.Debug, "3 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:0 From: Subject:Test 1", null),
					new LogForTest(LogType.Debug, "HR email 'Test 1' created", null),
					new LogForTest(LogType.Error, "Email processing start date did not have a value.", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "Reading email no. 2", null),
					new LogForTest(LogType.Error, "Server disconnected", null),
					new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
		}

		public void TestRunTask_IncompleteMailboxSettings()
		{
			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new HREmailsServiceTask();
				serviceTask.ServiceLogger = TestLogger;

				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");
					PopulateEmailReaderForTestRunTask(reader);
					return reader;
				};

				serviceTask.RunTask();
				AssertEquals("Should not have logs", 0, TestLogger.Logs.Count);
			}
		}

		public void TestRunTaskInvalidFormat()
		{
			SetupTestDataForTestRunTask();

			TestLogger.Logs.Clear();
			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "bademail@cargowise.com";
			Factory.Save();

			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, "testServer", 110, "testAcc", "", "");
				reader.AddEmailBundle(new string[] { "test invalid format" });
				return reader;
			};

			serviceTask.RunTask();
			AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:testServer | Mailbox:testAcc", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:0 From: Subject:", null),
					new LogForTest(LogType.Debug, "HR email '' created", null),
					new LogForTest(LogType.Error, "Unable to determine date from email. This means the input email is probably corrupt or unreadable.", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null)
					);
		}

		public void TestProcessEmail()
		{
			SetupTestDataForTestRunTask();
			var serviceTask = new HREmailsServiceTask();
			serviceTask.ServiceLogger = TestLogger;
			serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
			{
				var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");
				PopulateEmailReaderForTestRunTask(reader);
				return reader;
			};
			serviceTask.RunTask();

			AssertEmail("attach@cargowise.com", "attach this to CS00000101", "attach body text", 2);
			AssertEmail("create@cargowise.com", "please create a new incident", "create incident body text", 1);
		}

		public void TestRunTask_CreateJobApplication()
		{
			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("Test FullName", "test@cargowise.com");
					emailForTest.Subject("attach this to CS00000101");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};
				serviceTask.RunTask();

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:\"Test FullName\" <test@cargowise.com> Subject:attach this to CS00000101", null),
					new LogForTest(LogType.Debug, "HR email 'attach this to CS00000101' created", null),
					new LogForTest(LogType.Debug, "ConvertApi Key is not set. Please set Recruiter -> Candidate Management -> ConvertApi Secret API Key to convert resumes to supported types", null),
					new LogForTest(LogType.Debug, "Job application for 'Test FullName' created", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestRunTask_NoExceptionWhenApplicationDeleted()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "Match Applicant";
			applicant.HA_EmailAddress = "match.applicant@gmail.com";

			var application = Factory.New<HRJobApplication>();
			application.HP_HA = applicant.PK;

			Factory.Save();

			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTestWithError();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("test@cargowise.com");
					emailForTest.Subject("Application received for New Role");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};

				serviceTask.RunTask();
				AssertEquals("Should not have exception in ExceptionReporter", 0, ExceptionReporterTestListener.Instance.Count);

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:<test@cargowise.com> Subject:Application received for New Role", null),
					new LogForTest(LogType.Debug, "HR email 'Application received for New Role' created", null),
					new LogForTest(LogType.Error, "Failed to process new application. HasErrors:True ApplicantIsNull:False IsNewApplication:True", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestRunTask_CreateJobApplication_NoResumeParsed()
		{
			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.TestCouldNotParseResume = true;
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("test@cargowise.com");
					emailForTest.Subject("Application received");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};

				AssertNoExceptionThrown("Shouldn't try to save JobApplication if couldn't set an applicant", () => serviceTask.RunTask());
				AssertEmail("test@cargowise.com", "Application received", string.Empty, 1);

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:<test@cargowise.com> Subject:Application received", null),
					new LogForTest(LogType.Debug, "HR email 'Application received' created", null),
					new LogForTest(LogType.Warning, "Could not parse the resume.", null),
					new LogForTest(LogType.Error, "Failed to process, job application was null, Daxtra failed to parse.", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestRunTask_CreateJobApplication_Resume()
		{
			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.TestCouldNotParseResume = true;
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("test@cargowise.com");
					emailForTest.Subject("Application received");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};

				AssertNoExceptionThrown("Shouldn't try to save JobApplication if couldn't set an applicant", () => serviceTask.RunTask());
				AssertEmail("test@cargowise.com", "Application received", string.Empty, 1);

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:<test@cargowise.com> Subject:Application received", null),
					new LogForTest(LogType.Debug, "HR email 'Application received' created", null),
					new LogForTest(LogType.Warning, "Could not parse the resume.", null),
					new LogForTest(LogType.Error, "Failed to process, job application was null, Daxtra failed to parse.", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestRunTask_CreateJobApplication_MatchApplication()
		{
			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "New Role";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "New Role";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 6, 1);
			opening.HV_HJ_JobRole = jobRole.PK;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "Match Applicant";
			applicant.HA_EmailAddress = "match.applicant@gmail.com";

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;

			Factory.Save();

			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("test@cargowise.com");
					emailForTest.Subject("Application received for New Role");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};

				serviceTask.RunTask();

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:<test@cargowise.com> Subject:Application received for New Role", null),
					new LogForTest(LogType.Debug, "HR email 'Application received for New Role' created", null),
					new LogForTest(LogType.Debug, "ConvertApi Key is not set. Please set Recruiter -> Candidate Management -> ConvertApi Secret API Key to convert resumes to supported types", null),
					new LogForTest(LogType.Debug, "Job application for 'Match Applicant' created", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestRunTask_CreateJobApplication_DisabledMatchJobOpening()
		{
			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "New Role";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "New Role";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 6, 1);
			opening.HV_HJ_JobRole = jobRole.PK;

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_FullName = "Match Applicant";
			applicant.HA_EmailAddress = "match.applicant@gmail.com";

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;

			Factory.Save();

			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From("test@cargowise.com");
					emailForTest.Subject("Application received for New Role");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};

				serviceTask.RunTask();
				AssertEquals(ZGuid.Empty, serviceTask.Parser.Parent.HP_HV);

				AssertLogs(
					new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
					new LogForTest(LogType.Debug, "1 emails found in the mailbox", null),
					new LogForTest(LogType.Debug, "Reading email no. 1", null),
					new LogForTest(LogType.Debug, "Email:1 Attachments:1 From:<test@cargowise.com> Subject:Application received for New Role", null),
					new LogForTest(LogType.Debug, "HR email 'Application received for New Role' created", null),
					new LogForTest(LogType.Debug, "ConvertApi Key is not set. Please set Recruiter -> Candidate Management -> ConvertApi Secret API Key to convert resumes to supported types", null),
					new LogForTest(LogType.Debug, "Job application for 'Match Applicant' created", null),
					new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
					new LogForTest(LogType.Debug, "1 emails read, processed and deleted", null));
			}
		}

		public void TestAttachEmailWithAttachments()
		{
			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var emailAddressForTest = "testAttachEmail@cargowise.com";

				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTaskForTest();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.ApplicantEmailForTest = emailAddressForTest;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");

					var emailForTest = new EmailBuilderForTesting();
					emailForTest.From(emailAddressForTest);
					emailForTest.Subject("attach this to CS00000101");
					emailForTest.WithAttachment(EmptyPdfPath);
					reader.AddEmailBundle(emailForTest.GetEmail());

					return reader;
				};
				serviceTask.RunTask();

				var queryApplicant = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, emailAddressForTest);
				var jobApplicant = Factory.LoadTop1<HRJobApplicant>(queryApplicant);
				var query = new ZQuery(HRJobApplicationSchema.HP_HA, jobApplicant.PK);
				var jobApplication = Factory.LoadTop1<HRJobApplication>(query);

				AssertEquals(2, jobApplication.DocManagerInfo.AllEDocs.Count); // Orginal email and Attachment
				var doc = jobApplication.DocManagerInfo.AllEDocs[0];
				var message = MimeMessageExtensions.CreateMessageFromEml(doc.ImageData);
				var attachments = message.GetFullAttachments();
				AssertEquals("attach this to CS00000101", message.Subject);
				AssertEquals(1, message.From.Count);
				AssertEquals(emailAddressForTest, message.From.Mailboxes.First().Address);
				AssertEquals(1, attachments.Count());
				var firstAttachment = attachments.First();
				AssertEquals("empty.pdf", firstAttachment.ContentDisposition?.FileName ?? firstAttachment.ContentType.Name);
			}
		}

		public void TestCallResumeConverter_NoApplicationCreated()
		{
			using (RecruiterDataRegistry.Instance.HREmailStartDaxtra.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			using (ObjectFactory.Substitute<IResumeConverter>(new ResumeConverterForTest(TestLogger)))
			{
				SetupTestDataForTestRunTask();
				var serviceTask = new HREmailsServiceTask();
				serviceTask.ServiceLogger = TestLogger;
				serviceTask.getEmailReaderForTest = (protocol, popServer, popUsername) =>
				{
					var reader = new EmailReaderForTest(protocol, popServer, 110, popUsername, "", "");
					PopulateEmailReaderForTestRunTask(reader);
					return reader;
				};
				serviceTask.RunTask();
				Assert(!TestLogger.Logs.Contains(new LogForTest(LogType.Information, "Called resume conversion", null)));
			}
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(HREmailsServiceTask).GetMethod(nameof(HREmailsServiceTask.CheckDaxtraEnabled));
			Assert("HostedServiceRequirement for Daxtra is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			methodInfo = typeof(HREmailsServiceTask).GetMethod(nameof(HREmailsServiceTask.CheckMailServerConfigured));
			Assert("HostedServiceRequirement for mail server is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			methodInfo = typeof(HREmailsServiceTask).GetMethod(nameof(HREmailsServiceTask.CheckMailAccountUsernameConfigured));
			Assert("HostedServiceRequirement for mail account username is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("This service requires Daxtra to be enabled", HREmailsServiceTask.CheckDaxtraEnabled());
			}

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("", HREmailsServiceTask.CheckDaxtraEnabled());
			}

			using (RecruiterDataRegistry.Instance.HREmailMailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				AssertEquals("This service requires mail server to be configured", HREmailsServiceTask.CheckMailServerConfigured());
			}

			using (RecruiterDataRegistry.Instance.HREmailMailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "mailserver"))
			{
				AssertEquals("", HREmailsServiceTask.CheckMailServerConfigured());
			}

			using (RecruiterDataRegistry.Instance.HREmailMailboxUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				AssertEquals("This service requires mail account username to be configured", HREmailsServiceTask.CheckMailAccountUsernameConfigured());
			}

			using (RecruiterDataRegistry.Instance.HREmailMailboxUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "username"))
			{
				AssertEquals("", HREmailsServiceTask.CheckMailAccountUsernameConfigured());
			}
		}

		public static void SetupTestDataForTestRunTask()
		{
			RecruiterDataRegistry.Instance.DaxtraEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RecruiterDataRegistry.Instance.HREmailMailServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestMailServer");
			RecruiterDataRegistry.Instance.HREmailMailboxUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HREmail");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Business.Testing.EmailParsingRuleTest).Assembly));

		string emptyPdfPath;
		string EmptyPdfPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyPdfPath))
				{
					emptyPdfPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.pdf", "empty.pdf");
				}
				return emptyPdfPath;
			}
		}

		void PopulateEmailReaderForTestRunTask(EmailReaderForTest reader)
		{
			reader.AddEmailBundle(
				new EmailBuilderForTesting().From("attach@cargowise.com").Subject("attach this to CS00000101").Body("attach body text").WithAttachment("123.pdf", new byte[] { 1, 2, 3 }).WithAttachment("456.pdf", new byte[] { 4, 5, 6 }).GetEmail(),
				new EmailBuilderForTesting().From("create@cargowise.com").Subject("please create a new incident").Body("create incident body text").WithAttachment("789.doc", new byte[] { 1, 2, 3 }).GetEmail());
		}

		void AssertLogsAndProcessedMailForTestRunTask()
		{
			AssertLogs(
				new LogForTest(LogType.Debug, "Attempting to connect to Server:TestMailServer | Mailbox:HREmail", null),
				new LogForTest(LogType.Debug, "2 emails found in the mailbox", null),
				new LogForTest(LogType.Debug, "Reading email no. 1", null),
				new LogForTest(LogType.Debug, "Email:1 Attachments:2 From:<attach@cargowise.com> Subject:attach this to CS00000101", null),
				new LogForTest(LogType.Debug, "HR email 'attach this to CS00000101' created", null),
				new LogForTest(LogType.Error, "Email processing start date did not have a value.", null),
				new LogForTest(LogType.Debug, "Marking email no. 1 as processed", null),
				new LogForTest(LogType.Debug, "Reading email no. 2", null),
				new LogForTest(LogType.Debug, "Email:2 Attachments:1 From:<create@cargowise.com> Subject:please create a new incident", null),
				new LogForTest(LogType.Debug, "HR email 'please create a new incident' created", null),
				new LogForTest(LogType.Error, "Email processing start date did not have a value.", null),
				new LogForTest(LogType.Debug, "Marking email no. 2 as processed", null),
				new LogForTest(LogType.Debug, "2 emails read, processed and deleted", null));
		}

		Exception GetExpectedExceptionForTestRunTask_ExceptionWhileProcessingEmail()
		{
			return new NullReferenceException();
		}

		void AssertEmail(string emailAddress, string subject, string body, int attachmentsCount)
		{
			var email = Factory.LoadTop1<HREmails>(new ZQuery(MailDBItemsSchema.MI_From, emailAddress));
			AssertNotNull("Should have saved an HREmails", email);
			AssertEquals(email.MI_Application, HREmailsServiceTask.Code);
			AssertEquals(email.MI_Status, MailStatus.Unprocessed);
			AssertEquals(email.MI_Direction, MailDirection.Receive);
			AssertEquals(email.MI_From, emailAddress);
			AssertEquals(email.MI_Subject, subject);
			AssertEquals(email.MI_Body, body);
			AssertEquals(attachmentsCount, email.MailAttachments.Count);
		}

		void AssertLogs(params LogForTest[] logs)
		{
			var expected = string.Join("\r\n", Array.ConvertAll(logs, l => l.ToString()));
			var actual = string.Join("\r\n", TestLogger.Logs.ConvertAll(l => l.ToString()).ToArray());
			var message = string.Format(CultureInfo.InvariantCulture, "\r\nEXPECTED:\r\n{0}\r\n\r\nACTUAL:\r\n{1}\r\n", expected, actual);

			AssertEquals(message, logs.Length, TestLogger.Logs.Count);
			for (int i = 0; i < logs.Length; i++)
			{
				var lineErrorMessage = string.Format(CultureInfo.InvariantCulture, "Line differs:{0}\r\n{1}", i + 1, message);
				AssertEquals(lineErrorMessage, logs[i], TestLogger.Logs[i]);
			}
		}

		LoggerForTest TestLogger
		{
			get { return testLogger ?? (testLogger = new LoggerForTest()); }
		}
		LoggerForTest testLogger;

		public abstract class HREmailsServiceTaskBaseForTest : HREmailsServiceTask
		{
			public bool TestCouldNotParseResume;
			public string ApplicantEmailForTest;

			protected HRJobApplicationEmailParser CreateParser(HRJobApplicationEmailParserForTest parser)
			{
				parser.TestCouldNotParseResume = TestCouldNotParseResume;
				parser.TestApplicantEmailAddress = ApplicantEmailForTest;
				return parser;
			}
		}

		public sealed class HREmailsServiceTaskForTest : HREmailsServiceTaskBaseForTest
		{
			public HRJobApplicationEmailParser Parser;
			protected override HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(MailItem mailItem, DaxtraResumeParser daxtraResumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail = false)
			{
				var parser = new HRJobApplicationEmailParserForTest(mailItem, daxtraResumeParser, factory, createApplicantWithEmptyEmail);
				Parser = CreateParser(parser);
				return Parser;
			}
		}

		public sealed class HREmailsServiceTaskForTestWithError : HREmailsServiceTaskBaseForTest
		{
			protected override HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(MailItem mailItem, DaxtraResumeParser daxtraResumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail = false)
			{
				var parser = new HRJobApplicationEmailParserForTest(mailItem, daxtraResumeParser, factory, createApplicantWithEmptyEmail, hasError: true);
				return CreateParser(parser);
			}
		}

		public sealed class HRJobApplicationEmailParserForTest : HRJobApplicationEmailParser
		{
			public bool TestCouldNotParseResume;
			public string TestApplicantEmailAddress;
			public bool TestEmptyEmailAddress;
			readonly bool blowup;
			readonly bool hasError;

			public HRJobApplicationEmailParserForTest(MailItem mailItem, IApplicantResumeParser resumeParser, BusinessObjectFactory factory, bool createApplicantWithEmptyEmail, bool blowup = false, bool hasError = false)
				: base(mailItem, resumeParser, factory, createApplicantWithEmptyEmail, allowMatchJobOpening: false)
			{
				this.blowup = blowup;
				this.hasError = hasError;
			}

			protected override IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
			{
				if (TestCouldNotParseResume)
				{
					return new ApplicantResumeParseResult();
				}

				var applicant = new ApplicantResume();
				applicant.Name = "Test FullName";
				if (TestEmptyEmailAddress)
				{
					applicant.EmailAddress = string.Empty;
				}
				else
				{
					applicant.EmailAddress = string.IsNullOrEmpty(TestApplicantEmailAddress) ? "match.applicant@gmail.com" : TestApplicantEmailAddress;
				}
				return new ApplicantResumeParseResult() { ParsedResume = applicant };
			}

			protected override List<ParseDataToBeProcessed> ParseMailItemCore(EmailData emailData, HRRecruitmentJobCampaign jobOpening, string filename = null)
			{
				if (blowup)
				{
					throw new Exception("boom");
				}

				var result = base.ParseMailItemCore(emailData, jobOpening, filename);

				if (hasError)
				{
					Parent.ApplicationOverallRatingDescription = "Wrong one";
				}

				return result;
			}
		}

		public sealed class ResumeConverterForTest : IResumeConverter
		{
			readonly ILogger logger;

			public ResumeConverterForTest(ILogger logger = null)
			{
				this.logger = logger;
			}

			public void AttachEdocCore(HRJobApplication jobApplication, string filename, Stream dataStream, string docType)
			{
				throw new NotImplementedException();
			}

			public bool CanConvert(string ext)
			{
				throw new NotImplementedException();
			}

			public void ConvertAvailableResumes(HRJobApplication host)
			{
				logger?.Information("Called resume conversion"); // Service Task logs are not translated
			}

			public IeDoc GetResumeToConvert(HRJobApplication host)
			{
				throw new NotImplementedException();
			}

			public Task<Stream> ConvertToPdfAsync(string inputFileExtension, Stream input)
			{
				throw new NotImplementedException();
			}

			public string ConvertedFileExtension => throw new NotImplementedException();
		}
	}
}
