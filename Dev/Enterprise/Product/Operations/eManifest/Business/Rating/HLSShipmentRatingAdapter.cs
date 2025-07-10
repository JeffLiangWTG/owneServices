using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.eManifest.Business.Rating
{
	public class HLSShipmentRatingAdapter : ShipmentRatingAdapter<CommonShipment>, IAutoRatingChargeApplicabilityDecider
	{
		public HLSShipmentRatingAdapter(CommonShipment shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}

		readonly CommonShipment shipment;

		public override AdapterType AdapterType => AdapterType.Shipment;

		public override JobServicesCollection JobServices
		{
			get { return shipment.GetOverweightServices(); }
		}

		public override IRateableMeasureSet RateableMeasures
			=> new RateableMeasureSet(AdapterType);

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.HLSMerge; }
		}

		#region IAutoRatingChargeApplicabilityDecider

		public bool ShouldRemoveCharge(AccChargeCode chargeCode)
		{
			return chargeCode == null
				|| (chargeCode.AC_ChargeSubGroup != ChargeCodeSubGroupList.OverweightPenalty
					&& chargeCode.AC_ChargeSubGroup != ChargeCodeSubGroupList.OverweightSurcharge);
		}

		#endregion
	}
}
