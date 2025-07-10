using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.eTail.Business
{
	public class HVLVItemFHLMessageDetailsProvider : IAWBRateLineMessageDetailsProvider
	{
		public HVLVItemFHLMessageDetailsProvider(HVLVItem item)
		{
			this.item = item;
		}

		readonly HVLVItem item;

		public ZString WeightInLBsOrKGs => Constants.Weight.IsImperial(item.Consignment.HVC_WeightUQ) ? Constants.AWB.RateLineUQ.Pounds : Constants.AWB.RateLineUQ.Kilos;

		public ZString NatureAndQtyOfGoodsDescription => item.Consignment.HVC_GoodsDescription;
	}
}
