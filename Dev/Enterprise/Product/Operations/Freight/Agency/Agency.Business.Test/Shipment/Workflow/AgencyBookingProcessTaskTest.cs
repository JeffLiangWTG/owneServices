using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingProcessTask))]
	internal class AgencyBookingProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			AgencyBooking shipment = Factory.NewWithValidTestData<AgencyBooking>();
			ProcessTask milestone = shipment.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(AgencyBooking), milestone.Parent.GetType());
			Factory.Save();
			AssertEquals("type decided correctly", typeof(AgencyBookingProcessTask), new BusinessObjectFactory().Load<ProcessTask>(milestone.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			return shipment.WorkflowItems.AddNew();
		}
	}
}
