using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderWorkflowDescriptor))]
	public class MNRWorkOrderHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<MNRWorkOrderHeaderWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Client
					| MessageRecipientPartyType.ControllingCustomer;
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Container Yard MNR Work Order Header", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.MNRWorkOrderHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert(!WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}
		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestClientLabelName()
		{
			AssertEquals("Client", WorkflowDescriptor.ClientName);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.Client.OrganisationPK = ClientOrg.PK;
			receiveAdvice.Lessee.OrganisationPK = ClientOrg.PK;

			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;

			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;

			var workOrder = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			workOrder.MWO_ParentID = yardUnitState.PK;

			return new IWorkflowProvider[] { workOrder };
		}
	}
}

