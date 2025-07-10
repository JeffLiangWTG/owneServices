using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	class UniversalCFSHelper : IUniversalFreightHelper
	{
		void IUniversalFreightHelper.AddConsolParameters(ZQuery query)
		{
			query.AddToFilter(JobConsolSchema.JK_IsCFS, true);
			query.AddToFilter(JobConsolSchema.JK_IsForwarding, false);
			query.AddToFilter(JobConsolSchema.JK_IsCancelled, false);
		}

		void IUniversalFreightHelper.AddShipmentParameters(ZQuery query)
		{
			query.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, true);
			query.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
			query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
		}

		CommonConsol IUniversalFreightHelper.GetRelatedConsolForContextValues(CommonShipment shipment)
		{
			return null;
		}

		bool IUniversalFreightHelper.ConsolHasAdditionalReferences
		{
			get { return false; }
		}

		bool IUniversalFreightHelper.ShipmentHasAdditionalReferences
		{
			get { return false; }
		}

		bool IUniversalFreightHelper.ShipmentHasOrders
		{
			get { return false; }
		}
	}
}
