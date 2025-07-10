using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record WhsWarehouseUNDGTotalsInfo
	{
		public WhsWarehouseUNDGTotalsInfo(
			ZGuid undgSubstance,
			ZGuid undgCountryReference,
			ZString undgClass,
			ZDecimal totalWeight,
			ZDecimal totalWeightLimit,
			ZString totalWeightLimitUQ,
			ZDecimal totalVolume,
			ZDecimal totalVolumeLimit,
			ZString totalVolumeLimitUQ)
		{
			UNDGSubStance = undgSubstance;
			UNDGCountryReference = undgCountryReference;
			UNDGClass = undgClass;
			TotalWeight = totalWeight;
			TotalWeightLimit = totalWeightLimit;
			TotalWeightLimitUQ = totalWeightLimitUQ;
			TotalVolume = totalVolume;
			TotalVolumeLimit = totalVolumeLimit;
			TotalVolumeLimitUQ = totalVolumeLimitUQ;
		}

		public ZGuid UNDGSubStance;
		public ZGuid UNDGCountryReference;
		public ZString UNDGClass;
		public ZDecimal TotalWeight;
		public ZDecimal TotalWeightLimit;
		public ZString TotalWeightLimitUQ;
		public ZDecimal TotalVolume;
		public ZDecimal TotalVolumeLimit;
		public ZString TotalVolumeLimitUQ;
	}
}
