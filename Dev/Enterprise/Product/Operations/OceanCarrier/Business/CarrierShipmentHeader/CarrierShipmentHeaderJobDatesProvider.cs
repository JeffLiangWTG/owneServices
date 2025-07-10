using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	sealed class CarrierShipmentJobDatesProvider : JobDatesProvider<CarrierShipmentHeader>
	{
		public CarrierShipmentJobDatesProvider(CarrierShipmentHeader parent)
			: base(parent)
		{
		}

		protected override ZDateTime GetDepartureDateCore() => Parent.ExpectedDepartureDate;
		protected override ZDateTime GetArrivalDateCore() => Parent.ExpectedArrivalDate;
	}
}
