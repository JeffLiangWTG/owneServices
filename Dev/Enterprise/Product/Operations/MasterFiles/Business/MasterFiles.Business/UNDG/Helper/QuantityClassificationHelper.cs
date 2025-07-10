using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class QuantityClassificationHelper
	{
		public static ZString? GetQuantityClassification(ZBool? isLimitedQuantity)
		{
			if (!isLimitedQuantity.HasValue)
			{
				return null;
			}
			return isLimitedQuantity.Value ? UNDGDataItemLookups.UNDGDataItemQuantityClasses.Code.LIM : ZString.Empty;
		}

		public static void SetQuantityClassification(UNDGDataItem item, ZBool? isLimitedQuantity)
		{
			var quantityClassification = GetQuantityClassification(isLimitedQuantity);
			if (quantityClassification.HasValue)
			{
				item.DI_QuantityClassification = quantityClassification.Value;
			}
		}
	}
}
