using System;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationInfoTest : WhsTestCaseWithFactory
	{
		public void TestWhsCycleCountLocationInfo_NullArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsCycleCountLocationInfo(Guid.NewGuid(), null, 5));
			AssertExceptionThrown<ArgumentException>(() => new WhsCycleCountLocationInfo(Guid.NewGuid(), "", 5));
		}

		public void TestWhsCycleCountLocationInfo_Constructor()
		{
			var locPK = Guid.NewGuid();
			var info = new WhsCycleCountLocationInfo(locPK, "PWA", 5);

			AssertEquals("LocationPK correct", locPK, info.LocationPK);
			AssertEquals("Granularity correct", "PWA", info.Granularity);
			AssertEquals("Priority correct", (byte)5, info.Priority);
		}
	}
}
