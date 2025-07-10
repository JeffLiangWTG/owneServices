using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingShipmentModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
			Factory.Save();

			var shipment = Factory.LoadTop1<TrackingShipment>(new ZQuery(JobShipmentSchema.PK, testShipment.PK));

			var milestone1 = shipment.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			shipment.ReloadMilestones();
			AssertNotEquals(0, shipment.Milestones.Count);

			return shipment;
		}
	}
}
