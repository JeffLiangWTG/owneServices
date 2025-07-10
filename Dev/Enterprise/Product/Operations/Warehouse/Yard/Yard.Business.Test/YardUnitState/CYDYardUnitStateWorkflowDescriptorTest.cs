using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStateWorkflowDescriptor))]
	public class CYDYardUnitStateWorkflowDescriptorTest : WorkflowDescriptorTestCase<CYDYardUnitStateWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Client
					| MessageRecipientPartyType.DeliveryCartage
					| MessageRecipientPartyType.PickupCartage
					| MessageRecipientPartyType.Warehouse;
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Container Yard Yard Unit State", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CYDYardUnitStateWorkflowDescriptorCode, WorkflowDescriptor.Code);
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
			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState.CurrentYard.WarehouseAddress.OA_OH = ClientOrg.PK;

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
			receiveAdvice.Client.OrganisationPK = ClientOrg.PK;

			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var transportOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportOrgAddress.OA_OH = ClientOrg.PK;
			transportationUnit.DocAddresses.FindOrCreateWithDocAddressType(transportOrgAddress.PK, DocAddressType.TransportCompanyDocumentaryAddress);

			yardUnitState.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;
			yardUnitState.YUS_YTU_ReceiveTransportationUnit = transportationUnit.PK;
			yardUnitState.YUS_YTU_DispatchTransportationUnit = transportationUnit.PK;

			return new IWorkflowProvider[] { yardUnitState };
		}
	}
}
