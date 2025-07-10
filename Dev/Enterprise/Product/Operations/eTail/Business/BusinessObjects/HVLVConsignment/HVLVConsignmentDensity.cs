using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentDensity : Density
	{
		public HVLVConsignmentDensity(HVLVConsignment parentConsignment)
		{
			consignment = parentConsignment;
			RefreshAllValues();
		}

		#region Property Members

		protected override bool IsRefreshAllowed => true;

		protected override bool IsChargeableByWeight => consignment.IsConsignmentChargeableByWeight;

		protected override ZDecimal TotalWeight
		{
			get
			{
				if (!IsChargeableByWeight)
				{
					return consignment.EffectiveWeight;
				}
				else if (consignment.ChargeableUQ == consignment.HVC_WeightUQ)
				{
					return consignment.EffectiveWeight;
				}
				else
				{
					return consignment.ConvertToChargeableUQ(consignment.EffectiveWeight, consignment.HVC_WeightUQ);
				}
			}
		}

		protected override ZDecimal TotalVolume
		{
			get
			{
				if (IsChargeableByWeight)
				{
					return consignment.EffectiveVolume;
				}
				else if (consignment.ChargeableUQ == consignment.HVC_VolumeUQ)
				{
					return consignment.EffectiveVolume;
				}
				else
				{
					return consignment.ConvertToChargeableUQ(consignment.EffectiveVolume, consignment.HVC_VolumeUQ);
				}
			}
		}

		protected override ZDecimal CalculatedVolumeWeight => consignment.VolumeWeight;

		#endregion

		#region Implementation

		readonly HVLVConsignment consignment;

		#endregion
	}
}
