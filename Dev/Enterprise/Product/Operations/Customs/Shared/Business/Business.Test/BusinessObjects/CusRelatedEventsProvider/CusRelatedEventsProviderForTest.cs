using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusRelatedEventsProviderForTest : CusRelatedEventsProvider
	{
		public ZQuery GetDateOfFirstArrivalQueryForTesting(CommonConsol consol)
		{
			return GetDateOfFirstArrivalQuery(consol);
		}

		protected override IEnumerable<ZString> GetAvailableApplicationCodes()
		{
			yield return Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
		}

		public void SetupShipment(Integration.Forwarding.IForwardingShipment shipment)
		{
			base.shipment = shipment as ForwardingShipment;
			factory = base.shipment?.Factory;
		}
	}
}
