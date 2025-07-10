using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Recruiter.ServiceTasks.HRJobApplicationParsingQueueServiceTask;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	[TestedType(typeof(HRJobApplicationParsingQueueServiceTask))]
	public sealed class HRJobApplicationParsingQueueServiceTaskTest : ServiceTaskTestCase<HRJobApplicationParsingQueueServiceTask>
	{
		protected override void TearDownCore()
		{
			base.TearDownCore();

			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		public void TestRunTask()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			AssertEquals(0, application.Documents.Count);

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForTest(true, false, false);
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			var applicationWithNewData = new BusinessObjectFactory().Load<HRJobApplication>(application.PK);
			AssertEquals("Should not change data already saved in database", "Test Name", applicationWithNewData.Applicant.HA_FullName);
			AssertEquals("Should not change data already saved in database", "test1@wisetechglobal.com", applicationWithNewData.Applicant.HA_EmailAddress);
			AssertEquals("Should add new data", "+61 413444555", applicationWithNewData.Applicant.HA_MobilePhone);
			AssertEquals("Should add new data", "2121", applicationWithNewData.Applicant.HA_Postcode);
			AssertEquals("Should add new data", "AU", applicationWithNewData.Applicant.HA_RN_NKCountry);
			AssertEquals("Should create a new document", 1, applicationWithNewData.Documents.Count);

			AssertLogs(serviceTask.TestLogger,
				new LogForTest(LogType.Information, "Job application for 'Test Name' (test1@wisetechglobal.com)", null),
				new LogForTest(LogType.Information, "Processing document 'empty.pdf'", null),
				new LogForTest(LogType.Information, "Document 'empty.pdf' has been successfully processed", null),
				new LogForTest(LogType.Debug, "ConvertApi Key is not set. Please set Recruiter -> Candidate Management -> ConvertApi Secret API Key to convert resumes to supported types", null),
				new LogForTest(LogType.Information, "Processed 1 queued document(s)", null));

			AssertEquals(0, new BusinessObjectFactory().Load<HRJobApplicationParsingQueue>(new ZQuery()).Length);
		}

		public void TestRunTaskWillCallResumeConverter()
		{
			var filePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.docx", "empty.docx");

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(filePath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			AssertEquals("PRE: Nothing up my sleeves", 0, application.Documents.Count);

			var dummy = new Mock<IResumeConverter>();
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			using (ObjectFactory.Substitute(dummy.Object))
			{
				var serviceTask = new HRJobApplicationParsingQueueServiceTaskForTest(true, false, false);
				serviceTask.RunTask();
			}

			dummy.Verify(d => d.ConvertAvailableResumes(It.Is<HRJobApplication>(a => a.PK == application.PK)), "We should request a conversion for any processed applications");
		}

		public void TestRunTask_SaveIssue()
		{
			CreateStaffAndGroup();
			CreateJobApplications();

			AssertEquals("Should have 10 items in the queue", 10, new BusinessObjectFactory().Load<HRJobApplicationParsingQueue>(new ZQuery()).Length);

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForTest(false, true, false);
			AssertEquals("Pre-Condition: Batch Size", 0, serviceTask.BatchSizeLog.Count);

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			AssertEquals(3, serviceTask.TestLogger.Logs.Count(a => a.type == LogType.Error));
			AssertEquals("Should process 10 files", new LogForTest(LogType.Information, "Processed 10 queued document(s)", null), serviceTask.TestLogger.Logs[serviceTask.TestLogger.Logs.Count - 1]);
			AssertArrayEqualsByElements("Batch Size", new int[] { 4, 4, 2 }, serviceTask.BatchSizeLog.ToArray());
			AssertEquals("Should have removed all items from the queue", 0, new BusinessObjectFactory().Load<HRJobApplicationParsingQueue>(new ZQuery()).Length);
		}

		public void TestRunTask_ApplicantIssue()
		{
			CreateStaffAndGroup();
			CreateJobApplications();

			var parsingQueue = new BusinessObjectFactory().Load<HRJobApplicationParsingQueue>(new ZQuery());
			AssertEquals("Should have 10 items in the queue", 10, parsingQueue.Length);

			var applicationPKs = new List<ZGuid>();

			foreach (var item in parsingQueue)
			{
				applicationPKs.Add(item.Application.PK);
				Assert(string.IsNullOrEmpty(item.Application.Applicant.Mobile));
			}

			AssertEquals(0, new BusinessObjectFactory().Load<HRJobApplicationDocument>(new ZQuery()).Length);

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForTest(true, false, false);
			serviceTask.TestEmailError = "test2@wisetechglobal.com";
			AssertEquals("Pre-Condition: Batch Size", 0, serviceTask.BatchSizeLog.Count);

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			ErrorReporter.Clear();

			AssertEquals(10, new BusinessObjectFactory().Load<HRJobApplicationDocument>(new ZQuery()).Length);

			var applications = new BusinessObjectFactory().Load<HRJobApplication>(new ZQuery(HRJobApplicationSchema.PK, applicationPKs));

			AssertNotEquals(0, serviceTask.TestLogger.Logs.Count(a => a.type == LogType.Error));
			AssertEquals("Should process 10 files", new LogForTest(LogType.Information, "Processed 10 queued document(s)", null), serviceTask.TestLogger.Logs[serviceTask.TestLogger.Logs.Count - 1]);
			AssertArrayEqualsByElements("Batch Size", new int[] { 4, 4, 2 }, serviceTask.BatchSizeLog.ToArray());
			AssertEquals("Should have removed all items from the queue", 0, new BusinessObjectFactory().Load<HRJobApplicationParsingQueue>(new ZQuery()).Length);
		}

		public void TestRunTask_SaveForceSave()
		{
			CreateStaffAndGroup();
			CreateJobApplications();

			var allApplications = new List<ZGuid>();
			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForTest(false, true, true);

			serviceTask.VerifyApplicationsNotSavedForTest += (object sender, VerifyApplicationsForTestEventArgs eventArgs) =>
			{
				// Called before Force Save to verify if the applications aren't saved yet
				var applicationInDb = new BusinessObjectFactory().Load<HRJobApplication>(new ZQuery(HRJobApplicationSchema.PK, eventArgs.Applications.Select(a => a.PK)));
				foreach (var item in applicationInDb)
				{
					AssertEquals("Application shouldn't be saved yet", "", item.Applicant.Mobile);
					allApplications.Add(item.PK);
				}
			};

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			var allApplicationInDb = new BusinessObjectFactory().Load<HRJobApplication>(new ZQuery(HRJobApplicationSchema.PK, allApplications));

			AssertEquals(4, serviceTask.TestLogger.Logs.Count(a => a.type == LogType.Error));

			AssertEquals(3, serviceTask.TestLogger.Logs.Count(a => a.type == LogType.Information && a.message == "Force saving applications"));
			AssertEquals(1, serviceTask.TestLogger.Logs.Count(a => a.type == LogType.Error && a.message.Contains("Error force saving application")));
		}

		void CreateStaffAndGroup()
		{
			using (Env.Instance.SuspendBranchAccessError())
			{
				var group = Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Wisetech Test";
				staff.GS_EmailAddress = "test@wisetech.com";
				staff.GS_IsActive = true;
				staff.Groups.Add(group);
			}
		}

		void CreateJobApplications()
		{
			using (Env.Instance.SuspendBranchAccessError())
			{
				var filePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.Don Antonio resume.docx", "Don Antonio resume.docx");

				for (var i = 0; i < 10; i++)
				{
					var applicant = Factory.New<HRJobApplicantForTest>();
					applicant.HA_FullName = "Test Name " + i;
					applicant.HA_EmailAddress = $"test{i}@wisetechglobal.com";
					var application = Factory.NewWithValidTestData<HRJobApplication>();
					application.HP_HA = applicant.PK;

					var doc = application.DocManagerInfo.AddFileOrDocument(filePath, "RCV");
					var queue = Factory.New<HRJobApplicationParsingQueue>();
					queue.HPQ_HP = application.PK;
					queue.HPQ_StorageDocReference = doc.UniqueKey;

					application.DocManagerInfo.Save();
				}
				Factory.Save();
			}
		}

		void AssertLogs(LoggerForTest testLogger, params LogForTest[] logs)
		{
			var expected = string.Join("\r\n", Array.ConvertAll(logs, l => l.ToString()));
			var actual = string.Join("\r\n", testLogger.Logs.ConvertAll(l => l.ToString()).ToArray());
			var message = string.Format(CultureInfo.InvariantCulture, "\r\nEXPECTED:\r\n{0}\r\n\r\nACTUAL:\r\n{1}\r\n", expected, actual);

			AssertEquals(message, logs.Length, testLogger.Logs.Count);
			for (int i = 0; i < logs.Length; i++)
			{
				var lineErrorMessage = string.Format(CultureInfo.InvariantCulture, "Line differs:{0}\r\n{1}", i + 1, message);
				AssertEquals(lineErrorMessage, logs[i], testLogger.Logs[i]);
			}
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(HRJobApplicationParsingQueueServiceTask).GetMethod(nameof(CheckDaxtraEnabled));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("This service requires Daxtra to be enabled", CheckDaxtraEnabled());
			}

			using (RecruiterDataRegistry.Instance.DaxtraEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("", CheckDaxtraEnabled());
			}
		}

		public void TestErrorReporterForProcessDocument()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			ErrorReporter.Clear();
			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForErrorReportTest(true, false, false, false, false);
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			Assert(ErrorReporter.LastMessageReported.Contains("Error processing document for application " + application.PK));
			ErrorReporter.Clear();
		}

		public void TestRunTask_ApplicantErrorsCannotSave()
		{
			var application = Factory.New<HRJobApplication>();

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			ErrorReporter.Clear();

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForErrorReportTest(false, true, false, true, false);

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			Assert(ErrorReporter.LastMessageReported.Contains("Application has errors " + application.PK));
			ErrorReporter.Clear();
		}

		public void TestRunTask_ApplicantErrorsCanSave()
		{
			var application = Factory.New<HRJobApplication>();

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			ErrorReporter.Clear();

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForErrorReportTest(false, true, false, false, false);
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			AssertEquals(0, ErrorReporter.LastMessageReported.Length);
			ErrorReporter.Clear();
		}

		public void TestErrorReporterForForceDeleteParsingQueueItems()
		{
			var application = Factory.New<HRJobApplication>();

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			ErrorReporter.Clear();

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForErrorReportTest(false, false, true, true, false);

			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			Assert(ErrorReporter.LastMessageReported.Contains("Error deleting queue items"));
			ErrorReporter.Clear();
		}

		public void TestErrorReporterForForceSaveOneByOne()
		{
			var application = Factory.New<HRJobApplication>();

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(EmptyPdfPath, "RCV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			ErrorReporter.Clear();

			var serviceTask = new HRJobApplicationParsingQueueServiceTaskForErrorReportTest(false, false, false, true, true);
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			Assert(ErrorReporter.LastMessageReported.Contains("Error force saving application"));
			ErrorReporter.Clear();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						HRJobApplicationParsingQueueSchema.Constants.TableName,
						"Parsing Queue"),
				};
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Business.Testing.HRJobApplicantTest).Assembly));

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

		sealed class HRJobApplicantForTest : HRJobApplicant
		{
			public bool HasSaveError { get; set; }

			public HRJobApplicantForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				if (HasSaveError)
				{
					throw new Exception("Can't save");
				}
			}
		}

		sealed class HRJobApplicationParsingQueueServiceTaskForErrorReportTest : HRJobApplicationParsingQueueServiceTask
		{
			readonly bool errorProcess;
			readonly bool errorApplication;
			readonly bool errorForceDelete;
			readonly bool errorSave;
			readonly bool errorForceSaving;

			public string TestEmailError { get; set; }

			public HRJobApplicationParsingQueueServiceTaskForErrorReportTest(bool errorProcess, bool errorApplication, bool errorForceDelete, bool errorSave, bool errorForceSaving)
			{
				ServiceLogger = TestLogger;
				this.errorProcess = errorProcess;
				this.errorApplication = errorApplication;
				this.errorForceDelete = errorForceDelete;
				this.errorSave = errorSave;
				this.errorForceSaving = errorForceSaving;
			}

			protected override void ForceDeleteParsingQueueItemsErrorForTest()
			{
				if (errorForceDelete)
				{
					throw new Exception("force delete exception");
				}
			}

			protected override void CreateErrorForTest(HRJobApplication application)
			{
				if (!errorProcess && !errorApplication)
				{
					return;
				}

				if (errorApplication)
				{
					application.AddRowError("application error");
				}

				if (errorProcess)
				{
					throw new Exception("boom");
				}
			}

			protected override void SaveBatchAndLoadNew(FilteredBusinessObjectReader reader)
			{
				if (!errorSave)
				{
					base.SaveBatchAndLoadNew(reader);
				}
				else
				{
					throw new Exception("boom");
				}
			}

			protected override void SimulateEmailErrorForTest(HRJobApplication application)
			{
				if (errorForceSaving)
				{
					throw new Exception("boom");
				}
			}

			public LoggerForTest TestLogger
			{
				get { return testLogger ?? (testLogger = new LoggerForTest()); }
			}

			LoggerForTest testLogger;
		}

		public sealed class HRJobApplicationParsingQueueServiceTaskForTest : HRJobApplicationParsingQueueServiceTask
		{
			public List<int> BatchSizeLog = new List<int>();
			readonly bool errorProcess;
			readonly bool errorSave;
			readonly bool errorForceSaving;
			bool testErrorDone;

			public string TestEmailError { get; set; }

			public HRJobApplicationParsingQueueServiceTaskForTest(bool errorProcess, bool errorSave, bool errorForceSaving)
			{
				ServiceLogger = TestLogger;
				BatchSize = 4;
				this.errorProcess = errorProcess;
				this.errorSave = errorSave;
				this.errorForceSaving = errorForceSaving;
			}

			protected override HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(HRJobApplication application, DaxtraResumeParser daxtraResumeParser)
			{
				return new HRJobApplicationEmailParserForTest(application, daxtraResumeParser);
			}

			protected override void CreateErrorForTest(HRJobApplication application)
			{
				if (!errorProcess)
				{
					return;
				}

				if (string.IsNullOrEmpty(TestEmailError))
				{
					TestEmailError = application.Applicant.HA_EmailAddress;
				}

				if (TestEmailError != application.Applicant.HA_EmailAddress && !testErrorDone)
				{
					application.Applicant.HA_EmailAddress = TestEmailError;
					testErrorDone = true;
				}
			}

			protected override void SaveBatchAndLoadNew(FilteredBusinessObjectReader reader)
			{
				if (!errorSave)
				{
					base.SaveBatchAndLoadNew(reader);
				}
				else
				{
					throw new Exception("boom");
				}
			}

			protected override void LogBatchSizeForTest(int batchLength)
			{
				BatchSizeLog.Add(batchLength);
			}

			protected override void SimulateEmailErrorForTest(HRJobApplication application)
			{
				if (errorForceSaving)
				{
					if (application.Applicant.HA_EmailAddress == "test2@wisetechglobal.com")
					{
						application.Applicant.HA_EmailAddress = "test3@wisetechglobal.com";
					}
				}
			}

			public LoggerForTest TestLogger
			{
				get { return testLogger ?? (testLogger = new LoggerForTest()); }
			}
			LoggerForTest testLogger;
		}

		sealed class HRJobApplicationEmailParserForTest : HRJobApplicationEmailParser
		{
			public HRJobApplicationEmailParserForTest(HRJobApplication jobApplication, IApplicantResumeParser resumeParser, bool createApplicantWithEmptyEmail = false)
				: base(jobApplication, resumeParser, createApplicantWithEmptyEmail)
			{
			}

			protected override IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
			{
				var applicant = new ApplicantResume();
				applicant.Name = "Test FullName";
				applicant.EmailAddress = "test@wisetech.com";
				applicant.Mobile = "+61 413444555";
				applicant.Postcode = "2121";
				applicant.Country = "AU";
				var resume = new ApplicantResumeParseResult() { ParsedResume = applicant, Status = ApplicantResumeParseStatus.Success };

				if (resume.Status == ApplicantResumeParseStatus.Success)
				{
					var parsedResume = resume.ParsedResume;
					CreateDocument(documentType, parsedResume.ResumeXml);
				}

				return resume;
			}
		}
	}
}
