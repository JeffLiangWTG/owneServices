using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.eTail.Business.Rating
{
	public class HVLVShipmentRatingAdapter : ShipmentRatingAdapter<ForwardingShipment>, IAutoRatingChargeApplicabilityDecider
	{
		public HVLVShipmentRatingAdapter(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override AdapterType AdapterType => AdapterType.HVLVShipment;

		public override JobServicesCollection JobServices => parent.GetOverweightServices();

		public override IRateableMeasureSet RateableMeasures => new RateableMeasureSet(AdapterType);

		public override MergeChargeOptions MergeCharges => MergeChargeOptions.HLSMerge;

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
