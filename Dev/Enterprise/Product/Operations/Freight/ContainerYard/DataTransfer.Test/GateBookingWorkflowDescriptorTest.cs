using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.DataTransfer.Testing
{
	[TestedType(typeof(GateBookingWorkflowDescriptor))]
	sealed class GateBookingWorkflowDescriptorTest : WorkflowDescriptorTestCase<GateBookingWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.DeliveryCartage
					| MessageRecipientPartyType.Carrier
					| MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.Email
					| MessageRecipientPartyType.Consignee
					| MessageRecipientPartyType.Consignor;
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Gate Booking", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.FacilityGateWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert(WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(!WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			CombineAssertions(() =>
			{
				Assert(!WorkflowDescriptor.RequiresPort1);
				Assert(!WorkflowDescriptor.RequiresPort1);
			});
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool RequiresWarehouseExpectedResult => true;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var gateBooking = Factory.New<GateBooking>();

			gateBooking.GTB_OA_TransportCompanyAddress = DeliveryCartageOrg.Addresses[0].PK;

			var yardUnit = Factory.NewWithValidTestData<YardUnit>();
			yardUnit.GTY_OA_UnitOwnerAddress = CarrierOrg.Addresses[0].PK;

			var detail = gateBooking.GateBookingDetails.AddNew();
			detail.GTD_GTY_YardUnit = yardUnit.PK;

			detail.GTD_OA_BookingPartyAddress = ConsigneeOrg.Addresses[0].PK;

			return new IWorkflowProvider[] { gateBooking };
		}
	}
}
