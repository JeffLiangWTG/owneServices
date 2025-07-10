using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVItemDensity : Density
	{
		public HVLVItemDensity(HVLVItem parentItem)
		{
			this.parentItem = parentItem;
		}

		readonly HVLVItem parentItem;

		protected override bool IsRefreshAllowed => true;

		protected override bool IsChargeableByWeight => parentItem.Consignment.IsConsignmentChargeableByWeight;

		protected override ZDecimal TotalWeight
		{
			get
			{
				if (!IsChargeableByWeight)
				{
					return parentItem.EffectiveWeight;
				}
				else
				{
					var consignment = parentItem.Consignment;
					if (consignment.ChargeableUQ == consignment.HVC_WeightUQ)
					{
						return parentItem.EffectiveWeight;
					}
					else
					{
						return consignment.ConvertToChargeableUQ(parentItem.EffectiveWeight, consignment.HVC_WeightUQ);
					}
				}
			}
		}

		protected override ZDecimal TotalVolume
		{
			get
			{
				if (IsChargeableByWeight)
				{
					return parentItem.EffectiveVolume;
				}
				else
				{
					var consignment = parentItem.Consignment;
					if (consignment.ChargeableUQ == consignment.HVC_VolumeUQ)
					{
						return parentItem.EffectiveVolume;
					}
					else
					{
						return consignment.ConvertToChargeableUQ(parentItem.EffectiveVolume, consignment.HVC_VolumeUQ);
					}
				}
			}
		}

		protected override ZDecimal CalculatedVolumeWeight => parentItem.VolumeWeight;
	}
}
