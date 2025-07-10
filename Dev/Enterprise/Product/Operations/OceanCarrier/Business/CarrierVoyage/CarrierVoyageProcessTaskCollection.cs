using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageProcessTaskCollection : ProcessTaskCollection
	{
		public CarrierVoyageProcessTaskCollection(CarrierVoyage carrierVoyage)
			: base(carrierVoyage)
		{
			this.carrierVoyage = carrierVoyage;
		}

		readonly CarrierVoyage carrierVoyage;

		public new CarrierVoyageProcessTask this[int index] => (CarrierVoyageProcessTask)Elements[index];

		public new CarrierVoyageProcessTask AddNew() => (CarrierVoyageProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new CarrierVoyageProcessTaskCollection(carrierVoyage);
	}
}
