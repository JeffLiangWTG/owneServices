using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record WhsWarehouseUNDGWeightAndVolume
	{
		public WhsWarehouseUNDGWeightAndVolume(
			ZDecimal totalWeight,
			ZString totalWeightUQ,
			ZDecimal totalVolume,
			ZString totalVolumeUQ)
		{
			TotalWeight = totalWeight;
			TotalWeightUQ = totalWeightUQ;
			TotalVolume = totalVolume;
			TotalVolumeUQ = totalVolumeUQ;
		}

		public ZDecimal TotalWeight;
		public ZString TotalWeightUQ;
		public ZDecimal TotalVolume;
		public ZString TotalVolumeUQ;
	}
}
