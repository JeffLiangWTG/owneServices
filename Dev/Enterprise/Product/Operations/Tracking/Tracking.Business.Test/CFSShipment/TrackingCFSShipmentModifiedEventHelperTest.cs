using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCFSShipmentModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			return Factory.NewWithValidTestData<TrackingCFSShipment>();
		}
	}
}
