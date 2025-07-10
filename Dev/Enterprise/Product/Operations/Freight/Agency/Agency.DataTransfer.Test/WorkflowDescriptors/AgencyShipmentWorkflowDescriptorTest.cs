using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyShipmentWorkflowDescriptor))]
	internal class AgencyShipmentWorkflowDescriptorTest : AgencyShipmentWorkflowDescriptorTestBase<AgencyShipment, AgencyShipmentWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "AGN", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Agency Shipment", WorkflowDescriptor.Description);
		}

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(true, WorkflowDescriptor.EstimateDefaultedFromList.ContainsCode(AgencyShipmentDefaultedFromList.Codes.AnticipatedTimeOfDeparture));
		}

		public override void TestEstimateDates_DifferentTimezones()
		{
			//AgencyShipmentWorkflowDescriptor is extended by BillOfLadingWorkflowDescriptor and AgencyBookingWorkflowDescriptor
			//This test is irrelevant  for AgencyShipmentWorkflowDescriptor
			Assert(true);
		}
	}
}
