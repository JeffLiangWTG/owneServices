using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	public static class NZWeightHelper
	{
		public static ZDecimal ApplyWeightRounding(ZDecimal unroundedWeight)
		{
			const decimal noWeight = 0m;
			const decimal minWeightForRounding = 1m;

			ZDecimal roundedWeight = noWeight;
			if (unroundedWeight > minWeightForRounding)
			{
				roundedWeight = unroundedWeight.Round(0);
			}
			else if (unroundedWeight > noWeight)
			{
				roundedWeight = minWeightForRounding;
			}

			return roundedWeight;
		}
	}
}
