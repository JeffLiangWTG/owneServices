using CargoWise.Definitions;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyBookingWorkflowDescriptor))]
	internal class AgencyBookingWorkflowDescriptorTest : AgencyShipmentWorkflowDescriptorTestBase<AgencyBooking, AgencyBookingWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "BKN", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Shipping Manager Booking", WorkflowDescriptor.Description);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.AgencyBooking, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return base.ExpectedSupportedMessageRecipientParties | MessageRecipientPartyType.PickupCartage | MessageRecipientPartyType.DeliveryCartage;
			}
		}
	}
}
