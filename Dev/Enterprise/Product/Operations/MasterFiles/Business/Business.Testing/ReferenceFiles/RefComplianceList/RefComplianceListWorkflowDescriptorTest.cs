using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceListWorkflowDescriptor))]
	public class RefComplianceListWorkflowDescriptorTest : WorkflowDescriptorTestCase<RefComplianceListWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Compliance List", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var refComplianceList = Factory.NewWithValidTestData<RefComplianceList>();
			return new IWorkflowProvider[] { refComplianceList };
		}
	}
}
