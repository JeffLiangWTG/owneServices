using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationWorkflowDescriptor))]
	sealed class HRJobApplicationWorkflowDescriptorTest : WorkflowDescriptorTestCase<HRJobApplicationWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "HRA", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Job Application", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			ICodeDescriptionPairList type1List = new CodeDescriptionPairList();
			ICodeDescriptionPairList type2List = new CodeDescriptionPairList();
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Country", "Country/Region", WorkflowDescriptor.SubTypeInformation[0].Description);
			//AssertListEquals("Correct List", Type1List, WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Sub Type 2 is R. Coordinator", "R. Coordinator", WorkflowDescriptor.SubTypeInformation[1].Description);
			//AssertListEquals("Correct List", Type2List, WorkflowDescriptor.SubTypeInformation[1].List);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				Factory.New<HRJobApplication>()
			};
		}

		public void TestEmailSubject()
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();

			var trigger = application.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Welcome to company ABC";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test";

			trigger.Parent.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Welcome to company ABC", email.Subject);
		}
	}
}
