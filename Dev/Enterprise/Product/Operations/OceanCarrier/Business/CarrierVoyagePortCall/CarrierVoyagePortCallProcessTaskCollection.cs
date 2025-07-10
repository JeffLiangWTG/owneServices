using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallProcessTaskCollection : ProcessTaskCollection
	{
		public CarrierVoyagePortCallProcessTaskCollection(CarrierVoyagePortCall carrierVoyagePortCall)
			: base(carrierVoyagePortCall)
		{
			this.carrierVoyagePortCall = carrierVoyagePortCall;
		}

		readonly CarrierVoyagePortCall carrierVoyagePortCall;

		public new CarrierVoyagePortCallProcessTask this[int index] => (CarrierVoyagePortCallProcessTask)Elements[index];

		public new CarrierVoyagePortCallProcessTask AddNew() => (CarrierVoyagePortCallProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CarrierVoyagePortCallProcessTaskCollection(carrierVoyagePortCall);
	}
}
