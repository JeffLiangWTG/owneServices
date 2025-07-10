using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class UniversalForwardingHelper : IUniversalFreightHelper
	{
		void IUniversalFreightHelper.AddConsolParameters(ZQuery query)
		{
			query.AddToFilter(JobConsolSchema.JK_IsForwarding, true);
			query.AddToFilter(JobConsolSchema.JK_IsCancelled, false);
		}

		void IUniversalFreightHelper.AddShipmentParameters(ZQuery query)
		{
			query.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, true);
			query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
		}

		CommonConsol IUniversalFreightHelper.GetRelatedConsolForContextValues(CommonShipment shipment)
		{
			return shipment.MostInterestingDepartureConsol;
		}

		bool IUniversalFreightHelper.ConsolHasAdditionalReferences
		{
			get { return true; }
		}

		bool IUniversalFreightHelper.ShipmentHasAdditionalReferences
		{
			get { return true; }
		}

		bool IUniversalFreightHelper.ShipmentHasOrders
		{
			get { return true; }
		}
	}
}
