using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(ContainerMovementWorkflowDescriptor))]
	internal class ContainerMovementWorkflowDescriptorTest : WorkflowDescriptorTestCase<ContainerMovementWorkflowDescriptor>
	{
		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<ContainerMovement>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy;
			}
		}

		public void TestWorkflowType()
		{
			AssertEquals(typeof(ContainerMovement), WorkflowDescriptor.WorkflowProviderType);
		}

		public override void TestID()
		{
			AssertEquals(EDICommunicationsMode.Modules.ContainerMovements, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Container Movement", WorkflowDescriptor.Description);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresBranch()
		{
			Assert(!WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(!WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}
	}
}
