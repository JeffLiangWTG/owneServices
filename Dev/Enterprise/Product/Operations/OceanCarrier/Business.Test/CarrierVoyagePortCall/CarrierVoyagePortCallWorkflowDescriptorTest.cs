using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCallWorkflowDescriptor))]
	sealed class CarrierVoyagePortCallWorkflowDescriptorTest : WorkflowDescriptorTestCase<CarrierVoyagePortCallWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals("Expected Workflow Descriptor Code", "PRT", WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("Expected Workflow Descriptor Description", "Ocean Carrier Voyage Port Call", WorkflowDescriptor.Description);

		public override void TestSubTypes()
		{
			AssertEquals("we have 0 selection criterias", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			Assert(nameof(WorkflowDescriptor.RequiresPort1), !WorkflowDescriptor.RequiresPort1);
			Assert(nameof(WorkflowDescriptor.RequiresPort2), !WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient() => Assert(nameof(WorkflowDescriptor.RequiresClient), !WorkflowDescriptor.RequiresClient);

		public override void TestRequiresBranch() => Assert(nameof(WorkflowDescriptor.RequiresBranch), WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresDepartment() => Assert(nameof(WorkflowDescriptor.RequiresDepartment), WorkflowDescriptor.RequiresDepartment);

		public override void TestSupportsEventTracking() => Assert(nameof(WorkflowDescriptor.SupportsEventTracking), WorkflowDescriptor.SupportsEventTracking);

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var voyage = Factory.New<CarrierVoyage>();

			return new IWorkflowProvider[] { voyage };
		}
	}
}
