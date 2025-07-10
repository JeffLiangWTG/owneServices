using CargoWise.Common;
using CargoWise.Data.Testing;
using Enterprise.eTail.Business.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	public class HVLVReadyLogSubscriberUsingSnapshotProtectionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestHVLVReadyLogSubscriberDoesNotSaveInLWKTransmitter()
		{
			HVLVTestHelper.CreateShipmentWithHLREvent(new CargoWise.EntityFramework.BusinessObjectFactory(), Core.Constants.ShipmentTypes.HighVolumeLowValue, Core.Constants.TransportModes.Air);

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("There should not be an error regarding factory save.", 0, ErrorReporter.ExceptionsThrown.Count);
		}
	}
}
