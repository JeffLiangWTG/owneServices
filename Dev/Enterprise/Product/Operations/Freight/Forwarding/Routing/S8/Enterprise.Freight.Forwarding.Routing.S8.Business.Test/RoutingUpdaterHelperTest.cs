using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class RoutingUpdaterHelperTest : TestCaseWithFactory
	{
		public void TestGetUNLOCOFromIATACode()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals("AUSYD", RoutingUpdaterHelper.GetUNLOCOFromIATACode("SYD", factory));
			AssertEquals("HKHKG", RoutingUpdaterHelper.GetUNLOCOFromIATACode("HKG", factory));
			AssertEquals(ZString.Empty, RoutingUpdaterHelper.GetUNLOCOFromIATACode("QQQ", factory));
		}

		public void TestUpdateDateTime()
		{
			var today = ZDateTime.Today;
			var dateComponent = today.Date;

			AssertEquals(dateComponent.Add(new TimeSpan(8, 50, 0)), RoutingUpdaterHelper.UpdateDateTime(today, "08:50"));
			AssertEquals(dateComponent.Add(new TimeSpan(12, 59, 0)), RoutingUpdaterHelper.UpdateDateTime(today, "12:59"));
			AssertEquals(dateComponent.Add(new TimeSpan(1, 13, 3, 0)), RoutingUpdaterHelper.UpdateDateTime(today, "1+13:03"));
			AssertEquals(dateComponent.Add(new TimeSpan(2, 23, 59, 0)), RoutingUpdaterHelper.UpdateDateTime(today, "2+23:59"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "2abcd23:593"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, string.Empty));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "1+"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "1abcdef+"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "x+abcdef+"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "29:30"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "24:30"));
			AssertEquals(dateComponent.ToDateTime(), RoutingUpdaterHelper.UpdateDateTime(today, "20:90"));
		}
	}
}
