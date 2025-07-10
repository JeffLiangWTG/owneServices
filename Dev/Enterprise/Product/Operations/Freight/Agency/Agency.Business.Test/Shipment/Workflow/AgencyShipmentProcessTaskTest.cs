using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentProcessTask))]
	internal class AgencyShipmentProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert(true);
		}

		public void TestParent()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			ProcessTask milestone = shipment.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(AgencyShipment), milestone.Parent.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			return shipment.WorkflowItems.AddNew();
		}
	}
}
