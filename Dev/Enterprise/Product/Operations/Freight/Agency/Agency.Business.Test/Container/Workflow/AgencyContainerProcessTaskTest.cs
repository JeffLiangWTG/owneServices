using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyContainerProcessTask))]
	internal class AgencyContainerProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert(true);
		}

		public void TestParent_Booking_Unsaved()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.BookedContainers.AddNew();
			var milestone = container.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(AgencyBookingContainer), milestone.Parent.GetType());
			AssertContainsExactElementsInAnyOrder("process task parent does not cause to load another bizObj around row", new[] { booking }, Factory.GetBizOsForPK(booking.PK.ToGuid()));
		}

		public void TestParent_Booking_Saved()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.BookedContainers.AddNew();
			var milestone = container.WorkflowItems.Milestones.AddNew();
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			milestone = otherFactory.Load<AgencyContainerProcessTask>(milestone.PK);
			AssertEquals(typeof(AgencyBookingContainer), milestone.Parent.GetType());
			AssertContainsExactElementsInAnyOrder("process task parent does not cause to load another bizObj around row", new[] { booking }, Factory.GetBizOsForPK(booking.PK.ToGuid()));
		}

		public void TestParent_BillOfLading_Unsaved()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.RealContainers.AddNew();
			var milestone = container.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(BillOfLadingContainer), milestone.Parent.GetType());
			AssertContainsExactElementsInAnyOrder("process task parent does not cause to load another bizObj around row", new[] { billOfLading }, Factory.GetBizOsForPK(billOfLading.PK.ToGuid()));
		}

		public void TestParent_BillOfLading_Saved()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.RealContainers.AddNew();
			var milestone = container.WorkflowItems.Milestones.AddNew();
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			milestone = otherFactory.Load<AgencyContainerProcessTask>(milestone.PK);
			AssertEquals(typeof(BillOfLadingContainer), milestone.Parent.GetType());
			AssertContainsExactElementsInAnyOrder("process task parent does not cause to load another bizObj around row", new[] { billOfLading }, Factory.GetBizOsForPK(billOfLading.PK.ToGuid()));
		}

		public void TestParent_StandaloneWhichShouldNeverHappenButLetsTestItAnyway()
		{
			var container = Factory.New<AgencyBookingContainer>();
			var milestone = container.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(AgencyBookingContainer), milestone.Parent.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			return container.WorkflowItems.AddNew();
		}
	}
}
