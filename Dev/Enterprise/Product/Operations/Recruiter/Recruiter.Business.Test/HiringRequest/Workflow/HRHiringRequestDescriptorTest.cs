using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequestDescriptor))]
	sealed class HRHiringRequestDescriptorTest : WorkflowDescriptorTestCase<HRHiringRequestDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Hiring Request", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.HRHiringRequestDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking() => AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var hiringRequest = Factory.NewWithValidTestData<HRHiringRequest>();
			return new IWorkflowProvider[] { hiringRequest };
		}

		public void TestNotificationEmailSendToMultipleRecipients()
		{
			var triggerCode = AutoEvents.AddedARecordToTheSystemCode;
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "AU";
			company.GC_Code = "HRQ";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "HRQ";
			department.GE_Desc = "HRQ Test";

			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "Job Rr Applicant";

			var jobApplicant = Factory.New<HRJobApplicant>();
			jobApplicant.HA_EmailAddress = "test@test.com";
			jobApplicant.HA_PER = person.PK;

			var hiringRequest = Factory.New<HRHiringRequest>();
			hiringRequest.HRR_HA_JobApplicant = jobApplicant.PK;
			hiringRequest.HRR_ProbationDurationOverride = ZDateTime.BrettsBirthday;
			hiringRequest.HRR_JobTitle = "Another Job";
			hiringRequest.HRR_StartDate = ZDateTime.BrettsBirthday;
			hiringRequest.HRR_WorkingDays = 0;

			var trigger = hiringRequest.WorkflowItems.Triggers.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = triggerCode;
			trigger.P9_RespondToCascadedEvents = true;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "rando@email.com";

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

			var hiringRequestDescriptor = new HRHiringRequestDescriptor();
			WorkflowTriggerNotification processor = (WorkflowTriggerNotification)hiringRequestDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			AssertEquals("Should have 1 email tasks", 1, processor.Modes.Destinations.Count);
			AssertEquals("rando@email.com", processor.Modes.CommunicationModes[0].EK_Destination);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}
	}
}
