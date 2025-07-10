using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public interface IUniversalFreightHelper
	{
		void AddConsolParameters(ZQuery query);
		void AddShipmentParameters(ZQuery query);

		CommonConsol GetRelatedConsolForContextValues(CommonShipment shipment);

		bool ConsolHasAdditionalReferences { get; }
		bool ShipmentHasAdditionalReferences { get; }
		bool ShipmentHasOrders { get; }
	}
}
