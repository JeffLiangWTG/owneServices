using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoProcessTaskCollection : ProcessTaskCollection
	{
		public CarrierShipmentCargoProcessTaskCollection(CarrierShipmentCargo carrierShipmentCargo)
			: base(carrierShipmentCargo)
		{
			this.carrierShipmentCargo = carrierShipmentCargo;
		}

		readonly CarrierShipmentCargo carrierShipmentCargo;

		public new CarrierShipmentCargoProcessTask this[int index] => (CarrierShipmentCargoProcessTask)Elements[index];

		public new CarrierShipmentCargoProcessTask AddNew() => (CarrierShipmentCargoProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CarrierShipmentCargoProcessTaskCollection(carrierShipmentCargo);
	}
}
