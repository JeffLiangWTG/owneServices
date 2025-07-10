using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipmentWorkflowDescriptor))]
	class CFSShipmentWorkflowDescriptorTest : WorkflowDescriptorTestCase<CFSShipmentWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.CFSShipment.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.CFSShipment.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Sub Type 2 is Department", "Container / Packing Mode", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
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

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CFSShipmentReceival, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var shipment = Factory.New<CFSShipment>();

			shipment.CartageCoPK = DeliveryCartageOrg.PK;
			shipment.JS_OH_HandledOnBehalfOfForwarder = Forwarder.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = DeliverToOrg.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = PickupFromOrg.PK;

			return new[] { shipment };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.Email
					| MessageRecipientPartyType.TransportCo
					| MessageRecipientPartyType.Forwarder
					| MessageRecipientPartyType.Consignee
					| MessageRecipientPartyType.Consignor;
			}
		}
	}
}
