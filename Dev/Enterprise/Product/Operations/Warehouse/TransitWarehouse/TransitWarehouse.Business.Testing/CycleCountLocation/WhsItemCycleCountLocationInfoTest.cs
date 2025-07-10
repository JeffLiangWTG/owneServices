using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemCycleCountLocationInfoTest : TestCaseWithFactory
	{
		public void TestWhsCycleCountLocationInfo_Constructor()
		{
			var locPK = Guid.NewGuid();
			var info = new WhsItemCycleCountLocationInfo(locPK, 5);

			AssertEquals("LocationPK correct", locPK, info.LocationPK);
			AssertEquals("Priority correct", (byte)5, info.Priority);
		}
	}
}
