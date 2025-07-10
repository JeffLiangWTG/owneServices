using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	[TestedType(typeof(GPSEvent))]
	public class GPSEventTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEventShortInfoLength()
		{
			var gpsEvent = (GPSEvent)GetNewBusinessObject();
			AssertEquals("Maximum character length should be 400 since we set this property from EN_ActivityInformation in LocalCartageVehicleActivity table.", 400, gpsEvent.EventShortInfoInfo.MaxLength);
		}
	}
}
