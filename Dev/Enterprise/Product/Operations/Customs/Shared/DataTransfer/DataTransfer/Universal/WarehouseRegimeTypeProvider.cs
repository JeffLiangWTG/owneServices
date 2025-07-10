using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseRegimeTypeProvider : IWarehouseRegimeTypeProvider
	{
		public WarehouseRegimeTypeProvider(Shipment shipment)
		{
			this.shipment = shipment;
		}
		protected readonly Shipment shipment;

		public CustomsRegime GetCustomsRegime() => GetCustomsRegimeCore();

		// Implementation to be completed by Customs
		protected virtual CustomsRegime GetCustomsRegimeCore() => CustomsRegime.BondedWarehouse;
	}
}
