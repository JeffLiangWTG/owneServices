using Enterprise.HRM.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing.RemunerationReview.Workflow
{
	[TestedType(typeof(ReviewProcessNodeWorkflowDescriptor))]
	class ReviewProcessNodeWorkflowDescriptorTest : WorkflowDescriptorTestCase<ReviewProcessNodeWorkflowDescriptor>
	{ 
		public override void TestDescription()
			=> AssertEquals("Manager Review", WorkflowDescriptor.Description);

		public override void TestID()
			=> AssertEquals(WorkflowDescriptors.ReviewProcessNodeWorkflowDescriptorCode, WorkflowDescriptor.Code);

		public override void TestRequiresBranch()
			=> Assert(!WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresClient()
			=> Assert(!WorkflowDescriptor.RequiresClient);

		public override void TestRequiresDepartment()
			=> Assert(!WorkflowDescriptor.RequiresDepartment);

		public override void TestSubTypes()
			=> AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

		protected override bool ExpectingTasksToBeCompanySpecific
			=> false;

		public override void TestRequiresPorts()
		{
			Assert(nameof(WorkflowDescriptor.RequiresPort1), !WorkflowDescriptor.RequiresPort1);
			Assert(nameof(WorkflowDescriptor.RequiresPort2), !WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsEventTracking()
			=> Assert(nameof(WorkflowDescriptor.SupportsEventTracking), WorkflowDescriptor.SupportsEventTracking);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
			=> new IWorkflowProvider[] { Factory.NewWithValidTestData<ReviewProcessNode>() };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;
	}
}
