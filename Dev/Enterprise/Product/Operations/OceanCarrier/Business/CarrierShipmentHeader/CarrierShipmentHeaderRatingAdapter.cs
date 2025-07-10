using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.OceanCarrier.Business
{
	sealed class CarrierShipmentRatingAdapter : RatingAdapter<CarrierShipmentHeader>
	{
		public CarrierShipmentRatingAdapter(CarrierShipmentHeader parent)
			: base(parent)
		{
		}

		public override AdapterType AdapterType => AdapterType.BillOfLading;
		public override RateType RateTypeToUse => RateType.Shipping;

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider => new CarrierShipmentJobDatesProvider(Parent);

		#endregion

		#region ChargeCodeGroups

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.FreightRatedCodes);
				return result;
			}
		}

		#endregion
	}
}
