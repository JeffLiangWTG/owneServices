using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HROnBoardingWorkflowDescriptor))]
	class HROnBoardingWorkflowDescriptorTest : WorkflowDescriptorTestCase<HROnBoardingWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("On-boarding", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode, WorkflowDescriptor.Code);
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
			AssertEquals(2, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals(WorkflowDescriptor.SubTypeInformation[0].Description, "Team");

			AssertEquals(WorkflowDescriptor.SubTypeInformation[1].Description, "Contract Status");
			AssertEquals(WorkflowDescriptor.SubTypeInformation[1].List.Count, 5);
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[0]).Code, string.Empty);
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[0]).Description, "All");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[1]).Code, "NST");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[1]).Description, "Not Sent");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[2]).Code, "SNT");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[2]).Description, "Sent");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[3]).Code, "REJ");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[3]).Description, "Rejected");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[4]).Code, "ACP");
			AssertEquals(((CodeDescriptionPair)WorkflowDescriptor.SubTypeInformation[1].List[4]).Description, "Accepted");
		}

		public override void TestSupportsEventTracking() => AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var onbaording = Factory.NewWithValidTestData<HROnBoarding>();
			onbaording.JobApplicant.HA_EmailAddress = "dummy@test.com";
			return new IWorkflowProvider[] { onbaording };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email | MessageRecipientPartyType.OnBoardingEmail;

		public void TestOnBoardingApplicantEmailUsedToSendEmail()
		{
			var onboarding = Factory.NewWithValidTestData<HROnBoarding>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "applicant@email.com";
			onboarding.HOB_HA_JobApplicant = applicant.PK;

			var trigger = MasterFilesTestHelper.CreateTrigger(onboarding, AutoEvents.AddedARecordToTheSystem);

			MasterFilesTestHelper.CreateTriggerAction(
				trigger,
				WorkflowTriggerActionTypeConstants.Codes.NotificationEmail,
				MessageRecipientPartyTypeList.Codes.OnBoardingEmail,
				emailAddress: "recipient@email.com",
				emailText: "the message");

			Factory.Save();
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();
			AssertEmailsSent("the message", new[] { "applicant@email.com" });
		}

		static void AssertEmailsSent(string expectedEmailBody, string[] recipientEmailAddresses)
		{
			var emails = Env.AllEmailsCreated.ToArray();

			CombineAssertions("Details regarding the emails that have been sent", () =>
			{
				AssertEquals("Number of emails sent", recipientEmailAddresses.Length, emails.Length);

				foreach (var email in emails)
				{
					AssertContains("Email body text", expectedEmailBody, email.Body);
				}

				AssertContainsExactElementsInAnyOrder("Email recipients", recipientEmailAddresses, emails.SelectMany(e => e.Recipients.Cast<RecipientDef>().Select(r => r.Email)));
			});
		}

		public void TestSupportsBufferManagement() => AssertEquals(true, WorkflowDescriptor.SupportsBufferManagement);
	}
}
