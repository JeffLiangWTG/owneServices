using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleUpdateNullQueryProviderTest : TestCase
	{
		public void TestShouldUpdateAgencyShipmentDatesFromATD()
		{
			AssertEquals(true, provider.ShouldUpdateAgencyShipmentDatesFromATD);
		}

		public void TestShouldUpdateRelatedShipmentsETD()
		{
			AssertEquals(true, provider.ShouldUpdateRelatedShipmentsETD);
		}

		public void TestParentPK()
		{
			AssertEquals(null, provider.ParentPK);
		}

		public void TestShouldUpdateRelatedShipmentsETA()
		{
			AssertEquals(null, provider.ParentPK);
		}

		public void TestShouldSendDelayAlerts()
		{
			AssertEquals(true, provider.ShouldSendDelayAlerts(true, true));
			AssertEquals(true, provider.ShouldSendDelayAlerts(false, true));
			AssertEquals(true, provider.ShouldSendDelayAlerts(true, false));
		}

		#region Implementation

		readonly ScheduleUpdateNullQueryProvider provider = new ScheduleUpdateNullQueryProvider();

		#endregion
	}
}
