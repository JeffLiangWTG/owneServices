using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationAttemptDocumentSenderTest : TestCaseWithFactory
	{
		public void TestGlbAccreditationAttemptDocumentSenderShouldNotSave()
		{
			var data = PrepareData();
			var action = data.action;
			var attempt = data.attempt;

			var notifications = new NotificationCollection();
			var sender = new GlbAccreditationAttemptDocumentSender(action, attempt, data.log);
			sender.Process(notifications);

			var filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, attempt.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);
			Assert(!printJobs[0].IsInDatabase);
		}

		public void TestGlbAccreditationAttemptDocumentSender()
		{
			var data = PrepareData();
			var attempt = data.attempt;

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, attempt.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);
			Assert(printJobs[0].IsInDatabase);
		}

		public void TestProcessWithMoreRecipientTypes()
		{
			var data = PrepareData();
			var action = data.action;
			var person = data.person;
			var attempt = data.attempt;
			person.PER_EmailAddress = "";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			Factory.Save();

			var notifications = new NotificationCollection();
			var sender = new GlbAccreditationAttemptDocumentSender(action, attempt, data.log);
			sender.Process(notifications);
			AssertEquals(action.EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(attempt), notifications.GetFirstMessage());

			person.PER_EmailAddress = "Just1n@email.com";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Factory.Save();

			sender.Process(notifications);
			var filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, attempt.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertEquals(Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("Just1n@email.com", printJob.SP_Destination);
		}

		public void TestProcess()
		{
			var data = PrepareData();
			var action = data.action;
			var attempt = data.attempt;

			var notifications = new NotificationCollection();
			var sender = new GlbAccreditationAttemptDocumentSender(action, attempt, data.log);
			sender.Process(notifications);

			var filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, attempt.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals(Core.Constants.ContactNotifyModes.Email, printJob.SP_JobType);
			AssertEquals("someone@123.org", printJob.SP_Destination);
			AssertEquals(OrgConstants.AttachmentType.HTMF, printJob.SP_EmailAttachmentFormat);
			AssertEquals("Certificate" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob.SP_DocumentName);
			AssertEquals(GlbAccreditationAttemptSchema.Constants.TableName, printJob.SP_ParentTableName);
			AssertEquals(attempt.PK, printJob.SP_ParentGuid);
		}

		TestData PrepareData()
		{
			var accreditation = Factory.New<GlbAccreditation>();
			accreditation.HAC_Code = "CCO";
			accreditation.HAC_Description = "Certified Operator";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_EmailAddress = "someone@123.org";

			var attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_PER = person.PK;
			attempt.HAA_HAC = accreditation.PK;
			attempt.HAA_CommencementDate = ZDate.Today;
			attempt.HAA_CompletionDueDate = attempt.HAA_CommencementDate.AddYears(1);
			attempt.HAA_CompletionDate = attempt.HAA_CommencementDate;

			var trigger = attempt.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Complete CCO";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "(*Person.PER_EmailAddress*)";

			var document = Factory.New<StmMenuItem>();
			document.SU_BusinessContext = nameof(BusinessContext.AccreditationAttempt);
			document.SU_MenuName = "Accreditation Certificate";
			document.SU_PreventAutoDelivery = false;
			document.SU_DefaultAttachmentType = OrgConstants.AttachmentType.HTMF;

			var docTemplate = Factory.New<StmTemplate>();
			docTemplate.SO_Name = "Certificate";
			docTemplate.SO_Template = DocumentEngineTestHelperBase.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Certificate]
{A}-[#SectionBody]
{B}-[<Now>]
{A}-[#EndOfReport]
");
			docTemplate.SO_DataContext = nameof(Core.Constants.DataContext.GenericFreightJob);

			var docPivot = Factory.New<StmMenuTemplatePivot>();
			docPivot.SI_SU = document.PK;
			docPivot.SI_SO = docTemplate.PK;

			action.PQ_SU_Document = document.PK;

			Factory.Save();

			var log = person.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");

			Factory.Save();

			return new TestData(action, attempt, person, log);
		}

		sealed class TestData
		{
			internal readonly ProcessTaskNotification action;
			internal readonly GlbAccreditationAttempt attempt;
			internal readonly GlbPerson person;
			internal readonly StmALog log;

			public TestData(ProcessTaskNotification action, GlbAccreditationAttempt attempt, GlbPerson person, StmALog log)
			{
				this.action = action;
				this.attempt = attempt;
				this.person = person;
				this.log = log;
			}
		}

		[ExpectNoExceptions]
		public void TestProcess_MissingDocument()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();

			var trigger = attempt.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Complete CCO";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;

			var log = attempt.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "A");

			var notifications = new NotificationCollection();
			var sender = new GlbAccreditationAttemptDocumentSender(action, attempt, log);
			sender.Process(notifications);
		}

		[ExpectNoExceptions]
		public void TestProcess_IncorrectParent()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();

			var trigger = application.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Complete CCO";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;

			var log = application.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");

			var notifications = new NotificationCollection();
			var sender = new GlbAccreditationAttemptDocumentSender(action, application, log);
			sender.Process(notifications);
		}
	}
}
