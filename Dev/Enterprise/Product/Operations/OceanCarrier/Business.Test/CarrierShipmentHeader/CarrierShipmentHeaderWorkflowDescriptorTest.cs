using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderWorkflowDescriptor))]
	sealed class CarrierShipmentHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CarrierShipmentHeaderWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals("Expected Workflow Descriptor Code", "OCS", WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("Expected Workflow Descriptor Description", "Ocean Carrier Shipment", WorkflowDescriptor.Description);

		public override void TestSubTypes()
		{
			AssertEquals("we have 1 selection criterias", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Entity Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
		}

		public override void TestRequiresPorts()
		{
			Assert(nameof(WorkflowDescriptor.RequiresPort1), WorkflowDescriptor.RequiresPort1);
			Assert(nameof(WorkflowDescriptor.RequiresPort2), WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient() => Assert(nameof(WorkflowDescriptor.RequiresClient), !WorkflowDescriptor.RequiresClient);

		public override void TestRequiresBranch() => Assert(nameof(WorkflowDescriptor.RequiresBranch), WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresDepartment() => Assert(nameof(WorkflowDescriptor.RequiresDepartment), WorkflowDescriptor.RequiresDepartment);

		public override void TestSupportsEventTracking() => Assert(nameof(WorkflowDescriptor.SupportsEventTracking), WorkflowDescriptor.SupportsEventTracking);

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var shipment = Factory.New<CarrierShipmentHeader>();
			return new IWorkflowProvider[] { shipment };
		}
	}
}
