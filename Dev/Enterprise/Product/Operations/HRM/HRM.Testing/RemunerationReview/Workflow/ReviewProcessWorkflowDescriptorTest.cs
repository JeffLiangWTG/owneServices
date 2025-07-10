using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Common.Testing
{
	[TestedType(typeof(ReviewProcessWorkflowDescriptor))]
	class ReviewProcessWorkflowDescriptorTest : WorkflowDescriptorTestCase<ReviewProcessWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.ReviewProcessWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Review", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
			=> AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
			=> AssertEquals(false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresDepartment()
			=> AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresBranch()
			=> AssertEquals(false, WorkflowDescriptor.RequiresBranch);

		public override void TestSupportsEventTracking()
			=> AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);

		protected override bool ExpectingTasksToBeCompanySpecific
			=> false;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var reviewProcess = Factory.NewWithValidTestData<ReviewProcess>();
			return new IWorkflowProvider[] { reviewProcess };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;
	}
}
