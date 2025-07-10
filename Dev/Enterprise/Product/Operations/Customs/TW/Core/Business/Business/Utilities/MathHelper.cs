using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	static class MathHelper
	{
		public static ZDecimal CalculateDefaultQuantity(ZDecimal totalQuantity, ZDecimal usedQuantity)
		{
			var quantity = totalQuantity - usedQuantity;
			if (quantity < 0)
			{
				quantity = 0;
			}
			return quantity;
		}
	}
}
