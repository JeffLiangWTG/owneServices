using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public CarrierShipmentHeaderProcessTaskCollection(CarrierShipmentHeader carrierShipmentHeader)
			: base(carrierShipmentHeader)
		{
			this.carrierShipmentHeader = carrierShipmentHeader;
		}

		readonly CarrierShipmentHeader carrierShipmentHeader;

		public new CarrierShipmentHeaderProcessTask this[int index] => (CarrierShipmentHeaderProcessTask)Elements[index];

		public new CarrierShipmentHeaderProcessTask AddNew() => (CarrierShipmentHeaderProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CarrierShipmentHeaderProcessTaskCollection(carrierShipmentHeader);
	}
}
