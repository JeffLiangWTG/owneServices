using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TariffHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 6, 5)]
		public void TestGetHSCodeEffectiveDateFromShipment()
		{
			var shipment1 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment2 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			shipment1.JS_E_DEP = ZDateTime.Now.AddDays(10);

			Factory.Save();

			var shipment3 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			AssertEquals(new ZDateTime(2017, 6, 15), TariffHelper.GetHSCodeEffectiveDate(shipment1));
			AssertEquals(shipment2.JS_SystemCreateTimeUtc, TariffHelper.GetHSCodeEffectiveDate(shipment2));
			AssertEquals(new ZDateTime(2017, 6, 5), TariffHelper.GetHSCodeEffectiveDate(shipment3));
		}

		[TestDate(2018, 3, 20)]
		public void TestGetHSCodeEffectiveDateFromConsol()
		{
			var consol1 = Factory.New<CommonConsol>();
			var consol2 = Factory.New<CommonConsol>();

			consol1.JK_TransportMode = "SEA";
			consol1.Transports[0].JW_TransportMode = "SEA";
			consol1.Transports[0].JW_ETD = ZDateTime.Now.AddDays(10);

			AssertEquals(ZDateTime.Now.AddDays(10), TariffHelper.GetHSCodeEffectiveDate(consol1));
			AssertEquals(new ZDateTime(2018, 3, 20), TariffHelper.GetHSCodeEffectiveDate(consol2));
		}
	}
}
